
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
        private readonly InfectionService _infection;

        public requesterController(HazadousService req, requesterUploadServices scrapImo, itcDBservice itc_imo, RecycleService waste, faeDBservice fae, UserService user, InfectionService infection)
        {
            _hazadous = req;
            _requester = scrapImo;
            _itcDB = itc_imo;
            _waste = waste;
            _faeDB = fae;
            _user = user;
            _infection = infection;
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

                List<UserSchema> userDB = _user.Getlist(user.empNo);
                Console.WriteLine(body.status.IndexOf("check"));

                if (body.status.IndexOf("check") > 0 && (userDB.FindAll(item => item.permission == "Prepared").Count > 0))
                {
                    return Unauthorized(new { success = false, message = "Can't send, Permission denied." });
                }
                else if (body.status.IndexOf("approve") > 0 && userDB.FindAll(item => item.permission == "Approved" || user.dept.ToUpper() == "FAE").Count == 0)
                {
                    return Unauthorized(new { success = false, message = "Can't approve, Permission denied." });
                }

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

                List<requesterUploadSchema> grouped = data.GroupBy(x => new { x.moveOutDate, x.phase, x.boiType, x.uploadEmpNo, x.filename }).Select(y => new requesterUploadSchema
                {
                    boiType = y.Key.boiType,
                    moveOutDate = y.Key.moveOutDate,
                    phase = y.Key.phase,
                    filename = y.Key.filename,
                    uploadEmpNo = y.Key.uploadEmpNo,
                }).ToList();

                // return Ok(grouped);
                List<dynamic> returnData = new List<dynamic>();

                foreach (requesterUploadSchema item in grouped)
                {
                    List<requesterUploadSchema> itemInGroup = _requester.getGroupingItems(item.moveOutDate, item.phase, item.boiType, status, dept, item.uploadEmpNo, item.filename);

                    // return Ok(new { itemInGroup, item, } );

                    // Console.WriteLine("itemInGroup: " + itemInGroup.Count);
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
                        name = itemInGroup[0].req_prepared.name,
                        filename = item.filename,
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

                Profile req_prepare = new Profile();

                req_prepare.empNo = User.FindFirst("username")?.Value;
                req_prepare.band = User.FindFirst("band")?.Value;
                req_prepare.dept = User.FindFirst("dept")?.Value;
                req_prepare.div = User.FindFirst("div")?.Value;
                req_prepare.name = User.FindFirst("name")?.Value;
                req_prepare.tel = User.FindFirst("tel")?.Value;
                req_prepare.date = DateTime.Now.ToString("yyyy/MM/dd");

                requesterUploadSchema uploadedItem = _requester.checkDuplicatedUpload(req_prepare.empNo);
                if (uploadedItem != null)
                {
                    return BadRequest(new { success = false, message = "You have data in progress, Please wait for approve." });
                }
                if (!System.IO.Directory.Exists(serverPath))
                {
                    Directory.CreateDirectory(serverPath);
                }

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
                if (data.Count == 0)
                {
                    return BadRequest(new { success = false, message = "Error, please check file upload" });
                }
                // else if (data.Find(item => item.totalPrice == null || item.totalPrice == "-") != null)
                // { // Return error check with price = -
                //     int no = 1;
                //     List<dynamic> returnData = new List<dynamic>();
                //     foreach (requesterUploadSchema item in data)
                //     {
                //         returnData.Add(
                //             new
                //             {
                //                 id = no.ToString(),
                //                 kind = item.kind,
                //                 moveOutDate = item.moveOutDate,
                //                 lotNo = item.lotNo,
                //                 matrialCode = item.matrialCode,
                //                 matrialName = item.matrialName,
                //                 totalWeight = item.totalWeight,
                //                 containerWeight = item.containerWeight,
                //                 qtyOfContainer = item.qtyOfContainer,
                //                 netWasteWeight = item.netWasteWeight,
                //                 unit = item.unit,
                //             }
                //         );
                //         no += 1;
                //     }
                //     return BadRequest(new { success = false, message = "Error, please check file upload", data = returnData });
                // }
                else
                {
                    _requester.handleUpload(data);
                }


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
                        else if (dataItem.status == "itc-approved")
                        {
                            status = "Waiting for FAE acknokledge";
                        }
                        else if (dataItem.status == "itc-checked")
                        {
                            status = "Waiting for ITC approving";
                        }
                        else
                        {
                            status = "Approve completed.";
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

        [HttpPatch("fae/tracking"), AllowAnonymous]
        public ActionResult tracking(request.requesterHistory body)
        {
            // HAZADOUS
            List<HazadousSchema> hazadousItem = _hazadous.getBymonthYear(body.month, body.year);

            List<dynamic> returnHazadous = new List<dynamic>();
            foreach (HazadousSchema item in hazadousItem)
            {
                string statusMessage = "";
                if (item.status == "req-prepared")
                {
                    statusMessage = "Waiting for requester check";
                }
                else if (item.status == "req-checked")
                {
                    statusMessage = "Waiting for requester Approve";
                }
                else if (item.status == "req-approved")
                {
                    statusMessage = "Waiting for FAE Prepare";
                }
                else if (item.status == "fae-prepared")
                {
                    statusMessage = "Waiting for FAE Check";
                }
                else if (item.status == "fae-checked")
                {
                    statusMessage = "Waiting for FAE Approve";
                }
                else if (item.status == "fae-approved")
                {
                    statusMessage = "Waiting for FAE Receive";
                }
                else if (item.status == "fae-recevied")
                {
                    statusMessage = "Receive complete";
                }

                returnHazadous.Add(
                    new
                    {
                        id = item._id,
                        moveOutDate = item.date,
                        type = "hazadous",
                        issueNo = item.runningNo,
                        runningNo = item.runningNo,
                        dept = item.dept,
                        phase = item.phase,
                        netWasteWeight = item.netWasteWeight,
                        status = statusMessage,
                        files = new List<string>(),
                    }
                );
            }
            // HAZADOUS

            // Infection
            List<InfectionSchema> infections = _infection.getHistory(body.month, body.year);

            List<dynamic> infectionData = new List<dynamic>();

            foreach (InfectionSchema item in infections)
            {
                string status = "";
                if (item.status == "req-prepared")
                {
                    status = "Wait requester check";
                }
                else if (item.status == "req-checked")
                {
                    status = "Wait requester approve";
                }
                else if (item.status == "req-approved")
                {
                    status = "Waiting for FAE Acknowleage";
                }
                else if (item.status == "fae-approved")
                {
                    status = "FAE Approve completed";
                }

                infectionData.Add(new
                {
                    id = item._id,
                    status = status,
                    requestDate = item.date,
                    dept = item.dept,
                    phase = item.phase,
                    netWasteWeight = item.netWasteWeight
                });

            }
            // Infection

            // Parts
            List<requesterUploadSchema> data = _requester.getbyYearMonth(body.year, body.month);

            List<requesterUploadSchema> grouped = data.GroupBy(x => new { x.moveOutDate, x.phase, x.boiType }).Select(y => new requesterUploadSchema
            {
                boiType = y.Key.boiType,
                moveOutDate = y.Key.moveOutDate,
                phase = y.Key.phase
            }).ToList();
            List<dynamic> partData = new List<dynamic>();

            foreach (requesterUploadSchema item in grouped)
            {
                List<requesterUploadSchema> itemInGroup = _requester.getGroupingTracking(item.moveOutDate, item.phase, item.boiType);
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
                        status = "Waiting for requester check";
                    }
                    else if (dataItem.status == "req-checked")
                    {
                        status = "Waiting for requester Approve";
                    }
                    else if (dataItem.status == "req-approved")
                    {
                        status = "Waiting for PDC Check";
                    }
                    else if (dataItem.status == "pdc-prepared")
                    {
                        status = "Waiting for PDC checking";
                    }
                    else if (dataItem.status == "pdc-checked")
                    {
                        status = "Waiting for ITC Check";
                    }
                    else if (dataItem.status == "itc-approved")
                    {
                        status = "Waiting for FAE Acknowleage";
                    }
                    else if (dataItem.status == "itc-checked")
                    {
                        status = "Waiting for ITC Approve";
                    }
                    else
                    {
                        status = "FAE Approve completed";
                    }
                    partData.Add(new
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
            // Parts
            return Ok(
                new
                {
                    success = true,
                    message = "All requester tracking data.",
                    hazadous = returnHazadous,
                    infection = infectionData,
                    requester = partData,
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
            _requester.rejectByFileName(data.filename, body.commend, user);

            return Ok(new { success = true, message = "Reject success." });
        }

        [HttpPut("upload"), Consumes("multipart/form-data")]
        public ActionResult editwithUpload([FromForm] request.editByUpload body)
        {
            try
            {
                _requester.deleteWithFilename(body.sourceFilename);

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

                return Ok(new { success = true, message = "Edit complete" });
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }

        [HttpPost("summary/recal"), AllowAnonymous]
        public ActionResult reCalculatePrice(request.recalCulateSummary[] body)
        {

            try
            {
                int updatedRow = 0;
                foreach (recalCulateSummary item in body)
                {
                    List<requesterUploadSchema> requester = _requester.getByLotAndBoi(item.lotNo, item.boiType);
                    Console.WriteLine(requester.Count);
                    foreach (requesterUploadSchema requesterRow in requester)
                    {
                        faeDBschema faeDB = null;
                        if (requesterRow.matrialCode != "-" && requesterRow.matrialName != "-")
                        {
                            faeDB = _faeDB.getByKind_matCode_matName(requesterRow.kind, requesterRow.matrialCode, requesterRow.matrialName);
                        }
                        else
                        {
                            faeDB = _faeDB.getByKind(requesterRow.kind);
                        }
                        if (faeDB != null)
                        {
                            _requester.updatePricingRequester(
                                requesterRow._id,
                                requesterRow.netWasteWeight,
                                faeDB.biddingType,
                                faeDB.wasteName,
                                faeDB.biddingNo,
                                faeDB.color,
                                faeDB.pricePerUnit
                                );
                            updatedRow += 1;
                        }

                    }
                }
                return Ok(new { success = true, message = "Updated " + updatedRow + " row completed" });
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }
    }

}