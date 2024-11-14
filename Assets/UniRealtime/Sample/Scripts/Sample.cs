using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using MikeSchweitzer.WebSocket;
using TMPro;
using UnityEngine;

namespace UniRealtime.Sample
{
    /// <summary>
    /// サンプルクラス
    /// </summary>
    public class Sample : MonoBehaviour
    {
        [SerializeField] private string _apiKey;

        /// <summary>
        /// レスポンスを表示するTextMeshProUGUI
        /// </summary>
        [SerializeField] private TextMeshProUGUI responseText;

        /// <summary>
        /// 入力した音声を表示するTextMeshProUGUI
        /// </summary>
        [SerializeField] private TextMeshProUGUI inputText;

        /// <summary>
        /// 音声を再生するAudioSource
        /// </summary>
        [SerializeField] private AudioSource audioSource;

        /// <summary>
        /// OpenAI Realtimeに関するクラス
        /// </summary>
        private OpenAIRealtimeClient _openAIRealtimeClient;

        /// <summary>
        /// マイクから取得した最後のサンプル位置
        /// </summary>
        private int _lastSamplePosition = 0;

        /// <summary>
        /// 音声データのバッファを float 型のスレッドセーフなキューに変更
        /// </summary>
        private readonly ConcurrentQueue<float> _audioBuffer = new ConcurrentQueue<float>();

        /// <summary>
        /// 音声の入力に関するクラス
        /// </summary>
        private AudioRecorder _audioRecorder;

        /// <summary>
        /// CancellationTokenSource
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// WebSocketConnection
        /// </summary>
        [SerializeField] private WebSocketConnection webSocketConnection;

        private void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _openAIRealtimeClient = new OpenAIRealtimeClient(webSocketConnection, _apiKey);
            _audioRecorder = new AudioRecorder();
            _openAIRealtimeClient.OnMessageReceivedEvent += HandleMessageReceived;
        }


        /// <summary>
        /// 開始処理
        /// </summary>
        private async void Start()
        {
            // マイクの設定
            audioSource.loop = true;
            audioSource.Play();

            // Realtime APIに接続
            await _openAIRealtimeClient.ConnectToRealtimeAPI(_cancellationTokenSource.Token);

            // 入力した音声の文字起こし情報も取得する場合
            _openAIRealtimeClient.SendSessionUpdate();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update()
        {
            // 接続が確立されるまで音声データの送信を停止
            if (!_openAIRealtimeClient.IsConnected)
            {
                return;
            }

#if UNITY_WEBGL
            // WebGLのマイク入力の実装
#else
            // マイクから音声データを取得して送信
            if (Microphone.IsRecording(_audioRecorder.Microphone))
            {
                int currentPosition = Microphone.GetPosition(_audioRecorder.Microphone);

                if (currentPosition < _lastSamplePosition)
                {
                    // ループした場合
                    _lastSamplePosition = 0;
                }

                int sampleLength = currentPosition - _lastSamplePosition;

                if (sampleLength > 0)
                {
                    float[] samples = new float[sampleLength];
                    _audioRecorder.AudioClip.GetData(samples, _lastSamplePosition);

                    // 更新
                    _lastSamplePosition = currentPosition;

                    // 音声データを送信
                    _openAIRealtimeClient.SendAudioData(samples);
                }
            }
#endif
        }

        /// <summary>
        /// 音声データを取得するためのメソッド
        /// </summary>
        /// <param name="data"></param>
        /// <param name="channels"></param>
        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (_audioBuffer == null) return;

            for (int i = 0; i < data.Length; i += channels)
            {
                float sample;
                if (_audioBuffer.TryDequeue(out sample))
                {
                    data[i] = sample;

                    // ステレオ対応
                    if (channels == 2)
                    {
                        data[i + 1] = sample;
                    }
                }
                else
                {
                    // バッファが空の場合は無音にする
                    data[i] = 0;

                    if (channels == 2)
                    {
                        data[i + 1] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// メッセージを受信したときに呼び出されるメソッド
        /// </summary>
        /// <param name="response"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void HandleMessageReceived(RealtimeResponse response)
        {
            switch (response.ResponseType)
            {
                case ResponseType.SessionCreated: break;
                case ResponseType.SessionUpdated: break;
                case ResponseType.ResponseCreated: break;
                case ResponseType.RateLimitsUpdated: break;
                case ResponseType.ConversationItemCreated: break;
                case ResponseType.ResponseOutputItemAdded: break;
                case ResponseType.ResponseOutputItemDone: break;
                case ResponseType.ResponseTextDelta: break;
                case ResponseType.ResponseTextDone: break;
                case ResponseType.ResponseAudioTranscriptDelta:
                    if (!string.IsNullOrEmpty(response.Text))
                    {
                        responseText.text += response.Text;
                    }
                    break;
                case ResponseType.ResponseAudioTranscriptDone:
                    if (!string.IsNullOrEmpty(response.Text))
                    {
                        responseText.text = response.Text;
                    }
                    break;
                case ResponseType.ResponseAudioDelta:
                    // 受信した音声データ（PCM16 データ）をデコードしてバッファに追加
                    byte[] pcmData = response.AudioData;

                    // 入力サンプル数を計算
                    int inputSampleCount = pcmData.Length / 2;
                    float[] inputSamples = new float[inputSampleCount];

                    // バイト配列をfloat配列に変換（正規化）
                    for (int i = 0; i < inputSampleCount; i++)
                    {
                        // リトルエンディアンの場合
                        short sample = BitConverter.ToInt16(pcmData, i * 2);

                        // short の最大値で割って -1.0f ～ 1.0f に正規化
                        inputSamples[i] = sample / (float)short.MaxValue;
                    }

                    // Unity のサンプリングレートを取得
                    int unitySampleRate = AudioSettings.outputSampleRate;

                    // 入力データのサンプリングレートに合わせてリサンプリング
                    // DOCS: https://platform.openai.com/docs/guides/realtime#audio-formats
                    // raw 16 bit PCM audio at 24kHz, 1 channel, little-endian
                    int inputSampleRate = 24000;

                    // リサンプリングの比率を計算
                    float resampleRatio = (float)unitySampleRate / inputSampleRate;

                    // リサンプリングを行う
                    float[] resampledSamples = AudioUtility.ResampleAudio(inputSamples, resampleRatio);

                    // バッファに追加
                    foreach (var sample in resampledSamples)
                    {
                        _audioBuffer.Enqueue(sample);
                    }
                    break;
                case ResponseType.ResponseAudioDone:
                    // 何もしない
                    break;
                case ResponseType.ResponseDone: break;
                case ResponseType.InputAudioBufferSpeechStarted: break;
                case ResponseType.InputAudioBufferSpeechStopped:
                    responseText.text = string.Empty;
                    break;
                case ResponseType.InputAudioBufferCommitted: break;
                case ResponseType.Error: break;
                case ResponseType.Unknown: break;
                case ResponseType.InputAudioTranscriptPartial:
                    if (!string.IsNullOrEmpty(response.Text))
                    {
                        inputText.text += response.Text;
                    }
                    break;
                case ResponseType.InputAudioTranscriptDone:
                    if (!string.IsNullOrEmpty(response.Text))
                    {
                        inputText.text = response.Text;
                    }
                    break;
                case ResponseType.ConversationItemInputAudioTranscriptionCompleted:
                    // ユーザーの音声入力の転写が完了した際の処理
                    if (!string.IsNullOrEmpty(response.Text))
                    {
                        inputText.text = response.Text;
                    }
                    break;
                case ResponseType.ResponseContentPartAdded:
                case ResponseType.ResponseContentPartDone:
                    break;
                default:
                    Debug.LogError("Unknown ResponseType: " + response.ResponseType);
                    break;
            }
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        private void OnDestroy()
        {
            // イベントの解除とクライアントの破棄
            _openAIRealtimeClient.OnMessageReceivedEvent -= HandleMessageReceived;
            _openAIRealtimeClient.Dispose();

            // cancellationTokenSource を破棄
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}
