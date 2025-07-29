using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReaderLibrary.Interfaces
{
    public interface IGenericRecordReader : IDisposable
    {
        void Open();

        bool Next();

        Dictionary<string, object?> GetRecord();

        void Close();
    }

}
