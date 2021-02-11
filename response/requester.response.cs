
using backend.Models;

namespace backend.response
{

    public class RequesterResponse
    {
        public bool success { get; set; }
        public string message { get; set; }

        public typeItem data { get; set; }

    }

    public class typeItem
    {

        public HazadousSchema[] hazadousWaste { get; set; }
        public InfectionSchema[] infectionsWaste { get; set; }
        public ScrapMatrialimoSchema[] scrapImo { get; set; }
    }

    public class lastItemResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public string data { get; set; }
    }
}