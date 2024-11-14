using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using UniRealtime.WebSocket;

namespace UniRealtime
{
    /// <summary>
    /// WebSocket接続を管理し、プラットフォームに応じて適切なIWebSocket実装を使用します。
    /// </summary>
    public class WebSocketConnection : IDisposable
    {
        private IWebSocket _webSocket;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _receiveTask;

        public event Action<string> MessageReceived;
        public event Action<string> ErrorMessageReceived;

        public WebSocketState State => _webSocket?.State ?? WebSocketState.None;

        // ヘッダーなどの設定（ClientWebSocketでは使用できないため、代替手段を検討）
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WebSocketConnection()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 接続
        /// </summary>
        public async Task ConnectAsync(string url, CancellationToken cancellationToken = default)
        {
            if (_webSocket != null)
            {
                throw new InvalidOperationException("WebSocket is already connected or connecting.");
            }

            // プラットフォームに応じて適切なIWebSocket実装をインスタンス化
#if UNITY_WEBGL && !UNITY_EDITOR
            _webSocket = new WebGLWebSocket();
#else
            _webSocket = new WebSocketWrapper();

            // ClientWebSocketではヘッダーの設定が直接できないため、認証情報をURLに含めるなどの方法を検討
#endif
            try
            {
                await _webSocket.ConnectAsync(new Uri(url), cancellationToken).ConfigureAwait(false);

                // 受信ループを開始
                _receiveTask = StartReceiveLoop(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                ErrorMessageReceived?.Invoke(ex.Message);
            }
        }

        /// <summary>
        /// 受信ループを開始
        /// </summary>
        private async Task StartReceiveLoop(CancellationToken cancellationToken)
        {
            var buffer = new byte[8192];

            // メッセージのフラグメントを蓄積するためのストリーム
            using (var messageStream = new System.IO.MemoryStream())
            {
                try
                {
                    while (_webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                    {
                        var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken).ConfigureAwait(false);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", cancellationToken).ConfigureAwait(false);
                            break;
                        }
                        else if (result.MessageType == WebSocketMessageType.Text)
                        {
                            // 受信したデータをメッセージストリームに書き込み
                            messageStream.Write(buffer, 0, result.Count);

                            // メッセージが終了した場合
                            if (result.EndOfMessage)
                            {
                                // メッセージを文字列に変換
                                messageStream.Seek(0, System.IO.SeekOrigin.Begin);
                                using (var reader = new System.IO.StreamReader(messageStream, System.Text.Encoding.UTF8))
                                {
                                    var message = await reader.ReadToEndAsync().ConfigureAwait(false);

                                    // メッセージをイベントで通知
                                    MessageReceived?.Invoke(message);
                                }

                                // メッセージストリームをリセット
                                messageStream.SetLength(0);
                                messageStream.Position = 0;
                            }
                        }
                        else if (result.MessageType == WebSocketMessageType.Binary)
                        {
                            // バイナリメッセージの処理
                            UnityEngine.Debug.LogWarning("Received binary message, but binary messages are not handled.");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // キャンセルされた場合の処理
                }
                catch (Exception ex)
                {
                    ErrorMessageReceived?.Invoke(ex.Message);
                }
            }
        }

        /// <summary>
        /// メッセージを送信
        /// </summary>
        public async Task SendAsync(string message, CancellationToken cancellationToken = default)
        {
            if (_webSocket?.State != WebSocketState.Open)
            {
                throw new InvalidOperationException("WebSocket is not open.");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 切断
        /// </summary>
        public async Task DisconnectAsync()
        {
            _cancellationTokenSource.Cancel();

            if (_receiveTask != null)
            {
                try
                {
                    await _receiveTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // キャンセルされた場合の処理
                }
                catch (Exception ex)
                {
                    ErrorMessageReceived?.Invoke(ex.Message);
                }
            }

            if (_webSocket != null && (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.Connecting))
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected", CancellationToken.None).ConfigureAwait(false);
            }

            _webSocket?.Dispose();
            _webSocket = null;
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// リソースの解放
        /// </summary>
        public void Dispose()
        {
            DisconnectAsync().GetAwaiter().GetResult();
        }
    }
}
