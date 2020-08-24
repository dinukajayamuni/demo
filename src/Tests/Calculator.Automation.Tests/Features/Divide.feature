Feature: Divide
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the divition of two numbers

@tc:19
@mytag
Scenario: Divide two numbers
	Given the first number is 50
	And the second number is 10
	When the first number is divided by the second
	Then the result should be 5