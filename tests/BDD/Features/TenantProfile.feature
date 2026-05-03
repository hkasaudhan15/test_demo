Feature: Tenant Profile Management
    As a platform administrator
    I want to update tenant profile information
    So that tenant details stay current and accurate

    Background:
        Given a tenant "profile-corp" exists with name "Profile Corp" and email "admin@profile.com"

    @profile @update
    Scenario: Update tenant profile name and description
        When I update the tenant profile:
            | Field        | Value                       |
            | Name         | Profile Corporation         |
            | Description  | Updated company description |
            | ContactEmail | admin@profile.com           |
        Then the tenant name should be "Profile Corporation"
        And the tenant description should be "Updated company description"

    @profile @update
    Scenario: Update tenant contact email
        When I update the tenant profile:
            | Field        | Value                 |
            | Name         | Profile Corp          |
            | ContactEmail | new-admin@profile.com |
        Then the tenant contact email should be "new-admin@profile.com"

    @profile @custom-domain
    Scenario: Set custom domain for tenant
        When I set the custom domain to "app.profile-corp.com"
        Then the tenant custom domain should be "app.profile-corp.com"

    @profile @custom-domain
    Scenario: Remove custom domain
        Given the tenant has custom domain "app.profile-corp.com"
        When I set the custom domain to null
        Then the tenant custom domain should be null

    @profile @logo
    Scenario: Set tenant logo URL
        When I set the logo URL to "https://cdn.example.com/logos/profile-corp.png"
        Then the tenant logo URL should be "https://cdn.example.com/logos/profile-corp.png"

    @profile @connection-string
    Scenario: Set tenant-specific connection string
        When I set the connection string to "Server=tenant-db;Database=ProfileCorp"
        Then the tenant connection string should be "Server=tenant-db;Database=ProfileCorp"

    @profile @validation
    Scenario: Update profile with empty name throws exception
        When I attempt to update the tenant profile with empty name
        Then it should throw an ArgumentException for "name"

    @profile @validation
    Scenario: Update profile with empty email throws exception
        When I attempt to update the tenant profile with empty email
        Then it should throw an ArgumentException for "contactEmail"

    @profile @normalization
    Scenario: Custom domain is normalized to lowercase
        When I set the custom domain to "App.Profile-Corp.COM"
        Then the tenant custom domain should be "app.profile-corp.com"
