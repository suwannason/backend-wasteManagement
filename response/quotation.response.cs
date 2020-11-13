
using backend.Models;

namespace backend.response
{
    public class QuotationResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public Quotation[] data { get; set; }

    }
}