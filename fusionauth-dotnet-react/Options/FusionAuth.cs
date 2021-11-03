namespace fusionauth_dotnet_react.Options
{
    public class FusionAuth
    {
        public static string ConfigName = "FusionAuth";

        public string Authority { get; set; }

        public string CookieName { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string ApiKey { get; set; }

        public string RedirectUri { get; set; }
    }
}
