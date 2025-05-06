using System.Security.Cryptography;
using System.Text;
using Lib.Dto;

namespace Lib;

public static class Helper
{
    public static string GenerateHash(string input)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(hash);
    }
    
    public static string FormatFileSize(decimal bytes)
    {
        List<string> sizes = ["B", "KB", "MB", "GB", "TB"];
        int order = 0;
        decimal size = bytes;
            
        while (size >= 1024 && order < sizes.Count - 1)
        {
            order++;
            size /= 1024;
        }
            
        return $"{size:0.##} {sizes[order]}";
    }
    
    public static Dictionary<string, decimal> GenerateMetadata(IEnumerable<JsonResponse> responses)
    {
        Dictionary<string, decimal> resolutionSizes = new();
            
        // TODO: Work Here
        foreach (MovieDetail movieDetail in responses.SelectMany(response => response.Files.FirstOrDefault().Value.MovieMp4s))
        {
            string resolution = movieDetail.Label;
            resolutionSizes.TryAdd(resolution, 0);
            resolutionSizes[resolution] += movieDetail.FileSize;
        }
        return resolutionSizes;
    }
    
    public static string SanitizeFileName(string name)
    {
        return Path.GetInvalidFileNameChars().Aggregate(name, (current, c) => current.Replace(c, '_'));
    }

}