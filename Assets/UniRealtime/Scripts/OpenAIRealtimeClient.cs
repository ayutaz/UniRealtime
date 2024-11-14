using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Net.WebSockets;

#if UNIREALTIME_SUPPORT_UNITASK
using Cysharp.Threading.Tasks;

#else
using System.Threading.Tasks;
#endif

namespace UniRealtime
{
    /// <summary>
    /// OpenAI's Realtime API for Client
    /// </summary>
    public class OpenAIRealtimeClient : IDisposable
    {
        /// <summary>
        /// APIキー
        /// </summary>
        private readonly string _apiKey;

        /// <summary>
        /// モデル名
        /// </summary>
        private readonly string _modelName;

        /// <summary>
        /// WebSocket接続クラス
        /// </summary>
        private readonly WebSocketConnection _connection;

        /// <summary>
        /// 接続フラグ
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// メッセージ受信時に発行されるイベント
        /// </summary>
        public event Action<RealtimeResponse> OnMessageReceivedEvent;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="modelName"></param>
        public OpenAIRealtimeClient(string apiKey, string modelName = "gpt-4o-realtime-preview-2024-10-01")
        {
            _apiKey = apiKey;
            _modelName = modelName;

            _connection = new WebSocketConnection();
            _connection.MessageReceived += OnMessageReceived;
            _connection.ErrorMessageReceived += OnErrorMessageReceived;
        }

        /// <summary>
        /// Realtime APIに接続
        /// </summary>
#if UNIREALTIME_SUPPORT_UNITASK
        public async UniTask ConnectToRealtimeAPI(CancellationToken cancellationToken = default, string instructions = "あなたは優秀はアシスタントです。",
            Modalities[] modalities = null, string headerKey = "OpenAI-Beta",
            string headerValue = "realtime=v1")
#else
        public async Task ConnectToRealtimeAPI(CancellationToken cancellationToken = default, string instructions = "あなたは優秀はアシスタントです。",
            Modalities[] modalities = null, string headerKey = "OpenAI-Beta",
            string headerValue = "realtime=v1")
#endif
        {
            string url = $"wss://api.openai.com/v1/realtime?model={_modelName}";

            // ヘッダーの設定
            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {_apiKey}" },
                { headerKey, headerValue }
            };

            _connection.Headers = headers;

            // 接続
            await _connection.ConnectAsync(url, cancellationToken);

            // 接続が確立されるまで待機
#if UNIREALTIME_SUPPORT_UNITASK
            await UniTask.WaitUntil(() => _connection.State == WebSocketState.Open, cancellationToken: cancellationToken);
#else
            await Task.Run(async () =>
            {
                while (_connection.State == WebSocketState.Connecting && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(100, cancellationToken);
                }
            }, cancellationToken);
#endif
            if (_connection.State != WebSocketState.Open)
            {
                throw new Exception("Failed to connect to Realtime API");
            }

            Debug.Log("Connected to Realtime API");

            // 接続フラグを設定
            IsConnected = true;

            // デフォルトのモダリティを設定（必要に応じて調整）
            if (modalities == null)
            {
                modalities = new Modalities[] { Modalities.Text };
            }

            // response.create メッセージを送信
            await SendResponseCreate(instructions, modalities);
        }

        /// <summary>
        /// 初期の response.create メッセージを送信
        /// </summary>
        /// <param name="instructions">アシスタントへの指示</param>
        /// <param name="modalities">使用するモダリティ（例：Modalities.Text, Modalities.Audio）</param>
        public async Task SendResponseCreate(string instructions, Modalities[] modalities)
        {
            // Modalities enum の値を文字列に変換
            var modalitiesStrings = modalities.Select(m => m.ToString().ToLower()).ToArray();

            var responseCreateMessage = new
            {
                type = "response.create",
                response = new
                {
                    modalities = modalitiesStrings,
                    instructions = instructions
                }
            };

            string jsonMessage = JsonConvert.SerializeObject(responseCreateMessage);
            await _connection.SendAsync(jsonMessage);

            Debug.Log("Sent response.create message with instructions.");
        }

        /// <summary>
        /// セッションの更新を送信
        /// </summary>
        public async Task SendSessionUpdate(string modelName = "whisper-1")
        {
            var sessionUpdateMessage = new
            {
                type = "session.update",
                session = new
                {
                    input_audio_transcription = new
                    {
                        model = modelName
                    }
                }
            };

            string jsonMessage = JsonConvert.SerializeObject(sessionUpdateMessage);
            await _connection.SendAsync(jsonMessage);

            Debug.Log("Session update message sent with input_audio_transcription settings.");
        }

        /// <summary>
        /// Realtime APIに音声データを送信
        /// </summary>
        /// <param name="audioData"></param>
        public async Task SendAudioData(float[] audioData)
        {
            if (_connection.State != WebSocketState.Open)
            {
                // 接続が確立されていない場合は送信しない
                return;
            }

            byte[] pcmData = AudioUtility.FloatToPCM16(audioData);
            string base64Audio = Convert.ToBase64String(pcmData);

            var eventMessage = new
            {
                type = "input_audio_buffer.append",
                audio = base64Audio
            };

            string jsonMessage = JsonConvert.SerializeObject(eventMessage);
            await _connection.SendAsync(jsonMessage);
        }

        /// <summary>
        /// Parse Response Type
        /// </summary>
        /// <param name="typeString"></param>
        /// <returns></returns>
        private ResponseType ParseResponseType(string typeString)
        {
            return typeString switch
            {
                "session.created" => ResponseType.SessionCreated,
                "response.created" => ResponseType.ResponseCreated,
                "session.updated" => ResponseType.SessionUpdated,
                "rate_limits.updated" => ResponseType.RateLimitsUpdated,
                "conversation.item.created" => ResponseType.ConversationItemCreated,
                "response.output_item.added" => ResponseType.ResponseOutputItemAdded,
                "response.output_item.done" => ResponseType.ResponseOutputItemDone,
                "response.text.delta" => ResponseType.ResponseTextDelta,
                "response.text.done" => ResponseType.ResponseTextDone,
                "response.content_part.added" => ResponseType.ResponseContentPartAdded,
                "response.content_part.done" => ResponseType.ResponseContentPartDone,
                "response.audio_transcript.delta" => ResponseType.ResponseAudioTranscriptDelta,
                "response.audio_transcript.done" => ResponseType.ResponseAudioTranscriptDone,
                "response.audio.delta" => ResponseType.ResponseAudioDelta,
                "response.audio.done" => ResponseType.ResponseAudioDone,
                "response.done" => ResponseType.ResponseDone,
                "input_audio_buffer.speech_started" => ResponseType.InputAudioBufferSpeechStarted,
                "input_audio_buffer.speech_stopped" => ResponseType.InputAudioBufferSpeechStopped,
                "input_audio_buffer.committed" => ResponseType.InputAudioBufferCommitted,
                "input_audio_transcript.partial" => ResponseType.InputAudioTranscriptPartial,
                "input_audio_transcript.done" => ResponseType.InputAudioTranscriptDone,
                "conversation.item.input_audio_transcription.completed" => ResponseType.ConversationItemInputAudioTranscriptionCompleted,
                "error" => ResponseType.Error,
                _ => ResponseType.Unknown
            };
        }


        /// <summary>
        /// メッセージを受信
        /// </summary>
        /// <param name="message"></param>
        private void OnMessageReceived(string message)
        {
            // 非メインスレッドから呼び出される可能性があるため、メインスレッドで処理を行う
#if UNIREALTIME_SUPPORT_UNITASK
            UniTask.Post(() => ProcessMessage(message));
#else
            UnityMainThreadContext.Post(() => ProcessMessage(message));
#endif
        }

        /// <summary>
        /// メッセージを処理
        /// </summary>
        private void ProcessMessage(string messageString)
        {
            // メッセージの解析とRealtimeResponseオブジェクトの作成
            RealtimeResponse response = ParseMessage(messageString);

            if (response == null)
            {
                Debug.LogWarning("Failed to parse message.");
                return;
            }

            // イベントを発行
            OnMessageReceivedEvent?.Invoke(response);
        }

        /// <summary>
        /// メッセージを解析
        /// </summary>
        private RealtimeResponse ParseMessage(string messageString)
        {
            try
            {
                JObject json = JObject.Parse(messageString);
                string typeString = (string)json["type"];
                ResponseType responseType = ParseResponseType(typeString);

                RealtimeResponse response = new RealtimeResponse
                {
                    ResponseType = responseType,
                    Type = typeString,
                    EventId = (string)json["event_id"],
                };

                // レスポンスタイプに応じてプロパティを設定
                switch (responseType)
                {
                    case ResponseType.SessionCreated:
                    case ResponseType.SessionUpdated:
                    case ResponseType.InputAudioBufferSpeechStarted:
                    case ResponseType.InputAudioBufferSpeechStopped:
                    case ResponseType.InputAudioBufferCommitted:
                    case ResponseType.ConversationItemCreated:
                    case ResponseType.ResponseCreated:
                    case ResponseType.RateLimitsUpdated:
                    case ResponseType.ResponseOutputItemAdded:
                    case ResponseType.ResponseOutputItemDone:
                    case ResponseType.ResponseContentPartDone:
                    case ResponseType.ResponseDone:
                        break;

                    case ResponseType.ResponseTextDelta:
                    case ResponseType.ResponseTextDone:
                        response.Text = (string)json["text"] ?? (string)json["delta"];
                        break;

                    case ResponseType.ResponseAudioTranscriptDelta:
                    case ResponseType.ResponseAudioTranscriptDone:
                        response.Text = (string)json["delta"] ?? (string)json["text"];
                        break;

                    case ResponseType.InputAudioTranscriptPartial:
                    case ResponseType.InputAudioTranscriptDone:
                        response.Text = (string)json["delta"] ?? (string)json["text"];
                        break;

                    case ResponseType.ConversationItemInputAudioTranscriptionCompleted:
                        response.Text = (string)json["transcript"];
                        response.ItemId = (string)json["item_id"];
                        response.ContentIndex = (int?)json["content_index"] ?? 0;
                        response.Transcript = (string)json["transcript"];
                        break;

                    case ResponseType.ResponseContentPartAdded:
                        // `part` オブジェクトを取得
                        JObject part = (JObject)json["part"];
                        if (part != null)
                        {
                            string partType = (string)part["type"];
                            response.Type = partType;

                            if (partType == "text")
                            {
                                response.Text = (string)part["text"];
                            }
                            else if (partType == "audio")
                            {
                                response.Text = (string)part["transcript"];
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Part object is missing in response.content_part.added message.");
                        }
                        break;

                    case ResponseType.ResponseAudioDelta:
                    case ResponseType.ResponseAudioDone:
                        string audioBase64 = (string)json["delta"] ?? (string)json["audio"];
                        if (!string.IsNullOrEmpty(audioBase64))
                        {
                            response.AudioData = Convert.FromBase64String(audioBase64);
                        }
                        break;

                    case ResponseType.Error:
                        response.Text = (string)json["error"]?["message"];
                        Debug.LogError("Error: " + response.Text);
                        break;

                    case ResponseType.Unknown:
                    default:
                        Debug.LogWarning("Unhandled message type: " + typeString);
                        break;
                }

                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing message: {ex.Message}");
                OnMessageReceivedEvent?.Invoke(new RealtimeResponse
                {
                    ResponseType = ResponseType.Error,
                    Text = $"JSON parsing error: {ex.Message}"
                });
                return null;
            }
        }

        /// <summary>
        /// エラーメッセージを受信
        /// </summary>
        /// <param name="errorMessage"></param>
        private void OnErrorMessageReceived(string errorMessage)
        {
            // エラーメッセージをメインスレッドでログ出力
#if UNIREALTIME_SUPPORT_UNITASK
            UniTask.Post(() => Debug.LogError($"WebSocket Error: {errorMessage}"));
#else
            UnityMainThreadContext.Post(() => Debug.LogError($"WebSocket Error: {errorMessage}"));
#endif
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (_connection == null) return;

            _connection.MessageReceived -= OnMessageReceived;
            _connection.ErrorMessageReceived -= OnErrorMessageReceived;
            _connection.Dispose();
        }
    }
}
