using SimpleDbReader.Concrete;
using System.Data.Common;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebAppRunner.Service
{
    public class ReaderService
    {
        public async Task RunReaderAsync()
        {
            var reader = new CommanderReader(GetReaderFromCommanderAsync);
            await reader.OpenProcessRecordsAsync();
            await reader.ProcessRecordsAsync(5);
            await reader.CloseProcessRecordsAsync();
        }

        private async Task<DbDataReader> GetReaderFromCommanderAsync()
        {
            var conn = new SqlConnection("Server=localhost;Database=TestDb;User Id=sa;Password=Pass123;");
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT TOP 5 * FROM Users";

            return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }
    }
}
