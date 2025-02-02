# 인증

MagicOnion 자체에는 인증 기능이 없지만, ASP.NET Core의 인증 기능을 이용하여 MagicOnion 서버에 인증 기능을 구현할 수 있습니다. ASP.NET Core의 기능을 사용하지 않는 경우에도, MagicOnion의 필더(Filter) 기능을 사용하여 독자적인 인증 기능을 구현할 수도 있습니다.

이 가이드에서는 각각의 구현 방법을 간단히 소개합니다.

## JWT (JSON Web Token) Bearer 인증 (Header 인증)

JWT(JSON Web Token)를 사용한 Bearer 인증은 ASP.NET Core의 인증 기능으로 구현할 수 있습니다. 여기서는 JWT를 사용한 인증의 구현 예시를 설명합니다. 또한, [ASP.NET Core 인증](https://learn.microsoft.com/ko-kr/aspnet/core/security/authentication/)에 대한 자세한 정보는 ASP.NET Core의 문서를 참조해 주세요.

또한, ASP.NET Core의 기능을 이용하는 것의 장점으로 `HttpContext`에서 인증된 사용자 정보를 가져올 수 있다는 점이 있습니다.

### 인증(Authentication)/권한 부여(Authorization) 미들웨어 추가
ASP.NET Core의 인증 기능을 사용하기 위해 서비스의 인증 관련 서비스의 추가와 HTTP 파이프라인에 인증/권한 부여 미들웨어의 추가가 필요합니다.

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

### JWT 토큰 생성

JWT 토큰의 생성은 `System.IdentityModel.Tokens.Jwt` 패키지를 사용하여 수행합니다. 다음은 JWT 토큰을 생성하여 클라이언트에 반환하는 예시입니다.

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

### 서비스에서 인증을 필수로 하기

서비스에서 인증된 사용자만이 접근할 수 있도록 하려면 `Authorize` 속성을 클래스 또는 메서드에 적용합니다. 이는 ASP.NET Core의 `Authorize` 속성입니다. 필요에 따라 `Role`을 지정하는 것도 가능합니다. 자세한 내용은 [ASP.NET Core의 문서](https://learn.microsoft.com/ko-kr/aspnet/core/security/authorization/simple)를 참조해 주세요.

올바르게 인증된 경우, `Context.CallContext.GetHttpContext().User`와, 그 `UserPrincipal`의 `Identity` 속성에서 인증된 사용자를 가져올 수 있습니다.

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

#### 익명 액세스를 허용하기

서비스 전체에 인증을 필수로 하고 있는 경우에도, 개별 메서드에 `AllowAnonymous` 속성을 사용하여 익명 액세스를 허용할 수도 있습니다.

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

### 클라이언트에서 JWT 토큰을 요청에 추가하기
클라이언트에서 JWT 토큰을 요청에 추가하려면, `MagicOnionClient`의 `WithHeaders` 메서드를 사용합니다.

```csharp
var client = MagicOnionClient.Create<IGreeterService>(channel).WithHeaders(new Metadata {
    { "authorization", "Bearer {token}" }
});
```

요청에 추가하는 기타 방법에 대해서는 [메타데이터와 헤더](../unary/metadata) 페이지를 참조하시기 바랍니다.


## 필터(Filter) 인증
필터에서는 요청을 처리하기 전에 헤더의 값을 검증할 수 있음을 이용하여, 독자적인 간단한 API 키 검증을 구현할 수 있습니다.

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

        // 여기서 인증 처리 하기

        await next(context);
    }
}
```

자세한 내용은 [필터](/filter/fundamentals) 페이지를 참조하시기 바랍니다.
