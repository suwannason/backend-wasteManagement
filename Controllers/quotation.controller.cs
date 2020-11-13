
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
    public class quotationController : ControllerBase
    {
        private readonly QuotationService _quotationService;

        QuotationResponse res = new QuotationResponse();
        public quotationController(QuotationService quotationeService)
        {
            _quotationService = quotationeService;
        }

        [HttpPost]
        public IActionResult create(Quotation body) {
            _quotationService.Create(body);

            res.success = true;
            res.message = "Create quotation success";
            return Ok(res);
        }

        [HttpGet]
        public IActionResult getItem() {

            List<Quotation> data = _quotationService.getData();
            
            res.success = true;
            res.message = "Get quotation success";
            res.data = data.ToArray();

            return Ok(res);
            
        }
        [HttpDelete("{id}")]
        public IActionResult deleteItem(string id) {
            res.success = true;
            res.message = "Delete item success";
            _quotationService.deleteQuotation(id);
            return Ok(res);
        }

        [HttpPut("{id}")]
        public IActionResult updateItem(string id, Quotation body) {

            res.success = true;
            res.message = "update success";

            _quotationService.updateItem(id, body);
            return Ok(res);
        }
    }
}