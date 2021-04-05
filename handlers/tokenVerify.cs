

using Microsoft.AspNetCore.Authorization;
using System.Security.Principal;
using backend.request;
using Microsoft.AspNetCore.Mvc;



namespace backend.Controllers
{

    [Authorize]
    public class TokenVerify
    {
        public bool permission(string feature, string action)
        {

            return true;
        }

        public Profile currentUser()
        {

            return new Profile
            {
                band = "",
                name = "",
                // empNo = User.FindFirst("username")?.Value,
                dept = "",
                div = "",
                tel = ""
            };
        }
    }
}