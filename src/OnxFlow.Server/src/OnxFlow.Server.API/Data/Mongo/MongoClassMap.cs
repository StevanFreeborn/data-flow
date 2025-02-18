namespace OnxFlow.Server.API.Data.Mongo;

internal static class MongoClassMap
{
  public static void RegisterMappings()
  {
    BsonClassMap.TryRegisterClassMap<Entity>(
      static cm =>
      {
        cm.AutoMap();
        cm.SetIgnoreExtraElements(true);
        cm.MapIdProperty(static e => e.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
        cm.MapProperty(static e => e.CreatedDate).SetElementName("createdDate");
        cm.MapProperty(static e => e.UpdatedDate).SetElementName("updatedDate");
      }
    );

    BsonClassMap.TryRegisterClassMap<User>(
      static cm =>
      {
        cm.AutoMap();
        cm.SetIgnoreExtraElements(true);
        cm.MapProperty(static u => u.Username).SetElementName("username");
        cm.MapProperty(static u => u.Email).SetElementName("email");
        cm.MapProperty(static u => u.Password).SetElementName("password");
        cm.MapProperty(static u => u.IsVerified).SetElementName("isVerified");
        cm.MapProperty(static u => u.EncryptionKey).SetElementName("encryptionKey");
      }
    );

    BsonClassMap.TryRegisterClassMap<BaseToken>(
      static cm =>
      {
        cm.AutoMap();
        cm.SetIgnoreExtraElements(true);
        cm.MapProperty(static rt => rt.UserId).SetElementName("userId");
        cm.MapProperty(static rt => rt.Token).SetElementName("token");
        cm.MapProperty(static rt => rt.ExpiresAt).SetElementName("expiresAt");
        cm.MapProperty(static rt => rt.Revoked).SetElementName("revoked");
        cm.MapProperty(static rt => rt.TokenType).SetElementName("tokenType");
      }
    );
  }
}