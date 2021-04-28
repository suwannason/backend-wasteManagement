
namespace backend.request
{
    public class Login {
        public string username { get; set; }

        public string password { get; set; }
    }

    public class uploadFile {
        public Microsoft.AspNetCore.Http.IFormFile file { get; set; }
    }
}