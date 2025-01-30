# 認証

MagicOnion 自体には認証機能はありませんが、ASP.NET Core の認証機能を利用することで MagicOnion サーバーに認証機能を実装できます。ASP.NET Core の機能を使用しない場合であっても、MagicOnion のフィルター機能を使用して独自の認証機能を実装することもできます。

このガイドではそれぞれの実装方法を簡単に紹介します。

## JWT (JSON Web Token) ベアラー認証 (ヘッダー認証)

JWT (JSON Web Token) を使用したベアラー認証は ASP.NET Core の認証機能で実装できます。ここでは、JWT を使用した認証の実装例を説明します。また、ASP.NET Core の認証についての詳しい情報は [ASP.NET Core のドキュメント](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-9.0) を参照してください。

また、ASP.NET Core の機能を利用することの利点として `HttpContext` から認証されたユーザー情報を取得できる点があります。

### 認証/認可ミドルウェアの追加
ASP.NET Core の認証機能を使用するためにサービスの認証関連のサービスの追加と HTTP パイプラインに認証/認可のミドルウェアを追加が必要です。

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String("<Base64EncodedSecretKey>")),
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.FromSeconds(10),

            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
        };
#if DEBUG
        options.RequireHttpsMetadata = false;
#endif
    });
builder.Services.AddAuthorization();

...

app.UseAuthentication();
app.UseAuthorization();
app.MapMagicOnionService();
```

### JWT トークンの生成

JWT トークンの生成は、`System.IdentityModel.Tokens.Jwt` パッケージを使用して行います。以下は JWT トークンを生成してクライアントに返す例です。

```csharp
var userName = "Alice";
var userId = 12345;

var securityKey = new SymmetricSecurityKey(Convert.FromBase64String("<Base64EncodedSecretKey>"));
var jwtTokenHandler = new JwtSecurityTokenHandler();
var expires = DateTime.UtcNow.AddSeconds(10);
var token = jwtTokenHandler.CreateEncodedJwt(new SecurityTokenDescriptor()
{
    SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256),
    Subject = new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.Name, userName),
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
    }),
    Expires = expires,
});

return new TokenResponse
{
    Token = token,
    Expires = expires,
};
```

### サービスで認証を必須とする

サービスで認証されたユーザーのみがアクセスできるようにするには `Authorize` 属性をクラスまたはメソッドに適用します。これは ASP.NET Core の `Authorize` 属性です。必要に応じて `Role` を指定することも可能です。詳しくは ASP.NET Core のドキュメントを参照してください。

正しく認証された場合、`Context.CallContext.GetHttpContext().User` と、その `UserPrincipal` の `Identity` プロパティから認証されたユーザーを取得できます。

```csharp
[Authorize]
public class GreeterService : ServiceBase<IGreeterService>, IGreeterService
{
    public async UnaryResult<string> SayHelloAsync()
    {
        var user = Context.CallContext.GetHttpContext().User;
        return $"Hello {user.Identity?.Name}!";
    }
}
```

#### 匿名アクセスを許可する

サービス全体に認証を必須としている場合でも、個別のメソッドに `AllowAnonymous` 属性を使用して匿名アクセスを許可することもできます。

```csharp
[Authorize]
public class GreeterService : ServiceBase<IGreeterService>, IGreeterService
{
    public async UnaryResult<string> SayHelloAsync()
    {
        var user = Context.CallContext.GetHttpContext().User;
        return $"Hello {user.Identity?.Name}!";
    }

    [AllowAnonymous]
    public async UnaryResult<string> SayHelloAnonymousAsync()
    {
        return $"Hello Unknown!";
    }
}
```

### クライアントで JWT トークンをリクエストに付加する
クライアントで JWT トークンをリクエストに付加するには、`MagicOnionClient` の `WithHeaders` メソッドを使用します。

```csharp
var client = MagicOnionClient.Create<IGreeterService>(channel).WithHeaders(new Metadata {
    { "authorization", "Bearer {token}" }
});
```

リクエストに付加するその他の方法については [メタデータとヘッダー](/unary/metadata) を参照してください。


## フィルター認証
フィルターではリクエストを処理する前にヘッダーの値を検証できることを利用して、独自の簡単な API キー検証を実装できます。

```csharp
public class CustomAuthorizeAttribute : MagicOnionFilterAttribute
{
    public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
    {
        var httpContext = context.CallContext.GetHttpContext();
        var authorization = httpContext.Request.Headers["Authorization"];
        if (string.IsNullOrEmpty(authorization))
        {
            context.Status = new Status(StatusCode.Unauthenticated, "Authorization header is required.");
            return;
        }

        // ここで認証処理を行う

        await next(context);
    }
}
```

詳しくは [フィルター](/filter/) を参照してください。
