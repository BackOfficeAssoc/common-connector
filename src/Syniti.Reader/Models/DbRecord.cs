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

        public object? Record { get; set; }

        public object? State { get; set; }

        public object? Log { get; set; }

        public object? Spec { get; set; }

        public object? ConnectionStatus { get; set; }

        public object? Catalog { get; set; }
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
