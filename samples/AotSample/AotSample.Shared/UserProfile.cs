using MessagePack;

namespace AotSample.Shared;

[MessagePackObject]
public class UserProfile
{
    [Key(0)]
    public int Id { get; set; }

    [Key(1)]
    public string Name { get; set; } = string.Empty;

    [Key(2)]
    public string Email { get; set; } = string.Empty;

    [Key(3)]
    public int Age { get; set; }
}

[MessagePackObject]
public class CreateUserRequest
{
    [Key(0)]
    public string Name { get; set; } = string.Empty;

    [Key(1)]
    public string Email { get; set; } = string.Empty;

    [Key(2)]
    public int Age { get; set; }
}
