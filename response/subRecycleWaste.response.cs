
using backend.Models;

namespace backend.response
{
    class SubRecycleResponse {
        public bool success { get; set; }

        public string message { get; set; }

        public SubRecycleWaste[] data { get; set; }
    }
}