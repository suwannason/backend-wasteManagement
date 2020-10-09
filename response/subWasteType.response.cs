
using backend.Models;

namespace backend.response {
    public class SubWasteTypeResponse {

        public bool success { get; set; }

        public string message { get; set; }

        public SubWasteType[] data { get; set; }

    }
}