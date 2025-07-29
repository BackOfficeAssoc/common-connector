using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Syniti.Common.Security;
using SimpleDbReader.Concrete;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.Connections;
using ReaderLibrary.Concrete;
using Syniti;
using MetadataScanner.Services.DbMoto.MetadataService;
using DataAccess.Libraries.DBGate.Brokers;
using System.Text.Json.Serialization;

namespace WebAppRunner.Controllers
{
    [ApiController]
    [Route("test")]
    public class TestController : Controller
    {
        public ILogger<TestController> Logger { get; }

        protected IDBGateConnectionFactory connectionFactory { get; }

        public TestController(ILogger<TestController> logger, IDBGateConnectionFactory ConnectionFactory)
        {
            Logger = logger;
            connectionFactory = ConnectionFactory;
        }

        [HttpGet("")]
        [AuthorizeSynitiClaim("sac_24AC43017F0F4B9EB9DBE8C8B6423242")]
        public ActionResult<string> Get()
        {
            return "hello world";
        }

        [HttpGet("read")]
        public async Task<ActionResult<List<Dictionary<string, object>>>> ReadFromDbAsync()
        {
            try
            {
                var reader = new CommanderReader(GetReaderFromCommanderAsync);
                var results = new List<Dictionary<string, object>>();

                await reader.OpenProcessRecordsAsync();

                // Capture records into list
                for (int i = 0; i < 5; i++)
                {
                    var record = await reader.GetNextRecordInternalAsync();
                    if (record == null)
                        break;

                    results.Add(record.Fields);
                }

                await reader.CloseProcessRecordsAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error reading from DB");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("records/{connectionId}")]
        public async Task<IActionResult> GetRecordsAsync(string connectionId, [FromBody] CommandInput Command)
        {
            try
            {
                var connId = new SkpAssetId(connectionId);
                var iConnection = await connectionFactory.GetIConn(connId, HttpContext.RequestAborted);

                var results = new List<Dictionary<string, object?>>();

                using var reader = new CommanderRecordReader(iConnection, Command.Command);
                reader.Open();

                while (reader.Next())
                {
                    results.Add(reader.GetRecord());
                }

                reader.Close();
                return Ok(results);
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                // _logger?.LogError(ex, "Error while fetching records for connectionId: {ConnectionId}", connectionId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    error = "An error occurred while fetching records.",
                    details = ex.Message
                });
            }
        }

        public class CommandInput
        {
            [JsonPropertyName("command")]
            public string Command { get; set; }
        }

        private async Task<DbDataReader> GetReaderFromCommanderAsync()
        {
            var conn = new SqlConnection("Server=localhost;Database=TestDb;User Id=sa;Password=Pass123;");
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT TOP 5 * FROM Users";
            return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        [HttpGet("error")]
        public Task<ActionResult<string>> SimulateError()
        {
            throw new InvalidOperationException("Something was wrong");
        }
    }
}
