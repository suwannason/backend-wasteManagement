
namespace backend.request
{

    public class RejectRequest
    {
        public string[] id { get; set; }
        public string commend { get; set; }
    }

    public class editByUpload
    {
        public Microsoft.AspNetCore.Http.IFormFile file { get; set; }
        public string sourceFilename { get; set; }
    }

    public class recalCulateSummary
    {
        public string lotNo { get; set; }
        public string boiType { get; set; }
    }
}