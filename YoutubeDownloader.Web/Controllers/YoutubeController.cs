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
            var result = await _youtubeService.DownloadVideoAsync(request.Url, request.Quality);
            
            var fileBytes = await System.IO.File.ReadAllBytesAsync(result.FilePath);
            
            // Clean up the temporary file
            System.IO.File.Delete(result.FilePath);
            
            return File(fileBytes, "video/mp4", result.FileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
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
