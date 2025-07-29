namespace WebAppRunner.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Text.Json;
    using WebAppRunner.Service;

    [ApiController]
    [Route("airbyte")]
    public class AirbyteConnectorController : ControllerBase
    {
        private readonly IAirbyteConnectorService _connectorService;

        public AirbyteConnectorController(IAirbyteConnectorService connectorService)
        {
            _connectorService = connectorService;
        }

        [HttpGet("spec")]
        public async Task<IActionResult> GetSpec()
        {
            var result = await _connectorService.GetSpecAsync(); 
            return Ok(result);
        }

        [HttpPost("check")]
        public async Task<IActionResult> Check([FromBody] ConnectionRequest request)
        {
            var result = await _connectorService.CheckAsync(request.ConnectionId);
            return Ok(result);
        }

        [HttpPost("discover")]
        public async Task<IActionResult> Discover([FromBody] ConnectionRequest request)
        {
            var result = await _connectorService.DiscoverAsync(request.ConnectionId);
            return Ok(result);
        }

        [HttpPost("read")]
        public async Task<IActionResult> Read([FromBody] ReadRequest request)
        {
            var result = await _connectorService.ReadAsync(request.ConnectionId, request.Tables);
            return Ok(result);
        }

        [HttpPost("readStream")]
        public async Task ReadStream([FromBody] ReadRequest request)
        {
            Response.StatusCode = StatusCodes.Status200OK;
            Response.ContentType = "application/x-ndjson";

            await foreach (var message in _connectorService.ReadAsyncStreamed(request.ConnectionId, request.Tables))
            {
                var json = JsonSerializer.Serialize(message);
                await Response.WriteAsync(json + "\n");
                await Response.Body.FlushAsync();
            }
        }

        public class ConnectionRequest
        {
            public string ConnectionId { get; set; }
        }

        public class ReadRequest : ConnectionRequest
        {
            public List<string> Tables { get; set; } = new();
        }
    }

}
