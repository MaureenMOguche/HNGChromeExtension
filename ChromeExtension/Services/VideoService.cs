using ChromeExtension.Model;
using System.Net;
using Hangfire;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System.Text;
using Microsoft.AspNetCore.SignalR.Protocol;
using Newtonsoft.Json;

namespace ChromeExtension.Services
{
    public class VideoService
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly VideoDbContext _db;
        private readonly string _transcribeUrl;
        private readonly string _whisperKey;
        private string jobId = string.Empty;
        private readonly IConfiguration _config;
        private readonly ILogger<VideoService> _logger;
        public ApiResponse response = new();

        public VideoService(IHttpContextAccessor httpContext, 
            VideoDbContext db, IConfiguration config, ILogger<VideoService> logger)
        {
            _httpContext = httpContext;
            _db = db;
            _config = config;
            _logger = logger;
            _transcribeUrl = _config.GetValue<string>("OpenAi:WhisperUrl")!;
            _whisperKey = _config.GetValue<string>("OpenAi:Key")!;
        }

        public async Task<ApiResponse> UploadChunk(VideoChunkDto videoChunkDto)
        {
            _logger.LogInformation(videoChunkDto.Id, videoChunkDto.ChunkBlob.Length);
            var videoFolderpath = Path.Combine(Directory.GetCurrentDirectory(), "/UploadedVideos/");
            string tempFilePath = Path.Combine(videoFolderpath, $"{ videoChunkDto.Id}_temp.mp4");
            try
            {
                if (videoChunkDto.ChunkBlob == null || videoChunkDto.ChunkBlob.Length == 0)
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid video data";
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                using (var stream = new FileStream(tempFilePath, FileMode.Append))
                {
                    await stream.WriteAsync(videoChunkDto.ChunkBlob, 0, videoChunkDto.ChunkBlob.Length);
                }

                if (videoChunkDto.Completed)
                {
                    string videoFilePath = Path.Combine(videoFolderpath, $"{videoChunkDto.Id}.mp4");

                    System.IO.File.Move(tempFilePath, videoFilePath);

                    var request = _httpContext.HttpContext!.Request;
                    var videoUrl = $"{request.Scheme}://{request.Host}/{videoFolderpath}/{videoChunkDto.Id}.mp4";

                    jobId = BackgroundJob.Enqueue(() => TranscribeVideo(videoUrl, videoChunkDto.Id));
                    BackgroundJob.ContinueJobWith(jobId, () => HandleJobCompletion(videoChunkDto.Id));

                    if (videoUrl != null)
                    {
                        response.IsSuccess = true;
                        response.Message = "Successfully saved video data";
                        response.StatusCode = HttpStatusCode.OK;
                        response.Data = videoUrl;
                        return response;
                    }
                }

                response.IsSuccess = true;
                response.Message = "Successfully uploaded video chunk";
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch (Exception ex)
            { 
                response.IsSuccess = false;
                response.Message = ex.Message;
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }
        }

        private async Task HandleJobCompletion(string videoId)
        {
            var videoData = await _db.VideoDatas.FirstOrDefaultAsync(x => x.Id == videoId);
            var message = new TranscriptionResponse
            {
                CompletedMessage = "Transcription completed",
                Id = videoId,
                Transcription = videoData!.VideoTranscription,
                VideoUrl = videoData.Url
            };
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "job_completion", type: ExchangeType.Fanout);

                var messageJson = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(messageJson);

                channel.BasicPublish(exchange: "job_completion", routingKey: "", basicProperties: null, body: body);
            }
        }

        private async Task TranscribeVideo(string videoUrl, string videoId)
        {
            Console.WriteLine(videoUrl);

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(_transcribeUrl);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_whisperKey);

                var content = new MultipartFormDataContent();

                using (var filestream = new FileStream(videoUrl, FileMode.Open))
                {
                    var formfile = new FormFile(filestream, 0, filestream.Length, null!, Path.GetFileName(videoUrl));

                    content.Add(new StreamContent(formfile.OpenReadStream())
                    {
                        Headers =
                        {
                            ContentLength = formfile.Length,
                            ContentType = new MediaTypeHeaderValue(formfile.ContentType),
                            ContentDisposition = new ContentDispositionHeaderValue("form-data")
                            {
                                Name = "file",
                                FileName =formfile.FileName
                            }
                        }
                    });

                    content.Add(new StringContent("whisper-1"), "model");

                    var response = await httpClient.PostAsync(_transcribeUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var videoData = await _db.VideoDatas.FirstOrDefaultAsync(x => x.Id == videoId);
                        
                        if (videoData != null)
                        {
                            videoData.Url = videoUrl;
                            videoData.VideoTranscription = await response.Content.ReadAsStringAsync();
                            videoData.JobId = jobId;
                            _db.VideoDatas.Update(videoData);
                            await _db.SaveChangesAsync();
                        }
                    }
                    
                }
            }

        }

        public async Task<ApiResponse> StartRecording(string videoId)
        {
            _logger.LogInformation($"Recording {videoId}");
            var videoData = new VideoData
            {
                Id = videoId,
            };
            await _db.VideoDatas.AddAsync(videoData);
            await _db.SaveChangesAsync();

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Message = "Video recording successfully started";
            return response;
        }
    }
}
