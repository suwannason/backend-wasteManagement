
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
        private readonly RecycleService _wasteService;

        InvoiceResponse res = new InvoiceResponse();

        public invoiceController(InvoiceService invoiceService, RecycleService wasteService)
        {
            _invoiceService = invoiceService;
            _wasteService = wasteService;
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, Invoices body)
        {
            try
            {
                var data = _invoiceService.GetById(id);

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
                item.counterpartyName = body.counterpartyName;
                item.fax = body.fax;
                item.invoiceDate = body.invoiceDate;
                item.lotNo = body.lotNo;
                item.moveOutDate = body.moveOutDate;
                item.phoneNo = body.phoneNo;
                item.typeBoi = body.typeBoi;
                item.wasteItem = body.wasteItem;
                item.wasteName = body.wasteName;
                item.status = "prepared";
                item.subTotal = body.subTotal;
                item.grandTotal = body.grandTotal;
                item.year = DateTime.Now.ToString("yyyy");
                item.month = DateTime.Now.ToString("MMM");
                item.createDate = DateTimeOffset.Now.ToUnixTimeSeconds();
                item.createBy = User.FindFirst("username")?.Value;

                foreach (var record in body.wasteItem)
                {
                    _wasteService.updateStatus(record.wasteId, "toInvoice");
                }
                _invoiceService.Create(item);
                res.success = true;
                res.message = "create invoice";
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
                // PREMISSION CHECKING
                string permission = User.FindFirst("permission")?.Value;
                JObject permissionObj = JObject.Parse(@"{ 'permission': " + permission + "}");

                int allowed = 0;
                foreach (var record in permissionObj["permission"])
                {
                    if (body.status == "checked")
                    {
                        if (record["dept"].ToString() == "FAE" && record["feature"].ToString() == "invoice" && record["action"].ToString() == "check")
                        {
                            allowed = allowed + 1;
                            break;
                        }
                    }

                    if (body.status == "approved")
                    {
                        if (record["dept"].ToString() == "FAE" && record["feature"].ToString() == "invoice" && record["action"].ToString() == "approve")
                        {
                            allowed = allowed + 1;
                            break;
                        }
                    }
                    if (body.status == "makingApproved")
                    {
                        if (record["dept"].ToString() == "FAE" && record["feature"].ToString() == "invoice" && record["action"].ToString() == "making")
                        {
                            allowed = allowed + 1;
                            break;
                        }
                    }
                }
                if (allowed == 0)
                {
                    res.success = false;
                    res.message = "Permission denied";
                    return Forbid();
                }
                // PREMISSION CHECKING


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
                Console.WriteLine(status);
                List<Invoices> data = _invoiceService.getByStatus(status);
                res.success = true;
                res.data = data.ToArray();

                if (res.data.Length == 0)
                {
                    res.message = "No data.";
                    return NotFound(res);
                }
                res.message = "Get invoice success";
                return Ok(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Conflict();
            }
        }

        [HttpDelete("{id}")]
        public IActionResult deleteInvoice(string id)
        {

            Invoices data = _invoiceService.GetById(id);

            if (data == null)
            {
                res.success = false;
                res.message = "Not found record.";
                return NotFound(res);
            }

            _invoiceService.deleteInvoice(id);
            res.success = true;
            res.message = "Delete invoice success";

            foreach (var item in data.wasteItem)
            {
                _wasteService.updateStatus(item.wasteId, "approve");
            }
            return Ok(res);
        }
    }
}