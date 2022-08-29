namespace Notebook.Core.AppSettings
{
    public class AuthTokenParameters
    {
        public string Issuer { get; set; }

        public string Audience { get; set; }

        public int TokenExpirationTimeInHours { get; set; }

        public string SigningKey { get; set; }
    }
}