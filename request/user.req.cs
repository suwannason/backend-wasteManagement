
namespace backend.request
{
    public class Login {
        public string username { get; set; }

        public string password { get; set; }
    }

    public class uploadFile {
        public Microsoft.AspNetCore.Http.IFormFile file { get; set; }
    }

    public class uploadFileMulti {
        public Microsoft.AspNetCore.Http.IFormFile[] files { get; set; }
    }

    public class itcPrepareInvoice {
        public Microsoft.AspNetCore.Http.IFormFile[] files { get; set; }
        public string invoiceNo { get; set; }
        public string dueDate { get; set; }
    }
}