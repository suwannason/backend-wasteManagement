
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{

    [Route("fae-part/[controller]")]
    [ApiController]
    [Authorize]
    public class notificationController : ControllerBase
    {
        private readonly HazadousService _hazadous;
        private readonly InfectionsService _infections;
        private readonly ScrapMatrialImoService _scrapImo;

        public notificationController(HazadousService haz, InfectionsService infect, ScrapMatrialImoService scrapImo)
        {
            _hazadous = haz;
            _infections = infect;
            _scrapImo = scrapImo;
        }

        [HttpGet]
        public ActionResult getNumber() {
            try {
                return Ok();
            } catch (Exception e) {
                return Problem(e.StackTrace);
            }
        }
    }
}