# Authentication

MagicOnion itself does not have authentication mechanism, but you can implement authentication mechanism in MagicOnion servers using ASP.NET Core authentication mechanism. Even if you don't use ASP.NET Core mechanism, you can implement your own authentication mechanism using MagicOnion's filter.

In this guide, we will briefly introduce how to implement each of these.

## JWT (JSON Web Token) Bearer Authentication (Header Authentication)

JWT (JSON Web Token) bearer authentication can be implemented using ASP.NET Core's authentication mechanism. Here, we will explain an example of implementing authentication using JWT. For more information on ASP.NET Core authentication, see the [ASP.NET Core documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-9.0).

Also, one of the advantages of using ASP.NET Core features is that you can get authenticated user information from `HttpContext`.

### Adding Authentication/Authorization Middleware
To use ASP.NET Core's authentication mechanism, you need to add authentication-related services to the service and add authentication/authorization middleware to the HTTP pipeline.

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

### Generating JWT Token

JWT token generation is done using the `System.IdentityModel.Tokens.Jwt` package. The following is an example of generating a JWT token and returning it to the client.

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

### Requiring Authentication in Services

To restrict access to only authenticated users who have been authenticated by the authentication service, apply the `Authorize` attribute to the class or method. This is the `Authorize` attribute of ASP.NET Core. You can also specify `Role` if necessary. For more information, see the ASP.NET Core documentation.

When properly authenticated, you can get the authenticated user from `Context.CallContext.GetHttpContext().User` and its `Identity` property.

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

#### Allowing Anonymous Access

Even if you require authentication for the entire service, you can allow anonymous access to individual methods by using the `AllowAnonymous` attribute.

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

### Adding JWT Token to Request on the client
JWT tokens can be added to requests on the client using the `WithHeaders` method of `MagicOnionClient`.

```csharp
var client = MagicOnionClient.Create<IGreeterService>(channel).WithHeaders(new Metadata {
    { "authorization", "Bearer {token}" }
});
```

For other ways to add headers to requests, see [Metadata and Headers](/unary/metadata).

## Filter Authentication
Filters can be used to validate header values before processing requests. You can implement your own simple API key validation using the ability of filters to validate header values before processing requests.

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

        // TODO: Validate the API key

        await next(context);
    }
}
```

For more information on filters, see [Filters](/filter/).
