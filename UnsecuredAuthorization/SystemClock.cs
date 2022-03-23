namespace UnsecuredAuthorization
{
    public class SystemClock : IClock
    {
        private SystemClock() { }

        public static readonly IClock Instance = new SystemClock();
        public DateTime Now => DateTime.Now;
    }
}
