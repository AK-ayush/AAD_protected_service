using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AadAuthClient.API
{
    public class ProtectedAPICaller : IProtectedAPICaller
    {
        public ProtectedAPICaller(
            IAuthenticationConfig authenticationConfig, 
            ILogger<ProtectedAPICaller> logger)
        {
            _authenticationConfig = authenticationConfig;
            _logger = logger;

            HttpClient = new HttpClient();

            config = _authenticationConfig.ReadFromJsonFile("appsettings.json");
            app = BuildConfidentialApp();
            scope = new string[] { config.AADAuthScope };
            FetchAccessToken().GetAwaiter().GetResult();
        }

        private readonly HttpClient HttpClient; //  { get; private set; }
        private readonly IAuthenticationConfig _authenticationConfig;
        private readonly ILogger _logger;

        private readonly IAuthenticationConfig config;
        private readonly IConfidentialClientApplication app;
        private readonly string[] scope;
        private string AccessToken;


        private IConfidentialClientApplication BuildConfidentialApp()
        {
            bool isUsingClientSecret;
            
            if (!string.IsNullOrWhiteSpace(config.ClientSecret))
            {
                isUsingClientSecret = true;
            }
            else if (!string.IsNullOrWhiteSpace(config.CertificateName))
            {
                isUsingClientSecret = false;
            }
            else
            {
                throw new Exception("You must choose between using the " +
                    "client secret or certificate. Please update the appsetting.json file.");
            }

            if (isUsingClientSecret)
            {
                _logger.LogInformation("Using the Client Secret.");
                return ConfidentialClientApplicationBuilder.Create(config.ClientId)
                    .WithClientSecret(config.ClientSecret)
                    .WithAuthority(new Uri(config.Authority))
                    .Build();
            }
            _logger.LogInformation("Using the Client Certificate.");
            X509Certificate2 certificate = ReadCertificate(config.CertificateName);
            return ConfidentialClientApplicationBuilder.Create(config.ClientId)
                .WithCertificate(certificate)
                .WithAuthority(new Uri(config.Authority))
                .Build();
        }

        private static X509Certificate2 ReadCertificate(string CertificateName)
        {
            if (string.IsNullOrEmpty(CertificateName))
            {
                throw new ArgumentException("CertificateName should not be empty. " +
                    "Please set the CertificateName in the appsettings.json", "CertificateName");
            }
            CertificateDescription certificateDescription = 
                CertificateDescription.FromStoreWithDistinguishedName(CertificateName);
            DefaultCertificateLoader defaultCertificateLoader = new DefaultCertificateLoader();
            defaultCertificateLoader.LoadIfNeeded(certificateDescription);
            return certificateDescription.Certificate;
        }

        public async Task FetchAccessToken()
        {
            try
            {
                AuthenticationResult result = await app.AcquireTokenForClient(scope).ExecuteAsync();
                // Console.ForegroundColor = ConsoleColor.Green;
                // Console.WriteLine($"Token acquired successfully for scope:{scope[0]}");
                _logger.LogInformation($"Token acquired successfully for scope:{scope[0]}");
                _logger.LogError($"{result.AccessToken}");
                // Console.ResetColor();
                AccessToken = result.AccessToken;
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                // Console.ForegroundColor = ConsoleColor.Red;
                // Console.WriteLine("Scope: provided is not supported! Unable to fetch the token!");
                _logger.LogError($"Scope: provided is not supported! " +
                    $"Unable to fetch the token! {ex.Message}");
                // Console.ResetColor();
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Unable to fetch token \n {ex.Message}");
                _logger.LogError($"Unable to fetch token! Exception: {ex.Message}");
            }
        }

        public async Task<HttpResponseMessage> CallWebApiAsync(string webApiUri)
        {
            if (string.IsNullOrEmpty(AccessToken))
            {
                _logger.LogWarning("Unable to find the Access Token. Trying to fetch again...");
                await FetchAccessToken();
            }
            if (!string.IsNullOrEmpty(AccessToken))
            {
                var defaultRequestHeaders = HttpClient.DefaultRequestHeaders;
                if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
                {
                    HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
                defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                try
                {
                    HttpResponseMessage response = await HttpClient.GetAsync(webApiUri);

                    return response;
                }
                catch (Exception ex)
                {
                    // Console.ForegroundColor = ConsoleColor.Red;
                    // Console.WriteLine($"Unable to connect to server! Error: {ex.Message}");
                    _logger.LogError($"Unable to connect to server! Error: {ex.Message}");
                    // Console.ResetColor();
                    return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                }

            }
            _logger.LogError($"Invalid Access Token! Access Token : {AccessToken}");
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);//new List<string> { $"Invalid Access Token! Access Token : {AccessToken}" };
        }
    }
}
