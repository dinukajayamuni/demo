using System.Collections.Generic;
using System.Linq;

namespace Calculator.Operations
{
    public class OperatorFactory : IOperatorFactory
    {
        private readonly IEnumerable<IOperator> _operators;

        public OperatorFactory(IEnumerable<IOperator> operators)
        {
            _operators = operators;
        }

        public IOperator Create(OperationType operationType)
        {
            return _operators.FirstOrDefault(o => o.Type == operationType);
        }
    }
}