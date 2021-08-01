using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AadAuthClient.API
{
    public interface IProtectedAPICaller
    {
        Task<HttpResponseMessage> CallWebApiAsync(string webApiUri);
        Task FetchAccessToken();
    }
}