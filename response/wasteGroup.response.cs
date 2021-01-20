
using backend.Models;

namespace backend.response {
    public class WasteGroupResponse {

        public bool success { get; set; }

        public string message { get; set; }

        public WasteGroup[] data { get; set; }

    }
}