
using backend.Models;
using backend.Services;
using backend.response;
using backend.request;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using OfficeOpenXml;
using System.IO;
using System.Linq;

namespace backend.Controllers
{

    [Route("fae-part/[controller]")]
    [ApiController]
    [Authorize]
    public class requesterController : ControllerBase
    {
        private readonly HazadousService _hazadous;
        private readonly InfectionsService _infections;
        private readonly requesterUploadServices _requester;
        private readonly itcDBservice _itcDB;
        private readonly RecycleService _waste;
        private readonly faeDBservice _faeDB;

        RequesterResponse res = new RequesterResponse();

        public requesterController(HazadousService req, InfectionsService infect, requesterUploadServices scrapImo, itcDBservice itc_imo, RecycleService waste, faeDBservice fae)
        {
            _hazadous = req;
            _infections = infect;
            _requester = scrapImo;
            _itcDB = itc_imo;
            _waste = waste;
            _faeDB = fae;
        }

        [HttpPut("status")]
        public ActionResult<RequesterResponse> updateStatus(UpdateStatusFormRequester body)
        {
            Profile user = new Profile();

            user.empNo = User.FindFirst("username")?.Value;
            user.band = User.FindFirst("band")?.Value;
            user.dept = User.FindFirst("dept")?.Value;
            user.div = User.FindFirst("div")?.Value;
            user.name = User.FindFirst("name")?.Value;
            user.tel = User.FindFirst("tel")?.Value;

            Parallel.ForEach(body.lotNo, item =>
            {
                _requester.updateStatus(item, body.status);
            });

            Parallel.ForEach(body.lotNo, item =>
            {
                _requester.signedProfile(item, body.status, user);
            });
            res.success = true;
            res.message = "Update status to " + body.status + " success.";
            return Ok(res);
        }

        [HttpGet("{status}")]
        // [Obsolete]
        public ActionResult<dynamic> getByStatus(string status)
        {
            try
            {

                string dept = User.FindFirst("dept")?.Value;
                List<requesterUploadSchema> data = new List<requesterUploadSchema>();
                if (dept.ToUpper() == "FAE" || dept.ToUpper() == "ITC" || dept.ToUpper() == "PDC")
                {
                    data = _requester.getByStatus_fae(status);
                }
                else
                {
                    data = _requester.getByStatus(status, dept);
                }
                return Ok(new { success = true, message = "Requester data.", data });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Problem(e.StackTrace);
            }
        }

        [HttpGet("invoice/{lotNo}")]
        public ActionResult getByLotNo(string lotNo)
        {
            try
            {
                List<requesterUploadSchema> data = _requester.getByLotno(lotNo);

                return Ok(new
                {
                    success = true,
                    message = "Lot no data",
                    data,
                });
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }


        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public ActionResult uploadData([FromForm] uploadData body)
        {
            try
            {
                string rootFolder = Directory.GetCurrentDirectory();

                string pathString2 = @"\API site\files\wastemanagement\upload\";
                string serverPath = rootFolder.Substring(0, rootFolder.LastIndexOf(@"\")) + pathString2;

                Console.WriteLine(serverPath);
                if (!System.IO.Directory.Exists(serverPath))
                {
                    Directory.CreateDirectory(serverPath);
                }
                Profile req_prepare = new Profile();

                req_prepare.empNo = User.FindFirst("username")?.Value;
                req_prepare.band = User.FindFirst("band")?.Value;
                req_prepare.dept = User.FindFirst("dept")?.Value;
                req_prepare.div = User.FindFirst("div")?.Value;
                req_prepare.name = User.FindFirst("name")?.Value;
                req_prepare.tel = User.FindFirst("tel")?.Value;
                req_prepare.date = DateTime.Now.ToString("yyyy/MM/dd");

                // if (req_prepare.dept.ToUpper() != body.form.ToUpper() || req_prepare.dept.ToUpper() == "fae")
                // {
                //     return Forbid();
                // }

                string filename = serverPath + System.Guid.NewGuid().ToString() + "-" + body.file.FileName;
                using (FileStream strem = System.IO.File.Create(filename))
                {
                    body.file.CopyTo(strem);
                }
                Profile usertmp = new Profile();
                usertmp.band = "-";
                usertmp.dept = "-";
                usertmp.empNo = "-";
                usertmp.name = "-";
                usertmp.div = "-";
                usertmp.tel = "-";

                handleUpload action = new handleUpload(_itcDB, _faeDB);

                List<requesterUploadSchema> data = action.Upload(filename, req_prepare, usertmp);

                List<requesterUploadSchema> errrorItems = data.FindAll(item => item.totalPrice == "-");

                if (errrorItems.Count > 0)
                {
                    List<dynamic> returnError = new List<dynamic>();

                    foreach(requesterUploadSchema item in errrorItems) {
                        returnError.Add(new {
                            kind = item.kind,
                            moveOutDate = item.moveOutDate,
                            lotNo = item.lotNo,
                            matrialCode = item.matrialCode,
                            matrialName = item.matrialName,
                            totalWeight = item.totalWeight,
                            containerWeight = item.containerWeight,
                            qtyOfContainer = item.qtyOfContainer,
                            netWasteWeight = item.netWasteWeight,
                            unit = item.unit
                        });
                    }
                    return BadRequest(new { success = false, message = "Upload Error please check data.", data = returnError });
                }
                _requester.handleUpload(data);

                return Ok(new { success = true, message = "Upload data success." });
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }

        [HttpPatch("history")]
        public ActionResult history(startEndDate body)
        {
            Console.WriteLine("HISTORY REUESTER");

            if (User.FindFirst("dept")?.Value.ToLower() == "imo")
            {
                List<requesterUploadSchema> data = _requester.getHistory(body.startDate, body.endDate);
                return Ok(new { success = true, message = "Get imo history success", data, });
            }

            return Unauthorized(new { success = false, message = "User login department DB not match" });
        }

        [HttpPatch("invoice")]
        public ActionResult addInvoice(invoiceRef body)
        {
            try
            {

                foreach (string item in body.check)
                {
                    _requester.updateRefInvoice(item);
                }

                foreach (string item in body.uncheck)
                {
                    _waste.updateInvoiceRef(item);
                }
                return Ok();
            }
            catch (System.Exception e)
            {

                return Problem(e.StackTrace);
            }
        }

        [HttpGet("lotApproved")]
        public ActionResult getLotNoOnApproved()
        {
            try
            {

                List<requesterUploadSchema> data = _requester.getLotNo();

                List<requesterUploadSchema> distinct = data.GroupBy(x => x.lotNo).Select(x => x.First()).ToList();

                List<string> returnData = new List<string>();

                foreach (requesterUploadSchema item in distinct)
                {
                    returnData.Add(item.lotNo);
                }
                return Ok(new { success = true, data = returnData });
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }
    }

}