Feature: Addition
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@tc:25
@mytag
Scenario: Add two numbers
	Given the first number is 50.0
	And the second number is 70.0
	When the two numbers are added
	Then the result should be 120.0