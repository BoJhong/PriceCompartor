using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using PriceCompartor.Data;
using PriceCompartor.Models;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml;

namespace PriceCompartor.Utilities
{
    public class WebCrawler
    {
        private readonly ApplicationDbContext _context;

        public WebCrawler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetProducts(string keyword, int page)
        {
            keyword = System.Web.HttpUtility.UrlEncode(keyword);
            Task<List<Product>?>[] tasks = { ScrapeMomoAsync(keyword, page), ScrapePCHomeAsync(keyword, page) };
            List<Product>?[] result = await Task.WhenAll(tasks);
            List<Product> products = new List<Product>();
            foreach (var item in result)
            {
                if (item == null) continue;
                products.AddRange(item);
            }
            return products;
        }

        public async Task<List<Product>?> ScrapeShopeeAsync(string keyword, int pg)
        {
            var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            });
            var contextOptions = new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:124.0) Gecko/20100101 Firefox/124.0"
            };
            var context = await browser.NewContextAsync(contextOptions);
            var page = await context.NewPageAsync();
            var url = $"https://shopee.tw/search?keyword={keyword}";
            await page.GotoAsync(url);

            var shopeePlatform = _context.Platforms.FirstOrDefault(p => p.Name == "蝦皮");
            if (shopeePlatform == null) return null;

            try
            {

                // 等待直到 className 為 "shopee-search-item-result" 的元素顯示出來
                await page.WaitForSelectorAsync(".shopee-search-item-result");
                // 等待直到 className 為 "shopee-search-item-result__item" 的元素顯示出來
                await page.WaitForSelectorAsync("//*/li[@class='col-xs-2-4 shopee-search-item-result__item']/div/div/a/div/div/img");

                var elements = await page.QuerySelectorAllAsync("//*/li[@class='col-xs-2-4 shopee-search-item-result__item']");
                var products = new List<Product>();

                // 滾動頁面
                for (int i = 0; i < 8; i++)
                {
                    await page.EvaluateAsync("window.scrollBy(0,500);");
                    await Task.Delay(new Random().Next(250, 300));
                }

                if (elements == null || elements.Count == 0)
                {
                    return products;
                }

                foreach (var element in elements)
                {
                    var p = await element.QuerySelectorAsync("./div/div/a/div/div[2]/div[2]/div/div/div/div/span[2]");
                    if (p == null) continue;
                    string priceStr = await p.InnerTextAsync();
                    uint price = uint.Parse(Regex.Replace(priceStr, ",", ""));

                    var hrefElement = await element.QuerySelectorAsync("./div/div/a");
                    if (hrefElement == null) continue;
                    string? href = await hrefElement.GetAttributeAsync("href");
                    if (href == null) continue;


                    var productElement = await element.QuerySelectorAsync("./div/div/a/div/div[2]/div[1]/div[1]");
                    if (productElement == null) continue;
                    string productName = await productElement.InnerTextAsync();

                    var imageUrlElement = await element.QuerySelectorAsync("./div/div/a/div/div/img");
                    string? imageUrl = null;
                    if (imageUrlElement != null)
                    {
                        imageUrl = await imageUrlElement.GetAttributeAsync("src");
                    }

                    products.Add(new Product
                    {
                        Name = productName,
                        Link = href,
                        ImageUrl = imageUrl,
                        Price = price,
                        PlatformId = shopeePlatform.Id
                    });
                }

                await page.CloseAsync();
                return products;
            }
            catch (Exception)
            {
                await page.CloseAsync();
                return null;
            }
        }

        public async Task<List<Product>?> ScrapeMomoAsync(string keyword, int pg)
        {
            try
            {
                var momoPlatform = _context.Platforms.FirstOrDefault(p => p.Name == "Momo");
                if (momoPlatform == null) return null;

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://m.momoshop.com.tw/search.momo?searchKeyword={keyword}&curPage={pg}");
                HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                Match match = Regex.Match(responseBody, @"goodsInfoList\s*=\s*(\[.*?\]);", RegexOptions.Singleline);

                var products = new List<Product>();
                if (match.Success)
                {
                    JArray jsonArray = JArray.Parse(match.Groups[1].Value);
                    foreach (JObject item in jsonArray)
                    {
                        string priceStr = item["goodsPrice"]!.ToString();
                        priceStr = Regex.Replace(priceStr.Substring(1), ",", "").Split(' ')[0];
                        uint price = uint.Parse(priceStr);

                        string name = (string)item["goodsName"]!;
                        string imageUrl = (string)item["imgUrlArray"]?[0]!;

                        string salesInfo = item["totalSalesInfo"]!["text"]!.ToString();
                        uint sales = 0;
                        if (!string.IsNullOrEmpty(salesInfo))
                        {
                            sales = uint.Parse(Regex.Replace(salesInfo.Split(">")[1], ",", ""));
                        }

                        products.Add(new Product
                        {
                            Name = name,
                            Link = $"https://www.momoshop.com.tw/goods/GoodsDetail.jsp?i_code={item["goodsCode"]}",
                            ImageUrl = imageUrl,
                            Price = price,
                            PlatformId = momoPlatform.Id,
                            Sales = sales,
                        });
                    }
                }
                return products;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<Product>?> ScrapePCHomeAsync(string keyword, int pg)
        {
            try
            {
                var pchomePlatform = _context.Platforms.FirstOrDefault(p => p.Name == "PChome");
                if (pchomePlatform == null) return null;

                HttpClient client = new HttpClient();
                string url = $"https://ecshweb.pchome.com.tw/search/v3.3/all/results?q={keyword}&page={pg}&sort=sale/dc";
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(responseBody);

                var products = new List<Product>();

                foreach (JObject item in json["prods"]!)
                {
                    try
                    {
                        products.Add(new Product
                        {
                            Name = item["name"]!.ToString(),
                            Link = $"https://24h.pchome.com.tw/prod/{item["Id"]}",
                            ImageUrl = $"https://cs-a.ecimg.tw/{item["picS"]}",
                            Price = uint.Parse(item["price"]!.ToString()),
                            PlatformId = pchomePlatform.Id
                        });
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                return products;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
