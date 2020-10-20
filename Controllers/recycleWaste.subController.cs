

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.response;
using backend.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("fae-part/[controller]")]
    public class SubRecycleWasteController : ControllerBase
    {

        private readonly SubRecycleService _subRecycleService;

        RecycleWesteResponse res = new RecycleWesteResponse();

        public SubRecycleWasteController(SubRecycleService recycleService)
        {
            _subRecycleService = recycleService;
        }

        [HttpPost]
        public ActionResult<DefalutResponse> Create(SubRecycleWaste body)
        {
            try
            {
                SubRecycleWaste created = _subRecycleService.Create(body);
                List<SubRecycleWaste> data = new List<SubRecycleWaste>();
                data.Add(created);

                res.success = true;
                res.message = "Insert success";

                return Ok(res);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.StackTrace);
                return StatusCode(500);
            }
        }


        [HttpPut("{id}")]
        public IActionResult Update(string id, SubRecycleWaste body)
        {
            try
            {
                _subRecycleService.Update(id, body);
                res.success = true;
                res.message = "update success";

                return Ok(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                StatusCode(500);
                return Forbid();
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            _subRecycleService.Remove(id);

            res.success = true;
            res.message = "Delete success";

            return Ok(res);
        }


    }
}