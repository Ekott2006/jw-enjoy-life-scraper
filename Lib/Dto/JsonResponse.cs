namespace Lib.Dto;

public class JsonResponse
{
    public string PubName { get; set; }
    public string ParentPubName { get; set; }
    public string BookNum { get; set; }
    public string Pub { get; set; }
    public string Issue { get; set; }
    public string FormattedDate { get; set; }
    public string[] Fileformat { get; set; }
    public int? Track { get; set; }
    public string Specialty { get; set; }
    public PubImage PubImage { get; set; }
    public Dictionary<string, LanguageDetail> Languages { get; set; }
    public Dictionary<string, FileDetail> Files { get; set; }
}