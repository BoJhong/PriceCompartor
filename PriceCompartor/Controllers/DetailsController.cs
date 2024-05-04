using GenerativeAI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PriceCompartor.Infrastructure;
using PriceCompartor.Models;
using PriceCompartor.Models.GeminiModels;

namespace PriceCompartor.Controllers
{
    public class DetailsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : Controller
    {
        public IActionResult Index(int? id)
        {
            if (id == null) return NotFound();

            Product? product = context.Products
                .Include(p => p.Category)
                .Include(p => p.Platform)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.AppUser)
                .FirstOrDefault(p => p.Id == id);

            if (product == null) return NotFound();

            product.Comments = [.. product.Comments.OrderBy(c => c.Time)];

            return View(product);
        }

        [NonAction]
        private Product UpdateProductRating(Product product)
        {
            if (product.TotalRatingCount > 0 && product.TotalRating > 0)
            {
                // 計算評分平均值
                product.Rating = (float)product.TotalRating / product.TotalRatingCount;

                // 捨去小數點第一位後面的數字
                product.Rating = (float)Math.Floor(product.Rating * 10) / 10;
            }
            else
            {
                // 只要有其中一個評分的數值異常（低於0），則直接全部重設為0
                product.TotalRatingCount = 0;
                product.TotalRating = 0;
                product.Rating = 0;
            }

            return product;
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int id, string? content, int rating = 0)
        {
            if (ModelState.IsValid)
            {
                if (rating < 1 || rating > 5) return BadRequest();

                ApplicationUser? user = await userManager.GetUserAsync(User);

                if (user == null) return NotFound();

                Product? product = context.Products.FirstOrDefault(p => p.Id == id);

                if (product == null) return NotFound();

                Comment comment = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Time = DateTime.Now,
                    Rating = rating,
                    Content = content,
                    ProductId = id,
                    AppUserId = user.Id
                };
                context.Add(comment);

                product.TotalRating += rating;
                product.TotalRatingCount++;
                product = UpdateProductRating(product);
            

                context.Update(product);

                await context.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveComment(int id, string commentId)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);

                if (user == null) return NotFound();

                var product = context.Products.FirstOrDefault(p => p.Id == id);

                if (product == null) return NotFound();

                var comment = context.Comments.FirstOrDefault(c => c.Id == commentId && c.ProductId == id);

                bool isAdmin = await userManager.IsInRoleAsync(user, "Admin");

                // 如果找不到評論，並且不是自己發的評論也不是管理員，則取消操作
                if (comment == null || (!isAdmin && comment.AppUserId != user.Id)) return NotFound();

                context.Remove(comment);

                product.TotalRatingCount--;
                product.TotalRating -= comment.Rating;
                product = UpdateProductRating(product);

                context.Update(product);

                await context.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { id });
        }

        [HttpPost]
        public async Task GetAIAnalysis(int id)
        {
            Product? product = context.Products
                .Include(p => p.Category)
                .Include(p => p.Platform)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.AppUser)
                .FirstOrDefault(p => p.Id == id);

            if (product == null) return;

            string commentsStr = $"[{
                string.Join("\n",
                    product.Comments
                        .FindAll(c => !string.IsNullOrEmpty(c.Content))
                        .Select(c => $"評論者：\"{c.AppUser?.Nickname ?? "無名稱"}\"，評分：\"{c.Rating}/5\"，評論內容：\"{c.Content ?? "無"}\"")
                )
            }]";
            string prompt = string.Format(@"
# 角色
你是一位在商品分析和購物資訊提供方面精通的專家。你有能力深入探討商品的細節，并從中提供有關價格和建議的詳細購買資訊。

## 技能
### 技能 1: 商品詳細介紹
- 兩組数据 {1} 和 {2} 將用於瞭解和研究商品的規格和功能特點
- 將產品的規格和特點詳細地介紹給用戶。

### 技能 2: 購買建議
- 根據評論 {4}，將商品特點、用戶需求和市場狀況整合後，提出專業購買建議。

### 技能 3: 價格分析
- 為用戶提供商品 {2} 在電商平台 {0} 的價格分析，將市場趨勢和商品特點作為評價基準。
- 目前日期是：{5}。如果即將在2個月內舉行慣例的折扣活動，則提供該折扣活動的日期。如果沒有，則不需要提供。
- 提供策略讓用戶知道如何降低購買風險並獲得最大的利益，並給出建議的購買時間。

## 限制條件：
- 以中立的立場提供服務，不進行商品銷售。
- 請以繁體中文回答。

如此，你的兩項主要工作是確保提供的所有資訊準確無誤，並給用戶提供最詳細，最合理的購物資訊和建議。
            ", product.Platform?.Name, product.Link, product.Name, product.Price, commentsStr, DateTime.Now);

            WebCrawler webCrawler = new(context);
            if (product.OId != null)
            {
                List<PriceHistroy> priceHistory = await webCrawler.GetPriceHistory(product.OId);
                if (priceHistory.Count > 0)
                {
                    string priceHistoryStr = string.Join("\n", priceHistory.Select(p => $"日期：{p.DateTime}，價格：{p.Price}"));
                    prompt += $"\n該商品在該電商平台歷史90天的價格浮動：{priceHistoryStr}";
                }
            }

            var model = new GenerativeModel(AIHttpClient.GetApiKey(), "gemini-1.5-pro-latest");
            //or var model = new GeminiProModel(apiKey);

            var action = new Action<string>(async s =>
            {
                /*
                 * 即使呼叫了 FlushAsync()，Kestrel 會等累積到 1024 Bytes 後才真的送出
                 * 因此使用 PadRight(1024, ' ') 將輸出內容補空白到 1024 個字元
                 */
                await Response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes(s.PadRight(1024, '　')));
                await Response.Body.FlushAsync();
            });

            await model.StreamContentAsync(prompt, action);
        } 
    }
}
