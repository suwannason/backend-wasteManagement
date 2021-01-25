
using backend.Models;
using backend.Services;
using backend.response;
using backend.request;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;


namespace backend.Controllers
{

    [Route("fae-part/[controller]")]
    [ApiController]
    [Authorize]
    public class requesterController : ControllerBase
    {
        private readonly RequesterService _requester;

        RequesterResponse res = new RequesterResponse();

        public requesterController(RequesterService req)
        {
            _requester = req;
        }

        [HttpPost]
        public ActionResult<RequesterResponse> create(ReuqesterREQ body)
        {
            Profile req_prepare = new Profile();

            req_prepare.empNo = User.FindFirst("username")?.Value;
            req_prepare.band = User.FindFirst("band")?.Value;
            req_prepare.dept = User.FindFirst("dept")?.Value;
            req_prepare.div = User.FindFirst("div")?.Value;
            req_prepare.name = User.FindFirst("name")?.Value;

            _requester.create(body, req_prepare);
            res.success = true;
            res.message = "create requester data success";
            return Ok(res);
        }

        [HttpGet]

        public ActionResult<RequesterResponse> getAll() {
            res.success = true;
            res.message = "Get data success";

            res.data = _requester.getAll().ToArray();

            return Ok(res);
        }
    }

}