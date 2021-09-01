
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.request;
using backend.Models;
using System.IO;
using System;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Net.Http;

namespace backend.Controllers
{
    [Route("fae-part/[controller]")]
    [ApiController]
    [Authorize]
    public class masterController : ControllerBase
    {

        private readonly faeDBservice _faedb;
        private readonly itcDBservice _itcdb;
        private readonly CompanyService _company;
        private IConfiguration _config;

        public masterController(faeDBservice fae, itcDBservice itc, CompanyService company, IConfiguration config)
        {
            _faedb = fae;
            _itcdb = itc;
            _company = company;
            _config = config;
        }

        [HttpPost("pricing"), Consumes("multipart/form-data")]
        public ActionResult faeUpload([FromForm] uploadFile body)
        {
            string rootFolder = Directory.GetCurrentDirectory();

            string pathString2 = @"\API site\files\wastemanagement\upload\";
            string serverPath = rootFolder.Substring(0, rootFolder.LastIndexOf(@"\")) + pathString2;

            Console.WriteLine(serverPath);
            if (!System.IO.Directory.Exists(serverPath))
            {
                Directory.CreateDirectory(serverPath);
            }

            string filename = serverPath + System.Guid.NewGuid().ToString() + "-" + body.file.FileName;
            using (FileStream strem = System.IO.File.Create(filename))
            {
                body.file.CopyTo(strem);
            }
            FileInfo existFile = new FileInfo(filename);

            Profile user = new Profile();

            user.empNo = User.FindFirst("username")?.Value;
            user.band = User.FindFirst("band")?.Value;
            user.dept = User.FindFirst("dept")?.Value;
            user.div = User.FindFirst("div")?.Value;
            user.name = User.FindFirst("name")?.Value;
            user.tel = User.FindFirst("tel")?.Value;

            using (ExcelPackage package = new ExcelPackage(existFile))
            {
                ExcelWorkbook Workbook = package.Workbook;
                ExcelWorksheet sheet = Workbook.Worksheets[0];

                List<faeDBschema> data = new List<faeDBschema>();

                int rowCount = sheet.Dimension.Rows;

                if (sheet.Cells[3, 1].Value.ToString() != "Bidding No.")
                {
                    return BadRequest(new { success = false, message = "File upload invalid." });
                }
                for (int row = 4; row < rowCount; row += 1)
                {
                    faeDBschema item = new faeDBschema();
                    for (int col = 1; col <= 11; col += 1)
                    {
                        string value = "-";
                        if (sheet.Cells[row, col].Value != null)
                        {
                            value = sheet.Cells[row, col].Value.ToString();
                        }
                        switch (col)
                        {
                            case 1: item.biddingNo = value; break;
                            case 2: item.biddingType = value; break;
                            case 3: item.wasteName = value; break;
                            case 5: item.color = value; break;
                            case 6: item.kind = value; break;
                            case 7: item.unit = value; break;
                            case 8: item.pricePerUnit = value; break;
                            case 9: item.matrialCode = value; break;
                            case 10: item.matrialName = value; break;
                            case 11: item.colorName = value; break;
                        }

                    }
                    data.Add(item);
                }

                _faedb.replace(data);

            }

            return Ok(new { success = true, message = "Upload success." });
        }

        [HttpPost("boi"), Consumes("multipart/form-data")]
        public ActionResult itcUpload([FromForm] uploadFile body)
        {
            string rootFolder = Directory.GetCurrentDirectory();

            string pathString2 = @"\API site\files\wastemanagement\upload\";
            string serverPath = rootFolder.Substring(0, rootFolder.LastIndexOf(@"\")) + pathString2;

            Console.WriteLine(serverPath);
            if (!System.IO.Directory.Exists(serverPath))
            {
                Directory.CreateDirectory(serverPath);
            }

            string filename = serverPath + System.Guid.NewGuid().ToString() + "-" + body.file.FileName;
            using (FileStream strem = System.IO.File.Create(filename))
            {
                body.file.CopyTo(strem);
            }
            FileInfo existFile = new FileInfo(filename);

            using (ExcelPackage package = new ExcelPackage(existFile))
            {
                ExcelWorkbook Workbook = package.Workbook;
                ExcelWorksheet sheet = Workbook.Worksheets[0];

                int colCount = sheet.Dimension.Columns;
                int rowCount = sheet.Dimension.Rows;


                if (sheet.Cells[1, 4].Value.ToString() != "Department")
                {
                    return BadRequest(new { success = false, messagge = "File upload invalid." });
                }
                List<ITCDB> data = new List<ITCDB>();

                for (int row = 4; row < rowCount; row += 1)
                {
                    ITCDB item = new ITCDB();
                    for (int col = 1; col <= 10; col += 1)
                    {
                        string value = "-";
                        if (sheet.Cells[row, col].Value != null)
                        {
                            value = sheet.Cells[row, col].Value.ToString();
                        }
                        switch (col)
                        {
                            case 1: item.no = value; break;
                            case 2: item.matrialCode = value; break;
                            case 3: item.matrialName = value; break;
                            case 4: item.dept = value; break;
                            case 5: item.privilegeType = value; break;
                            case 6: item.groupBoiNo = value; break;
                            case 7: item.groupBoiName = value; break;
                            case 8: item.unit = value; break;
                            case 9: item.supplier = value; break;
                            case 10: item.remark = value; break;
                        }

                    }
                    data.Add(item);
                }
                _itcdb.replace(data);

            }
            return Ok(new { success = true, message = "Upload ITC DB success." });
        }

        [HttpPost("company"), Consumes("multipart/form-data")]
        public ActionResult companyUpload([FromForm] uploadFile body)
        {

            string rootFolder = Directory.GetCurrentDirectory();

            string pathString2 = @"\API site\files\wastemanagement\upload\";
            string serverPath = rootFolder.Substring(0, rootFolder.LastIndexOf(@"\")) + pathString2;

            Console.WriteLine(serverPath);
            if (!System.IO.Directory.Exists(serverPath))
            {
                Directory.CreateDirectory(serverPath);
            }
            string uuid = System.Guid.NewGuid().ToString();
            string filename = (serverPath + uuid + "-" + body.file.FileName).Trim();
            using (FileStream strem = System.IO.File.Create(filename))
            {
                body.file.CopyTo(strem);
            }
            FileInfo existFile = new FileInfo(filename);

            using (ExcelPackage package = new ExcelPackage(existFile))
            {
                ExcelWorkbook Workbook = package.Workbook;
                ExcelWorksheet sheet = Workbook.Worksheets[0];

                int colCount = sheet.Dimension.Columns;
                int rowCount = sheet.Dimension.Rows;

                List<Companies> data = new List<Companies>();

                if (sheet.Cells[1, 1].Value.ToString() != "Case")
                {

                    return BadRequest(new { success = false, message = "File upload invalid." });
                }
                for (int row = 2; row < rowCount; row += 1)
                {
                    Companies item = new Companies();
                    for (int col = 1; col <= 9; col += 1)
                    {
                        string value = "-";
                        if (sheet.Cells[row, col].Value != null)
                        {
                            value = sheet.Cells[row, col].Value.ToString();
                        }
                        switch (col)
                        {
                            case 1: break;
                            case 2: item.no = value; break;
                            case 3: item.companyName = value; break;
                            case 4: item.address = value; break;
                            case 5: item.tel = value; break;
                            case 6: item.fax = value; break;
                            case 7: item.contractNo = value; break;
                            case 8: item.contractStartDate = value; break;
                            case 9: item.contractEndDate = value; break;
                        }
                        item.fileName = (uuid + "-" + body.file.FileName).Trim();
                    }
                    data.Add(item);
                }
                _company.upload(data);
                // _itcdb.replace(data);

            }
            return Ok(new { success = true, message = "Upload Contrator DB success." });
        }

        [HttpGet("company/download")]
        public async Task<ActionResult> companyDownload()
        {

            Companies data = _company.getFirst();
            string fileUri = (_config["Endpoint:file_path"] + "/upload/" +data.fileName).Trim();

            Console.WriteLine(fileUri);
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync(fileUri);
            
            if (response.StatusCode.ToString() == "NotFound") {
                return NotFound();
            }
            return File(response.Content.ReadAsStream(), contentType);
        }

        [HttpGet("wastename")]
        public ActionResult getWastename()
        {

            List<faeDBschema> data = _faedb.getWastename();

            List<faeDBschema> distinct = data.GroupBy(x => x.wasteName).Select(x => x.First()).ToList();

            List<dynamic> wastename = new List<dynamic>();
            foreach (faeDBschema item in distinct)
            {
                wastename.Add(new
                {
                    wasteName = item.wasteName,
                    biddingType = item.biddingType,
                });
            }
            return Ok(new { success = true, message = "waste name", data = wastename });
        }
    }
}