
namespace backend.request
{

    public class ReuqesterREQ
    {
        public string date { get; set; }
        public string time { get; set; }
        public string dept { get; set; }
        public string div { get; set; }

        // FORM A
        public FormA[] formA { get; set; }
        public FormB[] fromB { get; set; }
    }

    public class FormA
    {
        public string phase { get; set; }
        public string wasteName { get; set; }
        public string wasteGroup { get; set; }
        public string contracterCompany { get; set; }
        public string bindingType { get; set; }
        public string cptMainType { get; set; }
        public string wasteType { get; set; }
        public string boiType { get; set; }
        public string partNormalType { get; set; }
        public string productType { get; set; }
        public string lotNo { get; set; }
        public string companyApproveNo { get; set; }
        public string containerType { get; set; }
        public string qtyOfContainer { get; set; }
        public string weightPerContainer { get; set; }
        public string totalWeight { get; set; }
    }
    public class FormB {
        public string item { get; set; }
    }
}