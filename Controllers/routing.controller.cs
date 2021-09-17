
using Microsoft.AspNetCore.Mvc;

using backend.Services;
using backend.request;
using System;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace backend.Controllers
{

    [Route("fae-part/[controller]")]
    [ApiController]
    [Authorize]
    public class routingController : ControllerBase
    {
        private readonly HazadousService _hazadous;
        private readonly requesterUploadServices _scrapImo;

        private readonly SummaryInvoiceService _summaryInvoice;
        private readonly ITC_invoiceService _itc_invoice;
        private readonly InvoiceService _invoice;

        public routingController(HazadousService req, requesterUploadServices scrapImo, SummaryInvoiceService summary, ITC_invoiceService itcInvoice, InvoiceService invoice)
        {
            _hazadous = req;
            _scrapImo = scrapImo;
            _summaryInvoice = summary;
            _itc_invoice = itcInvoice;
            _invoice = invoice;
        }

        [HttpGet("itc/summary/approved")] // get summary BOI for itc
        public ActionResult dataItcByStatus()
        {
            try
            {
                // string dept = User.FindFirst("dept")?.Value;

                List<Invoices> data = _invoice.ITCgetInvoice();
                List<ITCinvoiceSchema> itcReject = _itc_invoice.getByStatus("reject");
                List<SummaryInvoiceSchema> returnData = new List<SummaryInvoiceSchema>();

                foreach (Invoices item in data)
                {
                    foreach (string summaryId in item.summaryId)
                    {
                        SummaryInvoiceSchema summaryItem = _summaryInvoice.getById(summaryId);
                        returnData.Add(summaryItem);

                    }
                }
                foreach (ITCinvoiceSchema item in itcReject)
                {
                    SummaryInvoiceSchema summaryItem = _summaryInvoice.getById(item.summaryId);
                    if (summaryItem != null)
                    {
                        summaryItem.status = "reject";
                        returnData.Add(summaryItem);
                    }
                }
                return Ok(new
                {
                    success = true,
                    message = "Data for ITC Download.",
                    data = returnData,
                });
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }

        [HttpPost("itc/invoice/{id}")]
        public ActionResult itcUploadInvoice(string id, [FromForm] uploadFileMulti body)
        {
            try
            {
                string rootFolder = Directory.GetCurrentDirectory();

                string pathString2 = @"\API site\files\wastemanagement\";

                string serverPath = rootFolder.Substring(0, rootFolder.LastIndexOf(@"\")) + pathString2;

                if (!System.IO.Directory.Exists(serverPath))
                {
                    Directory.CreateDirectory(serverPath);
                }

                string g = Guid.NewGuid().ToString();
                List<string> file = new List<string>();
                foreach (IFormFile item in body.files)
                {
                    using (FileStream strem = System.IO.File.Create($"{serverPath}{g}-{item.FileName}"))
                    {
                        // body.file.CopyTo(strem);
                        item.CopyTo(strem);
                        file.Add(g + "-" + item.FileName);
                    }
                }
                Profile req_prepare = new Profile();

                req_prepare.empNo = User.FindFirst("username")?.Value;
                req_prepare.band = User.FindFirst("band")?.Value;
                req_prepare.dept = User.FindFirst("dept")?.Value;
                req_prepare.name = User.FindFirst("name")?.Value;
                req_prepare.date = DateTime.Now.ToString("yyyy/MM/dd");

                // Console.WriteLine($"{serverPath}{g}-{body.file.FileName}");
                // Console.WriteLine(id);

                _itc_invoice.create(new ITCinvoiceSchema
                {
                    files = file.ToArray(),
                    summaryId = id,
                    createDate = DateTime.Now.ToString("yyyy/MM/dd"),
                    prepare = req_prepare,
                    status = "prepared"
                });
                _summaryInvoice.updateStatus(id, "toInvoice", req_prepare);

                _invoice.changeStatusWhenITCprepare(id);

                return Ok(new { success = true, message = "Upload ITC invoice success", });
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }

        [HttpGet("itc/invoice/{status}")]
        public ActionResult getByStatus(string status)
        { // prepared, checked, approved
            List<ITCinvoiceSchema> data = _itc_invoice.getByStatus(status);

            List<dynamic> returnData = new List<dynamic>();

            int i = 1;
            foreach (ITCinvoiceSchema item in data)
            {
                SummaryInvoiceSchema summary = _summaryInvoice.getById(item.summaryId);
                returnData.Add(
                    new
                    {
                        id = item._id,
                        no = i,
                        summaryId = item.summaryId,
                        faeCreateBy = summary.prepare.name,
                        itcPrepareBy = item.prepare.name,
                        fileName = item.files,
                        createDate = item.createDate
                    }
                );
                i += 1;
            }
            return Ok(new { success = true, message = "Invoice ITC", data = returnData, });
        }

        [HttpPatch("itc/invoice/status")]
        public ActionResult itcUpdateStatusInvoice(ITCapproveInvoice body)
        {
            foreach (string id in body.id)
            {
                _itc_invoice.updateStatus(id, body.status);
            }
            return Ok(new { success = true, message = "Update status success." });
        }
        [HttpDelete("reject/{lotNo}")]
        public ActionResult reject(string lotNo)
        {
            try
            {
                Profile user = new Profile();
                user.empNo = User.FindFirst("username")?.Value;
                user.band = User.FindFirst("band")?.Value;
                user.dept = User.FindFirst("dept")?.Value;
                user.div = User.FindFirst("div")?.Value;
                user.name = User.FindFirst("name")?.Value;
                user.tel = User.FindFirst("tel")?.Value;

                return Ok("reject");
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }

        [HttpPatch("pdc/prepare")]
        public ActionResult pdcPrepare(invoiceRef body)
        {
            Profile user = new Profile();

            user.empNo = User.FindFirst("username")?.Value;
            user.band = User.FindFirst("band")?.Value;
            user.dept = User.FindFirst("dept")?.Value;
            user.div = User.FindFirst("div")?.Value;
            user.name = User.FindFirst("name")?.Value;

            Parallel.ForEach(body.check, item =>
            {
                _scrapImo.updateStatus(item, "pdc-prepared");
                _scrapImo.signedProfile(item, "pdc-prepared", user);
            });

            Parallel.ForEach(body.uncheck, item =>
            {
                _scrapImo.updateStatus(item, "pdc-approved");
                _scrapImo.signedProfile(item, "pdc-prepared", user);
            });
            return Ok(new
            {
                success = true,
                message = "Prepare data success"
            });
        }
    }
}