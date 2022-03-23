namespace UnsecuredAuthorization
{
    public class SystemGuid : IGuidGen
    {
        private SystemGuid() { }

        public static readonly IGuidGen Instance = new SystemGuid();
        public Guid NewGuid() => Guid.NewGuid();
    }
}
