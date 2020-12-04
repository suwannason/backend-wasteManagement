
using backend.Models;
using backend.Services;
using backend.response;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [Authorize]
    [Route("fae-part/[controller]")]
    [ApiController]
    public class wasteNameController : ControllerBase
    {
        private readonly WasteNameService _wastenameService;

        WastenameResponse res = new WastenameResponse();

        public wasteNameController(WasteNameService wasteNameService)
        {
            _wastenameService = wasteNameService;
        }

        [HttpGet]
        public ActionResult<WastenameResponse> Get()
        {
            List<WasteName> data = _wastenameService.Get();

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

        [HttpGet("{id}", Name = "GetWaste")]
        public ActionResult<WasteName> Get(string id)
        {
            var book = _wastenameService.Get(id);

            if (book == null)
            {
                return NotFound();
            }
            return book;
        }

        [HttpPost]
        public ActionResult<WastenameResponse> Create(WasteName book)
        {
            try
            {
                WasteName created = _wastenameService.Create(book);
                List<WasteName> data = new List<WasteName>();
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
        public IActionResult Update(string id, WasteName bookIn)
        {
            var book = _wastenameService.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            Console.Write(id);
            _wastenameService.Update(id, bookIn);

            res.success = true;
            res.message = "Update success";
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var book = _wastenameService.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            _wastenameService.Remove(book._id);

            res.success = true;
            res.message = "Delete success";

            return Ok(res);
        }
    }
}