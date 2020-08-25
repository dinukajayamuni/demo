namespace Calculator.Operations.SimpleArithmeticOperations
{
    public class
        SimpleAdditionOperator : SimpleArithmeticOperator
    {
        public override OperationType Type => OperationType.SimpleAddition;

        protected override IOperationResult InternalOperation(SimpleArithmeticOperation operation)
        {
            return new SimpleArithmeticOperationResult(operation.FirstNumber + operation.SecondNumber);
        }
    }
}