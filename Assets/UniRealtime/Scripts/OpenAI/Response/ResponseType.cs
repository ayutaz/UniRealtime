namespace UniRealtime.Response
{
    /// <summary>
    /// OpenAI リアルタイム API のレスポンスタイプを表す Enum
    /// </summary>
    public enum ResponseType
    {
        /// <summary>
        /// セッションが作成されたことを示すイベント。新しいセッションが開始された際に送信されます。
        /// </summary>
        SessionCreated,

        /// <summary>
        /// セッションが更新されたことを示すイベント。セッションの状態が変更された際に送信されます。
        /// </summary>
        SessionUpdated,

        /// <summary>
        /// 新しいレスポンスが作成されたことを示すイベント。モデルからの返答が生成され始めた際に送信されます。
        /// </summary>
        ResponseCreated,

        /// <summary>
        /// レート制限情報が更新されたことを示すイベント。使用状況や制限に関する最新情報が提供されます。
        /// </summary>
        RateLimitsUpdated,

        /// <summary>
        /// 会話のアイテムが作成されたことを示すイベント。新しいメッセージやユーザーの入力が会話に追加された際に送信されます。
        /// </summary>
        ConversationItemCreated,

        /// <summary>
        /// レスポンスの出力アイテムが追加されたことを示すイベント。モデルの応答の一部が利用可能になった際に送信されます。
        /// </summary>
        ResponseOutputItemAdded,

        /// <summary>
        /// レスポンスの出力アイテムの送信が完了したことを示すイベント。特定の出力ブロックが完了した際に送信されます。
        /// </summary>
        ResponseOutputItemDone,

        /// <summary>
        /// レスポンスのテキストの増分が追加されたことを示すイベント。テキスト応答の一部がストリーミングで提供されます。
        /// </summary>
        ResponseTextDelta,

        /// <summary>
        /// レスポンスのテキストが完了したことを示すイベント。モデルのテキスト応答が完了した際に送信されます。
        /// </summary>
        ResponseTextDone,

        /// <summary>
        /// レスポンスの音声転写の増分が追加されたことを示すイベント。音声からテキストへの転写結果が部分的に提供されます。
        /// </summary>
        ResponseAudioTranscriptDelta,

        /// <summary>
        /// レスポンスの音声転写が完了したことを示すイベント。音声からテキストへの転写が完了した際に送信されます。
        /// </summary>
        ResponseAudioTranscriptDone,

        /// <summary>
        /// レスポンスの音声データの増分が追加されたことを示すイベント。音声応答の一部がストリーミングで提供されます。
        /// </summary>
        ResponseAudioDelta,

        /// <summary>
        /// レスポンスの音声データが完了したことを示すイベント。モデルの音声応答が完了した際に送信されます。
        /// </summary>
        ResponseAudioDone,

        /// <summary>
        /// レスポンス全体が完了したことを示すイベント。モデルからの応答が全て送信された際に送信されます。
        /// </summary>
        ResponseDone,

        /// <summary>
        /// 入力音声バッファで音声の検出が開始されたことを示すイベント。ユーザーの音声入力が始まったことを示します。
        /// </summary>
        InputAudioBufferSpeechStarted,

        /// <summary>
        /// 入力音声バッファで音声の検出が停止したことを示すイベント。ユーザーの音声入力が終了したことを示します。
        /// </summary>
        InputAudioBufferSpeechStopped,

        /// <summary>
        /// 入力音声バッファがコミットされたことを示すイベント。音声入力が確定し、モデルに送信されたことを示します。
        /// </summary>
        InputAudioBufferCommitted,

        /// <summary>
        /// ユーザーの音声入力の部分的な文字起こし
        /// </summary>
        InputAudioTranscriptPartial,

        /// <summary>
        /// ユーザーの音声入力の最終的な文字起こし
        /// </summary>
        InputAudioTranscriptDone,

        /// <summary>
        /// 入力音声の文字起こしが完了したことを示すイベント。ユーザーの音声入力の転写が完了した際に送信されます。
        /// </summary>
        ConversationItemInputAudioTranscriptionCompleted,

        /// <summary>
        /// レスポンスのコンテンツの一部が追加されたことを示すイベント。レスポンスのコンテンツがストリーミングで提供されます。
        /// </summary>
        ResponseContentPartAdded,

        /// レスポンスのコンテンツの一部が完了したことを示すイベント。レスポンスのコンテンツの一部が完了した際に送信されます。
        /// <summary>
        /// </summary>
        ResponseContentPartDone,

        /// <summary>
        /// エラーが発生したことを示すイベント。詳細なエラーメッセージが含まれます。
        /// </summary>
        Error,

        /// <summary>
        /// 未知のタイプのイベント。新しいタイプのイベントや未対応のイベントの場合に使用されます。
        /// </summary>
        Unknown,
    }
}
