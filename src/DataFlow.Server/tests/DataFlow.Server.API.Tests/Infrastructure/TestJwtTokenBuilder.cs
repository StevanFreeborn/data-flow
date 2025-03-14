namespace DataFlow.Server.API.Tests.Infrastructure;

public class TestJwtTokenBuilder
{
  public static readonly string TestJwtSecret = "qqs+CKdh2KQOoXS4asnTaIdu+/DFnfsMIh10u1ODG1Q=";
  public static readonly string TestJwtAudience = "TestAudience";
  public static readonly string TestJwtIssuer = "TestIssuer";
  public static readonly int TestJwtExpiryInMinutes = 5;
  private readonly List<Claim> _claims = [];
  private DateTime IssuedAt { get; set; } = DateTime.UtcNow;

  private TestJwtTokenBuilder() { }

  public static TestJwtTokenBuilder Create()
  {
    return new();
  }

  public TestJwtTokenBuilder WithClaim(Claim claim)
  {
    _claims.Add(claim);
    return this;
  }

  public TestJwtTokenBuilder WithIssuedAt(DateTime issuedAt)
  {
    IssuedAt = issuedAt;
    return this;
  }

  public string Build()
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(TestJwtSecret);
    var issuedAt = IssuedAt;
    var expires = issuedAt.AddMinutes(TestJwtExpiryInMinutes);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(_claims),
      Expires = expires,
      IssuedAt = issuedAt,
      NotBefore = issuedAt,
      Issuer = TestJwtIssuer,
      Audience = TestJwtAudience,
      SigningCredentials = new SigningCredentials(
        new SymmetricSecurityKey(key),
        SecurityAlgorithms.HmacSha256Signature
      )
    };

    var securityToken = tokenHandler.CreateToken(tokenDescriptor);
    var jwtToken = tokenHandler.WriteToken(securityToken);
    return jwtToken;
  }
}