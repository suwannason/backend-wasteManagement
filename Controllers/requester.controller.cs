
using backend.Models;
using backend.Services;
using backend.request;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
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
        private readonly requesterUploadServices _requester;
        private readonly itcDBservice _itcDB;
        private readonly RecycleService _waste;
        private readonly faeDBservice _faeDB;
        private readonly UserService _user;

        public requesterController(HazadousService req, requesterUploadServices scrapImo, itcDBservice itc_imo, RecycleService waste, faeDBservice fae, UserService user)
        {
            _hazadous = req;
            _requester = scrapImo;
            _itcDB = itc_imo;
            _waste = waste;
            _faeDB = fae;
            _user = user;
        }

        [HttpGet("getById/{id}")]
        public ActionResult getById(string id)
        {
            requesterUploadSchema data = _requester.getById(id);
            return Ok(new { success = true, message = "item by ID.", data, });
        }

        [HttpPut("status")]
        public ActionResult updateStatus(UpdateStatusFormRequester body)
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


                foreach (string item in body.id)
                {
                    _requester.updateStatus(item, body.status);
                }

                foreach (string item in body.id)
                {
                   _requester.signedProfile(item, body.status, user);
                }
                return Ok(new { success = true, message = "Update status to " + body.status + " success." });

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Problem(e.Message);
            }
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

                Console.WriteLine(dept + " = " + data.Count);
                List<requesterUploadSchema> grouped = data.GroupBy(x => new { x.moveOutDate, x.phase, x.boiType }).Select(y => new requesterUploadSchema
                {
                    boiType = y.Key.boiType,
                    moveOutDate = y.Key.moveOutDate,
                    phase = y.Key.phase
                }).ToList();

                // return Ok(grouped);
                List<dynamic> returnData = new List<dynamic>();

                foreach (requesterUploadSchema item in grouped)
                {
                    List<requesterUploadSchema> itemInGroup = _requester.getGroupingItems(item.moveOutDate, item.phase, item.boiType, status, dept);

                    // return Ok(itemInGroup);
                    double totalNetweight = 0;
                    List<string> id = new List<string>();

                    foreach (requesterUploadSchema gItem in itemInGroup)
                    {
                        totalNetweight += Double.Parse(gItem.netWasteWeight);
                        id.Add(gItem._id);
                    }
                    returnData.Add(new
                    {
                        moveOutDate = item.moveOutDate,
                        boiType = item.boiType,
                        type = "parts",
                        lotNo = itemInGroup[0].lotNo,
                        dept = itemInGroup[0].dept,
                        netWasteWeight = totalNetweight.ToString("##,###.00"),
                        phase = item.phase,
                        id = id.ToArray(),
                    });
                }

                return Ok(new { success = true, message = "Requester data.", data = returnData });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Problem(e.StackTrace);
            }
        }

        [HttpGet("group/detail")]
        public ActionResult getExpandDetail(RequestGetDetail body)
        {
            string dept = User.FindFirst("dept")?.Value;
            List<requesterUploadSchema> data = _requester.getGroupingItems(body.moveOutDate, body.phase, body.boiType, body.status, dept);
            return Ok(new { success = true, message = "Data record", data, });
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

                List<requesterUploadSchema> data = action.uploadData(filename, System.Guid.NewGuid().ToString() + "-" + body.file.FileName, req_prepare, usertmp);
                _requester.handleUpload(data);

                return Ok(new { success = true, message = "Upload data success.", });
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpPatch("history")]
        public ActionResult history(requesterHistory body)
        {
            try
            {
                if (body.year == null)
                {
                    body.year = DateTime.Now.ToString("yyyy");
                }
                if (body.month == null)
                {
                    body.month = DateTime.Now.ToString("MMMM");
                }

                string dept = User.FindFirst("dept")?.Value;
                List<requesterUploadSchema> data = _requester.getHistory(body.month, body.year, dept);

                List<requesterUploadSchema> grouped = data.GroupBy(x => new { x.moveOutDate, x.phase, x.boiType }).Select(y => new requesterUploadSchema
                {
                    boiType = y.Key.boiType,
                    moveOutDate = y.Key.moveOutDate,
                    phase = y.Key.phase
                }).ToList();

                List<dynamic> returnData = new List<dynamic>();

                foreach (requesterUploadSchema item in grouped)
                {
                    List<requesterUploadSchema> itemInGroup = _requester.getGroupingTracking_dept(item.moveOutDate, item.phase, item.boiType, dept);
                    // return Ok(itemInGroup);
                    if (itemInGroup.Count > 0)
                    {
                        double totalNetweight = 0;
                        List<string> id = new List<string>();

                        foreach (requesterUploadSchema gItem in itemInGroup)
                        { // this loop for sum total and add id to return data
                            totalNetweight += Double.Parse(gItem.netWasteWeight);
                            id.Add(gItem._id);
                        }

                        requesterUploadSchema dataItem = _requester.getById(id[0]);
                        string status = "";
                        if (dataItem.status == "req-prepared")
                        {
                            status = "Waiting for requester checking";
                        }
                        else if (dataItem.status == "req-checked")
                        {
                            status = "Waiting for requester approving";
                        }
                        else if (dataItem.status == "req-approved")
                        {
                            status = "Waiting for PDC prepare data";
                        }
                        else if (dataItem.status == "pdc-prepared")
                        {
                            status = "Waiting for PDC checking";
                        }
                        else if (dataItem.status == "pdc-checked")
                        {
                            status = "Waiting for ITC checking data";
                        }
                        else if (dataItem.status == "itc-checked")
                        {
                            status = "Waiting for ITC approving";
                        }
                        returnData.Add(new
                        {
                            dept = dataItem.dept,
                            moveOutDate = item.moveOutDate,
                            boiType = item.boiType,
                            netWasteWeight = totalNetweight.ToString("##,###.00"),
                            phase = item.phase,
                            status,
                            id = id.ToArray(),
                        });
                    }
                }

                return Ok(
                    new
                    {
                        success = true,
                        message = "All requester tracking data.",
                        data = returnData,
                    }
                );
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }

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

        [HttpGet("fae/tracking")]
        public ActionResult tracking()
        {
            List<requesterUploadSchema> data = _requester.getTracking();

            List<requesterUploadSchema> grouped = data.GroupBy(x => new { x.moveOutDate, x.phase, x.boiType }).Select(y => new requesterUploadSchema
            {
                boiType = y.Key.boiType,
                moveOutDate = y.Key.moveOutDate,
                phase = y.Key.phase
            }).ToList();

            List<dynamic> returnData = new List<dynamic>();

            foreach (requesterUploadSchema item in grouped)
            {
                List<requesterUploadSchema> itemInGroup = _requester.getGroupingTracking(item.moveOutDate, item.phase, item.boiType);

                // return Ok(itemInGroup);
                double totalNetweight = 0;
                List<string> id = new List<string>();

                foreach (requesterUploadSchema gItem in itemInGroup)
                { // this loop for sum total and add id to return data
                    totalNetweight += Double.Parse(gItem.netWasteWeight);
                    id.Add(gItem._id);
                }

                requesterUploadSchema dataItem = _requester.getById(id[0]);
                string status = "";
                if (dataItem.status == "req-prepared")
                {
                    status = "Waiting for requester checking";
                }
                else if (dataItem.status == "req-checked")
                {
                    status = "Waiting for requester approving";
                }
                else if (dataItem.status == "req-approved")
                {
                    status = "Waiting for PDC prepare data";
                }
                else if (dataItem.status == "pdc-prepared")
                {
                    status = "Waiting for PDC checking";
                }
                else if (dataItem.status == "pdc-checked")
                {
                    status = "Waiting for ITC checking data";
                }
                else if (dataItem.status == "itc-checked")
                {
                    status = "Waiting for ITC approving";
                }
                returnData.Add(new
                {
                    dept = dataItem.dept,
                    moveOutDate = item.moveOutDate,
                    boiType = item.boiType,
                    netWasteWeight = totalNetweight.ToString("##,###.00"),
                    phase = item.phase,
                    status,
                    id = id.ToArray(),
                });
            }

            return Ok(
                new
                {
                    success = true,
                    message = "All requester tracking data.",
                    data = returnData,
                }
            );
        }

        [HttpPatch("reject")]
        public ActionResult rejection(rejectRequester body)
        {

            Profile user = new Profile();
            user.empNo = User.FindFirst("username")?.Value;
            user.band = User.FindFirst("band")?.Value;
            user.dept = User.FindFirst("dept")?.Value;
            user.div = User.FindFirst("div")?.Value;
            user.name = User.FindFirst("name")?.Value;
            user.tel = User.FindFirst("tel")?.Value;

            requesterUploadSchema data = _requester.getById(body.id[0]);
            _requester.rejectByFileName(data.fileUploadName, body.commend, user);

            return Ok(new { success = true, message = "Reject success." });
        }
    }

}