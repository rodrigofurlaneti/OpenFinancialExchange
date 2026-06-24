Feature: Financial Institution Management
    As a system user
    I want to manage financial institutions
    So that I can track bank accounts and OFX imports

Scenario: Create a valid financial institution
    Given I have a financial institution command with BankId "237" and OrgName "Bradesco"
    When I send the create command
    Then the result should be successful
    And the institution should be saved

Scenario: Reject duplicate financial institution
    Given a financial institution with BankId "237" already exists
    When I try to create another institution with BankId "237"
    Then the result should fail
    And the error code should be "FinancialInstitution.AlreadyExists"

Scenario: Reject institution with empty BankId
    Given I have a financial institution command with BankId "" and OrgName "Test"
    When I send the create command
    Then the result should fail
