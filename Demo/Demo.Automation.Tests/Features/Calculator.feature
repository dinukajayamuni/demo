Feature: Calculator
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@tc:9
@mytag
Scenario: Add two numbers
	Given the first number is 50
	And the second number is 70
	When the two numbers are added
	Then the result should be 120