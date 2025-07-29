using System;
using System.Data.Common;
using System.Threading.Tasks;
using global::SimpleDbReader.Core;
using ReaderLibrary.Models;

namespace SimpleDbReader.Concrete
{
        public class CommanderReader : Reader
        {
            private readonly Func<Task<DbDataReader>> _getReaderAsync;
            private DbDataReader? _reader;

            public CommanderReader(Func<Task<DbDataReader>> getReaderAsync)
            {
                _getReaderAsync = getReaderAsync ?? throw new ArgumentNullException(nameof(getReaderAsync));
            }

            protected override async Task<int> OpenAsync()
            {
                _reader = await _getReaderAsync.Invoke();
                return _reader != null ? 0 : -1;
            }

            public async Task<DbRecord?> GetNextRecordInternalAsync() => await GetNextRecordAsync();

            protected override async Task<DbRecord?> GetNextRecordAsync()
            {
                if (_reader == null)
                    return null;

                if (!await _reader.ReadAsync())
                    return null;

                var record = new DbRecord();
                for (int i = 0; i < _reader.FieldCount; i++)
                {
                    record.Fields[_reader.GetName(i)] = await _reader.IsDBNullAsync(i)
                        ? null
                        : _reader.GetValue(i);
                }

                return record;
            }

            protected override async Task CloseAsync()
            {
                if (_reader != null)
                {
                    await _reader.DisposeAsync();
                    _reader = null;
                }
            }
        }
}
