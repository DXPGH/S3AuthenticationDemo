using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using S3AuthenticationDemo.Models;

namespace S3AuthenticationDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IAmazonS3 _amazonS3;

        public FilesController(IAmazonS3 amazonS3)
        {
            _amazonS3 = amazonS3;   
        }

        // upload

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFileAsync(IFormFile file, string bucketName, string? prefix)
        {
            var bucketExists = await _amazonS3.DoesS3BucketExistAsync(bucketName);
            if (!bucketExists)
            {
                return NotFound($"Bucket {bucketName} does not exist");
            }

            var request = new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = string.IsNullOrEmpty(prefix) ? file.FileName : $"{prefix?.TrimEnd('/')}/{file.FileName}",
                InputStream = file.OpenReadStream()
            };

            request.Metadata.Add("Content-Type", file.ContentType);

            await _amazonS3.PutObjectAsync(request);

            return Ok($"File {prefix}/{file.FileName} uploaded successfully to S3");
        }

        [HttpGet("get-all-files")]
        public async Task<IActionResult> GetAllFilesAsync(string bucketName, string? prefix)
        {
            var bucketExists = await _amazonS3.DoesS3BucketExistAsync(bucketName);
            if (!bucketExists)
            {
                return NotFound($"Bucket {bucketName} does not exist");
            }

            var request = new ListObjectsV2Request()
            {
                BucketName = bucketName,
                Prefix = prefix
            };

            var result = await _amazonS3.ListObjectsV2Async(request);

            var s3Object = result.S3Objects.Select(s =>
            {
                var urlRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = bucketName,
                    Key = s.Key,
                    Expires = DateTime.UtcNow.AddMinutes(1)
                };

                return new S3ObjectDTO()
                {
                    Name = s.Key.ToString(),
                    PreSignedUrl = _amazonS3.GetPreSignedURL(urlRequest),
                };
            });

            return Ok(s3Object);
        }

        [HttpDelete("delete-file")]
        public async Task<IActionResult> DeleteFileAsync(string bucketName, string key)
        {
            var bucketExists = await _amazonS3.DoesS3BucketExistAsync(bucketName);
            if (!bucketExists)
            {
                return NotFound($"Bucket {bucketName} does not exist");
            }

            await _amazonS3.DeleteObjectAsync(bucketName, key);

            return NoContent();
        }
    }
}
