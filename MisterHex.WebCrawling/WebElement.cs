namespace MisterHex.WebCrawling
{
    public class WebElement
    {
        public WebElement(string innerHTML, string outerHTML, string innerText, string textContent)
        {
            InnerHTML = innerHTML;
            OuterHTML = outerHTML;
            InnerText = innerText;
            TextContent = textContent;
        }

        public string InnerHTML { get; private set; }
        public string OuterHTML { get; private set; }
        public string InnerText { get; private set; }
        public string TextContent { get; private set; }
    }
}