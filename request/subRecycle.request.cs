
using Microsoft.AspNetCore.Http;

namespace backend.request {

    public class RequestSubRecycle {
        public string wastype { get; set; }
        public string factory { get; set; }
        public string allWeight { get; set; }
        public string containerWeight { get; set; }

        public string total { get; set; }

        public string idMapping { get; set; }
        public  IFormFile[] files { get; set; }
    }
}