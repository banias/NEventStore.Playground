using System;
using System.Configuration;
using System.Web.Http;
using Infrastructure.Host.Projections;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json.Serialization;
using NEventStore;
using NEventStore.Persistence.Sql;
using NEventStore.Persistence.Sql.SqlDialects;
using Owin;

namespace Infrastructure.Host
{
    class Program
    {
        private static InvoicesProjection _invoicesProjection;
        public static IStoreEvents StoreEvents { get; set; }

        public static string ConnectionString { get; private set; }

        static void Main(string[] args)
        {

            ConnectionString = ConfigurationManager.AppSettings["connectionString"];
            var config = new ConfigurationConnectionFactory("NEventStoreProc", "system.data.sqlclient", ConnectionString);

            InvoiceProjectionSetup.CreateSchemaIfNotExists();

           StoreEvents = Wireup.Init()
                .UsingSqlPersistence((config))
                .WithDialect(new MsSqlDialect())
                .EnlistInAmbientTransaction()
                .InitializeStorageEngine()
                .UsingJsonSerialization()

                .Build();

            _invoicesProjection = new InvoicesProjection();
            _invoicesProjection.Init();

            var baseAddress = "http://localhost:9000/";

            // Start OWIN host 
            using (WebApp.Start(baseAddress, Configuration))
            {
                Console.WriteLine("Host started");
                Console.ReadLine();
            }
            _invoicesProjection.Dispose();
        }

        public static void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            appBuilder.UseWebApi(config);
        }
    }
}
