Feature: Transaction Management

  Scenario: Successfully create a transaction
    Given no transaction exists with FITID "N205C4:13/02/26:733.42:1300449"
    When I create a transaction with FITID "N205C4:13/02/26:733.42:1300449", amount 733.42, type "CREDIT", and statementId 1
    Then the transaction creation should succeed

  Scenario: Reject duplicate FITID
    Given a transaction already exists with FITID "N205C4:13/02/26:733.42:1300449"
    When I create a transaction with FITID "N205C4:13/02/26:733.42:1300449", amount 733.42, type "CREDIT", and statementId 1
    Then the transaction creation should fail with error "General.Conflict"

  Scenario: Successfully reconcile a transaction
    Given an existing unreconciled transaction with id 1
    When I reconcile the transaction with id 1
    Then the reconciliation should succeed

  Scenario: Reject reconciling an already reconciled transaction
    Given an existing already reconciled transaction with id 2
    When I reconcile the transaction with id 2
    Then the reconciliation should fail with error "Transaction.AlreadyReconciled"

  Scenario: Get transactions by date range returns results
    Given transactions exist between "2026-02-13" and "2026-03-31"
    When I query transactions from "2026-02-13" to "2026-03-31"
    Then the date range query should return results
