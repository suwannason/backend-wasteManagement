
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Authorization;

using backend.Services;
using backend.response;
using backend.Models;
using backend.request;
using Newtonsoft.Json;
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

            foreach (var item in body.body)
            {
                _recycleService.updateStatus(item, body.status);
            }
            res.success = true;
            res.message = "Update to " + body.status + " success";
            return Ok(res);
        }
        [HttpGet("{id}")]
        public ActionResult<Waste> Get(string id)
        {
            var book = _recycleService.Get(id);

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
                string rootFolder = Directory.GetCurrentDirectory();

                string pathString2 = @"\files\";
                Waste item = new Waste();
                item.companyApprove = body.companyApprove;
                item.containerType = body.containerType;
                item.containerWeight = body.containerWeight;
                item.cptType = body.cptType;
                item.date = body.date;

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
                item.gennerateGroup = body.gennerateGroup;
                item.lotNo = body.lotNo;
                item.netWasteWeight = body.netWasteWeight;
                item.phase = body.phase;
                item.status = body.status;
                item.time = body.time;
                item.totalWeight = body.totalWeight;
                item.typeBoi = body.typeBoi;
                item.wasteContractor = body.wasteContractor;
                item.wasteGroup = body.wasteGroup;
                item.wasteName = body.wasteName;
                item.year = DateTime.Now.ToString("yyyy");
                item.month = DateTime.Now.ToString("MMM");
                item.createBy = User.FindFirst("username")?.Value;
                item.status = "open";
                item.createDate = DateTimeOffset.Now.ToUnixTimeSeconds();
                Waste created = _recycleService.Create(item);
                List<Waste> data = new List<Waste>();
                data.Add(created);

                res.success = true;
                res.message = "Insert success";
                res.data = data.ToArray();

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
                Waste model = new Waste();

                model._id = id;
                model.companyApprove = body.companyApprove;
                model.containerType = body.containerType;
                model.containerWeight = body.containerWeight;
                model.cptType = body.cptType;
                model.date = body.date;

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

                string pathString2 = @"\files\";

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
                model.gennerateGroup = body.gennerateGroup;
                model.lotNo = body.lotNo;
                // model.year = DateTime.Now.ToString("yyyy");
                model.month = DateTime.Now.ToString("MMM");
                model.netWasteWeight = body.netWasteWeight;

                model.phase = body.phase;
                model.qtyOfContainer = body.qtyOfContainer;
                model.status = "open";
                model.time = body.time;
                model.totalWeight = body.totalWeight;
                model.typeBoi = body.typeBoi;
                model.wasteContractor = body.wasteContractor;
                model.wasteGroup = body.wasteGroup;
                model.wasteName = body.wasteName;
                model.year = DateTime.Now.ToString("yyyy");

                _recycleService.Update(id, model);
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
            JObject studentObj = JObject.Parse(@"{ 'permission': " + permission + "}");
            Console.WriteLine(studentObj["permission"][0]);
            // PREMISSION CHECKING



            DateTime startDate = DateTime.ParseExact(body.startDate, "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(body.endDate, "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture);

            Int64 startDateTimestamp = (Int64)(new DateTimeOffset(startDate)).ToUnixTimeSeconds();
            Int64 endDateTimestamp = (Int64)(new DateTimeOffset(endDate)).ToUnixTimeSeconds();
            // var charsToRemove = new string[] { "/" };
            // foreach (var c in charsToRemove)
            // {
            //     body.startDate = body.startDate.Replace(c, string.Empty);
            //     body.endDate = body.endDate.Replace(c, string.Empty);
            // }
            List<Waste> data = _recycleService.getHistory(startDateTimestamp, endDateTimestamp);
            res.success = true;
            res.message = "Get history success";

            res.data = data.ToArray();
            return Ok(res);
        }
    }
}