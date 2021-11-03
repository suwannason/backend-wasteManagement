
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;

namespace backend.Controllers
{
    [Route("fae-part/[controller]")]
    [ApiController, Authorize]
    public class infectionController : ControllerBase
    {
        private readonly InfectionService _tb;
        private readonly UserService _user;
        public infectionController(InfectionService infection, UserService user)
        {
            _tb = infection;
            _user = user;
        }

        [HttpGet("getById/{id}")]
        public ActionResult getById(string id)
        {

            InfectionSchema data = _tb.getById(id);

            return Ok(new { success = true, message = "Data infection.", data, });
        }
        [HttpPost("upload"), Consumes("multipart/form-data")]
        public ActionResult upload([FromForm] request.uploadFile body)
        {
            try
            {
                string rootFolder = Directory.GetCurrentDirectory();
                string pathString2 = @"\API site\files\wastemanagement\upload\";
                string serverPath = rootFolder.Substring(0, rootFolder.LastIndexOf(@"\")) + pathString2;

                if (!System.IO.Directory.Exists(serverPath))
                {
                    Directory.CreateDirectory(serverPath);
                }

                string fiilServername = System.Guid.NewGuid().ToString() + "-" + body.file.FileName;
                string filename = serverPath + fiilServername;

                double totalNetweight = 0;
                using (FileStream strem = System.IO.File.Create(filename))
                {
                    body.file.CopyTo(strem);
                }
                InfectionSchema data = new InfectionSchema();
                List<InfectionItems> infectionList = new List<InfectionItems>();
                using (ExcelPackage excel = new ExcelPackage(new FileInfo(filename)))
                {
                    ExcelWorkbook workbook = excel.Workbook;
                    ExcelWorksheet sheet = workbook.Worksheets[0];
                    int rowCount = sheet.Dimension.Rows;

                    Console.WriteLine(sheet.Cells[2, 1].Value);

                    data.date = DateTime.Now.ToString("dd-MMM-yyyy");
                    data.time = DateTime.Now.ToString("hh:mm");
                    data.phase = "-";
                    data.dept = User.FindFirst("dept")?.Value;
                    data.filename = fiilServername;
                    data.req_prepared = new request.Profile
                    {
                        date = DateTime.Now.ToString("yyyy/MM/dd"),
                        empNo = User.FindFirst("username")?.Value,
                        name = User.FindFirst("name")?.Value,
                        dept = User.FindFirst("dept")?.Value,
                        tel = sheet.Cells[2, 2].Value?.ToString()
                    };
                    data.div = User.FindFirst("div")?.Value;
                    data.status = "req-prepared";
                    data.year = DateTime.Now.ToString("yyyy");
                    data.month = DateTime.Now.ToString("MMMM");
                    data.req_checked = new request.Profile();
                    data.req_approved = new request.Profile();
                    data.fae_prepared = new request.Profile();

                    for (int row = 4; row <= rowCount; row += 1)
                    {
                        string no = sheet.Cells[row, 1].Value?.ToString();
                        if (String.IsNullOrEmpty(no))
                        {
                            break;
                        }
                        InfectionItems item = new InfectionItems();
                        for (int col = 1; col <= 5; col += 1)
                        {
                            string value = sheet.Cells[row, col].Value?.ToString();

                            switch (col)
                            {
                                case 1: item.no = value; break;
                                case 2: item.date = DateTime.FromOADate(Double.Parse(value)).ToString("dd-MMM-yyyy"); break;
                                case 3:
                                    item.totalWeight = value;
                                    totalNetweight += Double.Parse(value);
                                    break;
                                case 4: item.agency = value; break;
                                case 5: item.remark = value; break;
                            }
                        }
                        infectionList.Add(item);
                    }
                }
                data.netWasteWeight = totalNetweight.ToString();
                data.items = infectionList.ToArray();
                if (data.items.Length == 0)
                {
                    return BadRequest(new { success = false, messge = "No data upload, Please check file upload." });
                }
                _tb.create(data);
                return Ok(new { success = true, message = "Upload completed" });
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                return Problem(e.StackTrace);
            }
        }

        [HttpGet("{status}")]
        public ActionResult getByStatus(string status)
        {
            string dept = User.FindFirst("dept")?.Value;

            List<InfectionSchema> data = _tb.getByStatus(status, dept);
            return Ok(new { success = true, message = "Infections items", data, });
        }
        [HttpPut("status")]
        public ActionResult updateStatus(request.UpdateStatusFormRequester body)
        {

            request.Profile user = new request.Profile
            {
                date = DateTime.Now.ToString("yyyy/MM/dd"),
                empNo = User.FindFirst("username")?.Value,
                name = User.FindFirst("name")?.Value,
                dept = User.FindFirst("dept")?.Value,
                tel = "-"
            };

            List<UserSchema> userDB = _user.Getlist(user.empNo);

            if (body.status.IndexOf("check") > 0 && userDB.FindAll(item => item.permission == "Checked").Count == 0)
            {
                return Unauthorized(new { success = false, message = "Can't check, Permission denied." });
            }
            else if (body.status.IndexOf("approve") > 0 && userDB.FindAll(item => item.permission == "Approved" || user.dept.ToUpper() == "FAE").Count == 0)
            {
                return Unauthorized(new { success = false, message = "Can't approve, Permission denied." });
            }

            foreach (string id in body.id)
            {
                _tb.updateStatus(id, body.status, user);
            }
            return Ok(new { success = true, message = "Update status success." });
        }

        [HttpPatch("reject")]
        public ActionResult reject(request.RejectRequest body)
        {

            foreach (string item in body.id)
            {

                _tb.reject(item, body.commend);
            }
            return Ok(new { success = true, message = "Reject infection success." });
        }


        [HttpPut("upload"),]
        public ActionResult editByUpload([FromForm] request.editByUpload body)
        {
            try
            {
                _tb.deleteByFileName(body.sourceFilename);

                string rootFolder = Directory.GetCurrentDirectory();
                string pathString2 = @"\API site\files\wastemanagement\upload\";
                string serverPath = rootFolder.Substring(0, rootFolder.LastIndexOf(@"\")) + pathString2;

                if (!System.IO.Directory.Exists(serverPath))
                {
                    Directory.CreateDirectory(serverPath);
                }

                string fiilServername = System.Guid.NewGuid().ToString() + "-" + body.file.FileName;
                string filename = serverPath + fiilServername;

                double totalNetweight = 0;
                using (FileStream strem = System.IO.File.Create(filename))
                {
                    body.file.CopyTo(strem);
                }
                InfectionSchema data = new InfectionSchema();
                List<InfectionItems> infectionList = new List<InfectionItems>();
                using (ExcelPackage excel = new ExcelPackage(new FileInfo(filename)))
                {
                    ExcelWorkbook workbook = excel.Workbook;
                    ExcelWorksheet sheet = workbook.Worksheets[0];
                    int rowCount = sheet.Dimension.Rows;

                    Console.WriteLine(sheet.Cells[2, 1].Value);

                    data.date = DateTime.Now.ToString("dd-MMM-yyyy");
                    data.time = DateTime.Now.ToString("hh:mm");
                    data.phase = "-";
                    data.dept = User.FindFirst("dept")?.Value;
                    data.filename = fiilServername;
                    data.req_prepared = new request.Profile
                    {
                        date = DateTime.Now.ToString("yyyy/MM/dd"),
                        empNo = User.FindFirst("username")?.Value,
                        name = User.FindFirst("name")?.Value,
                        dept = User.FindFirst("dept")?.Value,
                        tel = sheet.Cells[2, 2].Value?.ToString()
                    };
                    data.div = User.FindFirst("div")?.Value;
                    data.status = "req-prepared";
                    data.year = DateTime.Now.ToString("yyyy");
                    data.month = DateTime.Now.ToString("MMMM");
                    data.req_checked = new request.Profile();
                    data.req_approved = new request.Profile();
                    data.fae_prepared = new request.Profile();

                    for (int row = 4; row <= rowCount; row += 1)
                    {
                        string no = sheet.Cells[row, 1].Value?.ToString();
                        if (String.IsNullOrEmpty(no))
                        {
                            break;
                        }
                        InfectionItems item = new InfectionItems();
                        for (int col = 1; col <= 5; col += 1)
                        {
                            string value = sheet.Cells[row, col].Value?.ToString();

                            switch (col)
                            {
                                case 1: item.no = value; break;
                                case 2: item.date = DateTime.FromOADate(Double.Parse(value)).ToString("dd-MMM-yyyy"); break;
                                case 3:
                                    item.totalWeight = value;
                                    totalNetweight += Double.Parse(value);
                                    break;
                                case 4: item.agency = value; break;
                                case 5: item.remark = value; break;
                            }
                        }
                        infectionList.Add(item);
                    }
                }
                data.netWasteWeight = totalNetweight.ToString();
                data.items = infectionList.ToArray();
                if (data.items.Length == 0)
                {
                    return BadRequest(new { success = false, messge = "No data upload, Please check file upload." });
                }
                _tb.create(data);
                return Ok(new { success = true, message = "Edit complete" });
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                return Problem(e.StackTrace);
            }

        }

        [HttpPatch("history")]
        public ActionResult getHistory(request.requesterHistory body)
        {
            try
            {
                string dept = User.FindFirst("dept")?.Value;
                List<InfectionSchema> data = _tb.getHistory(body.month, body.year, dept);

                List<dynamic> returnData = new List<dynamic>();

                foreach (InfectionSchema item in data)
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
                        status = "Wait FAE acknowledge";
                    }
                    else if (item.status == "fae-approved")
                    {
                        status = "Completed";
                    }

                    returnData.Add(new
                    {
                        id = item._id,
                        status = status,
                        requestDate = item.date,
                        dept = item.dept,
                        phase = item.phase,
                        netWasteWeight = item.netWasteWeight
                    });

                }
                return Ok(new { success = true, message = "Infection data history", data = returnData });
            }
            catch (System.Exception e)
            {
                return Problem(e.StackTrace);
            }
        }
    }
}