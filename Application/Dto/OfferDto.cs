using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto
{
   public class OfferDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Store { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
