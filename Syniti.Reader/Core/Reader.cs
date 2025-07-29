using ReaderLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleDbReader.Core
{
    public abstract class Reader
    {
        public ReaderState State { get; private set; } = ReaderState.Ready;

        public void SetState(ReaderState newState) => State = newState;

        public async Task<bool> OpenProcessRecordsAsync()
        {
            SetState(ReaderState.Running);
            var openResult = await OpenAsync();
            return openResult >= 0;
        }

        public async Task<bool> ProcessRecordsAsync(int maxRecords)
        {
            for (int i = 0; i < maxRecords; i++)
            {
                var record = await GetNextRecordAsync();
                if (record == null)
                    return false;

                await ProcessRecordAsync(record);
            }

            return true;
        }

        public async Task CloseProcessRecordsAsync()
        {
            await CloseAsync();
            SetState(ReaderState.Stopped);
        }

        protected abstract Task<int> OpenAsync();
        protected abstract Task CloseAsync();
        protected abstract Task<DbRecord?> GetNextRecordAsync();
        protected virtual Task ProcessRecordAsync(DbRecord record)
        {
            foreach (var field in record.Fields)
                System.Console.WriteLine($"{field.Key}: {field.Value}");
            return Task.CompletedTask;
        }
    }

    public enum ReaderState
    {
        Ready,
        Running,
        Stopped
    }
}