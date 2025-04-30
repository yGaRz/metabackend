using System.ComponentModel.DataAnnotations;
using NeoDaoBackend.Validation.Attributes;

namespace NeoDaoBackend.Models.Streams;

public class CreateStreamRequest
{
    [ValidNotEmptyString]
    public string Url { get; set; }
    [ValidStreamTime]
    public DateTimeOffset StartTime { get; set; }
    [ValidStreamTime(ValidateEndTime = true)]
    public DateTimeOffset? EndTime { get; set; }
}