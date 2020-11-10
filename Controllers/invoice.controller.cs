
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.response;
using backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{

    // [Authorize]
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

        [HttpPost("prepared")]
        public IActionResult prepared(Invoices body)
        {
            try
            {
                _invoiceService.Create(body);
                return Ok(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Conflict();
            }
        }

        [HttpPatch("status")]
        public IActionResult approve(Invoices body)
        {
            try
            {
                return Ok(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Conflict();
            }
        }

        [HttpGet("{status}")]
        public IActionResult getInvoicse(string status)
        {
            try
            {
                return Ok(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Conflict();
            }
        }
    }
}