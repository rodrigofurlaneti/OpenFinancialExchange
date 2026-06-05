Feature: Bank Management

  Scenario: Successfully create a bank
    Given no bank exists with COMPE code "0237"
    When I create a bank with COMPE code "0237", name "Banco Bradesco S.A.", and ISPB "60746948"
    Then the bank creation should succeed

  Scenario: Reject duplicate COMPE code
    Given a bank already exists with COMPE code "0237"
    When I create a bank with COMPE code "0237", name "Banco Bradesco S.A.", and ISPB "60746948"
    Then the bank creation should fail with error "General.Conflict"

  Scenario: Successfully update bank name
    Given an existing bank with id 1 and name "Old Bank Name"
    When I update the bank with id 1 to name "New Bank Name" and ISPB "12345678"
    Then the bank update should succeed

  Scenario: Get bank by id returns not found when bank does not exist
    Given no bank exists with id 999
    When I get the bank with id 999
    Then the bank query should fail with error "General.NotFound"

  Scenario: Delete bank returns not found when bank does not exist
    Given no bank exists with id 888
    When I delete the bank with id 888
    Then the bank deletion should fail with error "General.NotFound"
