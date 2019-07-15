using CsQuery;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MisterHex.WebCrawling
{
    public class WebPage
    {
        public WebPage(Uri uri, CQ cq)
        {
            this.Uri = uri;
            this.cq = cq;
        }

        public Uri Uri { get; private set; }

        public IEnumerable<WebElement> Query(string selector)
        {
            return cq[selector].Select(n => new WebElement(n.InnerHTML, n.OuterHTML, n.InnerText, n.TextContent));
        }

        private CQ cq;
    }
}