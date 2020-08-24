using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Calculator.Operations
{
    public class OperationConverter : JsonConverter
    {
        private readonly IOperatorFactory _operatorFactory;

        public OperationConverter(IOperatorFactory operatorFactory)
        {
            _operatorFactory = operatorFactory;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IOperation).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var readerJObject = JObject.Load(reader);
            if (readerJObject == null)
                throw new JsonSerializationException("Cannot deserialize, not a valid operation Json.");

            var type = readerJObject["type"].ToString();
            if (!Enum.TryParse(type, true, out OperationType operationType))
                throw new JsonSerializationException("Cannot deserialize, not a valid operation type " +
                                                     $"'{type}'.");

            var @operator = _operatorFactory.Create(operationType); 

            var operation = @operator.CreateOperation();
            
            serializer.Populate(readerJObject.CreateReader(), operation);
            return operation;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}