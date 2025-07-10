namespace StarConflictsRevolt.Server.WebApi.Security;

public record TokenRequest(string ClientId, string Secret);