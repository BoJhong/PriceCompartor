﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriceCompartor.Models
{
    public class Product
    {
        [Key]
        public long Id { get; set; }

        public required string Link { get; set; }

        public string? ImageUrl { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        public required int Price { get; set; }

        public int Sales { get; set; } = 0;

        public int Rating { get; set; } = 0;

        public string? Address { get; set; }

        public string? OId { get; set; }

        [ForeignKey("Categories")]
        public int? CategoryId { get; set; }

        public virtual Category? Categories { get; set; }

        [ForeignKey("Platforms")]
        public required int PlatformId { get; set; }

        public virtual Platform? Platforms { get; set; }

        public string PlatformImageSrc
        {
            get
            {
                return Platforms?.ImageSrc ?? string.Empty;
            }
        }
    }
}
