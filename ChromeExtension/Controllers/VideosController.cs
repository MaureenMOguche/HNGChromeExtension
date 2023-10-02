using ChromeExtension.Model;
using ChromeExtension.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChromeExtension.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideosController : ControllerBase
    {
        private readonly VideoService _videoService;
        private readonly ILogger<VideosController> logger;

        public VideosController(VideoService videoService, ILogger<VideosController> logger)
        {
            _videoService = videoService;
            this.logger = logger;
        }

        [HttpPost("uploadChunk")]
        public async Task<IActionResult> UploadChunk(VideoChunkDto videoChunkDto)
        {

            var result = await _videoService.UploadChunk(videoChunkDto);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("startRecording/{videoId}")]
        public async Task<IActionResult> StartRecording(string videoId)
        {
            var result = await _videoService.StartRecording(videoId);
            return StatusCode((int)result.StatusCode, result);
        }

    }
}
