using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Calculator.Tests.Steps;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using SolidToken.SpecFlow.DependencyInjection;
using TechTalk.SpecFlow;
using Xunit.Abstractions;

namespace Calculator.Tests
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
            services.AddTransient(provider =>
                new CalculatorFixture(null, new Dictionary<string, string> {{"CalculatorUrl", "https://test.com"}}));
        }
    }
}
