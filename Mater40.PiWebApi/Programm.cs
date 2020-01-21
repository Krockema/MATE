using System;
using System.Threading.Tasks;

namespace Mater40.PiWebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var client = new ZeissApiClient();
            client.PingServer().Wait();

            Console.WriteLine("Create new Data");
            Configuration.CreateAttribute(client.ApiClient).Wait();
            Console.ReadKey();

            Console.WriteLine("Deleting Data..");
            Configuration.ClearData(client.ApiClient).Wait();
            Console.ReadKey();

        }
    }
}