namespace AadAuthClient.API
{
    public interface IAuthenticationConfig
    {
        string ConfigSection { get; }
        string AADAuthBaseAddress { get; set; }
        string AADAuthScope { get; set; }
        string Authority { get; }
        string CertificateName { get; set; }
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string Instance { get; set; }
        string Tenant { get; set; }
        AuthenticationConfig ReadFromJsonFile(string path);
    }
}