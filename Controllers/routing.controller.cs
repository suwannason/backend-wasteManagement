
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
                List<SummaryInvoiceSchema> summaryData = new List<SummaryInvoiceSchema>();

                foreach (Invoices item in data)
                {
                    foreach (string summaryId in item.summaryId)
                    {
                        SummaryInvoiceSchema summaryItem = _summaryInvoice.getById(summaryId);
                        summaryData.Add(summaryItem);

                    }
                }
                foreach (ITCinvoiceSchema item in itcReject)
                {
                    SummaryInvoiceSchema summaryItem = _summaryInvoice.getById(item.summaryId);
                    if (summaryItem != null)
                    {
                        summaryItem.status = "reject";
                        summaryData.Add(summaryItem);
                    }
                }
                return Ok(new
                {
                    success = true,
                    message = "Data for ITC Download.",
                    data = summaryData,
                });
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }

        [HttpPost("itc/invoice/{id}")]
        public ActionResult itcUploadInvoice(string id, [FromForm] request.itcPrepareInvoice body)
        {
            try
            {

                foreach (IFormFile item in body.files)
                {
                    if (!item.FileName.Contains(".pdf"))
                    {
                        return BadRequest(new { success = false, message = "Please upload PDF files." });
                    }
                }
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


                _itc_invoice.create(new ITCinvoiceSchema
                {
                    files = file.ToArray(),
                    summaryId = id,
                    createDate = DateTime.Now.ToString("yyyy/MM/dd"),
                    prepare = req_prepare,
                    status = "prepared",
                    createMonth = DateTime.Now.ToString("MMMM"),
                    createYear = DateTime.Now.ToString("yyyy"),
                    invoiceNo = body.invoiceNo,
                    dueDate = body.dueDate,
                });
                _summaryInvoice.updateStatus(id, "toInvoice", req_prepare);

                _invoice.changeStatusWhenITCprepare(id);

                return Ok(new { success = true, message = "Upload ITC invoice success", });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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
                        createDate = item.createDate,
                        invoiceNo = item.invoiceNo,
                        lotNo = summary.requester[0].lotNo,
                        dueDate = item.dueDate,
                        totalPrice = summary.totalPrice,
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

        [HttpPatch("itc/invoice/tracking")]
        public ActionResult getITCinvoice(request.requesterHistory body)
        {
            try
            {
                List<ITCinvoiceSchema> data = _itc_invoice.getByYearMonth(body.year, body.month);
                List<dynamic> returnData = new List<dynamic>();
                List<Invoices> invoiceITCnew = _invoice.getInvoiceITC_new();
                Int32 no = 1;
                foreach (Invoices item in invoiceITCnew)
                {
                    SummaryInvoiceSchema summary = _summaryInvoice.getById(item.summaryId[0]);
                    returnData.Add(new
                    {
                        no,
                        status = "Waiting for ITC prepere",
                        id = item._id,
                        faeCreateBy = item.fae_prepared.name,
                        itcPrepareBy = "-",
                        rejectCommend = item.rejectCommend,
                        createDate = item.createDate,
                        files = new List<string>(),
                        summaryId = item.summaryId,
                        summaryType = summary.type,
                    });
                    no += 1;
                }

                if (data.Count > 0)
                {

                    foreach (ITCinvoiceSchema item in data)
                    {
                        List<string> fileAttachment = new List<string>();
                        string statusMessage = "";
                        if (item.status == "prepared")
                        {
                            statusMessage = "Wait for ITC checker";
                        }
                        else if (item.status == "checked")
                        {
                            statusMessage = "Wait for ITC approver";
                        }
                        else if (item.status == "approved")
                        {
                            statusMessage = "Wait for ACC prepare";
                        }
                        else if (item.status == "acc-prepared")
                        {
                            statusMessage = "Wait for ACC prepare";
                        }
                        else if (item.status == "acc-checked")
                        {
                            statusMessage = "Wait for ACC approve";
                        }
                        else if (item.status == "acc-approved")
                        {
                            statusMessage = "Approve completed";
                        }
                        else if (item.status == "reject")
                        {
                            statusMessage = "Reject to ITC prepare";
                        }
                        // Console.WriteLine(item.summaryId);
                        fileAttachment.AddRange(item.files);
                        SummaryInvoiceSchema summary = _summaryInvoice.getById(item.summaryId);

                        if (summary != null)
                        {
                            foreach (Waste recycle in summary.recycle)
                            {
                                fileAttachment.AddRange(recycle.files);
                            }
                            returnData.Add(new
                            {
                                no,
                                status = statusMessage,
                                id = item._id,
                                faeCreateBy = summary.prepare.name,
                                itcPrepareBy = item.prepare.name,
                                rejectCommend = item.rejectCommend,
                                createDate = item.createDate,
                                files = fileAttachment,
                                summaryId = item.summaryId,
                                summaryType = summary.type,
                            });
                            no += 1;
                        }

                    }
                }
                return Ok(new { success = true, message = "Invoice for ITC created.", data = returnData, });
            }
            catch (System.Exception e)
            {
                return Problem(e.StackTrace);
            }
        }
    
        [HttpGet("itc/invoice/bySummary/{summaryId}")]
        public ActionResult getITCinvoiceWithSummaryId(string summaryId) {
            ITCinvoiceSchema data = _itc_invoice.getBySummaryId(summaryId);

            return Ok(new { success = true, data, });
        }
    }
}