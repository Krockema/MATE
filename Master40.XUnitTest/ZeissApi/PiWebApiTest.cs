using Mater40.PiWebApi;
using Xunit;

namespace Master40.XUnitTest.ZeissApi
{
    public class PiWebApiTest
    {
        [Fact]
        public async System.Threading.Tasks.Task TestConnectionAsync()
        {
            var ZeissApi = new ZeissApiClient();
            await ZeissApi.PingServer();
        }
    }
}
