using UnityEngine;

namespace UniRealtime
{
    public static class AudioExtension
    {
        /// <summary>
        /// 音声データを再生
        /// </summary>
        /// <param name="audioSource"></param>
        /// <param name="audioBytes"></param>
        public static void PlayAudioFromBytes(this AudioSource audioSource,byte[] audioBytes)
        {
            if (audioBytes == null || audioBytes.Length == 0)
                return;

            float[] floatData = AudioUtility.PCM16ToFloat(audioBytes);

            AudioClip clip = AudioClip.Create("Response", floatData.Length, 1, 24000, false);
            clip.SetData(floatData, 0);
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}