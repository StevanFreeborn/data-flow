namespace DataFlow.Server.API.Tests.Mailhog;

internal sealed class MailHogService(HttpClient client)
{
  private readonly HttpClient _client = client;

  public async Task<MailHogSearchResult> SearchEmailAsync(MailHogSearchRequest req)
  {
    var kind = req.Kind switch
    {
      MailHogSearchKind.To => "to",
      MailHogSearchKind.From => "from",
      MailHogSearchKind.Containing => "containing",
      _ => throw new ArgumentException($"Invalid request: {nameof(req.Kind)} is not to, from, or containing.", nameof(req))
    };

    var url = $"/api/v2/search?kind={kind}&query={req.Query}&start={req.Start}&limit={req.Limit}";
    var response = await _client.GetFromJsonAsync<MailHogSearchResult>(url);
    return response ?? new MailHogSearchResult();
  }

  public async Task<MailHogMessage> GetEmailAsync(string id)
  {
    var url = $"/api/v1/messages/{id}";
    var response = await _client.GetFromJsonAsync<MailHogMessage>(url);
    return response ?? new MailHogMessage();
  }
}

internal sealed record MailHogSearchResult
{
  public List<MailHogSearchResultMessage> Items { get; init; } = [];
  public int Total { get; init; }
  public int Start { get; init; }
  public int Count { get; init; }
}

internal sealed record MailHogSearchResultMessage
{
  public string Id { get; init; } = string.Empty;
  public MailHogToOrFrom From { get; init; } = new();
  public List<MailHogToOrFrom> To { get; init; } = [];
  public Dictionary<string, string> Headers { get; init; } = [];
  public int Size { get; init; }
  public DateTime Created { get; init; }
}

internal sealed record MailHogToOrFrom
{
  public List<string> Relays { get; init; } = [];
  public string Mailbox { get; init; } = string.Empty;
  public string Domain { get; init; } = string.Empty;
  public string @Params { get; init; } = string.Empty;
}

internal sealed record MailHogMessage
{
  public string ID { get; init; } = string.Empty;
  public MailHogToOrFrom From { get; init; } = new();
  public List<MailHogToOrFrom> To { get; init; } = [];
  public MailHogMessageContent Content { get; init; } = new();
  public string Created { get; init; } = string.Empty;
  public object MIME { get; init; } = new();
  public MailHogRaw Raw { get; init; } = new();
}

internal sealed record MailHogMessageContent
{
  public Headers Headers { get; init; } = new();
  public string Body { get; init; } = string.Empty;
  public int Size { get; init; }
  public object MIME { get; init; } = new();
}

internal sealed record Headers
{
  public List<string> ContentTransferEncoding { get; init; } = [];
  public List<string> ContentType { get; init; } = [];
  public List<string> Date { get; init; } = [];
  public List<string> From { get; init; } = [];
  public List<string> MIMEVersion { get; init; } = [];
  public List<string> MessageID { get; init; } = [];
  public List<string> Received { get; init; } = [];
  public List<string> ReturnPath { get; init; } = [];
  public List<string> Subject { get; init; } = [];
  public List<string> To { get; init; } = [];
}

internal sealed record MailHogRaw
{
  public string From { get; init; } = string.Empty;
  public List<string> To { get; init; } = [];
  public string Data { get; init; } = string.Empty;
  public string Helo { get; init; } = string.Empty;
}

internal sealed class MailHogSearchRequest
{
  public MailHogSearchKind Kind { get; init; }
  public string Query { get; init; }
  public int Start { get; init; }
  public int Limit { get; init; } = 50;

  public MailHogSearchRequest(MailHogSearchKind kind, string query)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(query, nameof(query));

    Kind = kind;
    Query = query;
  }
}

internal enum MailHogSearchKind
{
  To,
  From,
  Containing,
}