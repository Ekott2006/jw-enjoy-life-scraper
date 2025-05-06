using System.Text.Json.Serialization;

namespace Lib.Dto;

public class JsonResponse
{
    public required string PubName { get; set; }
    public required string ParentPubName { get; set; }
    public required string BookNum { get; set; }
    public required string Pub { get; set; }
    public required string Issue { get; set; }
    public required string FormattedDate { get; set; }
    [JsonPropertyName("Fileformat")] public string[] FileFormat { get; set; } = [];
    public int? Track { get; set; }
    public required string Specialty { get; set; }
    public required PubImage PubImage { get; set; }
    public required Dictionary<string, LanguageDetail> Languages { get; set; }
    public required Dictionary<string, FileDetail> Files { get; set; }
}