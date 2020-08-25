Feature: Subtraction
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the difference of two numbers

@tc:24
@mytag
Scenario: Subtract a number from another
	Given the first number is 70
	And the second number is 50
	When the first number is subtracted from the second
	Then the result should be 20