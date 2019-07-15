using System;
using System.Collections.Generic;
using System.Linq;

namespace MisterHex.WebCrawling
{
    internal class ExternalUriFilter : IUriFilter
    {
        private Uri _root;
        public ExternalUriFilter(Uri root)
        {
            _root = root;
        }

        public List<Uri> Filter(IEnumerable<Uri> input)
        {
            return input.Where(i => !i.IsAbsoluteUri || i.Host.Equals(_root.Host)).ToList();
        }
    }
}
