
using backend.Models;

namespace backend.response {
    public class UserResponse {

        public bool success { get; set; }

        public string message { get; set; }

        public User[] data { get; set; }

    }
}