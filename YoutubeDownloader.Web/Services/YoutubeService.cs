using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeDownloader.Web.Models;

namespace YoutubeDownloader.Web.Services;

public class YoutubeService
{
    private readonly YoutubeClient _youtube;
    private readonly string _tempPath;

    public YoutubeService()
    {
        _youtube = new YoutubeClient();
        
        // Use /tmp for Railway container environment
        _tempPath = Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT") != null 
            ? "/tmp/YoutubeDownloader" 
            : Path.Combine(Path.GetTempPath(), "YoutubeDownloader");
            
        Directory.CreateDirectory(_tempPath);
        
        Console.WriteLine($"Using temp path: {_tempPath}");
    }

    public async Task<VideoInfo> GetVideoInfoAsync(string url)
    {
        var video = await _youtube.Videos.GetAsync(url);
        
        return new VideoInfo
        {
            Title = video.Title,
            Author = video.Author.ChannelTitle,
            Duration = video.Duration ?? TimeSpan.Zero,
            ThumbnailUrl = video.Thumbnails.OrderByDescending(t => t.Resolution.Area).FirstOrDefault()?.Url ?? string.Empty
        };
    }

    public async Task<DownloadResult> DownloadVideoAsync(string url, string quality)
    {
        try
        {
            Console.WriteLine($"Starting download for URL: {url}");
            
            var video = await _youtube.Videos.GetAsync(url);
            Console.WriteLine($"Video found: {video.Title}");
            
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(video.Id);
            Console.WriteLine($"Stream manifest loaded, {streamManifest.Streams.Count()} streams available");

            // Get the best stream based on quality preference
            IVideoStreamInfo? streamInfo = quality.ToLower() switch
            {
                "highest" => (IVideoStreamInfo?)streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality.MaxHeight).FirstOrDefault() ??
                            (IVideoStreamInfo?)streamManifest.GetVideoOnlyStreams().OrderByDescending(s => s.VideoQuality.MaxHeight).FirstOrDefault(),
                "high" => (IVideoStreamInfo?)streamManifest.GetMuxedStreams()
                            .Where(s => s.VideoQuality.Label.Contains("720p") || s.VideoQuality.Label.Contains("1080p"))
                            .OrderByDescending(s => s.VideoQuality.MaxHeight)
                            .FirstOrDefault() ??
                          (IVideoStreamInfo?)streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality.MaxHeight).FirstOrDefault(),
                "medium" => (IVideoStreamInfo?)streamManifest.GetMuxedStreams()
                            .Where(s => s.VideoQuality.Label.Contains("480p") || s.VideoQuality.Label.Contains("360p"))
                            .OrderByDescending(s => s.VideoQuality.MaxHeight)
                            .FirstOrDefault() ??
                           (IVideoStreamInfo?)streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality.MaxHeight).FirstOrDefault(),
                "low" => (IVideoStreamInfo?)streamManifest.GetMuxedStreams()
                            .Where(s => s.VideoQuality.Label.Contains("240p") || s.VideoQuality.Label.Contains("144p"))
                            .OrderByDescending(s => s.VideoQuality.MaxHeight)
                            .FirstOrDefault() ??
                         (IVideoStreamInfo?)streamManifest.GetMuxedStreams().OrderBy(s => s.VideoQuality.MaxHeight).FirstOrDefault(),
                _ => (IVideoStreamInfo?)streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality.MaxHeight).FirstOrDefault()
            };

            if (streamInfo == null)
            {
                throw new InvalidOperationException("No suitable video stream found.");
            }

            Console.WriteLine($"Selected stream: {streamInfo.VideoQuality} - {streamInfo.Container}");

            // Create a safe filename
            var safeTitle = string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars()));
            if (safeTitle.Length > 100) safeTitle = safeTitle.Substring(0, 100); // Limit filename length
            
            var fileName = $"{safeTitle}.{streamInfo.Container.Name}";
            var filePath = Path.Combine(_tempPath, $"{Guid.NewGuid()}_{fileName}");

            Console.WriteLine($"Downloading to: {filePath}");

            // Download the stream
            await _youtube.Videos.Streams.DownloadAsync(streamInfo, filePath);
            
            Console.WriteLine($"Download completed: {filePath}");

            return new DownloadResult
            {
                FilePath = filePath,
                FileName = fileName
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DownloadVideoAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
        {
            FilePath = filePath,
            FileName = fileName
        };
    }
}
