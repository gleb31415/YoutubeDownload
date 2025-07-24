namespace YoutubeDownloader.Web.Models;

public class DownloadRequest
{
    public string Url { get; set; } = string.Empty;
    public string Quality { get; set; } = "highest";
}

public class DownloadResult
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

public class VideoInfo
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
}
