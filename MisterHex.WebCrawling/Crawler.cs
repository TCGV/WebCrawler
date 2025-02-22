﻿using CsQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive;
using System.Threading;
using Polly;

namespace MisterHex.WebCrawling
{
    public class Crawler
    {
        class ReceivingCrawledUri : ObservableBase<WebPage>
        {
            private int _numberOfLinksLeft = 0;

            private ReplaySubject<WebPage> _subject = new ReplaySubject<WebPage>();

            private readonly Uri _uri;

            private IEnumerable<IUriFilter> _filters;

            public ReceivingCrawledUri(Uri uri)
                : this(uri, Enumerable.Empty<IUriFilter>().ToArray())
            { }

            public ReceivingCrawledUri(Uri uri, params IUriFilter[] filters)
            {
                _uri = uri;
                _filters = filters;

            }

            protected override IDisposable SubscribeCore(IObserver<WebPage> observer)
            {
                var _ = CrawlAsync(_uri);
                return _subject.Subscribe(observer);
            }

            private async Task CrawlAsync(Uri uri)
            {
                using (HttpClient client = new HttpClient() { Timeout = TimeSpan.FromMinutes(1) })
                {
                    IEnumerable<Uri> result = new List<Uri>();

                    try
                    {
                        string html = await Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(new[]
                        {
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(3),
                            TimeSpan.FromSeconds(5)
                        })
                        .ExecuteAsync(async () => await client.GetStringAsync(uri));

                        var cq = CQ.Create(html);
                        _subject.OnNext(new WebPage(uri, cq));

                        result = cq["a"].Select(i => i.Attributes["href"]).SafeSelect(x => CreateUri(x, uri));
                        result = Filter(result, _filters.ToArray());

                        result.ToList().ForEach(async i =>
                        {
                            Interlocked.Increment(ref _numberOfLinksLeft);
                            await CrawlAsync(i);
                        });
                    }
                    catch
                    {
                        
                    }

                    if (Interlocked.Decrement(ref _numberOfLinksLeft) == 0)
                        _subject.OnCompleted();
                }
            }

            private Uri CreateUri(string i, Uri source)
            {
                if (Uri.IsWellFormedUriString(i, UriKind.Relative))
                    return new Uri(source, i);
                return new Uri(i);
            }

            private static List<Uri> Filter(IEnumerable<Uri> uris, params IUriFilter[] filters)
            {
                var filtered = uris.ToList();
                foreach (var filter in filters.ToList())
                {
                    filtered = filter.Filter(filtered);
                }
                return filtered;
            }
        }

        public IObservable<WebPage> Crawl(Uri uri)
        {
            return new ReceivingCrawledUri(uri, new ExcludeRootUriFilter(uri), new ExternalUriFilter(uri), new AlreadyVisitedUriFilter());
        }

        public IObservable<WebPage> Crawl(Uri uri, params IUriFilter[] filters)
        {
            return new ReceivingCrawledUri(uri, filters);
        }
    }
}
