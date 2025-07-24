using Microsoft.AspNetCore.Mvc;
using YoutubeDownloader.Web.Services;
using YoutubeDownloader.Web.Models;

namespace YoutubeDownloader.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class YoutubeController : ControllerBase
{
    private readonly YoutubeService _youtubeService;

    public YoutubeController(YoutubeService youtubeService)
    {
        _youtubeService = youtubeService;
    }

    [HttpPost("download")]
    public async Task<IActionResult> Download([FromBody] DownloadRequest request)
    {
        try
        {
            // Validate input
            if (string.IsNullOrEmpty(request?.Url))
            {
                return BadRequest(new { message = "URL is required" });
            }

            // Log the request
            Console.WriteLine($"Download request for URL: {request.Url}, Quality: {request.Quality}");
            
            var result = await _youtubeService.DownloadVideoAsync(request.Url, request.Quality);
            
            var fileBytes = await System.IO.File.ReadAllBytesAsync(result.FilePath);
            
            // Clean up the temporary file
            System.IO.File.Delete(result.FilePath);
            
            return File(fileBytes, "video/mp4", result.FileName);
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Error downloading video: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            return BadRequest(new { message = ex.Message, details = ex.ToString() });
        }
    }

    [HttpGet("info")]
    public async Task<IActionResult> GetVideoInfo([FromQuery] string url)
    {
        try
        {
            var info = await _youtubeService.GetVideoInfoAsync(url);
            return Ok(info);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
