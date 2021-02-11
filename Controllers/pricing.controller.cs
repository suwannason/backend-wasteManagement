
using backend.Services;
using backend.Models;
using backend.response;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers {

    [Route("fae-part/[controller]")]
    [ApiController]
    // [Authorize]
    public class pricingController : ControllerBase
    {
        private readonly PricingService _pricing;

        pricingResponse res = new pricingResponse();

        public pricingController(PricingService pricing)
        {
            _pricing = pricing;
        }

        [HttpGet]
        public ActionResult getNumber() {
            try {
                List<pricingSchema> data = _pricing.getAll();
                res.success = true; 
                res.message = "Pricing response";
                res.data = data.ToArray();
                return Ok(res);
            } catch (Exception e) {
                return Problem(e.StackTrace);
            }
        }
    }
}