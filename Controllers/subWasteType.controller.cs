
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

    public class SubWasteTypeController : ControllerBase
    {
        private readonly SubWasteTypeService _subWasteType;

        SubWasteTypeResponse res = new SubWasteTypeResponse();

        public SubWasteTypeController(SubWasteTypeService SubWasteTypeService)
        {
            _subWasteType = SubWasteTypeService;
        }

        [HttpGet]
        public ActionResult<SubWasteTypeResponse> Get()
        {
            List<SubWasteType> data = _subWasteType.Get();

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
        public ActionResult<SubWasteTypeResponse> Create(SubWasteType book)
        {
            try
            {
                char[] firstLetter = book.typeName.ToCharArray();
                firstLetter[0] = char.ToUpper(firstLetter[0]);
                book.typeName = new String(firstLetter);

                SubWasteType created = _subWasteType.Create(book);
                List<SubWasteType> data = new List<SubWasteType>();
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
        public IActionResult Update(string id, SubWasteType bookIn)
        {
            var book = _subWasteType.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            _subWasteType.Update(id, bookIn);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var book = _subWasteType.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            _subWasteType.Remove(book._id);

            res.success = true;
            res.message = "Delete success";

            return Ok(res);
        }
    }
}