using System;
using System.Collections.Generic;
using DataAccess.Libraries.DBGate;
using ReaderLibrary.Interfaces;

namespace ReaderLibrary.Concrete
{
    public class CommanderRecordReader : IGenericRecordReader
    {
        private readonly IConnection _connection;
        private readonly string _query;
        private IResultSet? _reader;

        public CommanderRecordReader(IConnection connection, string query)
        {
            _connection = connection;
            _query = query;
        }

        public void Open()
        {
            if (!_connection.IsOpened)
            {
                _connection.Open();
            }

            var command = _connection.CreateCommand();
            command.CommandText = _query;
            _reader = command.ExecuteQuery(); // Now correctly assigned as IResultSet
        }

        public bool Next()
        {
            return _reader?.Next() ?? false;
        }

        public Dictionary<string, object?> GetRecord()
        {
            if (_reader == null)
                throw new InvalidOperationException("Reader not open");

            var record = new Dictionary<string, object?>();

            for (int i = 0; i < _reader.FieldCount; i++)
            {
                var colName = _reader.getField(i).Name;
                object? value = _reader.IsNull(i) ? null : _reader.GetValue(i);
                record[colName] = value;
            }

            return record;
        }

        public void Close()
        {
            _reader?.Close();
            _reader?.Dispose();
            _reader = null;
        }

        public void Dispose()
        {
            Close();
        }
    }
}
