using System;

namespace Calculator.Operations.SimpleArithmeticOperations
{
    public class
        SimpleExponentialOperator : SimpleArithmeticOperator
    {
        public override OperationType Type => OperationType.SimpleExponential;

        protected override IOperationResult InternalOperation(SimpleArithmeticOperation operation)
        {
            return new SimpleArithmeticOperationResult((decimal)Math.Pow((double)operation.FirstNumber, (double)operation.SecondNumber));
        }
    }
}