

using backend.Models;

namespace backend.response
{
    public class DisposalWesteResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public DisposalWaste[] data { get; set; }

    }
}