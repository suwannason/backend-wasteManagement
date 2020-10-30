
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

    public class carController : ControllerBase {
        private readonly CarService _carService;

        CarResponse res = new CarResponse();

        public carController(CarService carService)
        {
            _carService = carService;
        }

        [HttpGet]
        public ActionResult<CarResponse> Get()
        {
            List<Cars> data = _carService.Get();

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

        [HttpGet("{id}")]
        public ActionResult<Cars> Get(string id)
        {
            var book = _carService.Get(id);

            if (book == null)
            {
                return NotFound();
            }
            return book;
        }

        [HttpPost]
        public ActionResult<CarResponse> Create(Cars book)
        {
            try
            {
                Cars created = _carService.Create(book);
                List<Cars> data = new List<Cars>();
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
        public IActionResult Update(string id, Cars bookIn)
        {
            var book = _carService.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            _carService.Update(id, bookIn);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var book = _carService.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            _carService.Remove(book._id);

            res.success = true;
            res.message = "Delete success";

            return Ok(res);
        }
    }
}