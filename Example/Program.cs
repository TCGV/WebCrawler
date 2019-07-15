using System;
using MisterHex.WebCrawling;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Crawler crawler = new Crawler();
            IObservable<WebPage> observable = crawler.Crawl(new Uri("https://dotnet.microsoft.com"));

            observable.Subscribe(
                onNext: w => Console.WriteLine(w.Uri),
                onError: e => Console.WriteLine(e),
                onCompleted: () => Console.WriteLine("Crawling completed")
            );

            Console.ReadLine();
        }
    }
}
