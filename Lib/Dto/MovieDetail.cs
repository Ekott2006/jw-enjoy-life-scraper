using System.Text.Json.Serialization;

namespace Lib.Dto;

public class MovieDetail
{
    public string Title { get; set; }
    public File File { get; set; }
    public int FileSize { get; set; }
    public PubImage TrackImage { get; set; }
    public object Markers { get; set; }
    public string Label { get; set; }
    public int Track { get; set; }
    public bool HasTrack { get; set; }
    public string Pub { get; set; }
    public int Docid { get; set; }
    public int BookNum { get; set; }
    public string MimeType { get; set; }
    public string Edition { get; set; }
    [JsonPropertyName("editionDescr")]
    public string EditionDesc { get; set; }
    public string Format { get; set; }
    [JsonPropertyName("formatDescr")]
    public string FormatDesc { get; set; }
    public string Specialty { get; set; }
    [JsonPropertyName("specialtyDescr")]
    public string SpecialtyDesc { get; set; }
    public bool Subtitled { get; set; }
    public int FrameWidth { get; set; }
    public int FrameHeight { get; set; }
    public double FrameRate { get; set; }
    public double Duration { get; set; }
    public double BitRate { get; set; }
    public PubImage Subtitles { get; set; }
}