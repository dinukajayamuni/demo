namespace Calculator.Operations
{
    public interface IOperatorFactory
    {
        IOperator Create(OperationType operationType);
    }
}