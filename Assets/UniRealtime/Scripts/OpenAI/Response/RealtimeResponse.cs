namespace UniRealtime.Response
{
    /// <summary>
    /// OpenAI リアルタイム API からのレスポンスを表すクラス
    /// </summary>
    public class RealtimeResponse
    {
        /// <summary>
        /// レスポンスのタイプ
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// レスポンスのタイプ
        /// </summary>
        public ResponseType ResponseType;

        /// <summary>
        /// レスポンスのテキスト
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// レスポンスの音声データ
        /// </summary>
        public byte[] AudioData { get; set; }

        /// <summary>
        /// EventId
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        ///  レスポンスのアイテムID
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// レスポンスのコンテンツインデックス
        /// </summary>
        public int ContentIndex { get; set; }

        /// <summary>
        /// レスポンスの音声転写
        /// </summary>
        public string Transcript { get; set; }
    }
}
