
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Authorization;

using backend.Services;
using backend.response;
using backend.Models;
using backend.request;

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
            string username = User.FindFirst("username")?.Value;
            Console.Write(username);
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
        public ActionResult<RecycleWesteResponse> UpdateToChecked(updateWasteStatus body) {

            foreach(var item in body.body) {
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

                string[] allfile = new string[body.files.Length];
                string serverPath = rootFolder.Substring(0, rootFolder.LastIndexOf(@"\")) + pathString2;

                if (!System.IO.Directory.Exists(serverPath))
                {
                    Directory.CreateDirectory(serverPath);
                }
                string g = Guid.NewGuid().ToString();

                int i = 0;
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
                item.files = allfile;
                item.gennerateGroup = body.gennerateGroup;
                item.lotNo = body.lotNo;
                item.netWasteWeight = body.netWasteWeight;
                item.phase = body.phase;
                item.status = "open";
                item.time = body.time;
                item.totalWeight = body.totalWeight;
                item.typeBoi = body.typeBoi;
                item.wasteContractor = body.wasteContractor;
                item.wasteGroup = body.wasteGroup;
                item.wasteName = body.wasteName;
                item.year = DateTime.Now.ToString("yyyy");
                item.month = DateTime.Now.ToString("MMM");
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
        public IActionResult Update(string id, Waste body)
        {
            try
            {
                var data = _recycleService.Get(id);

                if (data == null)
                {
                    return NotFound();
                }
                _recycleService.Update(id, body);
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


    }
}