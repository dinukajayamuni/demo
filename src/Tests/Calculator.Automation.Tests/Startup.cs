using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using SolidToken.SpecFlow.DependencyInjection;
using TechTalk.SpecFlow;
using Xunit.Abstractions;

namespace Calculator.Automation.Tests
{
    public class Startup
    {
        [ScenarioDependencies]
        public static IServiceCollection CreateServices()
        {
            var services = new ServiceCollection();

            foreach (var type in typeof(Startup).Assembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(BindingAttribute))))
            {
                services.AddSingleton(type);
            }

            InternalConfigureServices(services);
            return services;
        }

        private static void InternalConfigureServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
            };

            var configurationBuilder = new ConfigurationBuilder();

            services.AddSingleton<ILogger>(provider =>
            {
                var context = provider.GetService<ScenarioContext>();
                var output = context.ScenarioContainer.Resolve<ITestOutputHelper>();
                return new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.TestOutput(output)
                    .CreateLogger();
            });

            configurationBuilder
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var config = configurationBuilder.Build();
            


            services.AddSingleton<IConfiguration>(config);

            services.AddHttpClient("Calculator", (provider, client) =>
            {
                var configuration = provider.GetService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["CalculatorUrl"]);
            });
        }
    }
}
