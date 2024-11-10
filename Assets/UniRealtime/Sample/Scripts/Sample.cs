using System;
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
        /// 音声データのバッファ
        /// </summary>
        private readonly List<byte> _audioBuffer = new List<byte>();

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
                    _audioBuffer.AddRange(response.AudioData);
                    break;
                case ResponseType.ResponseAudioDone:
                    // 音声データを再生
                    audioSource.PlayAudioFromBytes(_audioBuffer.ToArray());
                    // バッファをクリア
                    _audioBuffer.Clear();
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
