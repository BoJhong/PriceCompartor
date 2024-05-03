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
    public class DetailsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager { get; }

        public DetailsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index(int? id)
        {
            if (id == null) return NotFound();

            Product? product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Platform)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.AppUser)
                .FirstOrDefault(p => p.Id == id);

            if (product == null) return NotFound();

            product.Comments = product.Comments.OrderBy(c => c.Time).ToList();

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

                ApplicationUser? user = await _userManager.GetUserAsync(User);

                if (user == null) return NotFound();

                Product? product = _context.Products.FirstOrDefault(p => p.Id == id);

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
                _context.Add(comment);

                product.TotalRating += rating;
                product.TotalRatingCount++;
                product = UpdateProductRating(product);
            

                _context.Update(product);

                await _context.SaveChangesAsync();
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
                var user = await _userManager.GetUserAsync(User);

                if (user == null) return NotFound();

                var product = _context.Products.FirstOrDefault(p => p.Id == id);

                if (product == null) return NotFound();

                var comment = _context.Comments.FirstOrDefault(c => c.Id == commentId && c.ProductId == id);

                bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                if (comment == null || (!isAdmin && comment.AppUserId != user.Id)) return NotFound();

                _context.Remove(comment);

                product.TotalRatingCount--;
                product.TotalRating -= comment.Rating;
                product = UpdateProductRating(product);

                _context.Update(product);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { id });
        }

        [HttpPost]
        public async Task GetAIAnalysis(int id)
        {
            Product? product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Platform)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.AppUser)
                .FirstOrDefault(p => p.Id == id);

            if (product == null) return;

            string commentsStr = $"[{
                string.Join(@"\r\n",
                    product.Comments
                        .FindAll(c => !string.IsNullOrEmpty(c.Content))
                        .Select(c => $"評論者：\"{c.AppUser?.Nickname ?? "無名稱"}\"，評分：{c.Rating}/5，評論內容：\"{c.Content ?? "無"}\"")
                )
            }]";
            string prompt = string.Format(@"
# 角色
你是一位精通於分析商品與提供詳盡購物資訊的專家。 你有能力深入探討商品的細節，並從中分析價格與提供最佳的購買建議。

## 技能
### 產品詳述
- 透過產品連結 {1} 了解並研究商品 {2} 的規格與功能特性。
- 提供商品的詳細規格與特性介紹，避免與規格表的重複，讓使用者更清楚產品的特性。

### 購買建議
- 根據評價 {4}，整合商品的特性、使用者的需求與市場狀況，提供使用者專業的購買建議。

### 價格分析
- 分析商品 {2} 在電商平台 {0} 的價格 {3} 與官方網站價格是否相符。 當官方價格無法確認時，以市場潮流與商品特性為基礎，為使用者提供價值評估。
- 比較官方網站與電商平台 {0} 間的差異，以及提供如何降低購買風險、取得最大利益的策略。

## 限制條件：
- 維持中立客觀的立場，只提供基於數據分析的建議，不涉及商品推銷。
- 遵從消費者權益保護法，提醒用戶比較購物並理性消費。
- 繁體中文是你的主要語言

以這樣的方式，你的主要任務是在確保所有提供的資訊準確無誤的同時，為使用者提供最詳細、最合理的商品購買資訊與建議。
            ", product.Platform?.Name, product.Link, product.Name, product.Price, commentsStr);

            WebCrawler webCrawler = new(_context);
            if (product.OId != null)
            {
                List<PriceHistroy> priceHistory = await webCrawler.GetPriceHistory(product.OId);
                if (priceHistory.Count > 0)
                {
                    string priceHistoryStr = string.Join("\n", priceHistory.Select(p => $"日期：{p.DateTime}，價格：{p.Price}"));
                    prompt += $@"
                        歷史90天價格浮動：{priceHistoryStr}
                    ";
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
                await Response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes(s.PadRight(1024, '\0')));
                await Response.Body.FlushAsync();
            });

            await model.StreamContentAsync(prompt, action);
        } 
    }
}
