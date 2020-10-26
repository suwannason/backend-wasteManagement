
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using backend.Services;
using backend.response;
using backend.Models;
using backend.request;

namespace backend.Controllers
{
    [ApiController]
    [Route("fae-part/[controller]")]
    public class subRecycleWasteController : ControllerBase
    {

        private readonly SubRecycleService _subRecycleService;

        RecycleWesteResponse res = new RecycleWesteResponse();

        public subRecycleWasteController(SubRecycleService recycleService)
        {
            _subRecycleService = recycleService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public ActionResult<DefalutResponse> Create([FromForm] RequestSubRecycle body)
        {
            try
            {
                string rootFolder = Directory.GetCurrentDirectory();

                string pathString2 = @"\files\";

                string serverPath = rootFolder.Substring(0, rootFolder.LastIndexOf(@"\")) + pathString2;

                SubRecycleWaste DB = new SubRecycleWaste();

                if (!System.IO.Directory.Exists(serverPath))
                {
                    Directory.CreateDirectory(serverPath);
                }
                string[] allfile = new string[body.files.Length];

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
                DB.allWeight = body.allWeight;
                DB.containerWeight = body.containerWeight;
                DB.factory = body.factory;
                DB.idMapping = body.idMapping;
                DB.wasteType = body.wastype;
                DB.files = allfile;
                DB.total = body.total;
                _subRecycleService.Create(DB);
                res.success = true;
                res.message = serverPath;

                return Ok(res);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.StackTrace);
                return StatusCode(500);
            }
        }


        [HttpPut("{id}")]
        public IActionResult Update(string id, SubRecycleWaste body)
        {
            try
            {
                _subRecycleService.Update(id, body);
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
            _subRecycleService.Remove(id);

            res.success = true;
            res.message = "Delete success";
            return Ok(res);
        }

        [HttpGet("{idMapping}")]
        public IActionResult GetData(string idMapping) {
            List<SubRecycleWaste> data = _subRecycleService.GetByMapping(idMapping);

            SubRecycleResponse response = new SubRecycleResponse();

            response.success = true;
            response.message = "Get data success";
            response.data = data.ToArray();
            return Ok(response);

        }

    }
}