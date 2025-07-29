//using System;
//using System.Collections.Generic;
//using System.Text.Json;
//using System.Threading;
//using System.Threading.Tasks;
//using DataAccess.Libraries.DBGate;
//using MetadataScanner.Services.DbMoto.MetadataService;
//using ReaderLibrary.Interfaces;
//using ReaderLibrary.Models;
//using Syniti;

//namespace ReaderLibrary.Core
//{
//    public class AirbyteConnectorService : IAirbyteConnectorService
//    {
//        private readonly IDBGateConnectionFactory _connectionFactory;
//        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

//        public AirbyteConnectorService(IDBGateConnectionFactory connectionFactory)
//        {
//            _connectionFactory = connectionFactory;
//        }

//        public Task HandleSpecAsync()
//        {
//            var spec = new ConnectorSpec
//            {
//                connectionSpecification = new Dictionary<string, object>
//                {
//                    ["type"] = "object",
//                    ["required"] = new[] { "connectionId" },
//                    ["properties"] = new Dictionary<string, object>
//                    {
//                        ["connectionId"] = new Dictionary<string, object>
//                        {
//                            ["type"] = "string",
//                            ["title"] = "Connection ID",
//                            ["description"] = "The connection identifier registered in Syniti"
//                        }
//                    }
//                }
//            };

//            var message = new AirbyteMessage
//            {
//                type = AirbyteMessageType.SPEC,
//                spec = spec
//            };

//            Console.WriteLine(JsonSerializer.Serialize(message, _jsonOptions));
//            return Task.CompletedTask;
//        }

//        public async Task HandleCheckAsync(string connectionId)
//        {
//            try
//            {
//                var connId = new SkpAssetId(connectionId);
//                var iConnection = await _connectionFactory.GetIConn(connId, CancellationToken.None);
//                iConnection.Open();

//                var status = new AirbyteConnectionStatus
//                {
//                    status = "SUCCEEDED",
//                    message = "Connection test successful"
//                };

//                var message = new AirbyteMessage
//                {
//                    type = AirbyteMessageType.CONNECTION_STATUS,
//                    connectionStatus = status
//                };

//                Console.WriteLine(JsonSerializer.Serialize(message, _jsonOptions));
//            }
//            catch (Exception ex)
//            {
//                var message = new AirbyteMessage
//                {
//                    type = AirbyteMessageType.CONNECTION_STATUS,
//                    connectionStatus = new AirbyteConnectionStatus
//                    {
//                        status = "FAILED",
//                        message = ex.Message
//                    }
//                };

//                Console.WriteLine(JsonSerializer.Serialize(message, _jsonOptions));
//            }
//        }

//        public async Task HandleDiscoverAsync(string connectionId)
//        {
//            var connId = new SkpAssetId(connectionId);
//            var iConnection = await _connectionFactory.GetIConn(connId, CancellationToken.None);
//            iConnection.Open();

//            var cmd = iConnection.CreateCommand();
//            cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";

//            var resultSet = cmd.ExecuteQuery();

//            var catalog = new AirbyteCatalog();

//            while (resultSet.Next())
//            {
//                var tableName = resultSet.GetValue(0)?.ToString();
//                if (tableName == null) continue;

//                var stream = new AirbyteStream
//                {
//                    name = tableName,
//                    json_schema = new Dictionary<string, object>
//                    {
//                        ["type"] = "object",
//                        ["properties"] = new Dictionary<string, object>() // schema empty for now
//                    }
//                };

//                catalog.streams.Add(new AirbyteStreamCatalog { stream = stream });
//            }

//            var message = new AirbyteMessage
//            {
//                type = AirbyteMessageType.CATALOG,
//                catalog = catalog
//            };

//            Console.WriteLine(JsonSerializer.Serialize(message, _jsonOptions));
//        }

//        public async Task HandleReadAsync(string connectionId, List<string> tables)
//        {
//            var connId = new SkpAssetId(connectionId);
//            var iConnection = await _connectionFactory.GetIConn(connId, CancellationToken.None);
//            iConnection.Open();

//            foreach (var table in tables)
//            {
//                var cmd = iConnection.CreateCommand();
//                cmd.CommandText = $"SELECT * FROM [{table}]";
//                var reader = cmd.ExecuteQuery();

//                while (reader.Next())
//                {
//                    var record = new Dictionary<string, object>();
//                    for (int i = 0; i < reader.FieldCount; i++)
//                    {
//                        var field = reader.getField(i).Name;
//                        record[field] = reader.IsNull(i) ? null : reader.GetValue(i);
//                    }

//                    var airbyteRecord = new AirbyteRecord
//                    {
//                        stream = table,
//                        data = record,
//                        emitted_at = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
//                    };

//                    var message = new AirbyteMessage
//                    {
//                        type = AirbyteMessageType.RECORD,
//                        record = airbyteRecord
//                    };

//                    Console.WriteLine(JsonSerializer.Serialize(message, _jsonOptions));
//                }
//            }
//        }
//    }
//}
