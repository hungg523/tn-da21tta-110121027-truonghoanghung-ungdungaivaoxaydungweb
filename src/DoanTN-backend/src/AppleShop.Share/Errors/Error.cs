namespace AppleShop.Share.Errors
{
    public class Error
    {
        public List<string>? Details { get; }
        public string StackTrace { get; }

        public Error(string stackTrace, params string[]? details)
        {
            Details = new List<string>();
            if (details is not null) Details.AddRange(details);
            StackTrace = stackTrace;
        }
    }
}