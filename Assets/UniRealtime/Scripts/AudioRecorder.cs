using UnityEngine;

namespace UniRealtime
{
    /// <summary>
    /// Audio Recorder
    /// </summary>
    public class AudioRecorder
    {
#if !UNITY_WEBGL
        /// <summary>
        /// Use Microphone name
        /// </summary>
        public string Microphone { get; private set; }

        /// <summary>
        ///　AudioClip to store the audio data obtained from the microphone
        /// </summary>
        public AudioClip AudioClip { get; private set; }

        /// <summary>
        /// The sample rate of the AudioClip produced by the recording
        /// </summary>
        private const int SampleRate = 24000;

        /// <summary>
        /// Is the length of the AudioClip produced by the recording.
        /// </summary>
        private const int ClipLength = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        public AudioRecorder(string microphoneDeviceName = "")
        {
            if (string.IsNullOrEmpty(microphoneDeviceName))
            {
                // マイクデバイス名が指定されていない場合、プラットフォームごとに設定
#if UNITY_ANDROID
                microphoneDeviceName = "Android audio input";
#else
                if (UnityEngine.Microphone.devices.Length > 0)
                {
                    Microphone = UnityEngine.Microphone.devices[0];
                }
                else
                {
                    Debug.LogError("マイクが接続されていません");
                }
#endif
                AudioClip = UnityEngine.Microphone.Start(Microphone, true, ClipLength, SampleRate);
            }
            else
            {
                Microphone = microphoneDeviceName;
                AudioClip = UnityEngine.Microphone.Start(Microphone, true, ClipLength, SampleRate);
            }
        }


        /// <summary>
        /// Start recording
        /// </summary>
        public void StartRecording()
        {
            UnityEngine.Microphone.Start(Microphone, true, ClipLength, SampleRate);
        }

        /// <summary>
        /// Stop recording
        /// </summary>
        public void StopRecording()
        {
            UnityEngine.Microphone.End(Microphone);
        }
#endif
    }
}
