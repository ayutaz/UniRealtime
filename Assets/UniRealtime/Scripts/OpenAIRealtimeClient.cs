using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MikeSchweitzer.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UniRealtime.OpenAI.Request;
using UniRealtime.Response;
using UnityEngine;
#if UNIREALTIME_SUPPORT_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace UniRealtime
{
    /// <summary>
    /// OpenAI's Realtime API for Client
    /// </summary>
    public class OpenAIRealtimeClient : IDisposable
    {
        /// <summary>
        /// API Key
        /// </summary>
        private readonly string _apiKey;

        /// <summary>
        /// Model Name
        /// </summary>
        private readonly string _modelName;

        /// <summary>
        /// WebSocket Connection Class
        /// </summary>
        private readonly WebSocketConnection _connection;

        /// <summary>
        /// Connection Flag
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// メッセージが受信されたときに発行されるイベント
        /// </summary>
        public event Action<RealtimeResponse> OnMessageReceivedEvent;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="webSocketConnection"></param>
        /// <param name="apiKey"></param>
        /// <param name="modelName"></param>
        public OpenAIRealtimeClient(WebSocketConnection webSocketConnection, string apiKey, string modelName = "gpt-4o-realtime-preview-2024-10-01")
        {
            _connection = webSocketConnection;
            _apiKey = apiKey;
            _modelName = modelName;

            webSocketConnection.MessageReceived += OnMessageReceived;
            webSocketConnection.ErrorMessageReceived += OnErrorMessageReceived;
        }

        /// <summary>
        ///  Realtime APIに接続
        /// </summary>
#if UNIREALTIME_SUPPORT_UNITASK
        public async UniTask ConnectToRealtimeAPI(CancellationToken cancellationToken = default, string instructions = "あなたは優秀はアシスタントです。",
            Modalities[] modalities = null, string headerKey = "OpenAI-Beta",
            string headerValue = "realtime=v1", int maxReceiveMbValue = 1024 * 1024 * 5, int maxSendBytes = 1024 * 1024 * 5)
#else
        public async Task ConnectToRealtimeAPI(CancellationToken cancellationToken = default, string instructions = "あなたは優秀はアシスタントです。",
            Modalities[] modalities = null, string headerKey = "OpenAI-Beta",
            string headerValue = "realtime=v1", int maxReceiveMbValue = 1024 * 1024 * 5, int maxSendBytes = 1024 * 1024 * 5)
#endif
        {
            string url = $"wss://api.openai.com/v1/realtime?model={_modelName}";

            // ヘッダーの設定
            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {_apiKey}" },
                { headerKey, headerValue }
            };

            // 接続設定の作成
            _connection.DesiredConfig = new WebSocketConfig
            {
                Url = url,
                Headers = headers,
                MaxReceiveBytes = maxReceiveMbValue,
                MaxSendBytes = maxSendBytes
            };

            _connection.Connect();

            // 接続が確立されるまで待機
#if UNIREALTIME_SUPPORT_UNITASK
            await UniTask.WaitUntil(() => _connection.State == WebSocketState.Connected, cancellationToken: cancellationToken);
#else
            await UnityMainThreadContext.WaitUntilAsync(() => _connection.State == WebSocketState.Connected, cancellationToken);
#endif
            Debug.Log("Connected to Realtime API");

            // 接続フラグを設定
            IsConnected = true;

            // デフォルトのモダリティを設定（必要に応じて調整）
            if (modalities == null)
            {
                modalities = new Modalities[] { Modalities.Text };
            }

            // response.create メッセージを送信
            SendResponseCreate(instructions, modalities);
        }

        /// <summary>
        /// 初期の response.create メッセージを送信
        /// </summary>
        /// <param name="instructions">アシスタントへの指示</param>
        /// <param name="modalities">使用するモダリティ（例：Modalities.Text, Modalities.Audio）</param>
        public void SendResponseCreate(string instructions, Modalities[] modalities)
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
            _connection.AddOutgoingMessage(jsonMessage);

            Debug.Log("Sent response.create message with instructions.");
        }

        /// <summary>
        ///     セッションの更新を送信
        /// </summary>
        public void SendSessionUpdate(SessionUpdateMessage sessionUpdateMessage)
        {
            string jsonMessage = JsonConvert.SerializeObject(sessionUpdateMessage);
            _connection.AddOutgoingMessage(jsonMessage);

            Debug.Log("Session update message sent with updated settings.");
        }

        /// <summary>
        /// Send Audio Data to Realtime API
        /// </summary>
        /// <param name="audioData"></param>
        public void SendAudioData(float[] audioData)
        {
            if (_connection.State != WebSocketState.Connected)
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
            _connection.AddOutgoingMessage(jsonMessage);
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
        /// <param name="connection"></param>
        /// <param name="message"></param>
        private void OnMessageReceived(WebSocketConnection connection, WebSocketMessage message)
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
        /// <param name="message"></param>
        private void ProcessMessage(WebSocketMessage message)
        {
            // メッセージの解析と RealtimeResponse オブジェクトの作成
            RealtimeResponse response = ParseMessage(message);

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
        /// <param name="message"></param>
        /// <returns></returns>
        private RealtimeResponse ParseMessage(WebSocketMessage message)
        {
            try
            {
                JObject json = JObject.Parse(message.String);
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
        /// <param name="connection"></param>
        /// <param name="errorMessage"></param>
        private void OnErrorMessageReceived(WebSocketConnection connection, string errorMessage)
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
            _connection.Disconnect();
        }
    }
}
