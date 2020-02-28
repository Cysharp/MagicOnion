namespace JwtAuthApp.Server.Authentication
{
    public class CustomJwtAuthenticationPayload
    {
        public long UserId { get; set; }
        public string DisplayName { get; set; }
    }
}