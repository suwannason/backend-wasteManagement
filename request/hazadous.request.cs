
namespace backend.request
{
    public class HazadousFAEprepare {
        public string id { get; set; }
        public string description { get; set; }
        public HazadousFAEprepareItem[] items { get; set; }
    }
    public class HazadousFAEprepareItem
    {
        // public string id { get; set; }
        public string no { get; set; }
        public bool allowed { get; set; }
        public bool burn { get; set; }
        public bool recycle { get; set; }
    }
}