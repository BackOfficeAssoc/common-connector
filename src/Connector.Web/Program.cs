using Syniti.Common.Injection;
using Syniti.Hosting.Authentication;
using Syniti.Hosting.ChaosMuskie;
using Syniti.Hosting.Configuration;
using Syniti.Hosting.Injection;
using Syniti.Hosting.Logging;
using Syniti.Hosting.Mvc;
using Syniti.Hosting.Mvc.Health;

namespace WebAppRunner
{
    /// <summary>
    /// This project is meant to be a minimal way for you to run code in the framework directly for development/testing purposes instead of referencing from a nuget package.
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // create lightweight server which at least let's k8s know we are running, in case startup takes a long time.
            // totally optional behavior but supports best practices for k8s.
            using var livenessCheckServer = LivenessCheckBootstrapper.CreateTemporaryLivenessCheckServer(args);

            var builder = WebApplication.CreateBuilder(args);

            // if you call this here, it will log when the application starts, stops, and is disposed.
            // note you'll primary events either way as part of calling AddSynitiLogging(), but this will be more verbose/explicit, logging the finalization of this method.
            using var lifetimeMonitor = builder.AddSynitiApplicationLifetimeMonitoring(usesDisposableResult: true);

            // Adds standard syniti configuration options to the project.
            builder.AddSynitiConfiguration((opts) =>
            {
                // flip this to true to enable runtime configuration
                opts.RuntimeConfigOptions.RuntimeConfigEnabled = true;
            });

            // set up standard syniti microservice logging
            builder.AddSynitiLogging();

            // sets up standard syniti dependency-injection
            // and automatically registers various services into the IOC container
            builder.AddSynitiInjection();

            // sets up async message passing behavior
            // builder.AddSynitiMessaging();

            // sets up standard mvc services.
            builder.AddSynitiMvc();

            builder.AddSynitiChaosMuskie();

            // adds config & services for open-api / swagger definitions to be generated from this source code at runtime.
            builder.AddSynitiSwagger();

            // adds needed config for open-telemetry data to be made available to prometheus at the /metrics endpoint
            builder.AddSynitiTelemetry();

            // adds needed services for health checks to be made available at /health endpoint.
            builder.AddSynitiHealthCheck();

            // adds needed services to authenticate incoming requests via qzar issued jwt tokens.
            builder.AddSynitiAuthentication();

            // build the app, but invoke any needed hooks before or after the build.
            var app = builder.BuildWithSynitiHooks();

            // test that we are actually able to create the controllers and services
            // we'll need for the application to run correctly.
            app.AssertAllSynitiServicesAvailable(opts =>
            {
                // for our purposes, not going to actually verify any types
                opts.TypePredicate = (t) => false;
            });

            // set up the incoming request pipeline
            // using standard syniti best practices.
            app.UseSynitiRequestPipeline(options =>
            {
                options.UseStarfishSessionMiddleware = true;
            });

            // kill the bootstrap server to free port 8080.
            livenessCheckServer.Dispose();

            // start the application
            await app.RunAsync();
        }

        /// <summary>
        /// Create reference to types that otherwise may not get loaded into referenced assembly tree
        /// see: https://github.com/dotnet/runtime/issues/57714
        /// </summary>
        public static void CreateConcreteReferences()
        {
            List<Type> neededTypes = new List<Type>()
            {
                typeof(Syniti.Clients.QzarMint.OutboundJwtSecurityTokenAccessor),
                typeof(Syniti.Clients.QzarAuthorize.IQzarAuthorizeClient),
                typeof(Syniti.Clients.Starfish.IStarfishClient),
            };
        }
    }
}
