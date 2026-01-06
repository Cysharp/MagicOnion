using AotSample.Shared;
using MagicOnion;
using MagicOnion.Server;

namespace AotSample.Server.Services;

public class GreeterService : ServiceBase<IGreeterService>, IGreeterService
{
    public UnaryResult<string> SayHelloAsync(string name)
    {
        return UnaryResult.FromResult($"Hello, {name}!");
    }

    public UnaryResult<int> AddAsync(int a, int b)
    {
        return UnaryResult.FromResult(a + b);
    }

    public UnaryResult<UserProfile> GetUserAsync(int id)
    {
        // Simulate fetching a user
        var user = new UserProfile
        {
            Id = id,
            Name = $"User{id}",
            Email = $"user{id}@example.com",
            Age = 20 + id
        };
        return UnaryResult.FromResult(user);
    }

    public UnaryResult<UserProfile> CreateUserAsync(CreateUserRequest request)
    {
        // Simulate creating a user
        var user = new UserProfile
        {
            Id = Random.Shared.Next(1000, 9999),
            Name = request.Name,
            Email = request.Email,
            Age = request.Age
        };
        return UnaryResult.FromResult(user);
    }
}
