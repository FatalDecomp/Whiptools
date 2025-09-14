using System;
using System.Collections.Generic;

namespace Whiptools
{
    class Unmangler
    {
        public static byte[] Unmangle(byte[] inputData)
        {
            int outputLength = BitConverter.ToInt32(inputData, 0); // output length is first 4 bytes of input
            var outputData = new byte[outputLength];

            // start positions
            int inputPos = 4;
            int outputPos = 0;

            while ((inputPos < inputData.Length) && (outputPos < outputLength))
            {
                int byteValue = Convert.ToInt32(inputData[inputPos]);

                if (byteValue == 0x00) // 0x00: terminate output
                    return outputData;

                if (byteValue <= 0x3F) // 0x01 to 0x3F: read bytes from input
                {
                    if (inputPos + 1 + byteValue > inputData.Length || outputPos + byteValue > outputLength)
                        throw new Exception();
                    Array.Copy(inputData, inputPos + 1, outputData, outputPos, byteValue);
                    inputPos += byteValue + 1;
                    outputPos += byteValue;
                }
                else if (byteValue <= 0x4F) // 0x40 to 0x4F: generate ascending bytes based on last 2 bytes
                {
                    int delta = outputData[outputPos - 1] - outputData[outputPos - 2];
                    for (int i = 0; i < ((byteValue & 0x0F) + 3); i++)
                    {
                        outputData[outputPos] = (byte)((outputData[outputPos - 1] + delta) & 0xFF);
                        outputPos++;
                    }
                    inputPos++;
                }
                else if (byteValue <= 0x5F) // 0x50 to 0x5F: generate ascending words based on last 2 words
                {
                    short delta = (short)(BitConverter.ToInt16(outputData, outputPos - 2) -
                        BitConverter.ToInt16(outputData, outputPos - 4));
                    for (int i = 0; i < ((byteValue & 0x0F) + 2); i++)
                    {
                        short newShort = (short)(BitConverter.ToInt16(outputData, outputPos - 2) + delta);
                        outputData[outputPos] = (byte)(newShort & 0xFF);
                        outputData[outputPos + 1] = (byte)((newShort >> 8) & 0xFF);
                        outputPos += 2;
                    }
                    inputPos++;
                }
                else if (byteValue <= 0x6F) // 0x60 to 0x6F: clone last byte in output
                {
                    for (int i = 0; i < ((byteValue & 0x0F) + 3); i++)
                    {
                        outputData[outputPos] = outputData[outputPos - 1];
                        outputPos++;
                    }
                    inputPos++;
                }
                else if (byteValue <= 0x7F) // 0x70 to 0x7F: clone last word in output
                {
                    for (int i = 0; i < ((byteValue & 0x0F) + 2); i++)
                    {
                        outputData[outputPos] = outputData[outputPos - 2];
                        outputData[outputPos + 1] = outputData[outputPos - 1];
                        outputPos += 2;
                    }
                    inputPos++;
                }
                else if (byteValue <= 0xBF) // 0x80 to 0xBF: clone 3 bytes using offset
                {
                    int offset = (byteValue & 0x3F);
                    outputData[outputPos] = outputData[outputPos - offset - 3];
                    outputData[outputPos + 1] = outputData[outputPos - offset - 2];
                    outputData[outputPos + 2] = outputData[outputPos - offset - 1];
                    outputPos += 3;
                    inputPos++;
                }
                else if (byteValue <= 0xDF) // 0xC0 to 0xDF: clone using offset and length from next byte
                {
                    int offset = ((byteValue & 0x03) << 8) + Convert.ToInt32(inputData[inputPos + 1]) + 3;
                    int length = ((byteValue >> 2) & 0x07) + 4;
                    for (int i = 0; i < length; i++)
                    {
                        outputData[outputPos] = outputData[outputPos - offset];
                        outputPos++;
                    }
                    inputPos += 2;
                }
                else // 0xE0 to 0xFF: clone using offset and length from next 2 bytes
                {
                    int offset = ((byteValue & 0x1F) << 8) + Convert.ToInt32(inputData[inputPos + 1]) + 3;
                    int length = Convert.ToInt32(inputData[inputPos + 2]) + 5;
                    for (int i = 0; i < length; i++)
                    {
                        outputData[outputPos] = outputData[outputPos - offset];
                        outputPos++;
                    }
                    inputPos += 3;
                }
            }
            return outputData;
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
                // Try specialized encodings first
                if (TryCloneLastByte(input, pos, literals, output, ref pos)) continue;
                if (TryCloneLastWord(input, pos, literals, output, ref pos)) continue;
                if (TryPredictByteSeq(input, pos, literals, output, ref pos)) continue;
                if (TryPredictWordSeq(input, pos, literals, output, ref pos)) continue;

                // Try back-reference
                if (TryBackref(input, pos, literals, output, ref pos)) continue;

                // Otherwise literal
                literals.Add(input[pos]);
                pos++;
                if (literals.Count == 63)
                    FlushLiterals(literals, output);
            }
            FlushLiterals(literals, output);

            output.Add((byte)0x00); // terminate with zero
            return output.ToArray();
        }

        // ----------------- Literals -----------------

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

        // ----------------- RLE (0x60..0x6F) -----------------

        private static bool TryCloneLastByte(byte[] input, int pos, List<byte> literals, List<byte> output, ref int newPos)
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

        // ----------------- Word clone (0x70..0x7F) -----------------

        private static bool TryCloneLastWord(byte[] input, int pos, List<byte> literals, List<byte> output, ref int newPos)
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

        // ----------------- Predictor bytes (0x40..0x4F) -----------------

        private static bool TryPredictByteSeq(byte[] input, int pos, List<byte> literals, List<byte> output, ref int newPos)
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

        // ----------------- Predictor words (0x50..0x5F) -----------------

        private static bool TryPredictWordSeq(byte[] input, int pos, List<byte> literals, List<byte> output, ref int newPos)
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

        // ----------------- Backrefs -----------------

        private static bool TryBackref(byte[] input, int pos, List<byte> literals, List<byte> output, ref int newPos)
        {
            int bestLen = 0;
            int bestDist = 0;
            int maxSearch = Math.Min(pos, 8194);
            int maxMatch = Math.Min(input.Length - pos, 260);

            for (int dist = 3; dist <= maxSearch; dist++)
            {
                int s = pos - dist;
                int m = 0;

                // Quick reject: check first byte before entering loop
                if (input[s] != input[pos]) continue;

                while (m < maxMatch && input[s + m] == input[pos + m]) m++;

                if (m > bestLen)
                {
                    bestLen = m;
                    bestDist = dist;

                    // Early exit: can't encode longer than 260
                    if (bestLen == 260) break;

                    // Optional: break if bestLen already >= 18
                    if (bestLen >= 18 && bestDist <= 8194) break;
                }
            }

            if (bestLen < 3) return false;

            FlushLiterals(literals, output);

            if (bestLen >= 5 && bestDist <= 8194)
            {
                int len = Math.Min(bestLen, 260);
                EmitCopyLong(output, bestDist, len);
                newPos = pos + len;
                return true;
            }
            if (bestLen >= 4 && bestDist <= 1026)
            {
                int len = Math.Min(bestLen, 11);
                EmitCopyShort(output, bestDist, len);
                newPos = pos + len;
                return true;
            }
            if (bestLen >= 3 && bestDist <= 66)
            {
                EmitCopy3(output, bestDist);
                newPos = pos + 3;
                return true;
            }

            return false;
        }

        private static void EmitCopy3(List<byte> output, int distance)
        {
            int off = distance - 3;
            byte ctrl = (byte)(0x80 | off);
            output.Add(ctrl);
        }

        private static void EmitCopyShort(List<byte> output, int distance, int length)
        {
            int off = distance - 3;
            int offHi = (off >> 8) & 0x03;
            int offLo = off & 0xFF;
            byte ctrl = (byte)(0xC0 | ((length - 4) << 2) | offHi);
            output.Add(ctrl);
            output.Add((byte)offLo);
        }

        private static void EmitCopyLong(List<byte> output, int distance, int length)
        {
            int off = distance - 3;
            int offHi = (off >> 8) & 0x1F;
            int offLo = off & 0xFF;
            byte ctrl = (byte)(0xE0 | offHi);
            byte lenByte = (byte)(length - 5);
            output.Add(ctrl);
            output.Add((byte)offLo);
            output.Add(lenByte);
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