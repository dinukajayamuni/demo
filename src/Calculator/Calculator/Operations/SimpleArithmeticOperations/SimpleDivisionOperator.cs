namespace Calculator.Operations.SimpleArithmeticOperations
{
    public class
        SimpleDivisionOperator : SimpleArithmeticOperator
    {
        public override OperationType Type => OperationType.SimpleDivision;

        protected override IOperationResult InternalOperation(SimpleArithmeticOperation operation)
        {
            return new SimpleArithmeticOperationResult(operation.FirstNumber / operation.SecondNumber);
        }
    }
}