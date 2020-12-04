


using backend.Models;

namespace backend.response
{
    public class WastenameResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public WasteName[] data { get; set; }
    }

}