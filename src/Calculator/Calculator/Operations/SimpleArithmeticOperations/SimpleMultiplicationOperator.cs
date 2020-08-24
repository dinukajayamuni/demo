namespace Calculator.Operations.SimpleArithmeticOperations
{
    public class
        SimpleMultiplicationOperator : SimpleArithmeticOperator
    {
        public override OperationType Type => OperationType.SimpleMultiplication;
        
        protected override IOperationResult InternalOperation(SimpleArithmeticOperation operation)
        {
            return new SimpleArithmeticOperationResult(operation.FirstNumber * operation.SecondNumber);
        }
    }
}