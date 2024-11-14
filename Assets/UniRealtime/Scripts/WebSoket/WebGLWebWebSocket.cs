using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AOT;

namespace UniRealtime.WebSocket
{
    /// <summary>
    /// WebGL向けWebSocketのラッパークラス
    /// </summary>
    internal class WebGLWebSocket : IWebSocket
    {
        private static int _nextInstanceId = 0;

        /// <summary>
        /// staticなインスタンス管理用ディクショナリ
        /// </summary>
        private static readonly Dictionary<int, WebGLWebSocket> _instances = new();

        /// <summary>
        /// インスタンスID
        /// </summary>
        private readonly int _instanceId;

        /// <summary>
        /// 受信したメッセージを格納するキュー
        /// </summary>
        private readonly ConcurrentQueue<(WebSocketMessageType, byte[])> _receivedMessageQueue = new();

        private readonly CancellationTokenSource _cts = new();

        private bool _disposed = false;

        public WebSocketState State { get; private set; } = WebSocketState.None;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WebGLWebSocket()
        {
            _instanceId = _nextInstanceId++;
            _instances[_instanceId] = this;
        }

        /// <summary>
        /// 接続
        /// </summary>
        public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(WebGLWebSocket));

            State = WebSocketState.Connecting;
            Jslib_InitializeWebSocket(_instanceId, OnOpenFunc, OnMessageFunc, OnErrorFunc, OnCloseFunc);
            Jslib_ConnectWebSocket(_instanceId, uri.ToString());

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);

            try
            {
                while (State == WebSocketState.Connecting)
                {
                    await Task.Delay(100, linkedCts.Token).ConfigureAwait(false);
                }

                if (State != WebSocketState.Open)
                {
                    throw new WebSocketException("Failed to connect to the server.");
                }
            }
            catch (OperationCanceledException)
            {
                await CloseAsync(WebSocketCloseStatus.NormalClosure, "Operation canceled", CancellationToken.None);
                throw;
            }
        }

        /// <summary>
        /// データの送信 (ReadOnlyMemoryバージョン)
        /// </summary>
        public Task SendAsync(ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(WebGLWebSocket));
            if (State != WebSocketState.Open) throw new InvalidOperationException("WebSocket is not open.");

            cancellationToken.ThrowIfCancellationRequested();

            switch (messageType)
            {
                case WebSocketMessageType.Binary:
                    SendBinaryData(buffer);
                    break;
                case WebSocketMessageType.Text:
                    SendTextData(buffer);
                    break;
                case WebSocketMessageType.Close:
                    // Closeフレームの送信はCloseAsyncメソッドで行うため、ここでは何もしないか、例外をスローする
                    throw new InvalidOperationException("Cannot send close frame using SendAsync. Use CloseAsync instead.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), "Invalid message type.");
            }

            // endOfMessageはWebGLでは制御できないため無視します。

            return Task.CompletedTask;
        }

        /// <summary>
        /// データの送信 (ArraySegmentバージョン)
        /// </summary>
        public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            return SendAsync((ReadOnlyMemory<byte>)buffer, messageType, endOfMessage, cancellationToken);
        }

        /// <summary>
        /// バイナリデータの送信
        /// </summary>
        /// <param name="buffer"></param>
        private unsafe void SendBinaryData(ReadOnlyMemory<byte> buffer)
        {
            fixed (byte* data = &MemoryMarshal.GetReference(buffer.Span))
            {
                Jslib_SendWebSocketMessage(_instanceId, data, buffer.Length);
            }
        }

        /// <summary>
        /// テキストデータの送信
        /// </summary>
        /// <param name="buffer"></param>
        private void SendTextData(ReadOnlyMemory<byte> buffer)
        {
            // テキストデータをUTF8としてデコードし、文字列として送信
            string text = Encoding.UTF8.GetString(buffer.Span);
            Jslib_SendWebSocketTextMessage(_instanceId, text);
        }

        /// <summary>
        /// データの受信
        /// </summary>
        public async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken, int waitTime = 50)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(WebGLWebSocket));

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);

            (WebSocketMessageType, byte[]) msg;
            while (!_receivedMessageQueue.TryDequeue(out msg))
            {
                if (State == WebSocketState.Aborted || State == WebSocketState.Closed)
                {
                    return new WebSocketReceiveResult(0, WebSocketMessageType.Close, true);
                }

                await Task.Delay(waitTime, linkedCts.Token).ConfigureAwait(false);
            }

            var messageType = msg.Item1;
            var data = msg.Item2;
            var count = Math.Min(data.Length, buffer.Count);
            Array.Copy(data, 0, buffer.Array, buffer.Offset, count);

            var endOfMessage = data.Length <= buffer.Count;
            return new WebSocketReceiveResult(count, messageType, endOfMessage);
        }

        /// <summary>
        /// 切断
        /// </summary>
        public Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(WebGLWebSocket));

            cancellationToken.ThrowIfCancellationRequested();

            if (State == WebSocketState.Closed || State == WebSocketState.Aborted)
            {
                return Task.CompletedTask;
            }

            State = WebSocketState.CloseSent;
            Jslib_CloseWebSocket(_instanceId, (int)closeStatus, statusDescription);

            return Task.CompletedTask;
        }

        /// <summary>
        /// 破棄
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _cts.Cancel();

            if (State == WebSocketState.Open || State == WebSocketState.Connecting)
            {
                Jslib_CloseWebSocket(_instanceId, 1000, "Disposed");
            }

            State = WebSocketState.Closed;
            _instances.Remove(_instanceId);
            Jslib_DisposeWebSocket(_instanceId);
        }

        [MonoPInvokeCallback(typeof(OnOpen))]
        private static void OnOpenFunc(int instanceId)
        {
            if (_instances.TryGetValue(instanceId, out var instance))
            {
                instance.State = WebSocketState.Open;
            }
        }

        [MonoPInvokeCallback(typeof(OnMessage))]
        private static void OnMessageFunc(int instanceId, int isBinary, IntPtr ptr, int size)
        {
            if (_instances.TryGetValue(instanceId, out var instance))
            {
                try
                {
                    var messageType = isBinary == 1 ? WebSocketMessageType.Binary : WebSocketMessageType.Text;
                    var data = new byte[size];
                    Marshal.Copy(ptr, data, 0, size);
                    instance._receivedMessageQueue.Enqueue((messageType, data));
                }
                catch (Exception ex)
                {
                    // エラーハンドリング
                    UnityEngine.Debug.LogError($"OnMessageFunc Error: {ex}");
                }
            }
        }

        [MonoPInvokeCallback(typeof(OnError))]
        private static void OnErrorFunc(int instanceId)
        {
            if (_instances.TryGetValue(instanceId, out var instance))
            {
                instance.State = WebSocketState.Aborted;
            }
        }

        [MonoPInvokeCallback(typeof(OnClose))]
        private static void OnCloseFunc(int instanceId, int code)
        {
            if (_instances.TryGetValue(instanceId, out var instance))
            {
                instance.State = WebSocketState.Closed;
            }
        }

        private delegate void OnOpen(int instanceId);

        private delegate void OnMessage(int instanceId, int isBinary, IntPtr ptr, int size);

        private delegate void OnError(int instanceId);

        private delegate void OnClose(int instanceId, int code);

        [DllImport("__Internal")]
        private static extern void Jslib_InitializeWebSocket(int instanceId, OnOpen onOpen, OnMessage onMessage, OnError onError, OnClose onClose);

        [DllImport("__Internal")]
        private static extern void Jslib_ConnectWebSocket(int instanceId, string url);

        [DllImport("__Internal")]
        private static extern unsafe void Jslib_SendWebSocketMessage(int instanceId, byte* message, int length);

        [DllImport("__Internal")]
        private static extern void Jslib_SendWebSocketTextMessage(int instanceId, string message);

        [DllImport("__Internal")]
        private static extern void Jslib_CloseWebSocket(int instanceId, int code, string reason);

        [DllImport("__Internal")]
        private static extern void Jslib_DisposeWebSocket(int instanceId);
    }
}
