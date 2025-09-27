using System;
using System.Collections.Generic;

namespace Whiptools
{
    public static class Unmangler
    {
        public static byte[] Unmangle(byte[] input)
        {
            if (input == null) throw new ArgumentNullException();
            int outLength = BitConverter.ToInt32(input, 0); // output length is first 4 bytes of input
            if (outLength > 100000000) throw new OutOfMemoryException();
            var output = new byte[outLength];

            // start positions
            int inPos = 4;
            int outPos = 0;

            while ((inPos < input.Length) && (outPos < outLength))
            {
                int ctrl = Convert.ToInt32(input[inPos]);

                if (ctrl == 0x00) // 0x00: terminate output
                    return output;
                else if (ctrl <= 0x3F) // 0x01..0x3F: literal copy from input
                {
                    if (inPos + 1 + ctrl > input.Length || outPos + ctrl > outLength)
                        throw new IndexOutOfRangeException();
                    Array.Copy(input, inPos + 1, output, outPos, ctrl);
                    inPos += ctrl + 1;
                    outPos += ctrl;
                }
                else if (ctrl <= 0x4F) // 0x40..0x4F: byte diff
                {
                    int delta = output[outPos - 1] - output[outPos - 2];
                    for (int i = 0; i < (ctrl & 0x0F) + 3; i++)
                    {
                        output[outPos] = (byte)((output[outPos - 1] + delta) & 0xFF);
                        outPos++;
                    }
                    inPos++;
                }
                else if (ctrl <= 0x5F) // 0x50..0x5F: word diff
                {
                    short delta = (short)(BitConverter.ToInt16(output, outPos - 2) -
                        BitConverter.ToInt16(output, outPos - 4));
                    for (int i = 0; i < (ctrl & 0x0F) + 2; i++)
                    {
                        short newShort = (short)(BitConverter.ToInt16(output, outPos - 2) + delta);
                        output[outPos] = (byte)(newShort & 0xFF);
                        output[outPos + 1] = (byte)((newShort >> 8) & 0xFF);
                        outPos += 2;
                    }
                    inPos++;
                }
                else if (ctrl <= 0x6F) // 0x60..0x6F: byte repeat
                {
                    for (int i = 0; i < (ctrl & 0x0F) + 3; i++)
                    {
                        output[outPos] = output[outPos - 1];
                        outPos++;
                    }
                    inPos++;
                }
                else if (ctrl <= 0x7F) // 0x70..0x7F: word repeat
                {
                    for (int i = 0; i < (ctrl & 0x0F) + 2; i++)
                    {
                        output[outPos] = output[outPos - 2];
                        output[outPos + 1] = output[outPos - 1];
                        outPos += 2;
                    }
                    inPos++;
                }
                else if (ctrl <= 0xBF) // 0x80..0xBF: short block (3 bytes)
                {
                    int offset = (ctrl & 0x3F) + 3;
                    for (int i = 0; i < 3; i++)
                    {
                        output[outPos] = output[outPos - offset];
                        outPos++;
                    }
                    inPos++;
                }
                else if (ctrl <= 0xDF) // 0xC0..0xDF: medium block (offset and length from next byte)
                {
                    int offset = ((ctrl & 0x03) << 8) + Convert.ToInt32(input[inPos + 1]) + 3;
                    int length = ((ctrl >> 2) & 0x07) + 4;
                    for (int i = 0; i < length; i++)
                    {
                        output[outPos] = output[outPos - offset];
                        outPos++;
                    }
                    inPos += 2;
                }
                else // 0xE0..0xFF: long block (offset and length from next 2 bytes)
                {
                    int offset = ((ctrl & 0x1F) << 8) + Convert.ToInt32(input[inPos + 1]) + 3;
                    int length = Convert.ToInt32(input[inPos + 2]) + 5;
                    for (int i = 0; i < length; i++)
                    {
                        output[outPos] = output[outPos - offset];
                        outPos++;
                    }
                    inPos += 3;
                }
            }
            return output;
        }
    }

    public static class Mangler
    {
        private enum Opcode
        {
            None = 0, ByteDiff, WordDiff,
            ByteRepeat, WordRepeat,
            ShortBlock, MediumBlock, LongBlock
        }

        private struct Candidate
        {
            public Opcode Type; // opcode
            public int Cover;   // input bytes advanced
            public int Cost;    // output bytes added
            public int Dist;    // blocks
            public int Len;     // length (bytes or words)
            public bool IsValid { get { return Type != Opcode.None && Cover > 0; } }
        }

        public static byte[] Mangle(byte[] input)
        {
            if (input == null) throw new ArgumentNullException();
            if (input.Length > 100000000) throw new OutOfMemoryException();

            var outList = new List<byte>(input.Length / 2 + 16);
            outList.AddRange(BitConverter.GetBytes(input.Length)); // original length

            var literals = new List<byte>(64);
            int pos = 0;

            while (pos < input.Length)
            {
                Candidate bestNow = ChooseBest(input, pos);
                if (!bestNow.IsValid) // no non-literal candidate
                {
                    literals.Add(input[pos]);
                    pos++;
                    if (literals.Count == 63) FlushLiterals(literals, outList);
                    continue;
                }

                // lookahead: one literal + best at next pos
                Candidate bestNext = (pos + 1 < input.Length) ?
                    ChooseBest(input, pos + 1) : default;

                // lookahead: incremental cost
                int nextLitCost = (literals.Count % 63 == 0) ? 2 : 1;

                // cross-multiply cost/cover ratios and compare
                int lhs = (nextLitCost + bestNext.Cost) * bestNow.Cover;
                int rhs = bestNow.Cost * (1 + bestNext.Cover);
                if (lhs < rhs)
                {
                    literals.Add(input[pos]); // take one literal and re-evaluate
                    pos++;
                    if (literals.Count == 63) FlushLiterals(literals, outList);
                    continue;
                }

                FlushLiterals(literals, outList);
                Emit(outList, bestNow);
                pos += bestNow.Cover;
            }

            FlushLiterals(literals, outList);
            outList.Add((byte)0x00); // terminate with zero
            return outList.ToArray();
        }

        private static Candidate ChooseBest(byte[] input, int pos)
        {
            Candidate best = default;
            best = Better(best, TryByteDiff(input, pos));
            best = Better(best, TryWordDiff(input, pos));
            best = Better(best, TryByteRepeat(input, pos));
            best = Better(best, TryWordRepeat(input, pos));
            best = Better(best, TryBlock(input, pos));
            return best;
        }

        private static Candidate Better(Candidate a, Candidate b)
        {
            if (!a.IsValid) return b;
            if (!b.IsValid) return a;

            // cross-multiply, prefer lower cost/cover
            int lhs = a.Cost * b.Cover;
            int rhs = b.Cost * a.Cover;
            if (rhs < lhs) return b;
            if (lhs < rhs) return a;

            // tie-breaker: larger cover
            if (b.Cover > a.Cover) return b;
            if (a.Cover > b.Cover) return a;

            // tie-breaker: prefer block over diff/repeat
            bool aIsBlock = a.Type == Opcode.ShortBlock ||
                a.Type == Opcode.MediumBlock || a.Type == Opcode.LongBlock;
            bool bIsBlock = b.Type == Opcode.ShortBlock ||
                b.Type == Opcode.MediumBlock || b.Type == Opcode.LongBlock;
            if (bIsBlock && !aIsBlock) return b;
            if (aIsBlock && !bIsBlock) return a;

            // tie-breaker: shorter distance
            if (b.Dist != 0 && a.Dist != 0 && b.Dist < a.Dist) return b;
            return a;
        }

        private static void FlushLiterals(List<byte> literals, List<byte> output)
        {
            if (literals.Count == 0) return;
            int idx = 0;
            while (idx < literals.Count)
            {
                int chunk = Math.Min(63, literals.Count - idx);
                output.Add((byte)chunk);
                for (int i = 0; i < chunk; i++)
                    output.Add(literals[idx + i]);
                idx += chunk;
            }
            literals.Clear();
        }

        private static void Emit(List<byte> output, Candidate c)
        {
            switch (c.Type)
            {
                case Opcode.ByteDiff: // 0x40..0x4F
                {
                    int len = c.Len - 3;
                    output.Add((byte)(0x40 | len));
                    break;
                }
                case Opcode.WordDiff: // 0x50..0x5F
                {
                    int len = c.Len - 2;
                    output.Add((byte)(0x50 | len));
                    break;
                }
                case Opcode.ByteRepeat: // 0x60..0x6F
                {
                    int len = c.Len - 3;
                    output.Add((byte)(0x60 | len));
                    break;
                }
                case Opcode.WordRepeat: // 0x70..0x7F
                {
                    int len = c.Len - 2;
                    output.Add((byte)(0x70 | len));
                    break;
                }
                case Opcode.ShortBlock: // 0x80..0xBF
                {
                    int off = c.Dist - 3;
                    output.Add((byte)(0x80 | off));
                    break;
                }
                case Opcode.MediumBlock: // 0xC0..0xDF
                {
                    int off = c.Dist - 3;
                    output.Add((byte)(0xC0 | ((c.Len - 4) << 2) | ((off >> 8) & 0x03)));
                    output.Add((byte)(off & 0xFF));
                    break;
                }
                case Opcode.LongBlock: // 0xE0..0xFF
                {
                    int off = c.Dist - 3;
                    output.Add((byte)(0xE0 | ((off >> 8) & 0x1F)));
                    output.Add((byte)(off & 0xFF));
                    output.Add((byte)(c.Len - 5));
                    break;
                }
            }
        }

        private static Candidate TryByteDiff(byte[] input, int pos)
        {
            if (pos < 2) return default;

            byte b0 = input[pos - 2], b1 = input[pos - 1];
            int delta = b1 - b0;
            int len = 0;
            int max = Math.Min(18, input.Length - pos);
            while (len < max && input[pos + len] == (byte)(b1 + delta))
            {
                b1 = input[pos + len];
                len++;
            }
            if (len < 3) return default;

            return new Candidate
            {
                Type = Opcode.ByteDiff,
                Cover = len, Cost = 1, Len = len
            };
        }

        private static Candidate TryWordDiff(byte[] input, int pos)
        {
            if (pos < 4 || pos + 1 >= input.Length) return default;

            short b0 = BitConverter.ToInt16(input, pos - 4);
            short b1 = BitConverter.ToInt16(input, pos - 2);
            short delta = (short)(b1 - b0);
            int len = 0;
            int max = 17;
            while (len < max && pos + 2 * len + 1 < input.Length)
            {
                short expect = (short)(b1 + (len + 1) * delta);
                short actual = BitConverter.ToInt16(input, pos + 2 * len);
                if (expect != actual) break;
                len++;
            }
            if (len < 2) return default;

            return new Candidate
            {
                Type = Opcode.WordDiff,
                Cover = len * 2, Cost = 1, Len = len
            };
        }
        
        private static Candidate TryByteRepeat(byte[] input, int pos)
        {
            if (pos == 0) return default;

            byte a = input[pos], b = input[pos - 1];
            if (a != b) return default;

            int len = 1;
            int max = Math.Min(18, input.Length - pos);
            while (len < max && input[pos + len] == a) len++;
            if (len < 3) return default;

            return new Candidate
            {
                Type = Opcode.ByteRepeat,
                Cover = len, Cost = 1, Len = len
            };
        }

        private static Candidate TryWordRepeat(byte[] input, int pos)
        {
            if (pos < 2 || pos + 1 >= input.Length) return default;

            byte a0 = input[pos], a1 = input[pos + 1];
            byte b0 = input[pos - 2], b1 = input[pos - 1];
            if (a0 != b0 || a1 != b1) return default;

            int len = 1;
            int max = 17;
            while (len < max && pos + 2 * len + 1 < input.Length)
            {
                if (input[pos + 2 * len] != b0 ||
                    input[pos + 2 * len + 1] != b1) break;
                len++;
            }
            if (len < 2) return default;

            return new Candidate
            {
                Type = Opcode.WordRepeat,
                Cover = len * 2, Cost = 1, Len = len
            };
        }

        private static Candidate TryBlock(byte[] input, int pos)
        {
            Candidate bestShort = default,
                bestMedium = default, bestLong = default;

            int maxSearch = Math.Min(pos, 8194);
            int maxMatch = Math.Min(input.Length - pos, 260);

            for (int dist = 3; dist <= maxSearch; dist++)
            {
                int s = pos - dist;
                if (input[s] != input[pos]) continue; // quick reject

                int m = 0;
                while (m < maxMatch && input[s + m] == input[pos + m]) m++;
                if (m < 3) continue;

                // short: offset 3..66, len 3, cost 1
                if (dist <= 66 && m >= 3)
                {
                    var c = new Candidate
                    {
                        Type = Opcode.ShortBlock,
                        Cover = 3, Cost = 1,
                        Dist = dist, Len = 3
                    };
                    bestShort = Better(bestShort, c);
                }

                // medium: offset 3..1026, len 4..11, cost 2
                if (dist <= 1026 && m >= 4)
                {
                    int len = Math.Min(m, 11);
                    var c = new Candidate
                    {
                        Type = Opcode.MediumBlock,
                        Cover = len, Cost = 2,
                        Dist = dist, Len = len
                    };
                    bestMedium = Better(bestMedium, c);
                }

                // long: offset 3..8194, len 5..260, cost 3
                if (dist <= 8194 && m >= 5)
                {
                    int len = Math.Min(m, 260);
                    var c = new Candidate
                    {
                        Type = Opcode.LongBlock,
                        Cover = len, Cost = 3,
                        Dist = dist, Len = len
                    };
                    bestLong = Better(bestLong, c);
                    if (bestLong.Cover == 260) break; // max
                }
            }

            Candidate best = default;
            best = Better(best, bestShort);
            best = Better(best, bestMedium);
            best = Better(best, bestLong);
            return best;
        }
    }

    public static class VerifyMangle
    {
        public static bool Verify(byte[] original, byte[] mangled)
        {
            byte[] unmangled = Unmangler.Unmangle(mangled);
            if (original == null || unmangled == null) return false;
            if (original.Length != unmangled.Length) return false;
            for (int i = 0; i < original.Length; i++)
                if (original[i] != unmangled[i]) return false;
            return true;
        }
    }

    public static class FibCipher
    {
        public static byte[] Decode(byte[] inputData, int a0, int a1)
        {
            int length = inputData.Length;
            var outputData = new byte[length];

            for (int i = 0; i < length; i++)
            {
                int a2 = (a0 + a1) & 0xFF;
                outputData[i] = (byte)(inputData[i] ^ a2);
                a0 = a1;
                a1 = a2;
            }
            return outputData;
        }
    }
}