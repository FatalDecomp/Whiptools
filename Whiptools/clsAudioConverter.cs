using System;
using System.IO;
using System.Text;

namespace Whiptools
{
    class WavAudio
    {
        private const int sampleRate = 11025;
        private const short bitDepth = 8;

        public static byte[] ConvertRawToWav(byte[] rawBytes)
        {
            using (var outputStream = new MemoryStream())
            {
                WriteWavHeader(outputStream, rawBytes.Length, sampleRate, bitDepth);
                outputStream.Write(rawBytes, 0, rawBytes.Length);
                return outputStream.ToArray();
            }
        }

        private static void WriteWavHeader(Stream stream, int dataLength, int sampleRate, short bitDepth)
        {
            const int headerSize = 44;

            // RIFF header
            WriteString(stream, "RIFF");
            WriteInt32(stream, headerSize + dataLength - 8);
            WriteString(stream, "WAVE");

            // Format chunk
            WriteString(stream, "fmt ");
            WriteInt32(stream, 16); // Subchunk1Size
            WriteInt16(stream, 1); // AudioFormat (1 for PCM)
            WriteInt16(stream, 1); // NumChannels (1 for mono)
            WriteInt32(stream, sampleRate); // SampleRate
            WriteInt32(stream, sampleRate * (bitDepth / 8)); // ByteRate
            WriteInt16(stream, (short)(bitDepth / 8)); // BlockAlign
            WriteInt16(stream, bitDepth); // BitsPerSample

            // Data chunk
            WriteString(stream, "data");
            WriteInt32(stream, dataLength); // Subchunk2Size
        }

        static void WriteInt16(Stream stream, short value)
        {
            stream.WriteByte((byte)(value & 0xFF));
            stream.WriteByte((byte)((value >> 8) & 0xFF));
        }

        static void WriteInt32(Stream stream, int value)
        {
            stream.WriteByte((byte)(value & 0xFF));
            stream.WriteByte((byte)((value >> 8) & 0xFF));
            stream.WriteByte((byte)((value >> 16) & 0xFF));
            stream.WriteByte((byte)((value >> 24) & 0xFF));
        }

        static void WriteString(Stream stream, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }
    }

    class HMPMIDI
    {
        private const string headerOrig     = "HMIMIDIP";
        private const string headerRevised  = "HMIMIDIP013195";
        private const int headerLength      = 0x020;
        private const int chunkStartOrig    = 0x308;
        private const int chunkStartRevised = 0x388;

        public static byte[] ConvertToRevisedFormat(byte[] inputData)
        {
            // must be original HMP file format
            if (!CheckOriginalFormat(inputData)) throw new FormatException();
            
            int inputLen = inputData.Length;
            var outputData = new byte[inputLen + chunkStartRevised - chunkStartOrig];

            // up to 0x308
            Array.Copy(inputData, 0, outputData, 0, chunkStartOrig);

            // rest of input file but start at 0x388
            Array.Copy(inputData, chunkStartOrig, outputData, chunkStartRevised, inputLen - chunkStartOrig);

            // update header
            var prefixBytes = Encoding.ASCII.GetBytes(headerRevised);
            Array.Copy(prefixBytes, 0, outputData, 0, prefixBytes.Length);

            return outputData;
        }

        private static bool CheckOriginalFormat(byte[] inputData)
        {
            // check length
            if (inputData.Length < chunkStartOrig) return false;

            // check input starts with original padded header
            var paddedBytes = new byte[headerLength];
            var prefixBytes = Encoding.ASCII.GetBytes(headerOrig);
            Array.Copy(prefixBytes, 0, paddedBytes, 0, prefixBytes.Length);
            for (int i = 0; i < headerLength; i++)
                if (inputData[i] != paddedBytes[i]) return false;

            return true;
        }
    }
}