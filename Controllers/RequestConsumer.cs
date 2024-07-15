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
        private const string strAuthorization = "Your_Authorization"; // Replace with your actual authorization string
        private const bool useAuthorization = false; // set to true if you want to use Basic authorization 

        [HttpPost]
        public async Task<IActionResult> ConsumeRequest()
        {

            if (useAuthorization)
            {
                // Extract Authorization header
                if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeaders))
                {
                    return Unauthorized("Missing Authorization header.");
                }

                var authorizationHeader = authorizationHeaders.ToString();

                // Verify Authorization header format
                if (!authorizationHeader.StartsWith("Basic "))
                {
                    return Unauthorized("Invalid Authorization header format.");
                }

                var base64String = authorizationHeader.Substring("Basic ".Length).Trim();
                var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(base64String));

                // Compare the decoded string with the expected authorization string
                if (decodedString != strAuthorization)
                {
                    return Unauthorized("Invalid Authorization credentials.");
                }
            }

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
            if (Math.Abs(currentTimestamp - requestTimestamp) > TimeLimit)
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
