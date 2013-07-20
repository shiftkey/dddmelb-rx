using System;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MakingWebRequests
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "http://httpstat.us/404";
            //await MakeAnAsyncRequest(url);
            MakeAnObservableRequest(url).Wait();
        }

        static async Task MakeAnAsyncRequest(string url)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            var text = await response.Content.ReadAsStringAsync();
            // this should explode
            var json = await JsonConvert.DeserializeObjectAsync(text);
        }

        static IObservable<object> MakeAnObservableRequest(string url)
        {
            var client = new HttpClient();
            var text = client.GetAsync(url)
                             .ToObservable()
                             .Select(response => response.Content.ReadAsStringAsync().Result)
                             .Select(content => JsonConvert.DeserializeObject(content))
                             .Catch<object, Exception>(ex =>
                             {
                                 Console.WriteLine("An exception occurred: {0}", ex);
                                 return Observable.Return(new Object());
                             });
            return text;
        }

    }
}
