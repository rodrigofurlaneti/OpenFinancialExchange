Feature: Transaction Category Management

  Scenario: Successfully create a transaction category
    Given no transaction category exists with code "SALARY"
    When I create a transaction category with code "SALARY", description "Monthly Salary", operationType "CREDIT_RECEIPT", and accountingNature "REVENUE"
    Then the transaction category creation should succeed

  Scenario: Reject duplicate code
    Given a transaction category already exists with code "SALARY"
    When I create a transaction category with code "SALARY", description "Monthly Salary", operationType "CREDIT_RECEIPT", and accountingNature "REVENUE"
    Then the transaction category creation should fail with error "General.Conflict"

  Scenario: Reject invalid accounting nature
    Given no transaction category exists with code "BONUS"
    When I create a transaction category with code "BONUS", description "Annual Bonus", operationType "CREDIT_RECEIPT", and accountingNature "INVALID_NATURE"
    Then the transaction category creation should fail with error "TransactionCategory.InvalidNature"

  Scenario: Successfully update a category
    Given an existing transaction category with id 1 and description "Old Description"
    When I update the transaction category with id 1 to description "New Description", operationType "DEBIT_PAYMENT", and accountingNature "EXPENSE"
    Then the transaction category update should succeed

  Scenario: Delete returns not found when category does not exist
    Given no transaction category exists with id 99
    When I delete the transaction category with id 99
    Then the transaction category deletion should fail with error "General.NotFound"
