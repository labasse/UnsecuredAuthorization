namespace UnsecuredAuthorization
{
    public class UnsecuredAuthorizationServiceImpl : IUnsecuredAuthorizationService
    {
        private Dictionary<Guid, (DateTime lastAccess, AuthorizationInfo infos)> _authorizations = new();

        public IClock Clock { get; init; } = SystemClock.Instance;
        public IGuidGen GuidGen { get; init; } = SystemGuid.Instance;
        public TimeSpan TokenLifetime { get; init; } = TimeSpan.FromMinutes(15);

        public bool Exists(Guid token)
        {
            if(_authorizations.ContainsKey(token))
            {
                var auth = _authorizations[token];

                if (Clock.Now - auth.lastAccess <= TokenLifetime)
                {
                    _authorizations[token] = (Clock.Now, auth.infos);
                    return true;
                }
                _authorizations.Remove(token);
            }
            return false;
        }

        public AuthorizationInfo this[Guid token] => Exists(token)
            ? _authorizations[token].infos
            : throw new KeyNotFoundException("Token unknown or expired");

        public Guid SignIn(string username, string[] roles)
        {
            if(username.Length == 0)
            {
                throw new ArgumentException("Username cannot be empty");
            }
            if(roles.Length == 0)
            {
                throw new ArgumentException("At least one role");
            }
            Array.Sort(roles);
            var auth = new AuthorizationInfo(GuidGen.NewGuid(), username, String.Join(',', roles));

            _authorizations[auth.Token] = (Clock.Now, auth);
            return auth.Token;
        }

        public void SignOut(Guid token) 
        {
            var check = this[token];
            _authorizations.Remove(token);
        }
    }
}
