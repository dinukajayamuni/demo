namespace Calculator.Operations.SimpleArithmeticOperations
{
    public class SimpleArithmeticOperationResult : IOperationResult
    {
        public decimal Result { get; }

        public SimpleArithmeticOperationResult(decimal result)
        {
            Result = result;
        }
    }
}