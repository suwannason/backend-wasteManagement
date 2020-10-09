
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

    public class invoiceController : ControllerBase
    {

        private readonly InvoiceService _invoiceService;

        InvoiceResponse res = new InvoiceResponse();

        public invoiceController(InvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public ActionResult<InvoiceResponse> Get()
        {
            string year = DateTime.Now.Year.ToString();
            List<Invoices> data = _invoiceService.Get(year);

            res.success = true;
            res.data = data.ToArray();
            if (data.Count == 0)
            {
                res.message = "Notfound Data.";
                return NotFound(res);
            }
            res.message = "Get invoice success";
            return Ok(res);
        }

        [HttpPost]
        public ActionResult<InvoiceResponse> Create(Invoices body)
        {
            try
            {
                body.year = DateTime.Now.Year.ToString();
                Invoices created = _invoiceService.Create(body);
                List<Invoices> data = new List<Invoices>();
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
        public IActionResult Update(string id, Invoices body)
        {
            try
            {
                var data = _invoiceService.Get(id);

                if (data == null)
                {
                    return NotFound();
                }
                _invoiceService.Update(id, body);
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
    }
}