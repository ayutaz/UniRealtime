var WebSocketLibrary = {
    $instances: {},

    Jslib_InitializeWebSocket: function (instanceId, onOpen, onMessage, onError, onClose) {
        instances[instanceId] = {
            id: instanceId,
            socket: null,
            onOpen: onOpen,
            onMessage: onMessage,
            onError: onError,
            onClose: onClose,
        };
    },

    Jslib_ConnectWebSocket: function (instanceId, url) {
        var instance = instances[instanceId];
        if (!instance) return;

        var urlStr = UTF8ToString(url);
        instance.socket = new WebSocket(urlStr);
        instance.socket.binaryType = "arraybuffer";

        instance.socket.onopen = function () {
            if (instance.onOpen) {
                Module.dynCall_vi(instance.onOpen, instance.id);
            }
        };

        instance.socket.onmessage = function (e) {
            var dataHandler = function (data, isBinary) {
                var buffer = _malloc(data.length);
                HEAPU8.set(data, buffer);
                try {
                    if (instance.onMessage) {
                        Module.dynCall_viiii(instance.onMessage, instance.id, isBinary, buffer, data.length);
                    }
                } finally {
                    _free(buffer);
                }
            };

            if (typeof e.data === "string") {
                // テキストデータ
                var str = e.data;
                var length = lengthBytesUTF8(str) + 1;
                var buffer = _malloc(length);
                stringToUTF8(str, buffer, length);
                try {
                    if (instance.onMessage) {
                        Module.dynCall_viiii(instance.onMessage, instance.id, 0, buffer, length);
                    }
                } finally {
                    _free(buffer);
                }
            } else if (e.data instanceof ArrayBuffer) {
                dataHandler(new Uint8Array(e.data), 1);
            } else if (e.data instanceof Blob) {
                var reader = new FileReader();
                reader.onloadend = function () {
                    var arrayBuffer = reader.result;
                    dataHandler(new Uint8Array(arrayBuffer), 1);
                };
                reader.readAsArrayBuffer(e.data);
            }
        };

        instance.socket.onerror = function () {
            if (instance.onError) {
                Module.dynCall_vi(instance.onError, instance.id);
            }
        };

        instance.socket.onclose = function (e) {
            if (instance.onClose) {
                Module.dynCall_vii(instance.onClose, instance.id, e.code);
            }
        };
    },

    Jslib_SendWebSocketMessage: function (instanceId, ptr, length) {
        var instance = instances[instanceId];
        if (instance && instance.socket) {
            var data = HEAPU8.subarray(ptr, ptr + length);
            instance.socket.send(data);
        }
    },

    Jslib_SendWebSocketTextMessage: function (instanceId, ptr) {
        var instance = instances[instanceId];
        if (instance && instance.socket) {
            var message = UTF8ToString(ptr);
            instance.socket.send(message);
        }
    },

    Jslib_CloseWebSocket: function (instanceId, code, reason) {
        var instance = instances[instanceId];
        if (instance && instance.socket) {
            var reasonStr = UTF8ToString(reason);
            instance.socket.close(code, reasonStr);
        }
    },

    Jslib_DisposeWebSocket: function (instanceId) {
        delete instances[instanceId];
    }
};

autoAddDeps(WebSocketLibrary, '$instances');
mergeInto(LibraryManager.library, WebSocketLibrary);