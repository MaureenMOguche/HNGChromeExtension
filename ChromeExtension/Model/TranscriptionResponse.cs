namespace ChromeExtension.Model
{
    public class TranscriptionResponse
    {
        public string Id { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public string Transcription { get; set; } = string.Empty;
        public string CompletedMessage { get; set; } = string.Empty;
    }
}
