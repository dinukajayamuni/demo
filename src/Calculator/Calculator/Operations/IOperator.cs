namespace Calculator.Operations
{
    public interface IOperator
    {
        OperationType Type { get; }

        IOperationResult Operate(IOperation operation);

        IOperation CreateOperation();
    }
}