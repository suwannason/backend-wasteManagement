
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.response;
using backend.Models;
using backend.request;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;

namespace backend.Controllers
{

    [Authorize]
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
                // PREMISSION CHECKING
                string permission = User.FindFirst("permission")?.Value;
                JObject permissionObj = JObject.Parse(@"{ 'permission': " + permission + "}");

                int allowed = 0;
                foreach (var record in permissionObj["permission"])
                {
                    Console.WriteLine(record["dept"]);
                    if (record["dept"].ToString() == "FAE" && record["feature"].ToString() == "invoice" && record["action"].ToString() == "prepare")
                    {
                        allowed = allowed + 1;
                        break;
                    }
                }
                if (allowed == 0)
                {
                    res.success = false;
                    res.message = "Permission denied";
                    return Forbid();
                }
                // PREMISSION CHECKING

                Invoices item = new Invoices();

                item.contractEndDate = body.contractEndDate;
                item.contractNo = body.contractNo;
                item.contractStartDate = body.contractStartDate;
                item.counterpartyAddress = body.counterpartyAddress;
                item.counterpartyChange = body.counterpartyChange;
                item.counterPartyChangePosition = body.counterPartyChangePosition;
                item.counterpartyName = item.counterpartyName;
                item.fax = body.fax;
                item.invoiceDate = body.invoiceDate;
                item.lotNo = body.lotNo;
                item.moveOutDate = body.moveOutDate;
                item.phoneNo = body.phoneNo;
                item.typeBoi = body.typeBoi;
                item.wasteItem = body.wasteItem;
                item.wasteName = body.wasteName;
                item.status = "prepared";
                item.year = DateTime.Now.ToString("yyyy");
                item.month = DateTime.Now.ToString("MMM");
                item.createBy = User.FindFirst("username")?.Value;
                _invoiceService.Create(item);
                return Ok(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Conflict();
            }
        }

        [HttpPatch("status")]
        public IActionResult approve(RequestInvoiceUpdateStatus body)
        {
            try
            {
                foreach (var item in body.body)
                {
                    _invoiceService.updateStatus(item, body.status);
                }
                res.success = true;
                res.message = "Update to " + body.status + " success";
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
                List<Invoices> data = _invoiceService.getByStatus(status);
                res.success = true;
                res.message = "Get invoice success";
                res.data = data.ToArray();
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