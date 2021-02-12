
namespace backend.request {

    public class routingLotUpdate {

        public string[] lotNo { get; set; }
    }

    public class FaePrepareRequester {

        public string lotNo { get; set; }
        public string howTodestory { get; set; }
        public bool allowToDestroy { get; set; }
        public string remark { get; set; }
    }
}