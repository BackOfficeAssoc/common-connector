using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Syniti;
using Syniti.Clients.DataQuality;
using Syniti.Common.Caching;
using Syniti.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MetadataScanner.Services.Connections
{
    public class ConnectionInfo
    {
        public CredentialDto Credential { get; set; }

        public ConnectionInfo(CredentialDto credential)
        {
            Credential = credential;
        }
    }

    public interface IConnectionInfoService
    {
        public Task<ConnectionInfo> Retrieve(SkpAssetId connectionId, CancellationToken cancellationToken);

        public Task<string> RetrieveDbType(SkpAssetId connectionId, CancellationToken cancellationToken);

        public Task<SkpAssetId> RetrieveTenant(SkpAssetId connectionId, CancellationToken cancellationToken);

        public Task<SkpAssetId> RetrieveSystem(SkpAssetId connectionId, CancellationToken cancellationToken);
    }

    public class ConnectionInfoService : IConnectionInfoService
    {
        protected IDataQualityClientFactory ClientFactory { get; }

        protected IMemoryCache Cache { get; }

        public ConnectionInfoService(IDataQualityClientFactory clientFactory, IMemoryCache cache)
        {
            ClientFactory = clientFactory;
            Cache = cache;
        }

        public async Task<ConnectionInfo> Retrieve(SkpAssetId connectionId, CancellationToken cancellationToken)
        {
            var client = this.ClientFactory.Create();

            var credentialDto = await client.FetchCredentialAsync(connectionId, cancellationToken);

            return new ConnectionInfo(credentialDto)
            {
            };
        }

        public async Task<string> RetrieveDbType(SkpAssetId connectionId, CancellationToken cancellationToken)
        {
            var dbType = await this.Cache.MakeCodeStore<string, string>()
                .WithExpirationTime(TimeSpan.FromMinutes(5))
                .WithValueFactory(async (id, ct) =>
                {
                    return (await this.Retrieve(connectionId, cancellationToken)).Credential.Type;
                })
                .GetOrCreateResult(connectionId, cancellationToken);

            return dbType!;
        }

        public async Task<SkpAssetId> RetrieveTenant(SkpAssetId connectionId, CancellationToken cancellationToken)
        {
            string? tenantId = await this.Cache.MakeCodeStore<string, string>()
                .WithExpirationTime(TimeSpan.FromMinutes(5))
                .WithValueFactory(async (id, ct) =>
                {
                    return (await this.Retrieve(connectionId, cancellationToken)).Credential.Tenant_id;
                })
                .GetOrCreateResult(connectionId, cancellationToken);

            return SkpAssetId.Parse(tenantId!);
        }

        public async Task<SkpAssetId> RetrieveSystem(SkpAssetId connectionId, CancellationToken cancellationToken)
        {
            var systemId = await this.Cache.MakeCodeStore<string, string>()
                .WithExpirationTime(TimeSpan.FromMinutes(30))
                .WithValueFactory(async (id, ct) =>
                {
                    return (await this.Retrieve(connectionId, cancellationToken)).Credential.System_id;
                })
                .GetOrCreateResult(connectionId, cancellationToken);

            return (SkpAssetId)systemId!;
        }

        /// <summary>
        /// Ensure the memory cache will be available, this should be called by convention at startup
        /// </summary>
        private static void RegisterMemoryCache(IServiceCollection services)
        {
            services.AddMemoryCache(config =>
            {
            });
        }
    }
}
