using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace UniRealtime.WebSocket
{
    /// <summary>
    /// WebSocketのインターフェース
    /// </summary>
    internal interface IWebSocket : IDisposable
    {
        /// <summary>
        /// WebSocketの接続
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        Task ConnectAsync(Uri uri, CancellationToken cancellationToken);

        /// <summary>
        /// データの送信
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="messageType"></param>
        /// <param name="endOfMessage"></param>
        /// <param name="cancellationToken"></param>
        Task SendAsync(ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);

        /// <summary>
        /// データの送信
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="messageType"></param>
        /// <param name="endOfMessage"></param>
        /// <param name="cancellationToken"></param>
        Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);

        /// <summary>
        /// データの受信
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="waitTime"></param>
        Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken, int waitTime = 50);

        /// <summary>
        /// WebSocketの切断
        /// </summary>
        /// <param name="closeStatus"></param>
        /// <param name="statusDescription"></param>
        /// <param name="cancellationToken"></param>
        Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken);

        /// <summary>
        /// WebSocketの状態
        /// </summary>
        WebSocketState State { get; }
    }
}
