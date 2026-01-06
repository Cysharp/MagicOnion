using MagicOnion;

namespace AotSample.Shared;

public interface IGreeterService : IService<IGreeterService>
{
    UnaryResult<string> SayHelloAsync(string name);
    UnaryResult<int> AddAsync(int a, int b);
    
    // Methods using MessagePackObject DTOs
    UnaryResult<UserProfile> GetUserAsync(int id);
    UnaryResult<UserProfile> CreateUserAsync(CreateUserRequest request);
}
