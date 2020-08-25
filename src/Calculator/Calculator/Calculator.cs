using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Calculator.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Calculator
{
    /// <summary>
    /// Calculator function
    /// </summary>
    public class Calculator
    {
        private readonly IOperatorFactory _operatorFactory;

        public Calculator(IOperatorFactory operatorFactory)
        {
            _operatorFactory = operatorFactory;
        }

        /// <summary>
        /// Calculator function
        /// </summary>
        [FunctionName("Calculator")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage request)
        {
            var operation = await request.Content.ReadAsAsync<IOperation>(new[] {new JsonMediaTypeFormatter {
                    SerializerSettings = new JsonSerializerSettings {
                        Converters = new List<JsonConverter> {
                            new OperationConverter(_operatorFactory)
                        },
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        NullValueHandling = NullValueHandling.Ignore
                    }
                }
            });

            var @operator = _operatorFactory.Create(operation.Type);
            if (@operator == null)
            {
                return new BadRequestObjectResult("Invalid operation");
            }

            var operationResult = @operator.Operate(operation);

            return new OkObjectResult(operationResult);
        }
    }
}
