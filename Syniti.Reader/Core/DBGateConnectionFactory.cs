using System;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Libraries.DBGate;
using MetadataScanner.Services.Connections;
using MetadataScanner.Services.DataCommander;
using Syniti;

namespace MetadataScanner.Services.DbMoto.MetadataService
{
    public interface IDBGateConnectionFactory
    {
        /// <summary>
        /// Returns a new DBGate IConnection
        /// </summary>
        Task<IConnection> GetIConn(SkpAssetId connectionId, CancellationToken token);
    }

    public class DBGateConnectionFactory : IDBGateConnectionFactory
    {
        protected IConnectionInfoService ConnectionInfoService { get; }

        public IDbConnectionFactory DbConnectionFactory { get; set; }

        public DBGateConnectionFactory(IConnectionInfoService connectionInfoService, IDbConnectionFactory dbConnectionFactory)
        {
            ConnectionInfoService = connectionInfoService;
            DbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IConnection> GetIConn(SkpAssetId connectionId, CancellationToken token)
        {
            IConnection? iConnection = null;
            bool bUseDataCommander = true;

            if (bUseDataCommander)
            {
                var dbConnection = DbConnectionFactory.GetIDbConnection(connectionId);
                var connectionType = await ConnectionInfoService.RetrieveDbType(connectionId, token);
                iConnection = Util.GetConnection(skpType: connectionType, dbConnection, null);
            }
            else
            {
                // NOTE: for local debugging and direct connectivity
                // var connectionType = "postgresql";
                // string connectionString = "Server=10.2.3.193;User ID=postgres;Password=mypassword;Database=postgres;Timeout=90;"; // QA
                // string connectionString = "Host=localhost;Database=postgres;Username=postgres;Password=mypassword"; // local
                // var connectionType = "sql-server";
                // string connectionString = "Data Source=172.31.17.234;User ID=boaqe;Password=mypassword;Enlist=False;Encrypt=false;Command Timeout=120;";
                //     //  set   bool bUseDataCommander = false;
                //     var connectionType = "sap-hana";
                //     string connectionString = "Server=54.161.147.160:30215;UserID=SAPHANADB;Password=Dmrsap#1;";
                //     iConnection = Util.GetConnection(skpType: connectionType, null, null);
                //     iConnection.ConnectionString = connectionString;
                //  // probably I need to specify a full path for loading the driver
                //                 iConnection.ProviderClass.assemblyPath = ....;

                // var connectionType = "sap-hana";
                // string connectionString = "Server=fbb784c3-63f1-4f21-b940-54915053b634.hana.prod-us10.hanacloud.ondemand.com:443;UserID=user;Password=password;";
            }

            return iConnection!;
        }

        public async Task<bool> TestConnection(SkpAssetId connectionId, CancellationToken cancellationToken)
        {
            IConnection? iConnection = null;

            // test the connection
            try
            {
                iConnection = await GetIConn(connectionId, cancellationToken);

                iConnection.Open();
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                try
                {
                    if (iConnection != null)
                    {
                        iConnection.Close();
                    }
                }
                catch { }
            }

            return true;
        }
    }
}
