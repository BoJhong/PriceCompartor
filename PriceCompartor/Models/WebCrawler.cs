using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using PriceCompartor.Infrastructure;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PriceCompartor.Models
{
    public class WebCrawler(ApplicationDbContext context)
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IQueryable<Product>> GetProducts(string keyword, int page)
        {
            keyword = System.Web.HttpUtility.UrlEncode(keyword);

            Task<List<Product>?>[] tasks = [
                ScrapeMomoAsync(keyword, page),
                ScrapePCHomeAsync(keyword, page)
            ];

            List<Product>?[] result = await Task.WhenAll(tasks);
            List<Product> products = [];

            foreach (var item in result)
            {
                if (item == null) continue;
                products.AddRange(item);
            }

            return products.AsQueryable();
        }

        public async Task<List<Product>?> ScrapeShopeeAsync(string keyword, int pg)
        {
            var shopeePlatform = _context.Platforms.FirstOrDefault(p => p.Name == "Shopee");
            if (shopeePlatform == null) return null;

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
                    int price = int.Parse(Regex.Replace(priceStr, ",", ""));

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
                        PlatformId = shopeePlatform.Id,
                        OId = "shopee-"
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

                using (HttpClient client = new HttpClient())
                {
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
                            string? priceStr = item["goodsPrice"]?.ToString();
                            string? name = item["goodsName"]?.ToString();
                            string? imageUrl = item["imgUrlArray"]?[0]?.ToString();
                            string? goodsCode = item["goodsCode"]?.ToString();

                            if (priceStr == null || name == null || imageUrl == null || goodsCode == null) continue;

                            int price = int.Parse(Regex.Replace(priceStr.Substring(1), ",", "").Split(' ')[0]);

                            string? salesInfo = item["totalSalesInfo"]?["text"]?.ToString();
                            int sales = 0;
                            if (!string.IsNullOrEmpty(salesInfo))
                            {
                                sales = int.Parse(Regex.Replace(salesInfo.Split(">")[1], ",", ""));
                            }

                            products.Add(new Product
                            {
                                Name = name,
                                Link = $"https://www.momoshop.com.tw/goods/GoodsDetail.jsp?i_code={item["goodsCode"]}",
                                ImageUrl = imageUrl,
                                Price = price,
                                PlatformId = momoPlatform.Id,
                                Sales = sales,
                                OId = $"tw_pec_momoshop-{goodsCode}"
                            });
                        }
                    }
                    return products;
                }
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

                using (HttpClient client = new HttpClient())
                {
                    string url = $"https://ecshweb.pchome.com.tw/search/v3.3/all/results?q={keyword}&page={pg}&sort=sale/dc";
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(responseBody);

                    var products = new List<Product>();

                    if (int.Parse(json["totalRows"]!.ToString()) == 0 || json["prods"] == null)
                    {
                        return null;
                    }

                    foreach (JObject item in json["prods"]!)
                    {
                        products.Add(new Product
                        {
                            Name = item["name"]!.ToString(),
                            Link = $"https://24h.pchome.com.tw/prod/{item["Id"]}",
                            ImageUrl = $"https://cs-a.ecimg.tw/{item["picS"]}",
                            Price = int.Parse(item["price"]!.ToString()),
                            PlatformId = pchomePlatform.Id,
                            OId = $"tw_ec_pchome24h-{item["Id"]}"
                        });
                    }
                    return products;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<PriceHistroy>> GetPriceHistory(string id)
        {
            using (HttpClient client = new HttpClient())
            {
                var data = new
                {
                    days = 90,
                    item = new string[] { id }
                };

                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://extension.biggo.com/api/product_price_history.php", content);

                string responseBody = await response.Content.ReadAsStringAsync();

                var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;
                List<PriceHistroy> priceHistoryResult = new List<PriceHistroy>();

                // 判斷歷史價格的回傳結果是否成功
                if (root.TryGetProperty("result", out JsonElement resultProperty))
                {
                    // 如果回傳結果為 false，代表沒有歷史價格資料
                    if (resultProperty.ValueKind == JsonValueKind.False)
                    {
                        return priceHistoryResult;
                    }
                }

                var priceHistory = root.GetProperty(id).GetProperty("price_history");


                foreach (var history in priceHistory.EnumerateArray())
                {
                    int price = history.GetProperty("y").GetInt32();
                    priceHistoryResult.Add(new PriceHistroy
                    {
                        Price = price,
                        Timestamp = history.GetProperty("x").GetInt64()
                    }
                    );
                }

                return priceHistoryResult;
            }
        }
    }
}
