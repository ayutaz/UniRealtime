using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace UniRealtime.WebSocket
{
    /// <summary>
    /// WebSocketのラッパークラス
    /// </summary>
    public class WebSocketWrapper : IWebSocket
    {
        /// <summary>
        /// WebSocketクライアント
        /// </summary>
        private readonly ClientWebSocket _client;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WebSocketWrapper()
        {
            _client = new ClientWebSocket();
        }

        /// <summary>
        /// WebSocketの状態を取得します。
        /// </summary>
        public WebSocketState State => _client.State;

        /// <summary>
        /// WebSocketのオプションを取得
        /// </summary>
        public ClientWebSocketOptions Options => _client.Options;

        /// <summary>
        /// WebSocketに接続します。
        /// </summary>
        public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            await _client.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// データを送信します。（ReadOnlyMemoryバージョン）
        /// </summary>
        public async Task SendAsync(ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage,
            CancellationToken cancellationToken)
        {
            await _client.SendAsync(buffer, messageType, endOfMessage, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// データを送信します。（ArraySegmentバージョン）
        /// </summary>
        public async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            await _client.SendAsync(buffer, messageType, endOfMessage, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// データを受信します。
        /// </summary>
        public async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken, int waitTime = 50)
        {
            return await _client.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// WebSocketを閉じます。
        /// </summary>
        public async Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            await _client.CloseAsync(closeStatus, statusDescription, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// リソースを解放します。
        /// </summary>
        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
