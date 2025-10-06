using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string ExternalId { get; set; } // id do site
        public string Title { get; set; }
        public string Category { get; set; }
        public string Store { get; set; } // Amazon, Magalu
        public List<Offer> Offers { get; set; } = new();
    }
}
