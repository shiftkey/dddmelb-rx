using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Web;

namespace QueryDemo
{
    public class QueryModule : Nancy.NancyModule
    {
        public QueryModule()
        {
            Get["/search/{term}"] = parameters =>
            {
                // i should use the search terms here
                // and probably use a proper search API
                // if they even exist still
                // but that's not the point
                var first = HttpWebRequest.CreateHttp("http://www.google.com.au");
                var second = HttpWebRequest.CreateHttp("http://www.bing.com");
                var third = HttpWebRequest.CreateHttp("http://duckduckgo.com");

                // start them all up
                var allRequests = Observable.Merge(
                    first.GetResponseAsync().ToObservable(),
                    second.GetResponseAsync().ToObservable(),
                    third.GetResponseAsync().ToObservable());

                // give me the one that finishes first
                var fastest = allRequests.FirstAsync();

                // block until it gets here
                var result = fastest.Wait();

                // read the content and return it to the client
                var reader = new StreamReader(result.GetResponseStream());
                var content = Observable.FromAsync(reader.ReadToEndAsync);
                var text = content.Wait();
                return text;
            };
        }
    }
}