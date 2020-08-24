namespace Calculator.Operations
{
    public class Operation : IOperation
    {
        public OperationType Type { get; }

        public Operation(OperationType type)
        {
            Type = type;
        }
    }
}