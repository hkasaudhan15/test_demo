Feature: Tenant Lifecycle Management
    As a platform administrator
    I want to manage tenant lifecycle states
    So that I can control tenant access to the platform

    Background:
        Given a tenant "acme-corp" exists with status "Active"

    @lifecycle @activation
    Scenario: Activate a provisioning tenant
        Given the tenant status is "Provisioning"
        When I activate the tenant
        Then the tenant status should be "Active"
        And the activation date should be set
        And a TenantActivated domain event should be raised

    @lifecycle @activation
    Scenario: Activating an already active tenant is idempotent
        When I activate the tenant
        Then the tenant status should be "Active"
        And no additional domain events should be raised

    @lifecycle @activation
    Scenario: Reactivate a suspended tenant
        Given the tenant is suspended with reason "Payment overdue"
        When I activate the tenant
        Then the tenant status should be "Active"
        And the suspension reason should be cleared
        And a TenantActivated domain event should be raised

    @lifecycle @suspension
    Scenario: Suspend an active tenant
        When I suspend the tenant with reason "Terms of service violation"
        Then the tenant status should be "Suspended"
        And the suspension date should be set
        And the suspension reason should be "Terms of service violation"

    @lifecycle @suspension @validation
    Scenario: Suspend requires a reason
        When I attempt to suspend the tenant without a reason
        Then it should throw an ArgumentException for "reason"

    @lifecycle @deactivation
    Scenario: Deactivate an active tenant
        When I deactivate the tenant with reason "Account closed by customer"
        Then the tenant status should be "Deactivated"
        And a TenantDeactivated domain event should be raised

    @lifecycle @deactivation @validation
    Scenario: Deactivate requires a reason
        When I attempt to deactivate the tenant without a reason
        Then it should throw an ArgumentException for "reason"

    @lifecycle @deactivation
    Scenario: Reactivate a deactivated tenant
        Given the tenant is deactivated with reason "Account closed"
        When I activate the tenant
        Then the tenant status should be "Active"
        And the suspension reason should be cleared
        And a TenantActivated domain event should be raised

    @lifecycle @query
    Scenario: Query tenants by active status
        Given the following tenants exist:
            | Identifier   | Status       |
            | active-one   | Active       |
            | active-two   | Active       |
            | suspended-co | Suspended    |
            | deactive-co  | Deactivated  |
        When I query tenants with status "Active"
        Then I should get 3 tenants
        And the results should contain "active-one" and "active-two"
