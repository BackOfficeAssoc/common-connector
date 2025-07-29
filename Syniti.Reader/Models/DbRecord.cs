using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReaderLibrary.Models
{
    public class DbRecord
    {
        public Dictionary<string, object> Fields { get; set; } = new();
    }

    // Configuration model for the connector
    public class MSSQLSourceConfig
    {
        public string Host { get; set; }

        public int Port { get; set; } = 1433;

        public string Database { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public List<string> Tables { get; set; } = new List<string>();
    }

    // Airbyte message types
    public enum AirbyteMessageType
    {
        RECORD,
        STATE,
        LOG,
        SPEC,
        CONNECTION_STATUS,
        CATALOG
    }

    public class AirbyteMessage
    {
        public AirbyteMessageType type { get; set; }

        public object record { get; set; }

        public object state { get; set; }

        public object log { get; set; }

        public object spec { get; set; }

        public object connectionStatus { get; set; }

        public object catalog { get; set; }
    }

    public class AirbyteRecord
    {
        public string stream { get; set; }

        public Dictionary<string, object> data { get; set; }

        public long emitted_at { get; set; }
    }

    public class AirbyteConnectionStatus
    {
        public string status { get; set; } // "SUCCEEDED" or "FAILED"

        public string message { get; set; }
    }

    public class AirbyteStream
    {
        public string name { get; set; }

        public Dictionary<string, object> json_schema { get; set; }

        public List<string> supported_sync_modes { get; set; } = new List<string> { "full_refresh" };
    }

    public class AirbyteCatalog
    {
        public List<AirbyteStreamCatalog> streams { get; set; } = new List<AirbyteStreamCatalog>();
    }

    public class AirbyteStreamCatalog
    {
        public AirbyteStream stream { get; set; }

        public string sync_mode { get; set; } = "full_refresh";

        public string destination_sync_mode { get; set; } = "overwrite";
    }

    public class ConnectorSpec
    {
        public string documentationUrl { get; set; } = "https://docs.airbyte.io/integrations/sources/mssql";

        public Dictionary<string, object> connectionSpecification { get; set; }
    }
}
