namespace Calculator.Operations.SimpleArithmeticOperations
{
    public class
        SimpleSubtractionOperator : SimpleArithmeticOperator
    {
        public override OperationType Type => OperationType.SimpleSubtraction;

        protected override IOperationResult InternalOperation(SimpleArithmeticOperation operation)
        {
            return new SimpleArithmeticOperationResult(operation.FirstNumber - operation.SecondNumber);
        }
    }
}