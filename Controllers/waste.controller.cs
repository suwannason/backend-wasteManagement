
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Authorization;

using backend.Services;
using backend.response;
using backend.Models;
using backend.request;
using System.Globalization;
using System.Linq;

namespace backend.Controllers
{

    [Authorize]
    [ApiController]
    [Route("fae-part/[controller]")]

    public class wasteController : ControllerBase
    {
        private readonly RecycleService _recycleService;
        private readonly CompanyService _company;
        private readonly faeDBservice _faeDB;

        RecycleWesteResponse res = new RecycleWesteResponse();

        public wasteController(RecycleService recycleService, CompanyService company, faeDBservice fae)
        {
            _recycleService = recycleService;
            _company = company;
            _faeDB = fae;
        }

        [HttpGet("open")]
        public ActionResult<RecycleWesteResponse> GetForCheck()
        {
            // string username = User.FindFirst("username")?.Value;
            List<Waste> data = _recycleService.GetOpen();

            List<Waste> grouped = data.GroupBy(x => new { x.moveOutDate, x.phase, x.boiType }).Select(y => new Waste
            {
                boiType = y.Key.boiType,
                moveOutDate = y.Key.moveOutDate,
                phase = y.Key.phase
            }).ToList();

            List<wasteGroupedRecord> returnData = new List<wasteGroupedRecord>();

            foreach (Waste item in grouped)
            {
                List<Waste> itemInGroup = _recycleService.getGroupingItems(item.moveOutDate, item.phase, item.boiType, "open");

                List<string> id = new List<string>();
                double totalNetweight = 0;

                foreach (Waste gItem in itemInGroup)
                {
                    totalNetweight += Double.Parse(gItem.netWasteWeight);
                    id.Add(gItem._id);
                }
                returnData.Add(new wasteGroupedRecord
                {
                    moveOutDate = item.moveOutDate,
                    boiType = item.boiType,
                    companyApprove = itemInGroup[0].companyApprove,
                    cptMainType = itemInGroup[0].cptMainType,
                    lotNo = itemInGroup[0].lotNo,
                    netWasteWeight = totalNetweight.ToString("##,###.00"),
                    phase = item.phase,
                    wasteGroup = itemInGroup[0].wasteGroup,
                    id = id.ToArray(),
                });
            }
            if (data.Count == 0)
            {
                return NotFound(new { success = true, message = "Data not found." });
            }
            res.message = "Get data success";
            return Ok(new { success = true, message = "Data record.", data = returnData });
        }

        [HttpGet("reject")]
        public ActionResult getReject()
        {
            try
            {
                List<Waste> data = _recycleService.GetReject();
                if (data.Count == 0)
                {
                    return NotFound(new { success = true, message = "Data not found." });
                }
                List<Waste> grouped = data.GroupBy(x => new { x.moveOutDate, x.phase, x.boiType }).Select(y => new Waste
                {
                    boiType = y.Key.boiType,
                    moveOutDate = y.Key.moveOutDate,
                    phase = y.Key.phase
                }).ToList();
                List<wasteGroupedRecord> returnData = new List<wasteGroupedRecord>();

                foreach (Waste item in grouped)
                {
                    List<Waste> itemInGroup = _recycleService.getGroupingItems(item.moveOutDate, item.phase, item.boiType, "reject");

                    // return Ok(itemInGroup);
                    double totalNetweight = 0;
                    List<string> id = new List<string>();

                    foreach (Waste gItem in itemInGroup)
                    {
                        totalNetweight += Double.Parse(gItem.netWasteWeight);
                        id.Add(gItem._id);
                    }
                    returnData.Add(new wasteGroupedRecord
                    {
                        moveOutDate = item.moveOutDate,
                        boiType = item.boiType,
                        companyApprove = itemInGroup[0].companyApprove,
                        cptMainType = itemInGroup[0].cptMainType,
                        lotNo = itemInGroup[0].lotNo,
                        netWasteWeight = totalNetweight.ToString("##,###.00"),
                        phase = item.phase,
                        wasteGroup = itemInGroup[0].wasteGroup,
                        id = id.ToArray(),
                    });
                }

                res.message = "Get data success";
                return Ok(new { success = true, message = "Data record.", data = returnData });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Problem(e.StackTrace);
            }
        }
        [HttpGet("approve")]
        public ActionResult<RecycleWesteResponse> GetForApprove()
        {
            try
            {
                List<Waste> data = _recycleService.GetApprove();
                if (data.Count == 0)
                {
                    return NotFound(new { success = true, message = "Data not found." });
                }
                List<Waste> grouped = data.GroupBy(x => new { x.moveOutDate, x.phase, x.boiType }).Select(y => new Waste
                {
                    boiType = y.Key.boiType,
                    moveOutDate = y.Key.moveOutDate,
                    phase = y.Key.phase
                }).ToList();
                List<wasteGroupedRecord> returnData = new List<wasteGroupedRecord>();

                foreach (Waste item in grouped)
                {
                    List<Waste> itemInGroup = _recycleService.getGroupingItems(item.moveOutDate, item.phase, item.boiType, "checked");

                    // return Ok(itemInGroup);
                    double totalNetweight = 0;
                    List<string> id = new List<string>();

                    foreach (Waste gItem in itemInGroup)
                    {
                        totalNetweight += Double.Parse(gItem.netWasteWeight);
                        id.Add(gItem._id);
                    }
                    returnData.Add(new wasteGroupedRecord
                    {
                        moveOutDate = item.moveOutDate,
                        boiType = item.boiType,
                        companyApprove = itemInGroup[0].companyApprove,
                        cptMainType = itemInGroup[0].cptMainType,
                        lotNo = itemInGroup[0].lotNo,
                        netWasteWeight = totalNetweight.ToString("##,###.00"),
                        phase = item.phase,
                        wasteGroup = itemInGroup[0].wasteGroup,
                        id = id.ToArray(),
                    });
                }

                res.message = "Get data success";
                return Ok(new { success = true, message = "Data record.", data = returnData });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Problem(e.StackTrace);
            }
        }

        [HttpPut("status")]
        public ActionResult<RecycleWesteResponse> UpdateToChecked(updateWasteStatus body)
        {
            // PREMISSION CHECKING
            Profile user = new Profile();

            user.empNo = User.FindFirst("username")?.Value;
            user.band = User.FindFirst("band")?.Value;
            user.dept = User.FindFirst("dept")?.Value;
            user.div = User.FindFirst("div")?.Value;
            user.name = User.FindFirst("name")?.Value;

            // PREMISSION CHECKING

            foreach (string item in body.body)
            {
                _recycleService.updateStatus(item, body.status, user);
            }
            res.success = true;
            res.message = "Update to " + body.status + " success";
            return Ok(res);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public ActionResult<RecycleWesteResponse> Create([FromForm] RequestRecycle body)
        {
            try
            {
                // PREMISSION CHECKING
                // PREMISSION CHECKING
                string rootFolder = Directory.GetCurrentDirectory();

                string pathString2 = @"\API site\files\wastemanagement\";
                Waste item = new Waste();
                item.companyApprove = body.companyApprove;
                item.containerType = body.containerType;
                item.containerWeight = body.containerWeight;
                item.cptMainType = body.cptMainType;
                item.wasteType = body.wasteType;
                item.boiType = body.boiType;
                item.contractorCompany = body.contractorCompany;
                item.productionType = body.productionType;
                item.partNormalType = body.partNormalType;

                DateTime parsed = DateTime.ParseExact(body.date, "yyyy/MM/dd", CultureInfo.InvariantCulture);
                item.moveOutDate = parsed.ToString("dd-MMMM-yyyy");
                item.invoiceRef = false;

                int numberOfList = 0;
                if (body.imageCapture != null)
                {
                    numberOfList = numberOfList + body.imageCapture.Length;
                }
                if (body.files != null)
                {
                    numberOfList = numberOfList + body.files.Length;
                }
                string[] allfile = new string[numberOfList];
                string serverPath = rootFolder.Substring(0, rootFolder.LastIndexOf(@"\")) + pathString2;

                if (!System.IO.Directory.Exists(serverPath))
                {
                    Directory.CreateDirectory(serverPath);
                }
                string g = Guid.NewGuid().ToString();

                int i = 0;
                if (body.files != null)
                {
                    foreach (var file in body.files)
                    {
                        if (body.files.Length > 0)
                        {
                            allfile[i] = $"{g}-{file.FileName}";
                            // Console.WriteLine($"{g}-{file.FileName}");
                            // serverPath + file.FileName
                            using (FileStream strem = System.IO.File.Create($"{serverPath}{g}-{file.FileName}"))
                            {
                                file.CopyTo(strem);
                            }
                        }
                        i = i + 1;
                    }
                }

                if (body.imageCapture != null)
                {
                    foreach (var file in body.imageCapture)
                    {
                        allfile[i] = file;
                        i = i + 1;
                    }
                }
                item.files = allfile;
                item.lotNo = body.lotNo;
                item.netWasteWeight = body.netWasteWeight;
                item.phase = body.phase;
                item.status = body.status;
                item.time = body.time;
                item.qtyOfContainer = body.qtyOfContainer;
                item.totalWeight = body.totalWeight;
                item.wasteGroup = body.wasteGroup;
                item.wasteName = body.wasteName;
                item.year = parsed.ToString("yyyy");
                item.month = parsed.ToString("MMMM");
                item.department = body.department;
                item.division = body.division;
                item.biddingType = body.biddingType;
                
               

                Companies contrator = _company.getByName(body.contractorCompany);

                item.contractEndDate = contrator.contractEndDate.Substring(0, contrator.contractEndDate.IndexOf(" "));
                item.contractStartDate = contrator.contractStartDate.Substring(0, contrator.contractStartDate.IndexOf(" "));
                item.contractNo = contrator.contractNo;

                faeDBschema faeData = _faeDB.getByBiddingType(item.biddingType);
                item.biddingNo = faeData.biddingNo;
                item.color = faeData.color;
                item.unitPrice = faeData.pricePerUnit;
                item.totalPrice = (Double.Parse(item.netWasteWeight) * Double.Parse(faeData.pricePerUnit)).ToString();
                item.unit = faeData.unit;

                Profile user = new Profile();
                Profile Emptyuser = new Profile();

                Emptyuser.empNo = "-";
                Emptyuser.band = "-";
                Emptyuser.dept = "-";
                Emptyuser.div = "-";
                Emptyuser.name = "-";
                Emptyuser.tel = "-";

                user.empNo = User.FindFirst("username")?.Value;
                user.band = User.FindFirst("band")?.Value;
                user.dept = User.FindFirst("dept")?.Value;
                user.div = User.FindFirst("div")?.Value;
                user.name = User.FindFirst("name")?.Value;
                user.tel = User.FindFirst("tel")?.Value;

                item.prepareBy = user;
                item.checkBy = Emptyuser;
                item.approveBy = Emptyuser;
                item.makingBy = Emptyuser;
                // item.createBy = User.FindFirst("username")?.Value;
                item.status = "open";
                DateTime createDate = DateTime.ParseExact(DateTime.Now.ToString("yyyy/MM/dd"), "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal);
                item.createDate = (Int64)(new DateTimeOffset(createDate)).ToUnixTimeSeconds();
                Waste created = _recycleService.Create(item);
                List<Waste> data = new List<Waste>();
                data.Add(created);

                res.success = true;
                res.message = "Insert success";
                res.data = data.ToArray();

                res.serverPath = serverPath;
                return Ok(res);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.StackTrace);
                return BadRequest(new { error = err.StackTrace, body });
            }
        }


        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public IActionResult Update(string id, [FromForm] RequestRecycle body)
        {
            try
            {
                var data = _recycleService.Get(id);
                if (data == null)
                {
                    return NotFound();
                }
                if (data.status == "toInvoice")
                {
                    res.success = false;
                    res.message = "Can't update because this.record set to invoice";

                    return Conflict(res);
                }
                Waste model = new Waste();

                model._id = id;
                model.companyApprove = body.companyApprove;
                model.containerType = body.containerType;
                model.containerWeight = body.containerWeight;
                model.cptMainType = body.cptMainType;
                model.wasteType = body.wasteType;
                model.boiType = body.boiType;
                model.partNormalType = body.partNormalType;
                model.moveOutDate = body.date;
                model.department = body.department;
                model.division = body.division;
                model.biddingType = body.biddingType;
                model.contractorCompany = body.contractorCompany;

                int numberOfList = 0;
                if (body.imageCapture != null)
                {
                    numberOfList = numberOfList + body.imageCapture.Length;
                }
                if (body.files != null)
                {
                    numberOfList = numberOfList + body.files.Length;
                }


                string[] allfile = new string[numberOfList];
                string rootFolder = Directory.GetCurrentDirectory();

                string pathString2 = @"\API site\files\wastemanagement\";

                string serverPath = rootFolder.Substring(0, rootFolder.LastIndexOf(@"\")) + pathString2;

                if (!System.IO.Directory.Exists(serverPath))
                {
                    Directory.CreateDirectory(serverPath);
                }
                string g = Guid.NewGuid().ToString();

                int i = 0;
                if (body.files != null)
                {
                    foreach (var file in body.files)
                    {
                        if (body.files.Length > 0)
                        {
                            allfile[i] = $"{g}-{file.FileName}";
                            // Console.WriteLine($"{g}-{file.FileName}");
                            // serverPath + file.FileName
                            using (FileStream strem = System.IO.File.Create($"{serverPath}{g}-{file.FileName}"))
                            {
                                file.CopyTo(strem);
                            }
                        }
                        i = i + 1;
                    }
                }

                if (body.imageCapture != null)
                {
                    foreach (var file in body.imageCapture)
                    {
                        allfile[i] = file;
                        i = i + 1;
                    }
                }
                model.files = allfile;
                model.lotNo = body.lotNo;
                // model.year = DateTime.Now.ToString("yyyy");
                model.month = DateTime.Now.ToString("MMM");
                model.netWasteWeight = body.netWasteWeight;
                if (body.status == "reject")
                {
                    model.status = "open";
                }
                model.phase = body.phase;
                model.qtyOfContainer = body.qtyOfContainer;
                model.status = body.status;
                model.time = body.time;
                model.totalWeight = body.totalWeight;
                model.wasteGroup = body.wasteGroup;
                model.wasteName = body.wasteName;
                model.year = DateTime.Now.ToString("yyyy");

                _recycleService.Update(id, model);
                res.success = true;
                res.message = "updaste success";
                return Ok(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                StatusCode(500);
                return Forbid();
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            _recycleService.Remove(id);

            res.success = true;
            res.message = "Delete success";
            return Ok(res);
        }

        [HttpPatch("history")]
        public IActionResult getHistory(RequestgetHistory body)
        {

            // PREMISSION CHECKING



            DateTime startDate = DateTime.ParseExact(body.startDate, "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal);
            DateTime endDate = DateTime.ParseExact(body.endDate, "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal);

            // Console.WriteLine(startDate + " ==> " + endDate);
            Int64 startDateTimestamp = (Int64)(new DateTimeOffset(startDate)).ToUnixTimeSeconds();
            Int64 endDateTimestamp = (Int64)(new DateTimeOffset(endDate)).ToUnixTimeSeconds();

            List<Waste> data = _recycleService.getHistory(startDateTimestamp, endDateTimestamp);
            res.success = true;
            res.message = "Get history success";

            res.data = data.ToArray();
            return Ok(res);
        }

        [HttpPatch("invoice/name")]
        public IActionResult getDataToInvoiceWasteName(RequestInvoiceDataWithName body)
        {
            // PREMISSION CHECKING

            // PREMISSION CHECKING



            List<Waste> data = _recycleService.getToInvoiceName(body);
            res.success = true;
            res.message = "Get data to invoice success";
            res.data = data.ToArray();

            return Ok(res);
        }

        [HttpPatch("detail")]
        public ActionResult getDataDetail(RequestGetDetail body)
        {
            // for prepare and check page status = open
            // for prepare and approve = checked
            List<Waste> data = _recycleService.getGroupingItems(body.moveOutDate, body.phase, body.boiType, body.status);

            return Ok(new { success = true, message = "Data record", data, });
        }

        [HttpPatch("reject")]
        public ActionResult rejectWaste(RejectWaste body)
        {

            foreach (string item in body.id)
            {
                _recycleService.rejectWaste(item, body.commend);
            }
            return Ok(new { success = true, message = "Reject waste success." });
        }
    
        [HttpGet("test"), AllowAnonymous]
        public ActionResult test() {

            Console.WriteLine(Double.Parse("155,600.05"));
            Console.WriteLine( Math.Truncate(Double.Parse("155,600.05")));
            return Ok();
        }

    }
}