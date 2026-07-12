using System;

namespace RegexBuilder.Models
{
    public class FavoritePattern
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
