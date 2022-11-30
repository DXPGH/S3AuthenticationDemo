using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace S3AuthenticationDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BucketsController : ControllerBase
    {
        private readonly IAmazonS3 _amazonS3;

        public BucketsController(IAmazonS3 amazonS3)
        {
            _amazonS3 = amazonS3;
        }

        // Create
        [HttpPost]
        public async Task<IActionResult> CreateBucketAsync(string bucketName)
        {
            var bucketExists = await _amazonS3.DoesS3BucketExistAsync(bucketName);
            
            if (bucketExists)
            {
                return BadRequest($"Bucket {bucketName} already exists!");
            }


            await _amazonS3.PutBucketAsync(bucketName);

            return Ok($"Bucket {bucketName} created successfylly!");
        }

        [HttpGet("get-all-buckets")]
        public async Task<IActionResult> GetAllBucketsAsync()
        {
            var data = await _amazonS3.ListBucketsAsync();

            var buckets = data.Buckets.Select(b => b.BucketName);

            return Ok(buckets);
        }

        // delete
        [HttpDelete("delete-bucket")]
        public async Task<IActionResult> DeleteBucketAsync(string bucketName)
        {
            await _amazonS3.DeleteBucketAsync(bucketName);

            return NoContent();
        }
    }
}
