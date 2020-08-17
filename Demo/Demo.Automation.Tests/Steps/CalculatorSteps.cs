using TechTalk.SpecFlow;

namespace Demo.Automation.Tests.Steps
{
    [Binding]
    public class CalculatorSteps
    {
        [Given(@"the first number is (.*)")]
        public void GivenTheFirstNumberIs(int p0)
        {
        }

        [Given(@"the second number is (.*)")]
        public void GivenTheSecondNumberIs(int p0)
        {
        }

        [When(@"the two numbers are added")]
        public void WhenTheTwoNumbersAreAdded()
        {
        }

        [When(@"the first number is subtracted from the second")]
        public void WhenTheFirstNumberIsSubtractedFromTheSecond()
        {
        }

        [When(@"the first number is multiplied by the second")]
        public void WhenTheFirstNumberIsMultipliedByTheSecond()
        {
        }

        [Then(@"the result should be (.*)")]
        public void ThenTheResultShouldBe(int p0)
        {
        }
    }
}
