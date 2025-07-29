using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetadataScanner.Services.DataCommander
{
    public class DataCommanderOptions
    {
        public string SharedSecret { get; set; } = string.Empty;

        public Dictionary<string, Uri> ServiceUrl { get; set; } = new Dictionary<string, Uri>();

        public Uri DataCommanderBaseUrl()
        {
            return this.ServiceUrl["data-commander"];
        }
    }
}
