namespace OrganizationsService.Domain.Exceptions;

public sealed class DuplicateInnException(string tin) : Exception($"Organization with INN '{tin}' already exists.");