using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto
{
    public class OfferInput
    {
        public string Title { get; set; } = default!;
        public string Url { get; set; } = default!;
        public string Store { get; set; } = default!;
        public string Category { get; set; } = default!;
        public decimal Price { get; set; }
    }
}
