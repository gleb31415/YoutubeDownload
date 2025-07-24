using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeDownloader.Web.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace YoutubeDownloader.Web.Services
{
    public class YoutubeService
    {
        private readonly YoutubeClient _youtube;
        private readonly string _tempPath;
        private readonly ILogger<YoutubeService> _logger;

        public YoutubeService(ILogger<YoutubeService> logger)
        {
            _youtube = new YoutubeClient();
            _tempPath = Path.Combine("/tmp", "YoutubeDownloader");
            Directory.CreateDirectory(_tempPath);
            _logger = logger;
        }

        public async Task<VideoInfo> GetVideoInfoAsync(string url)
        {
            try
            {
                _logger.LogInformation("Getting video info for URL: {Url}", url);
                var video = await _youtube.Videos.GetAsync(url);

                return new VideoInfo
                {
                    Title = video.Title,
                    Author = video.Author.ChannelTitle,
                    Duration = video.Duration ?? TimeSpan.Zero,
                    ThumbnailUrl = video.Thumbnails.OrderByDescending(t => t.Resolution.Area).FirstOrDefault()?.Url ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting video info for URL: {Url}", url);
                throw;
            }
        }

        public async Task<DownloadResult> DownloadVideoAsync(string url, string quality)
        {
            try
            {
                _logger.LogInformation("Starting download for URL: {Url} with quality: {Quality}", url, quality);

                var video = await _youtube.Videos.GetAsync(url);
                var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(video.Id);
                _logger.LogInformation("Found {Count} muxed streams", streamManifest.GetMuxedStreams().Count());

                // Get the best stream based on quality preference
                IVideoStreamInfo? streamInfo = quality.ToLower() switch
                {
                    "highest" => streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality.MaxHeight).FirstOrDefault(),
                    "high" => streamManifest.GetMuxedStreams()
                                .Where(s => s.VideoQuality.Label.Contains("720p") || s.VideoQuality.Label.Contains("1080p"))
                                .OrderByDescending(s => s.VideoQuality.MaxHeight)
                                .FirstOrDefault(),
                    "medium" => streamManifest.GetMuxedStreams()
                                .Where(s => s.VideoQuality.Label.Contains("480p") || s.VideoQuality.Label.Contains("360p"))
                                .OrderByDescending(s => s.VideoQuality.MaxHeight)
                                .FirstOrDefault(),
                    "low" => streamManifest.GetMuxedStreams()
                                .Where(s => s.VideoQuality.Label.Contains("240p") || s.VideoQuality.Label.Contains("144p"))
                                .OrderByDescending(s => s.VideoQuality.MaxHeight)
                                .FirstOrDefault(),
                    _ => streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality.MaxHeight).FirstOrDefault()
                };

                if (streamInfo == null)
                {
                    // Fallback to any available stream
                    streamInfo = streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality.MaxHeight).FirstOrDefault();
                }

                if (streamInfo == null)
                {
                    _logger.LogError("No suitable video stream found for URL: {Url}", url);
                    throw new InvalidOperationException("No suitable video stream found.");
                }

                _logger.LogInformation("Selected stream: {Quality} - {Container}", streamInfo.VideoQuality.Label, streamInfo.Container.Name);

                // Create a safe filename
                var safeTitle = string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars()));
                var fileName = $"{safeTitle}.{streamInfo.Container.Name}";
                var filePath = Path.Combine(_tempPath, $"{Guid.NewGuid()}_{fileName}");

                _logger.LogInformation("Downloading to: {FilePath}", filePath);

                // Download the stream
                await _youtube.Videos.Streams.DownloadAsync(streamInfo, filePath);
                _logger.LogInformation("Download completed successfully");

                return new DownloadResult
                {
                    FilePath = filePath,
                    FileName = fileName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading video from URL: {Url}", url);
                throw;
            }
        }
    }
}