using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReaderLibrary.Interfaces
{
    public interface IAirbyteConnectorService
    {
        Task HandleSpecAsync();

        Task HandleCheckAsync(string connectionId);

        Task HandleDiscoverAsync(string connectionId);

        Task HandleReadAsync(string connectionId, List<string> tables);
    }
}
