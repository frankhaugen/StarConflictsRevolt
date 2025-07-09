using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StarConflictsRevolt.Clients.Shared;

public class TokenProviderOptions
{
    public string TokenEndpoint { get; set; } = "http://localhost:5153/token";
    public string ClientId { get; set; } = string.Empty;
    public string Secret { get; set; } = "changeme";
}