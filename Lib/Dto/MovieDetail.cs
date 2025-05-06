using System.Text.Json.Serialization;

namespace Lib.Dto;

public class MovieDetail
{
    public required string Title { get; set; }
    public required File File { get; set; }
    public int FileSize { get; set; }
    public required PubImage TrackImage { get; set; }
    public required object Markers { get; set; }
    public required string Label { get; set; }
    public int Track { get; set; }
    public bool HasTrack { get; set; }
    public required string Pub { get; set; }
    public int Docid { get; set; }
    public int BookNum { get; set; }
    public required string MimeType { get; set; }
    public required string Edition { get; set; }

    [JsonPropertyName("editionDescr")] public required string EditionDesc { get; set; }

    public required string Format { get; set; }

    [JsonPropertyName("formatDescr")] public required string FormatDesc { get; set; }

    public required string Specialty { get; set; }

    [JsonPropertyName("specialtyDescr")] public required string SpecialtyDesc { get; set; }

    public bool Subtitled { get; set; }
    public int FrameWidth { get; set; }
    public int FrameHeight { get; set; }
    public double FrameRate { get; set; }
    public double Duration { get; set; }
    public double BitRate { get; set; }
    public PubImage? Subtitles { get; set; }
}