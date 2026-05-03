Feature: Tenant Feature Flags
    As a platform administrator
    I want to configure feature flags per tenant
    So that I can control feature access granularly

    Background:
        Given a tenant "feature-corp" exists with status "Active"

    @features @enable
    Scenario: Enable a feature for a tenant
        When I enable feature "api_access" for the tenant
        Then the tenant should have feature "api_access" enabled
        And the tenant should have 1 feature configured

    @features @enable @expiry
    Scenario: Enable a feature with expiry date
        When I enable feature "advanced_reporting" with expiry "2030-12-31"
        Then the tenant should have feature "advanced_reporting" enabled
        And the feature "advanced_reporting" should expire on "2030-12-31"

    @features @disable
    Scenario: Disable a feature for a tenant
        Given the tenant has feature "api_access" enabled
        When I disable feature "api_access" for the tenant
        Then the tenant should not have feature "api_access" active

    @features @enable @idempotent
    Scenario: Enabling an already enabled feature updates expiry
        Given the tenant has feature "sso_integration" enabled
        When I enable feature "sso_integration" with expiry "2030-06-30"
        Then the feature "sso_integration" should expire on "2030-06-30"
        And the tenant should have 1 feature configured

    @features @check
    Scenario: HasFeature returns false for disabled feature
        Given the tenant has feature "audit_log" disabled
        When I check if the tenant has feature "audit_log"
        Then the result should be false

    @features @check
    Scenario: HasFeature returns false for expired feature
        Given the tenant has feature "bulk_operations" with past expiry
        When I check if the tenant has feature "bulk_operations"
        Then the result should be false

    @features @check
    Scenario: HasFeature returns true for active non-expired feature
        Given the tenant has feature "data_export" enabled
        When I check if the tenant has feature "data_export"
        Then the result should be true

    @features @multiple
    Scenario: Multiple features can be configured independently
        When I enable feature "api_access" for the tenant
        And I enable feature "sso_integration" for the tenant
        And I enable feature "webhook_integration" for the tenant
        Then the tenant should have 3 features configured
        And the tenant should have feature "api_access" enabled
        And the tenant should have feature "sso_integration" enabled
        And the tenant should have feature "webhook_integration" enabled

    @features @validation
    Scenario: Enable feature with empty code throws exception
        When I attempt to enable feature "" for the tenant
        Then it should throw an ArgumentException for "featureCode"
