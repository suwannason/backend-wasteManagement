
using Microsoft.AspNetCore.Mvc;

using backend.Services;
using backend.response;
using backend.request;
using System;

namespace backend.Controllers
{

    [Route("fae-part/[controller]")]
    [ApiController]
    public class routingController : ControllerBase
    {
        private readonly HazadousService _hazadous;
        private readonly InfectionsService _infections;
        private readonly ScrapMatrialImoService _scrapImo;

        public routingController(HazadousService req, InfectionsService infect, ScrapMatrialImoService scrapImo)
        {
            _hazadous = req;
            _infections = infect;
            _scrapImo = scrapImo;
        }

        [HttpGet("itc/{status}")]
        public ActionResult dataItcByStatus(string status)
        {
            try
            {
                return Ok(status);
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }

        [HttpGet("fae/{status}")]
        public ActionResult dataFaeByStatus(string status)
        {
            try
            {
                return Ok(status);
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }

        [HttpPatch("itc/checked")]
        public ActionResult updateToChecked(routingLotUpdate body)
        {
            try
            {
                Profile user = new Profile();
                user.empNo = User.FindFirst("username")?.Value;
                user.band = User.FindFirst("band")?.Value;
                user.dept = User.FindFirst("dept")?.Value;
                user.div = User.FindFirst("div")?.Value;
                user.name = User.FindFirst("name")?.Value;
                user.tel = User.FindFirst("tel")?.Value;
                return Ok("checked");
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }

        [HttpPatch("itc/approved")]
        public ActionResult updateToApproved(routingLotUpdate body)
        {
            try
            {
                Profile user = new Profile();
                user.empNo = User.FindFirst("username")?.Value;
                user.band = User.FindFirst("band")?.Value;
                user.dept = User.FindFirst("dept")?.Value;
                user.div = User.FindFirst("div")?.Value;
                user.name = User.FindFirst("name")?.Value;
                user.tel = User.FindFirst("tel")?.Value;

                return Ok("approved");
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }

        [HttpDelete("reject/{lotNo}")]
         public ActionResult reject(string lotNo)
        {
            try
            {
                Profile user = new Profile();
                user.empNo = User.FindFirst("username")?.Value;
                user.band = User.FindFirst("band")?.Value;
                user.dept = User.FindFirst("dept")?.Value;
                user.div = User.FindFirst("div")?.Value;
                user.name = User.FindFirst("name")?.Value;
                user.tel = User.FindFirst("tel")?.Value;

                return Ok("reject");
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }
    }
}