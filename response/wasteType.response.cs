
using backend.Models;

namespace backend.response {
    public class WasteTypeResponse {

        public bool success { get; set; }

        public string message { get; set; }

        public WasteType[] data { get; set; }

    }
}