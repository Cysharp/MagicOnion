using System;
using System.Collections.Generic;
using System.Text;
using MagicOnion;
using MessagePack;

namespace JwtAuthApp.Shared
{
    public interface IAccountService : IService<IAccountService>
    {
        UnaryResult<SignInResponse> SignInAsync(string signInId, string password);
        UnaryResult<CurrentUserResponse> GetCurrentUserNameAsync();
        UnaryResult<string> DangerousOperationAsync();
    }

    [MessagePackObject(true)]
    public class SignInResponse
    {
        public long UserId { get; set; }
        public string Name { get; set; }
        public byte[] Token { get; set; }
        public bool Success { get; set; }

        public static SignInResponse Failed { get; } = new SignInResponse() { Success = false };

        public SignInResponse() { }

        public SignInResponse(long userId, string name, byte[] token)
        {
            UserId = userId;
            Name = name;
            Token = token;
            Success = true;
        }
    }

    [MessagePackObject(true)]
    public class CurrentUserResponse
    {
        public static CurrentUserResponse Anonymous { get; } = new CurrentUserResponse() { IsAuthenticated = false, Name = "Anonymous" };

        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }
        public long UserId { get; set; }
    }
}
