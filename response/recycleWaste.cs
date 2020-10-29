

using backend.Models;

namespace backend.response
{
    public class RecycleWesteResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public Waste[] data { get; set; }

    }
}