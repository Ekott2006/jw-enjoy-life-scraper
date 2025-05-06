namespace Lib.Dto;

public class PubImage
{
    public required string Url { get; set; }
    public required string ModifiedDatetime { get; set; }
    public required object Checksum { get; set; }
}