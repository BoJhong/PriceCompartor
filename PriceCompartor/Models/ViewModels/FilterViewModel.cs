
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PriceCompartor.Models.ViewModels
{
    public class FilterViewModel
    {
        [Display(Name = "最低價格")]
        [Range(0, double.MaxValue, ErrorMessage = "價格必須是正整數")]
        public int? MinPrice { get; set; }

        [Display(Name = "最高價格")]
        [Range(0, double.MaxValue, ErrorMessage = "價格必須是正整數")]
        public int? MaxPrice { get; set; }

        [Display(Name = "排序")]
        public SortOrderType SelectedSortOrder { get; set; } = SortOrderType.None;

        [Display(Name = "平台")]
        public required List<SelectListItem> PlatformCheckboxes { get; set; }
    }
}
