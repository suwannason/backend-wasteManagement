
using backend.Models;

namespace backend.response
{
    public class UserResponse
    {

        public bool success { get; set; }

        public string message { get; set; }

        public string token { get; set; }
        public UserSchema[] data { get; set; }
    }

    public class LDAPresponse
    {
        public LDAPprofile profile { get; set; }
        public systemList systems { get; set; }
    }

    public class LDAPprofile
    {
        public string empNo { get; set; }
        public string fnameEn { get; set; }
        public string lnameEn { get; set; }
        public string deptCode { get; set; }
        public string deptShortName { get; set; }
        public string deptFullName { get; set; }
        public string divShortName { get; set; }
        public string band { get; set; }
    }
    public class systemList
    {
        public int systemId { get; set; }
        public string systemName { get; set; }
    }

    public class HRMS
    {

        public string empNo { get; set; }
        public string fnameEn { get; set; }
        public string lnameEn { get; set; }
        public string deptCode { get; set; }
        public string deptShortName { get; set; }
        public string deptFullName { get; set; }
        public string divShortName { get; set; }
        public string band { get; set; }

    }

    public class AD_API
    {
        public bool success { get; set; }
        public HRMS data { get; set; }
    }
}