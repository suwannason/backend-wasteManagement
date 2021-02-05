
using backend.Models;
using backend.Services;
using backend.response;
using backend.request;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace backend.Controllers
{

    [Route("fae-part/[controller]")]
    [ApiController]
    [Authorize]
    public class requesterController : ControllerBase
    {
        private readonly HazadousService _hazadous;
        private readonly InfectionsService _infections;

        RequesterResponse res = new RequesterResponse();

        public requesterController(HazadousService req, InfectionsService infect)
        {
            _hazadous = req;
            _infections = infect;
        }

        [HttpPost]
        public async Task<ActionResult<RequesterResponse>> create(ReuqesterREQ body)
        {
            Profile req_prepare = new Profile();

            req_prepare.empNo = User.FindFirst("username")?.Value;
            req_prepare.band = User.FindFirst("band")?.Value;
            req_prepare.dept = User.FindFirst("dept")?.Value;
            req_prepare.div = User.FindFirst("div")?.Value;
            req_prepare.name = User.FindFirst("name")?.Value;
            req_prepare.tel = User.FindFirst("tel")?.Value;

            body.div = User.FindFirst("div")?.Value;
            body.dept = User.FindFirst("dept")?.Value;

            string trackingId = System.Guid.NewGuid().ToString();

            List<Task> onCreate = new List<Task>();

            if (body.hazardous.Length > 0)
            {
                onCreate.Add(Task.Run(() => { _hazadous.create(body, req_prepare, trackingId); }));
            }
            if (body.infections.Length > 0)
            {
                onCreate.Add(Task.Run(() => { _infections.create(body, req_prepare, trackingId); }));
            }

            Task created = Task.WhenAll(onCreate.ToArray());
            await created;
            res.success = true;
            res.message = "create requester data success";
            return Ok(res);
        }

        [HttpGet]
        public ActionResult<RequesterResponse> getAll()
        {
            res.success = true;
            res.message = "Get data success";

            res.data = _hazadous.getAll().ToArray();

            return Ok(res);
        }
        [HttpPut("status")]
        public ActionResult<RequesterResponse> updateStatus(UpdateStatusFormRequester body)
        {

            foreach (string item in body.id)
            {
                _hazadous.updateStatus(item, body.status);
            }
            return Ok(res);
        }
    }

}