
using backend.Models;
using backend.Services;
using backend.response;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    // [Authorize]
    [Route("fae-part/[controller]")]
    [ApiController]
    
    public class wasteGroupController : ControllerBase
    {
        private readonly wasteGroupService _wasteGrouService;


        WasteGroupResponse res = new WasteGroupResponse();

        public wasteGroupController(wasteGroupService wasteGroupService)
        {
            _wasteGrouService = wasteGroupService;

        }

        [HttpGet]
        public ActionResult<WasteGroupResponse> Get()
        {
            List<WasteGroup> data = _wasteGrouService.Get();

            res.success = true;
            res.data = data.ToArray();
            if (data.Count == 0)
            {
                res.message = "Notfound Data.";
                return NotFound(res);
            }
            res.message = "Get company success";
            return Ok(res);
        }

        [HttpGet("{id}")]
        public ActionResult<WasteGroup> Get(string id)
        {
            var book = _wasteGrouService.Get(id);

            if (book == null)
            {
                return NotFound();
            }
            return book;
        }

        [HttpPost]
        public ActionResult<WasteGroupResponse> Create(WasteGroup book)
        {
            try
            {
                WasteGroup created = _wasteGrouService.Create(book);
                List<WasteGroup> data = new List<WasteGroup>();
                data.Add(created);

                res.success = true;
                res.message = "Insert success";
                res.data = data.ToArray();

                return Ok(res);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, WasteGroup bookIn)
        {
            var book = _wasteGrouService.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            Console.Write(id);
            _wasteGrouService.Update(id, bookIn);

            res.success = true;
            res.message = "Update success";
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var book = _wasteGrouService.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            _wasteGrouService.Remove(book._id);

            res.success = true;
            res.message = "Delete success";

            return Ok(res);
        }

        [HttpGet("mainType")]

        public IActionResult getMainType()
        {
            var data = _wasteGrouService.getMaintype();

            return Ok(data);
        }
    }
}