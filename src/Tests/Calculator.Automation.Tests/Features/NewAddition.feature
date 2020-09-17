Feature: New Addition
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@tc:25
@mytag
Scenario: Add two numbers
	Given the first number is <firstNumber>
	And the second number is <secondNumber>
	When the two numbers are added
	Then the result should be <result>

	Examples:
		| firstNumber | secondNumber | result |
		| 50.0        | 70.0         | 120.0  |
		| 20.0        | 20.0         | 40.0   |