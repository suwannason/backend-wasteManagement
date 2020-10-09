
// using backend.Models;

namespace backend.response {
    public class CarResponse {

        public bool success { get; set; }

        public string message { get; set; }

        public Cars[] data { get; set; }

    }
}