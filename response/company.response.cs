


using backend.Models;

namespace backend.response
{
    public class CompanyResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public Companies[] data { get; set; }
    }

}