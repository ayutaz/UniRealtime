using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UniRealtime
{
    /// <summary>
    ///    Unity のメインスレッドで処理を行うためのクラス
    /// </summary>
    public static class UnityMainThreadContext
    {
        /// <summary>
        ///   Unity のメインスレッドの SynchronizationContext
        /// </summary>
        private static SynchronizationContext _unitySynchronizationContext;

        /// <summary>
        ///   スレッドの初期化時に Unity のメインスレッドの SynchronizationContext を取得して保持
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // メインスレッドの SynchronizationContext を取得して保持します
            _unitySynchronizationContext = SynchronizationContext.Current;
        }

        /// <summary>
        ///  Unity のメインスレッドで処理を行う
        /// </summary>
        /// <param name="action"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void Post(Action action)
        {
            if (_unitySynchronizationContext == null)
            {
                throw new InvalidOperationException("Unity の SynchronizationContext が初期化されていません。");
            }

            _unitySynchronizationContext.Post(_ => action(), null);
        }

        /// <summary>
        /// Unity のメインスレッドで処理を行う
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="checkInterval"></param>
        public async static Task WaitUntilAsync(Func<bool> condition, CancellationToken cancellationToken = default, int checkInterval = 100)
        {
            // checkInterval（ミリ秒）ごとに条件をチェックし続けます
            while (!condition())
            {
                // キャンセルが要求されている場合は例外をスローします
                cancellationToken.ThrowIfCancellationRequested();

                // 次の条件チェックまで非同期的に待機します
                await Task.Delay(checkInterval, cancellationToken);
            }
        }
    }
}
