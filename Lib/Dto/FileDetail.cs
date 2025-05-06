using System.Text.Json.Serialization;

namespace Lib.Dto;

public class FileDetail
{
    [JsonPropertyName("MP4")] public required List<MovieDetail> MovieMp4S { get; set; }

    [JsonPropertyName("3GP")] public required List<MovieDetail> Movie3Gps { get; set; }
}