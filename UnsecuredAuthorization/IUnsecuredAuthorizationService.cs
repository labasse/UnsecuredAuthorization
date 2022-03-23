namespace UnsecuredAuthorization
{
    public interface IUnsecuredAuthorizationService
    {
        public Guid SignIn(string username, string[] roles);
        public void SignOut(Guid token);
        public bool Exists(Guid token);
        public AuthorizationInfo this[Guid token] { get; }
        TimeSpan TokenLifetime { get; }
    }
}