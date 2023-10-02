using Newtonsoft.Json;

namespace ChromeExtension.Model
{
    public class VideoChunkDto
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        [JsonProperty("chunkNumber")]
        public int ChunkNumber { get; set; }
        [JsonProperty("completed")]
        public bool Completed { get; set; }
        [JsonProperty("chunkBlob")]
        public byte[] ChunkBlob { get; set; }
    }
}
