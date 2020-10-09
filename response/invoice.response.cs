
using backend.Models;

namespace backend.response {
    public class InvoiceResponse {

        public bool success { get; set; }

        public string message { get; set; }

        public Invoices[] data { get; set; }

    }
}