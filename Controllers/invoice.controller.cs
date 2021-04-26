
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
        private readonly requesterUploadServices _requester;

        InvoiceResponse res = new InvoiceResponse();

        public invoiceController(InvoiceService invoiceService, RecycleService wasteService, requesterUploadServices requester)
        {
            _invoiceService = invoiceService;
            _wasteService = wasteService;
            _requester = requester;
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
        public IActionResult prepared(createInvoice body)
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
                Profile user = new Profile();

                user.empNo = User.FindFirst("username")?.Value;
                user.band = User.FindFirst("band")?.Value;
                user.dept = User.FindFirst("dept")?.Value;
                user.div = User.FindFirst("div")?.Value;
                user.name = User.FindFirst("name")?.Value;
                user.date = DateTime.Now.ToString("yyyy/MM/dd");

                Profile user_tmp = new Profile();

                user_tmp.empNo = "-";
                user_tmp.band = "-";
                user_tmp.dept = "-";
                user_tmp.div = "-";
                user_tmp.name = "-";
                user_tmp.date = "-";

                string currentDate = DateTime.Now.ToString("yyyy/MM/dd");
                List<Invoices> data = new List<Invoices>();
                foreach (string lotNo in body.lotNo)
                {
                    data.Add(new Invoices
                    {
                        company = body.company,
                        createDate = currentDate,
                        fae_prepared = user,
                        fae_checked = user_tmp,
                        fae_approved = user_tmp,
                        gm_approved = user_tmp,
                        status = "fae-prepared",
                        lotNo = lotNo,
                        year = DateTime.Now.ToString("yyyy"),
                        month = DateTime.Now.ToString("MM")
                    });
                }

                _invoiceService.Create(data);
                return Ok(new { success = true, message = "Create invoices success." });
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


                foreach (string item in body.lotNo)
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

            Profile user = new Profile();

            user.empNo = User.FindFirst("username")?.Value;
            user.band = User.FindFirst("band")?.Value;
            user.dept = User.FindFirst("dept")?.Value;
            user.div = User.FindFirst("div")?.Value;
            user.name = User.FindFirst("name")?.Value;

            return Ok(res);
        }
        [HttpPatch("data")]
        public ActionResult getRecordTocreateInvoice(RequestgetHistory body)
        {

            return Ok();
        }

        [HttpPatch("search")]
        public ActionResult getLotOnSearch(startEndDate body)
        {

            List<requesterUploadSchema> requester = _requester.searchToInvoice(body.startDate, body.endDate);
            List<Waste> waste = _wasteService.searchToInvoice(body.startDate, body.endDate);

            return Ok(new
            {
                success = true,
                message = "Waste data",
                data = new {
                    requester = requester,
                    fae = waste,
                }
            });
        }
    }
}