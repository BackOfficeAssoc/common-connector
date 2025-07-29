using MetadataScanner.Services.DbMoto.MetadataService;
using ReaderLibrary.Models;
using Syniti;
using System.Text.Json;

namespace WebAppRunner.Service
{
        public interface IAirbyteConnectorService
        {
            Task<AirbyteMessage> GetSpecAsync();
            Task<AirbyteMessage> CheckAsync(string connectionId);
            Task<AirbyteMessage> DiscoverAsync(string connectionId);
            Task<List<AirbyteMessage>> ReadAsync(string connectionId, List<string> tables);
            IAsyncEnumerable<AirbyteMessage> ReadAsyncStreamed(string connectionId, List<string> tables);
        }


        public class AirbyteConnectorService : IAirbyteConnectorService
        {
            private readonly IDBGateConnectionFactory _connectionFactory;
            private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            public AirbyteConnectorService(IDBGateConnectionFactory connectionFactory)
            {
                _connectionFactory = connectionFactory;
            }

            public Task<AirbyteMessage> GetSpecAsync()
            {
                var spec = new ConnectorSpec
                {
                    connectionSpecification = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["required"] = new[] { "connectionId" },
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["connectionId"] = new Dictionary<string, object>
                            {
                                ["type"] = "string",
                                ["title"] = "Connection ID",
                                ["description"] = "The connection identifier registered in Syniti"
                            }
                        }
                    }
                };

                return Task.FromResult(new AirbyteMessage
                {
                    type = AirbyteMessageType.SPEC,
                    spec = spec
                });
            }

            public async Task<AirbyteMessage> CheckAsync(string connectionId)
            {
                try
                {
                    var connId = new SkpAssetId(connectionId);
                    var iConnection = await _connectionFactory.GetIConn(connId, CancellationToken.None);
                    iConnection.Open();

                    return new AirbyteMessage
                    {
                        type = AirbyteMessageType.CONNECTION_STATUS,
                        connectionStatus = new AirbyteConnectionStatus
                        {
                            status = "SUCCEEDED",
                            message = "Connection test successful"
                        }
                    };
                }
                catch (Exception ex)
                {
                    return new AirbyteMessage
                    {
                        type = AirbyteMessageType.CONNECTION_STATUS,
                        connectionStatus = new AirbyteConnectionStatus
                        {
                            status = "FAILED",
                            message = ex.Message
                        }
                    };
                }
            }

            public async Task<AirbyteMessage> DiscoverAsync(string connectionId)
            {
                var connId = new SkpAssetId(connectionId);
                var iConnection = await _connectionFactory.GetIConn(connId, CancellationToken.None);
                iConnection.Open();

                var cmd = iConnection.CreateCommand();
                cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";

                var resultSet = cmd.ExecuteQuery();
                var catalog = new AirbyteCatalog();

                while (resultSet.Next())
                {
                    var tableName = resultSet.GetValue(0)?.ToString();
                    if (tableName == null) continue;

                    var stream = new AirbyteStream
                    {
                        name = tableName,
                        json_schema = new Dictionary<string, object>
                        {
                            ["type"] = "object",
                            ["properties"] = new Dictionary<string, object>()
                        }
                    };

                    catalog.streams.Add(new AirbyteStreamCatalog { stream = stream });
                }

                return new AirbyteMessage
                {
                    type = AirbyteMessageType.CATALOG,
                    catalog = catalog
                };
            }

            public async Task<List<AirbyteMessage>> ReadAsync(string connectionId, List<string> tables)
            {
                var messages = new List<AirbyteMessage>();

                var connId = new SkpAssetId(connectionId);
                var iConnection = await _connectionFactory.GetIConn(connId, CancellationToken.None);
                iConnection.Open();

                foreach (var table in tables)
                {
                    var cmd = iConnection.CreateCommand();
                    cmd.CommandText = $"SELECT * FROM [{table}]";
                    var reader = cmd.ExecuteQuery();

                    while (reader.Next())
                    {
                        var record = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var field = reader.getField(i).Name;
                            record[field] = reader.IsNull(i) ? null : reader.GetValue(i);
                        }

                        var airbyteRecord = new AirbyteRecord
                        {
                            stream = table,
                            data = record,
                            emitted_at = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                        };

                        messages.Add(new AirbyteMessage
                        {
                            type = AirbyteMessageType.RECORD,
                            record = airbyteRecord
                        });
                    }
                }

                return messages;
            }

            public async IAsyncEnumerable<AirbyteMessage> ReadAsyncStreamed(string connectionId, List<string> tables)
            {
                var connId = new SkpAssetId(connectionId);
                var conn = await _connectionFactory.GetIConn(connId, CancellationToken.None);
                conn.Open();

                foreach (var table in tables)
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = $"SELECT * FROM [{table}]";
                    var reader = cmd.ExecuteQuery();

                    while (reader.Next())
                    {
                        var record = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var field = reader.getField(i).Name;
                            record[field] = reader.IsNull(i) ? null : reader.GetValue(i);
                        }

                        yield return new AirbyteMessage
                        {
                            type = AirbyteMessageType.RECORD,
                            record = new AirbyteRecord
                            {
                                stream = table,
                                data = record,
                                emitted_at = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                            }
                        };
                    }
                }
            }
    }
}
