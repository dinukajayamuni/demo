using System;

namespace Calculator.Operations.SimpleArithmeticOperations
{
    public abstract class SimpleArithmeticOperator : IOperator
    {
        public abstract OperationType Type { get; }

        public IOperationResult Operate(IOperation operation)
        {
            if (!(operation is SimpleArithmeticOperation simpleArithmeticOperation))
            {
                throw new ArgumentNullException(nameof(simpleArithmeticOperation));
            }
            return InternalOperation(simpleArithmeticOperation);
        }

        public IOperation CreateOperation()
        {
            return new SimpleArithmeticOperation(Type);
        }

        protected abstract IOperationResult InternalOperation(SimpleArithmeticOperation operation);
    }
}