using backend.Models;
namespace backend.response {

    public class pricingResponse {
        public bool success { get; set; }
        public string message { get; set; }
        public pricingSchema[] data { get;set;}
        
    }
}