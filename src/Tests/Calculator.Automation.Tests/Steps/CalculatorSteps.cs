using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Xunit;

namespace Calculator.Automation.Tests.Steps
{
    [Binding]
    public class CalculatorSteps
    {
        private const string Result = nameof(Result);
        private const string FirstNumber = nameof(FirstNumber);
        private const string SecondNumber = nameof(SecondNumber);

        private readonly HttpClient _httpClient;
        private readonly ScenarioContext _context;

        public CalculatorSteps(ScenarioContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient("Calculator");
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

        [Then(@"the result should be (.*)")]
        public void ThenTheResultShouldBe(decimal result)
        {
            Assert.Equal(_context.Get<decimal>(Result), result);
        }

        private async Task ExecuteOperation(OperationType operationType)
        {
            var operation = new SimpleArithmeticOperation
            {
                FirstNumber = _context.Get<decimal>(FirstNumber),
                SecondNumber = _context.Get<decimal>(SecondNumber),
                Type = operationType
            };
            var response = await _httpClient.PostAsync("/api/calculator",
                new StringContent(
                    JsonSerializer.Serialize(operation,
                        options: new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            Converters = { new JsonStringEnumConverter() }
                        }),
                    Encoding.UTF8, "application/json"));

            Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());

            var result = await response.Content.ReadAsAsync<SimpleArithmeticOperationResult>();
            _context.Add(Result, result.Result);
        }
    }
}
