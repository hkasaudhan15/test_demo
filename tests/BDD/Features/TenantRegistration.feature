Feature: Tenant Registration
    As a platform administrator
    I want to register new tenants
    So that organizations can use the platform

    Background:
        Given the tenant repository is empty

    @registration @happy-path
    Scenario: Successfully register a new tenant with default tier
        Given I have tenant registration details:
            | Field        | Value              |
            | Identifier   | acme-corp          |
            | Name         | Acme Corporation   |
            | ContactEmail | admin@acme.com     |
            | AdminName    | John Smith         |
            | Description  | Primary account    |
        When I register the tenant
        Then the tenant should be created successfully
        And the tenant identifier should be "acme-corp"
        And the tenant name should be "Acme Corporation"
        And the tenant subscription tier should be "Free"
        And the tenant status should be "Active"
        And a TenantRegistered domain event should be raised
        And a TenantActivated domain event should be raised

    @registration @happy-path
    Scenario: Register a tenant with a specific subscription tier
        Given I have tenant registration details:
            | Field        | Value              |
            | Identifier   | enterprise-co      |
            | Name         | Enterprise Co      |
            | ContactEmail | admin@enterprise.com |
        And the subscription tier is "Enterprise"
        When I register the tenant
        Then the tenant should be created successfully
        And the tenant subscription tier should be "Enterprise"
        And the tenant max users should be unlimited

    @registration @happy-path
    Scenario: Register a tenant without auto-activation
        Given I have tenant registration details:
            | Field        | Value              |
            | Identifier   | pending-org        |
            | Name         | Pending Org        |
            | ContactEmail | admin@pending.org  |
        And auto-activation is disabled
        When I register the tenant
        Then the tenant should be created successfully
        And the tenant status should be "Provisioning"
        And only a TenantRegistered domain event should be raised

    @registration @validation
    Scenario: Reject registration with duplicate identifier
        Given a tenant with identifier "acme-corp" already exists
        And I have tenant registration details:
            | Field        | Value              |
            | Identifier   | acme-corp          |
            | Name         | Another Acme       |
            | ContactEmail | other@acme.com     |
        When I register the tenant
        Then the registration should fail with error "Tenant.DuplicateIdentifier"

    @registration @validation
    Scenario: Reject registration with empty identifier
        Given I have tenant registration details:
            | Field        | Value              |
            | Identifier   |                    |
            | Name         | Some Corp          |
            | ContactEmail | admin@some.com     |
        When I attempt to register the tenant
        Then it should throw an ArgumentException for "identifier"

    @registration @validation
    Scenario: Reject registration with empty name
        Given I have tenant registration details:
            | Field        | Value              |
            | Identifier   | some-corp          |
            | Name         |                    |
            | ContactEmail | admin@some.com     |
        When I attempt to register the tenant
        Then it should throw an ArgumentException for "name"

    @registration @validation
    Scenario: Reject registration with empty email
        Given I have tenant registration details:
            | Field        | Value              |
            | Identifier   | some-corp          |
            | Name         | Some Corp          |
            | ContactEmail |                    |
        When I attempt to register the tenant
        Then it should throw an ArgumentException for "contactEmail"

    @registration @identifier-format
    Scenario: Identifier is normalized to lowercase
        Given I have tenant registration details:
            | Field        | Value              |
            | Identifier   | ACME-Corp          |
            | Name         | Acme Corporation   |
            | ContactEmail | admin@acme.com     |
        When I register the tenant
        Then the tenant identifier should be "acme-corp"

    @registration @email-format
    Scenario: Contact email is normalized to lowercase
        Given I have tenant registration details:
            | Field        | Value              |
            | Identifier   | test-corp          |
            | Name         | Test Corp          |
            | ContactEmail | Admin@Test.COM     |
        When I register the tenant
        Then the tenant contact email should be "admin@test.com"
