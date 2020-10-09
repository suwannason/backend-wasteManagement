
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

    public class recycleController : ControllerBase
    {

        private readonly RecycleService _recycleService;

        RecycleWesteResponse res = new RecycleWesteResponse();

        public recycleController(RecycleService recycleService)
        {
            _recycleService = recycleService;
        }

        [HttpGet]
        public ActionResult<RecycleWesteResponse> Get()
        {
            List<RecycleWeste> data = _recycleService.Get();

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
        public ActionResult<RecycleWeste> Get(string id)
        {
            var book = _recycleService.Get(id);

            if (book == null)
            {
                return NotFound();
            }
            return book;
        }

        [HttpPost]
        public ActionResult<RecycleWesteResponse> Create(RecycleWeste body)
        {
            try
            {
                body.year = DateTime.Now.Year.ToString();
                body.status = "open";
                body.month = DateTime.Now.ToString("MMM");
                RecycleWeste created = _recycleService.Create(body);
                List<RecycleWeste> data = new List<RecycleWeste>();
                data.Add(created);

                res.success = true;
                res.message = "Insert success";
                res.data = data.ToArray();

                return Ok();
            }
            catch (Exception err)
            {
                Console.WriteLine(err.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, RecycleWeste body)
        {
            try
            {
                var data = _recycleService.Get(id);

                if (data == null)
                {
                    return NotFound();
                }
                _recycleService.Update(id, body);
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
            var book = _recycleService.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            _recycleService.Remove(book._id);

            res.success = true;
            res.message = "Delete success";

            return Ok(res);
        }
    }
}