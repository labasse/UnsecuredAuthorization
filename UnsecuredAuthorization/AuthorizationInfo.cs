namespace UnsecuredAuthorization
{
    public record AuthorizationInfo(Guid Token, string Username, string Roles)
    {
        public bool IsInRole(string role) => Roles.Split(',').Contains(role);
    }
}
