using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Syniti.Clients;
using Syniti.Common.Injection;
using Syniti.PidginClient;
using System;
using System.Data;
using System.Net.Http;

namespace MetadataScanner.Services.DataCommander
{
    public interface IDbConnectionFactory
    {
        IDbConnection GetIDbConnection(string connectionId);
    }

    [ServiceImplementation]
    public class DbConnectionFactory : IDbConnectionFactory
    {
        protected IOptions<DataCommanderOptions> Options { get; set; }

        public IServiceProvider ServiceProvider { get; set; }

        public DbConnectionFactory(IOptions<DataCommanderOptions> options, IServiceProvider serviceProvider)
        {
            Options = options;
            ServiceProvider = serviceProvider;
        }

        public IDbConnection GetIDbConnection(string connectionId)
        {
            var factory = ServiceProvider.GetRequiredService<IHttpClientFactory>();

            PidginConnection.SharedHttpClient = factory.GetSynitiHttpClient();
            PidginConnection.LoggerFactory = ServiceProvider.GetService<ILoggerFactory>() ?? PidginConnection.LoggerFactory;

            PidginConnectionStringBuilder sb = new PidginConnectionStringBuilder()
            {
                DataCommanderUrl = "https://data-commander.mgmt.syniti-dev.com",
                ConnectionId = connectionId,
                SharedSecret = "THIS_WILL_BE_A_CLOSELY_GUARDED_SECRET_IN_PROD__ONLY_PLAINTEXT_FOR_DEMO_PURPOSES",
                RemoteServerConnectionTimeout = 10,
                ConnectionTimeout = 60,
            };

            PidginConnection connection = new PidginConnection()
            {
                ConnectionString = sb.ConnectionString,
            };

            return connection;
        }
    }
}
