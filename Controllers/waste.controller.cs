
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Authorization;

using backend.Services;
using backend.response;
using backend.Models;
using backend.request;
using Newtonsoft.Json.Linq;

namespace backend.Controllers
{

    [Authorize]
    [ApiController]
    [Route("fae-part/[controller]")]

    public class wasteController : ControllerBase
    {
        private readonly RecycleService _recycleService;

        RecycleWesteResponse res = new RecycleWesteResponse();

        public wasteController(RecycleService recycleService)
        {
            _recycleService = recycleService;
        }

        [HttpGet("open")]
        public ActionResult<RecycleWesteResponse> GetForCheck()
        {
            // string username = User.FindFirst("username")?.Value;

            List<Waste> data = _recycleService.GetOpen();
            res.success = true;
            res.data = data.ToArray();
            if (data.Count == 0)
            {
                res.message = "Notfound Data.";
                return NotFound(res);
            }
            res.message = "Get data success";
            return Ok(res);


        }

        [HttpGet("approve")]
        public ActionResult<RecycleWesteResponse> GetForApprove()
        {

            List<Waste> data = _recycleService.GetApprove();
            res.success = true;
            res.data = data.ToArray();
            if (data.Count == 0)
            {
                res.message = "Notfound Data.";
                return NotFound(res);
            }
            res.message = "Get data success";
            return Ok(res);
        }

        [HttpPut("status")]
        public ActionResult<RecycleWesteResponse> UpdateToChecked(updateWasteStatus body)
        {
            // PREMISSION CHECKING
            string permission = User.FindFirst("permission")?.Value;
            Profile user = new Profile();

            user.empNo = User.FindFirst("username")?.Value;
            user.band = User.FindFirst("band")?.Value;
            user.dept = User.FindFirst("dept")?.Value;
            user.div = User.FindFirst("div")?.Value;
            user.name = User.FindFirst("name")?.Value;

            JObject permissionObj = JObject.Parse(@"{ 'permission': " + permission + "}");

            int allowed = 0;
            foreach (var record in permissionObj["permission"])
            {
                if (body.status == "checked")
                {
                    if (record["dept"].ToString() == "FAE" && record["feature"].ToString() == "waste" && record["action"].ToString() == "check")
                    {
                        allowed = allowed + 1;
                        break;
                    }
                }
                if (body.status == "approve")
                {
                    if (record["dept"].ToString() == "FAE" && record["feature"].ToString() == "waste" && record["action"].ToString() == "approve")
                    {
                        allowed = allowed + 1;
                        break;
                    }
                }
            }
            if (allowed == 0)
            {
                return Forbid();
            }
            // PREMISSION CHECKING

            foreach (string item in body.body)
            {
                _recycleService.updateStatus(item, body.status, user);
            }
            res.success = true;
            res.message = "Update to " + body.status + " success";
            return Ok(res);
        }
        [HttpGet("{id}")]
        public ActionResult<Waste> Get(string id)
        {
            Waste book = _recycleService.Get(id);

            if (book == null)
            {
                return NotFound();
            }
            return book;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public ActionResult<RecycleWesteResponse> Create([FromForm] RequestRecycle body)
        {
            try
            {
                // PREMISSION CHECKING
                string permission = User.FindFirst("permission")?.Value;
                JObject permissionObj = JObject.Parse(@"{ 'permission': " + permission + "}");

                int allowed = 0;
                foreach (var record in permissionObj["permission"])
                {
                    if (record["dept"].ToString() == "FAE" && record["feature"].ToString() == "waste" && record["action"].ToString() == "prepare")
                    {
                        allowed = allowed + 1;
                        break;
                    }
                }
                if (allowed == 0)
                {
                    return Forbid();
                }
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
                item.moveOutDate = body.date;
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
                item.year = DateTime.Now.ToString("yyyy");
                item.month = DateTime.Now.ToString("MMM");
                item.department = body.department;
                item.division = body.division;
                item.biddingType = body.biddingType;

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
                return StatusCode(500);
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
            string permission = User.FindFirst("permission")?.Value;
            JObject permissionObj = JObject.Parse(@"{ 'permission': " + permission + "}");

            int allowed = 0;
            foreach (var item in permissionObj["permission"])
            {
                if (item["dept"].ToString() == "FAE")
                {
                    allowed = allowed + 1;
                    break;
                }
            }
            // PREMISSION CHECKING

            if (allowed == 0)
            {
                res.success = false;
                res.message = "Permission denied";
                return Forbid();
            }



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

        [HttpPatch("invoice/all")]
        public IActionResult getDataToInvoiceAll(RequestInvoiceDataAll body)
        {
            // PREMISSION CHECKING
            string permission = User.FindFirst("permission")?.Value;
            JObject permissionObj = JObject.Parse(@"{ 'permission': " + permission + "}");
            int allowed = 0;
            foreach (var item in permissionObj["permission"])
            {
                if (item["dept"].ToString() == "FAE" && item["feature"].ToString() == "invoice" && item["action"].ToString() == "prepare")
                {
                    allowed = allowed + 1;
                    break;
                }
            }
            if (allowed == 0)
            {
                return Forbid();
            }
            // PREMISSION CHECKING

            List<Waste> data = _recycleService.getToInvoiceAll(body);
            res.success = true;
            res.message = "Get data to invoice success";
            res.data = data.ToArray();

            return Ok(res);
        }

        [HttpPatch("invoice/name")]
        public IActionResult getDataToInvoiceWasteName(RequestInvoiceDataWithName body)
        {
            // PREMISSION CHECKING
            string permission = User.FindFirst("permission")?.Value;
            JObject permissionObj = JObject.Parse(@"{ 'permission': " + permission + "}");
            int allowed = 0;
            foreach (var item in permissionObj["permission"])
            {
                if (item["dept"].ToString() == "FAE" && item["feature"].ToString() == "invoice" && item["action"].ToString() == "prepare")
                {
                    allowed = allowed + 1;
                    break;
                }
            }
            if (allowed == 0)
            {
                return Forbid();
            }
            // PREMISSION CHECKING



            List<Waste> data = _recycleService.getToInvoiceName(body);
            res.success = true;
            res.message = "Get data to invoice success";
            res.data = data.ToArray();

            return Ok(res);
        }
    }
}