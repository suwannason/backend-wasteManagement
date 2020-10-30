
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
    public class companyController : ControllerBase
    {
        private readonly CompanyService _companyService;

        CompanyResponse res = new CompanyResponse();

        public companyController(CompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        public ActionResult<CompanyResponse> Get()
        {
            List<Companies> data = _companyService.Get();

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

        [HttpGet("{id}", Name = "GetBook")]
        public ActionResult<Companies> Get(string id)
        {
            var book = _companyService.Get(id);

            if (book == null)
            {
                return NotFound();
            }
            return book;
        }

        [HttpPost]
        public ActionResult<CompanyResponse> Create(Companies book)
        {
            try
            {
                Companies created = _companyService.Create(book);
                List<Companies> data = new List<Companies>();
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
        public IActionResult Update(string id, Companies bookIn)
        {
            var book = _companyService.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            Console.Write(id);
            _companyService.Update(id, bookIn);

            res.success = true;
            res.message = "Update success";
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var book = _companyService.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            _companyService.Remove(book._id);

            res.success = true;
            res.message = "Delete success";

            return Ok(res);
        }
    }
}