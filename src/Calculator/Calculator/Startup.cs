using Calculator;
using Calculator.Operations;
using Calculator.Operations.SimpleArithmeticOperations;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Calculator
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services);
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IOperatorFactory, OperatorFactory>();
            services.AddScoped<IOperator, SimpleAdditionOperator>();
            services.AddScoped<IOperator, SimpleSubtractionOperator>();
            services.AddScoped<IOperator, SimpleMultiplicationOperator>();
            services.AddScoped<IOperator, SimpleDivisionOperator>();
            services.AddScoped<IOperator, SimpleExponentialOperator>();
        }
    }
}
