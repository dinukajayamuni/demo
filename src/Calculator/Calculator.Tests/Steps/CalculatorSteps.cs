using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Calculator.Operations;
using Calculator.Operations.SimpleArithmeticOperations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechTalk.SpecFlow;
using Xunit;

namespace Calculator.Tests.Steps
{
    public abstract class HttpFunctionAppFixture<T>
    {
        private readonly IDictionary<string, string> _appSettings;
        public T FunctionInstance { get; private set; }

        protected readonly ServiceCollection Services;

        protected HttpFunctionAppFixture(Action<ServiceCollection> configureDependencies, IDictionary<string, string> appSettings)
        {
            _appSettings = appSettings;
            Services = new ServiceCollection();
            Services.AddTransient(typeof(T));
            Services.AddLogging();
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(_appSettings);
            var config = configBuilder.Build();
            Services.AddSingleton<IConfiguration>(config);

            foreach (var startupType in typeof(T).Assembly.GetTypes())
            {
                if (startupType.IsSubclassOf(typeof(FunctionsStartup)))
                {
                    var methods = startupType.GetMethods(BindingFlags.Public | BindingFlags.Static);
                    foreach (var method in methods)
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 1 &&
                            parameters.First().ParameterType == typeof(IServiceCollection) ||
                            parameters.First().ParameterType == typeof(ServiceCollection))
                        {
                            method.Invoke(null, new object[] { Services });
                        }
                    }
                }
            }

            configureDependencies?.Invoke(Services);
        }

        public Task StartAsync(Action<IDictionary<string, string>> configure = null, Action<IServiceCollection> configureServices = null)
        {
            configure?.Invoke(_appSettings);
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(_appSettings);
            var config = configBuilder.Build();
            Services.AddSingleton<IConfiguration>(config);
            configureServices?.Invoke(Services);
            var provider = Services.BuildServiceProvider();
            FunctionInstance = (T)provider.GetService(typeof(T));
            return Task.CompletedTask;
        }
    }

    public class CalculatorFixture : HttpFunctionAppFixture<Calculator>
    {
        public CalculatorFixture(Action<ServiceCollection> configureDependencies,
            IDictionary<string, string> appSettings) : base(configureDependencies, appSettings)
        {
        }
    }

    [Binding]
    public class CalculatorSteps
    {
        private const string Result = nameof(Result);
        private const string FirstNumber = nameof(FirstNumber);
        private const string SecondNumber = nameof(SecondNumber);

        private readonly ScenarioContext _context;
        private readonly CalculatorFixture _fixture;

        public CalculatorSteps(ScenarioContext context, CalculatorFixture fixture)
        {
            _context = context;
            _fixture = fixture;
        }

        [Given(@"the first number is (.*)")]
        public void GivenTheFirstNumberIs(decimal firstNumber)
        {
            _context.Add(FirstNumber, firstNumber);
        }

        [Given(@"the second number is (.*)")]
        public void GivenTheSecondNumberIs(decimal secondNumber)
        {
            _context.Add(SecondNumber, secondNumber);
        }

        [When(@"the two numbers are added")]
        public async Task WhenTheTwoNumbersAreAdded()
        {
            await ExecuteOperation(OperationType.SimpleAddition);
        }

        [When(@"the first number is subtracted from the second")]
        public async Task WhenTheFirstNumberIsSubtractedFromTheSecond()
        {
            await ExecuteOperation(OperationType.SimpleSubtraction);
        }

        [When(@"the first number is multiplied by the second")]
        public async Task WhenTheFirstNumberIsMultipliedByTheSecond()
        {
            await ExecuteOperation(OperationType.SimpleMultiplication);
        }

        [When(@"the first number is divided by the second")]
        public async Task WhenTheFirstNumberIsDividedByTheSecond()
        {
            await ExecuteOperation(OperationType.SimpleDivision);
        }

        [When(@"the first number is to the power of second number")]
        public async Task WhenTheFirstNumberIsToThePowerOfSecondNumber()
        {
            await ExecuteOperation(OperationType.SimpleExponential);
        }

        [Then(@"the result should be (.*)")]
        public void ThenTheResultShouldBe(decimal result)
        {
            Assert.Equal(_context.Get<decimal>(Result), result);
        }

        private async Task ExecuteOperation(OperationType operationType)
        {
            await _fixture.StartAsync();
            var operation = new SimpleArithmeticOperation(operationType)
            {
                FirstNumber = _context.Get<decimal>(FirstNumber),
                SecondNumber = _context.Get<decimal>(SecondNumber),
            };
            
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/calculator"){Content = new StringContent(
                JsonSerializer.Serialize(operation,
                    options: new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        Converters = { new JsonStringEnumConverter() }
                    }),
                Encoding.UTF8, "application/json")
            };
            var response = await _fixture.FunctionInstance.Run(request);
            var successfulResponse = response as OkObjectResult;
            Assert.NotNull(successfulResponse);

            var result = successfulResponse.Value as SimpleArithmeticOperationResult;
            Assert.NotNull(result);

            _context.Add(Result, result.Result);
        }
    }
}
