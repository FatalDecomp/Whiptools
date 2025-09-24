using System;
using System.Collections.Generic;

namespace Whiptools
{
    class Unmangler
    {
        public static byte[] Unmangle(byte[] input)
        {
            int outLength = BitConverter.ToInt32(input, 0); // output length is first 4 bytes of input
            var output = new byte[outLength];

            // start positions
            int inPos = 4;
            int outPos = 0;

            while ((inPos < input.Length) && (outPos < outLength))
            {
                int ctrl = Convert.ToInt32(input[inPos]);

                if (ctrl == 0x00) // 0x00: terminate output
                    return output;

                if (ctrl <= 0x3F) // 0x01 to 0x3F: literal copy from input
                {
                    if (inPos + 1 + ctrl > input.Length || outPos + ctrl > outLength)
                        throw new Exception();
                    Array.Copy(input, inPos + 1, output, outPos, ctrl);
                    inPos += ctrl + 1;
                    outPos += ctrl;
                }
                else if (ctrl <= 0x4F) // 0x40 to 0x4F: byte difference sequence
                {
                    int delta = output[outPos - 1] - output[outPos - 2];
                    for (int i = 0; i < (ctrl & 0x0F) + 3; i++)
                    {
                        output[outPos] = (byte)((output[outPos - 1] + delta) & 0xFF);
                        outPos++;
                    }
                    inPos++;
                }
                else if (ctrl <= 0x5F) // 0x50 to 0x5F: word difference sequence
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
                else if (ctrl <= 0x6F) // 0x60 to 0x6F: byte repeat
                {
                    for (int i = 0; i < (ctrl & 0x0F) + 3; i++)
                    {
                        output[outPos] = output[outPos - 1];
                        outPos++;
                    }
                    inPos++;
                }
                else if (ctrl <= 0x7F) // 0x70 to 0x7F: word repeat
                {
                    for (int i = 0; i < (ctrl & 0x0F) + 2; i++)
                    {
                        output[outPos] = output[outPos - 2];
                        output[outPos + 1] = output[outPos - 1];
                        outPos += 2;
                    }
                    inPos++;
                }
                else if (ctrl <= 0xBF) // 0x80 to 0xBF: short block (3 bytes)
                {
                    int offset = (ctrl & 0x3F) + 3;
                    for (int i = 0; i < 3; i++)
                    {
                        output[outPos] = output[outPos - offset];
                        outPos++;
                    }
                    inPos++;
                }
                else if (ctrl <= 0xDF) // 0xC0 to 0xDF: medium block (offset and length from next byte)
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
                else // 0xE0 to 0xFF: long block (offset and length from next 2 bytes)
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

    class Mangler
    {
        public static byte[] Mangle(byte[] input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var output = new List<byte>(input.Length / 2 + 16);
            output.AddRange(BitConverter.GetBytes(input.Length)); // original length

            var literals = new List<byte>(64);
            int pos = 0;

            while (pos < input.Length)
            {
                // try repeat/diff
                if (TryByteRepeat(input, pos, literals, output, ref pos)) continue;
                if (TryWordRepeat(input, pos, literals, output, ref pos)) continue;
                if (TryByteDiffSeq(input, pos, literals, output, ref pos)) continue;
                if (TryWordDiffSeq(input, pos, literals, output, ref pos)) continue;

                // try block copy
                if (TryBlockCopy(input, pos, literals, output, ref pos)) continue;

                // otherwise literal
                literals.Add(input[pos]);
                pos++;
                if (literals.Count == 63)
                    FlushLiterals(literals, output);
            }
            FlushLiterals(literals, output);

            output.Add((byte)0x00); // terminate with zero
            return output.ToArray();
        }

        // literal (0x01 to 0x3F)

        private static void FlushLiterals(List<byte> literals, List<byte> output)
        {
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

        // byte repeat (0x60 to 0x6F)

        private static bool TryByteRepeat(byte[] input, int pos, List<byte> literals, List<byte> output, ref int newPos)
        {
            if (pos == 0) return false;
            byte val = input[pos];
            if (val != input[pos - 1]) return false;

            int run = 1;
            while (pos + run < input.Length && input[pos + run] == val) run++;
            if (run < 3) return false;

            FlushLiterals(literals, output);
            int toEmit = Math.Min(run, 18);
            byte ctrl = (byte)(0x60 | (toEmit - 3));
            output.Add(ctrl);

            newPos = pos + toEmit;
            return true;
        }

        // word repeat (0x70 to 0x7F)

        private static bool TryWordRepeat(byte[] input, int pos, List<byte> literals, List<byte> output, ref int newPos)
        {
            if (pos < 2) return false;

            if (pos + 1 >= input.Length) return false;
            byte a0 = input[pos], a1 = input[pos + 1];
            byte b0 = input[pos - 2], b1 = input[pos - 1];
            if (a0 != b0 || a1 != b1) return false;

            int runWords = 1;
            while (pos + 2 * runWords + 1 < input.Length)
            {
                if (input[pos + 2 * runWords] != b0 || input[pos + 2 * runWords + 1] != b1)
                    break;
                runWords++;
            }
            if (runWords < 2) return false;

            FlushLiterals(literals, output);
            int toEmit = Math.Min(runWords, 17); // 2..17 words
            byte ctrl = (byte)(0x70 | (toEmit - 2));
            output.Add(ctrl);

            newPos = pos + 2 * toEmit;
            return true;
        }

        // byte diff sequence (0x40 to 0x4F)

        private static bool TryByteDiffSeq(byte[] input, int pos, List<byte> literals, List<byte> output, ref int newPos)
        {
            if (pos < 2) return false;

            int delta = input[pos - 1] - input[pos - 2];
            int len = 0;
            byte prev = input[pos - 1];
            while (pos + len < input.Length && input[pos + len] == (byte)(prev + delta))
            {
                prev = input[pos + len];
                len++;
            }

            if (len < 3) return false;

            FlushLiterals(literals, output);
            int toEmit = Math.Min(len, 18); // 3..18
            byte ctrl = (byte)(0x40 | (toEmit - 3));
            output.Add(ctrl);

            newPos = pos + toEmit;
            return true;
        }

        // word diff sequence (0x50 to 0x5F)

        private static bool TryWordDiffSeq(byte[] input, int pos, List<byte> literals, List<byte> output, ref int newPos)
        {
            if (pos < 4 || pos + 1 >= input.Length) return false;

            short w0 = BitConverter.ToInt16(input, pos - 4);
            short w1 = BitConverter.ToInt16(input, pos - 2);
            short delta = (short)(w1 - w0);

            int len = 0;
            short prev = w1;
            while (pos + 2 * len + 1 < input.Length)
            {
                short expect = (short)(prev + delta);
                short actual = BitConverter.ToInt16(input, pos + 2 * len);
                if (expect != actual) break;
                prev = actual;
                len++;
            }

            if (len < 2) return false;

            FlushLiterals(literals, output);
            int toEmit = Math.Min(len, 17); // 2..17
            byte ctrl = (byte)(0x50 | (toEmit - 2));
            output.Add(ctrl);

            newPos = pos + 2 * toEmit;
            return true;
        }

        // block copy (0x80 to 0xFF)

        private static bool TryBlockCopy(byte[] input, int pos, List<byte> literals, List<byte> output, ref int newPos)
        {
            int bestLen = 0;
            int bestDist = 0;
            int maxSearch = Math.Min(pos, 8194);
            int maxMatch = Math.Min(input.Length - pos, 260);

            for (int dist = 3; dist <= maxSearch; dist++)
            {
                int s = pos - dist;

                // quick reject: check first byte before entering loop
                if (input[s] != input[pos]) continue;

                int m = 0;
                while (m < maxMatch && input[s + m] == input[pos + m]) m++;

                if (m > bestLen)
                {
                    bestLen = m;
                    bestDist = dist;

                    // early exit: can't encode longer than 260
                    if (bestLen == 260) break;

                    // optional: break if bestLen already >= 18
                    if (bestLen >= 18 && bestDist <= 8194) break;
                }
            }

            if (bestLen < 3) return false;

            FlushLiterals(literals, output);

            if (bestLen >= 5 && bestDist <= 8194) // long block (0xE0 to 0xFF)
            {
                int len = Math.Min(bestLen, 260);
                EmitLongBlock(output, bestDist, len);
                newPos = pos + len;
                return true;
            }
            if (bestLen >= 4 && bestDist <= 1026) // medium block (0xC0 to 0xDF)
            {
                int len = Math.Min(bestLen, 11);
                EmitMediumBlock(output, bestDist, len);
                newPos = pos + len;
                return true;
            }
            if (bestLen >= 3 && bestDist <= 66) // short block (0x80 to 0xBF)
            {
                EmitShortBlock(output, bestDist);
                newPos = pos + 3;
                return true;
            }

            return false;
        }

        private static void EmitShortBlock(List<byte> output, int distance)
        {
            int off = distance - 3;
            output.Add((byte)(0x80 | off));
        }

        private static void EmitMediumBlock(List<byte> output, int distance, int length)
        {
            int off = distance - 3;
            output.Add((byte)(0xC0 | ((length - 4) << 2) | ((off >> 8) & 0x03)));
            output.Add((byte)(off & 0xFF));
        }

        private static void EmitLongBlock(List<byte> output, int distance, int length)
        {
            int off = distance - 3;
            output.Add((byte)(0xE0 | ((off >> 8) & 0x1F)));
            output.Add((byte)(off & 0xFF));
            output.Add((byte)(length - 5));
        }
    }

    class FibCipher
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