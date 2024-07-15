using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace RequestConsumerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestConsumerController : ControllerBase
    {
        private const string Secret = "Your_Secret"; // Replace with your actual secret
        private const int TimeLimit = 60; // Replace with your own value of seconds

        [HttpPost]
        public async Task<IActionResult> ConsumeRequest()
        {
            // Extract headers
            if (!Request.Headers.TryGetValue("X-MR-TIMESTAMP", out var timestampHeaders) ||
                !Request.Headers.TryGetValue("X-MR-SIGNATURE", out var signatureHeaders))
            {
                return BadRequest("Missing required headers.");
            }

            var timestamp = timestampHeaders.ToString();
            var receivedSignature = signatureHeaders.ToString();

            // Verify timestamp
            if (!int.TryParse(timestamp, out int requestTimestamp))
            {
                return BadRequest("Invalid timestamp format.");
            }

            var currentTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            if (Math.Abs(currentTimestamp - requestTimestamp) > 60)
            {
                return BadRequest("Timestamp is not within the last minute.");
            }

            // Extract payload
            var payloadString = await new System.IO.StreamReader(Request.Body).ReadToEndAsync();

            // Compute HMAC signature
            var computedSignature = HmacSha256Digest($"{timestamp}.{payloadString}", Secret);

            // Verify signature
            if (computedSignature != receivedSignature)
            {
                return Unauthorized("Signature verification failed.");
            }

            Console.WriteLine($"Payload: {payloadString}");

            // Proceed with the payload processing
            // ...

            return Ok("Request consumed successfully.");
        }

        private static string HmacSha256Digest(string message, string secret)
        {
            var encoding = new UTF8Encoding();
            byte[] keyBytes = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            System.Security.Cryptography.HMACSHA256 cryptographer = new System.Security.Cryptography.HMACSHA256(keyBytes);
            byte[] bytes = cryptographer.ComputeHash(messageBytes);
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
