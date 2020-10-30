
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using System.IO;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;

using backend.request;
using backend.response;

namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("fae-part/[controller]")]
    public class handlerController : ControllerBase
    {

        [HttpPost("base64ToImage")]
        public ActionResult<convertBase64ToimageRes> base64ToImage(base64ToImageReq body)
        {
            string rootFolder = Directory.GetCurrentDirectory();

            string pathString2 = @"\files\";

            convertBase64ToimageRes res = new convertBase64ToimageRes();

            string serverPath = rootFolder.Substring(0, rootFolder.LastIndexOf(@"\")) + pathString2;

            if (!System.IO.Directory.Exists(serverPath))
            {
                Directory.CreateDirectory(serverPath);
            }

            string[] fileNameList = new string[body.data.Length];

            int i = 0;
            foreach (string base64data in body.data)
            {
                string pureBase64Str = body.data[i].Substring(body.data[i].IndexOf(",") + 1);
                byte[] imageBytes = Convert.FromBase64String(pureBase64Str);
                string uuid = Guid.NewGuid().ToString();

                fileNameList[i] = uuid + ".png";

                using (var imageFile = new FileStream(serverPath + @"\" + uuid + ".png", FileMode.Create))
                {
                    imageFile.Write(imageBytes, 0, imageBytes.Length);
                    imageFile.Flush();
                }
                i = i + 1;
            }

            res.success = true;
            res.message = "Converst base64 to image success";
            res.data = fileNameList;

            return Ok(res);
            // string g = Guid.NewGuid().ToString();
        }
    }
}