
using Microsoft.AspNetCore.Mvc;

using backend.Services;
using backend.request;
using System;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace backend.Controllers
{

    [Route("fae-part/[controller]")]
    [ApiController]
    [Authorize]
    public class routingController : ControllerBase
    {
        private readonly HazadousService _hazadous;
        private readonly InfectionsService _infections;
        private readonly requesterUploadServices _scrapImo;

        private readonly prepareLotService _prepareLot;

        private readonly SummaryInvoiceService _summaryInvoice;
        private readonly ITC_invoiceService _itc_invoice;

        public routingController(HazadousService req, InfectionsService infect, requesterUploadServices scrapImo, prepareLotService prepareLot, SummaryInvoiceService summary, ITC_invoiceService itcInvoice)
        {
            _hazadous = req;
            _infections = infect;
            _scrapImo = scrapImo;
            _prepareLot = prepareLot;
            _summaryInvoice = summary;
            _itc_invoice = itcInvoice;
        }

        [HttpGet("itc/summary/approved")] // get summary BOI for itc
        public ActionResult dataItcByStatus()
        {
            try
            {
                List<SummaryInvoiceSchema> data = _summaryInvoice.ITC_getsummary_approved();
                List<ITCinvoiceSchema> itcReject = _itc_invoice.getByStatus("reject");

                foreach (ITCinvoiceSchema item in itcReject)
                {
                    SummaryInvoiceSchema summaryItem = _summaryInvoice.getById(item.summaryId);
                    if (summaryItem != null)
                    {
                        summaryItem.status = "reject";
                        data.Add(summaryItem);
                    }
                }
                return Ok(new
                {
                    success = true,
                    message = "Data for ITC Download.",
                    data,
                });
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }

        [HttpPost("itc/invoice/{id}")]
        public ActionResult itcUploadInvoice(string id, [FromForm] uploadFile body)
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

                using (FileStream strem = System.IO.File.Create($"{serverPath}{g}-{body.file.FileName}"))
                {
                    body.file.CopyTo(strem);
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
                    files = g + body.file.FileName,
                    summaryId = id,
                    createDate = DateTime.Now.ToString("yyyy/MM/dd"),
                    prepare = req_prepare,
                    status = "prepared"
                });
                _summaryInvoice.updateStatus(id, "toInvoice", req_prepare);

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


        [HttpPost("fae/requester")]
        public ActionResult FaePrepareRequester(FaePrepareRequester body)
        { // FAE prepare record 
            try
            {
                FAEPreparedLotSchema data = new FAEPreparedLotSchema();

                Profile user = new Profile();

                user.empNo = User.FindFirst("username")?.Value;
                user.band = User.FindFirst("band")?.Value;
                user.dept = User.FindFirst("dept")?.Value;
                user.div = User.FindFirst("div")?.Value;
                user.name = User.FindFirst("name")?.Value;
                data.allowToDestroy = body.allowToDestroy;
                data.lotNo = body.lotNo;
                data.preparedBy = user;
                data.createDate = DateTime.Now.ToString("yyyy/MM/dd");
                data.remark = body.remark;
                data.howTodestory = body.howTodestory;

                _prepareLot.create(data);
                return Ok();
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