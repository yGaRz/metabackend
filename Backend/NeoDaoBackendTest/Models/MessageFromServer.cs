using Newtonsoft.Json;

namespace NeoDaoBackendTest.Models;

public class MessageFromServer<T,G> {
    [JsonProperty("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonProperty("eventType")]
    public G EventType { get; set; }

    [JsonProperty("data")]
    public T? Data { get; set; }
}
