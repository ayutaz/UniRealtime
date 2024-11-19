using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UniRealtime.OpenAI.Request
{
    /// <summary>
    ///    セッションの更新リクエスト
    /// </summary>
    public class SessionUpdateMessage
    {
        [JsonProperty("event_id")] public string EventId { get; set; }

        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("session")] public SessionDetails Session { get; set; }
    }

    /// <summary>
    ///   セッションの詳細
    /// </summary>
    public class SessionDetails
    {
        [JsonProperty("modalities")] public List<string> Modalities { get; set; }

        [JsonProperty("instructions")] public string Instructions { get; set; }

        [JsonProperty("voice")] public Voice Voice { get; set; }

        [JsonProperty("input_audio_format")] public AudioFormat InputAudioFormat { get; set; }

        [JsonProperty("output_audio_format")] public AudioFormat OutputAudioFormat { get; set; }

        [JsonProperty("input_audio_transcription")]
        public InputAudioTranscription InputAudioTranscription { get; set; }

        [JsonProperty("turn_detection")] public TurnDetection TurnDetection { get; set; }

        [JsonProperty("tools")] public List<Tool> Tools { get; set; }

        [JsonProperty("tool_choice")] public string ToolChoice { get; set; }

        [JsonProperty("temperature")] public double Temperature { get; set; }

        [JsonProperty("max_response_output_tokens")]
        public string MaxResponseOutputTokens { get; set; }
    }

    /// <summary>
    ///   入力音声の転写
    /// </summary>
    public class InputAudioTranscription
    {
        [JsonProperty("model")] public string Model { get; set; }
    }

    /// <summary>
    ///  ターン検出
    /// </summary>
    public class TurnDetection
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("threshold")] public double Threshold { get; set; }

        [JsonProperty("prefix_padding_ms")] public int PrefixPaddingMs { get; set; }

        [JsonProperty("silence_duration_ms")] public int SilenceDurationMs { get; set; }
    }

    /// <summary>
    ///  ツール
    /// </summary>
    public class Tool
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("parameters")] public ToolParameters Parameters { get; set; }
    }

    /// <summary>
    /// ツールのパラメータ
    /// </summary>
    public class ToolParameters
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("properties")] public Dictionary<string, ToolProperty> Properties { get; set; }

        [JsonProperty("required")] public List<string> Required { get; set; }
    }

    /// <summary>
    /// ツールのプロパティ
    /// </summary>
    public class ToolProperty
    {
        [JsonProperty("type")] public string Type { get; set; }
    }
}
