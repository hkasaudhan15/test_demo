Feature: Tenant Subscription Management
    As a platform administrator
    I want to manage tenant subscription tiers
    So that tenants get appropriate resource limits

    Background:
        Given a tenant "startup-inc" exists with tier "Free"

    @subscription @tier-change
    Scenario: Upgrade from Free to Professional
        When I change the subscription to "Professional"
        Then the tenant subscription tier should be "Professional"
        And the tenant max users should be 100
        And the tenant max storage should be 50000 MB
        And the tenant max API calls should be 100000
        And a TenantSubscriptionChanged domain event should be raised

    @subscription @tier-change
    Scenario: Downgrade from Professional to Starter
        Given the tenant tier is "Professional"
        When I change the subscription to "Starter"
        Then the tenant subscription tier should be "Starter"
        And the tenant max users should be 25
        And the tenant max storage should be 5000 MB
        And the tenant max API calls should be 10000
        And a TenantSubscriptionChanged domain event should be raised

    @subscription @tier-change
    Scenario: Upgrade to Enterprise gives unlimited resources
        When I change the subscription to "Enterprise"
        Then the tenant subscription tier should be "Enterprise"
        And the tenant max users should be unlimited
        And the tenant max storage should be unlimited
        And the tenant max API calls should be unlimited

    @subscription @expiry
    Scenario: Set subscription expiry date
        When I change the subscription to "Professional" with expiry "2030-12-31"
        Then the subscription expiry date should be "2030-12-31"
        And the tenant should not be expired

    @subscription @expiry
    Scenario: Expired subscription is detected
        When I change the subscription to "Professional" with expiry "2020-01-01"
        Then the tenant should be expired

    @subscription @no-change
    Scenario: Changing to the same tier does not raise event
        When I change the subscription to "Free"
        Then no subscription changed event should be raised

    @subscription @resource-limits
    Scenario Outline: Resource limits match tier defaults
        When I change the subscription to "<Tier>"
        Then the tenant max users should be <MaxUsers>
        And the tenant max storage should be <MaxStorageMb> MB
        And the tenant max API calls should be <MaxApiCalls>
        And the tenant max workspaces should be <MaxWorkspaces>

        Examples:
            | Tier         | MaxUsers | MaxStorageMb | MaxApiCalls | MaxWorkspaces |
            | Free         | 5        | 500          | 1000        | 3             |
            | Starter      | 25       | 5000         | 10000       | 20            |
            | Professional | 100      | 50000        | 100000      | 100           |
