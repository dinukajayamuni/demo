Feature: Multiplication
	In order to avoid silly mistakes
	As a math idiot
	I want to be told multiplication of two numbers

@tc:13
@mytag
Scenario: Multiply two numbers
	Given the first number is 3
	And the second number is 2
	When the first number is multiplied by the second
	Then the result should be 6