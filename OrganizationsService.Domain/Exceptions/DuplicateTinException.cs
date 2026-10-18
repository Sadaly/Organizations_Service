namespace OrganizationsService.Domain.Exceptions;

public sealed class DuplicateTinException(string tin) : Exception($"Organization with INN '{tin}' already exists.");