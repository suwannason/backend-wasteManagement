
using backend.Models;

namespace backend.response {

    public class RequesterResponse {
        public bool success { get; set; }
        public string message { get; set; }

        public HazadousSchema[] data { get; set; }
    }
}