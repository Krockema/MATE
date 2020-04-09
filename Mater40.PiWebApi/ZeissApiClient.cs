using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Zeiss.IMT.PiWeb.Api.DataService.Rest;

namespace Mater40.PiWebApi
{
    public class ZeissApiClient
    {
        public DataServiceRestClient ApiClient { get; }

        public ZeissApiClient()
        {
            // TODO: Configurierbar machen!
            ApiClient = new DataServiceRestClient(new Uri("http://desktop-3bejpcl:8080/"));
            
        }

        public async Task PingServer()
        {
            await ApiClient.GetServiceInformation();
        }




    }
}
