using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Offer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Store { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Domain { get; set; } = string.Empty;
    }
}
