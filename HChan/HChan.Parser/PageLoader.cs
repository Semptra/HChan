using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace HChan.Parser
{
    public class PageLoader
    {
        public async Task<IList<Manga>> SearchForMangaAsync(string query)
        {
            var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("do", "search"),
                new KeyValuePair<string, string>("subaction", "search"),
                new KeyValuePair<string, string>("search_start", "1"),
                new KeyValuePair<string, string>("full_search", "0"),
                new KeyValuePair<string, string>("result_from", "0"),
                new KeyValuePair<string, string>("result_num", "10000"),
                new KeyValuePair<string, string>("story", query)
            });

            var responce = await client.PostAsync("http://hentai-chan.me/index.php?do=search", content);
            var responceContent = await responce.Content.ReadAsStreamAsync();

            var htmlPage = new HtmlDocument();
            htmlPage.Load(responceContent);

            var mangaLinks = htmlPage.GetElementbyId("dle-content")
                .ChildNodes
                .Where(x => x.HasClass("content_row"))
                .Select(x => x.ChildNodes.SingleOrDefault(y => y.HasClass("manga_row1")))
                .Select(x => x.ChildNodes.SingleOrDefault(y => y.Name == "h2"))
                .Select(x => x.ChildNodes.SingleOrDefault(y => y.Name == "a"));

            var manga = mangaLinks
                .Select(x => new Manga(x.InnerText, x.Attributes.SingleOrDefault(y => y.Name == "href")?.Value))
                .Where(x => x.Url.Contains("manga") && !x.Url.Contains("games"))
                .ToList();

            return manga;
        }

        public async Task<IList<Image>> DownloadMangaAsync(string url)
        {
            string mangaUrl = url.Contains("/manga/") 
                ? url.Replace("/manga/", "/online/") 
                : url;

            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync(mangaUrl);
            var content = htmlDoc.GetElementbyId("content");
            var nodes = content.Descendants("script");
            var contentScript = nodes.FirstOrDefault(x => x.InnerHtml.Contains("fullimg"));

            var imagesRegex = new Regex("\"fullimg\":\\[(?<images>[-a-zA-Z0-9/_\\.\",:]+)\\]");
            var match = imagesRegex.Match(contentScript.InnerText);
            var imagesGroup = match.Groups["images"];

            var imageUrls = imagesGroup.Value
                .Replace("\"", string.Empty)
                .Split(',')
                .Where(x => !string.IsNullOrEmpty(x));

            var images = new List<Image>();

            using (var httpClient = new HttpClient())
            {
                int i = 1;
                foreach (var imageUrl in imageUrls)
                {
                    var bytes = await httpClient.GetByteArrayAsync(imageUrl);
                    images.Add(new Image($"{i++}.jpg", bytes));
                }
            }

            return images;
        }
    }
}
