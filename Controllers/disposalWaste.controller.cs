
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

    public class disposalController : ControllerBase
    {

        private readonly DisposalService _DisposalService;

        DisposalWesteResponse res = new DisposalWesteResponse();

        public disposalController(DisposalService DisposalService)
        {
            _DisposalService = DisposalService;
        }

        [HttpGet]
        public ActionResult<DisposalWesteResponse> Get()
        {
            List<DisposalWaste> data = _DisposalService.Get();

            res.success = true;
            res.data = data.ToArray();
            if (data.Count == 0)
            {
                res.message = "Notfound Data.";
                return NotFound(res);
            }
            res.message = "Get data success";
            return Ok(res);
        }

        [HttpGet("{id}")]
        public ActionResult<DisposalWaste> Get(string id)
        {
            var book = _DisposalService.Get(id);

            if (book == null)
            {
                return NotFound();
            }
            return book;
        }

        [HttpPost]
        public ActionResult<DisposalWesteResponse> Create(DisposalWaste body)
        {
            try
            {
                body.year = DateTime.Now.Year.ToString();
                body.status = "open";
                body.month = DateTime.Now.ToString("MMM");
                DisposalWaste created = _DisposalService.Create(body);
                List<DisposalWaste> data = new List<DisposalWaste>();
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
        public IActionResult Update(string id, DisposalWaste body)
        {
            try
            {
                var data = _DisposalService.Get(id);

                if (data == null)
                {
                    return NotFound();
                }
                _DisposalService.Update(id, body);
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
            var book = _DisposalService.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            _DisposalService.Remove(book._id);

            res.success = true;
            res.message = "Delete success";

            return Ok(res);
        }
    }
}