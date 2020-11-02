using System;
using System.Threading.Tasks;
using Zeiss.PiWeb.Api.Rest.HttpClient.Data;

namespace Master40.PiWebApi
{
    public class ZeissApiClient
    {
        public DataServiceRestClient ApiClient { get; }

        public ZeissApiClient()
        {
            // TODO: Configurierbar machen!
            ApiClient = new DataServiceRestClient(new Uri("http://desktop-5uuvok8:8080/"));
            
        }

        public async Task PingServer()
        {
            await ApiClient.GetServiceInformation();
        }




    }
}
