
using backend.Models;
using backend.Services;
using backend.response;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;

namespace backend.Controllers
{
    [Route("fae-part/[controller]")]
    [ApiController]

    public class wasteTypeController : ControllerBase
    {
        private readonly WasteTypeService _WasteTypeService;

        WasteTypeResponse res = new WasteTypeResponse();

        public wasteTypeController(WasteTypeService WasteTypeService)
        {
            _WasteTypeService = WasteTypeService;
        }

        [HttpGet]
        public ActionResult<WasteTypeResponse> Get()
        {
            List<WasteType> data = _WasteTypeService.Get();

            res.success = true;
            res.data = data.ToArray();
            if (data.Count == 0)
            {
                res.message = "Notfound Data.";
                return NotFound(res);
            }
            res.message = "Get car success";
            return Ok(res);
        }

        [HttpPost]
        public ActionResult<WasteTypeResponse> Create(WasteType book)
        {
            try
            {
                char[] firstLetter = book.typeName.ToCharArray();
                firstLetter[0] = char.ToUpper(firstLetter[0]);
                book.typeName = new String(firstLetter);

                WasteType created = _WasteTypeService.Create(book);
                List<WasteType> data = new List<WasteType>();
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
        public IActionResult Update(string id, WasteType bookIn)
        {
            var book = _WasteTypeService.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            _WasteTypeService.Update(id, bookIn);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var book = _WasteTypeService.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            _WasteTypeService.Remove(book._id);

            res.success = true;
            res.message = "Delete success";

            return Ok(res);
        }
    }
}