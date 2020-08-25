namespace Calculator.Operations.SimpleArithmeticOperations
{
    public class SimpleArithmeticOperation : Operation
    {
        public decimal FirstNumber { get; set; }
        public decimal SecondNumber { get; set; }

        public SimpleArithmeticOperation(OperationType type) : base(type)
        {
        }
    }
}