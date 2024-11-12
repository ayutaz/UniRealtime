using System;
using UnityEngine;

namespace UniRealtime
{
    public class AudioUtility
    {
        /// <summary>
        /// float配列をPCM16に変換
        /// </summary>
        /// <param name="floatData"></param>
        /// <returns></returns>
        public static byte[] FloatToPCM16(float[] floatData)
        {
            int length = floatData.Length;
            byte[] bytesData = new byte[length * sizeof(short)];

            for (int i = 0; i < length; i++)
            {
                float sample = floatData[i];
                if (sample < -1.0f) sample = -1.0f;
                if (sample > 1.0f) sample = 1.0f;

                short value = (short)(sample * short.MaxValue);
                bytesData[i * 2] = (byte)(value & 0x00ff);
                bytesData[i * 2 + 1] = (byte)((value & 0xff00) >> 8);
            }

            return bytesData;
        }

        /// <summary>
        /// PCM16をfloat配列に変換
        /// </summary>
        /// <param name="pcmData"></param>
        /// <returns></returns>
        public static float[] PCM16ToFloat(byte[] pcmData)
        {
            int length = pcmData.Length / 2;
            float[] floatData = new float[length];

            for (int i = 0; i < length; i++)
            {
                short value = BitConverter.ToInt16(pcmData, i * 2);
                floatData[i] = value / (float)short.MaxValue;
            }

            return floatData;
        }

        /// <summary>
        /// オーディオデータのリサンプリング
        /// </summary>
        /// <param name="inputSamples"></param>
        /// <param name="resampleRatio"></param>
        /// <returns></returns>
        public static float[] ResampleAudio(float[] inputSamples, float resampleRatio)
        {
            int inputLength = inputSamples.Length;
            int outputLength = Mathf.CeilToInt(inputLength * resampleRatio);
            float[] outputSamples = new float[outputLength];

            for (int i = 0; i < outputLength; i++)
            {
                float srcIndex = i / resampleRatio;
                int srcIndexInt = (int)srcIndex;
                float frac = srcIndex - srcIndexInt;

                if (srcIndexInt + 1 < inputLength)
                {
                    // 線形補間
                    float sample = Mathf.Lerp(inputSamples[srcIndexInt], inputSamples[srcIndexInt + 1], frac);
                    outputSamples[i] = sample;
                }
                else
                {
                    outputSamples[i] = inputSamples[inputLength - 1];
                }
            }

            return outputSamples;
        }
    }
}
