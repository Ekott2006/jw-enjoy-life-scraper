using System.Text.Json.Serialization;

namespace Lib.Dto;

public class FileDetail
{
    [JsonPropertyName("MP4")] public  List<MovieDetail> MovieMp4S { get; set; } = [];

    [JsonPropertyName("3GP")] public  List<MovieDetail> Movie3Gps { get; set; } = [];
}