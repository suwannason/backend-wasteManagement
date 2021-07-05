
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
    public class hazadousController : ControllerBase
    {

        private readonly HazadousService _tb;
        public hazadousController(HazadousService hazadous)
        {
            _tb = hazadous;
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
                HazadousSchema data = new HazadousSchema();
                List<HazadousItems> hazadousList = new List<HazadousItems>();
                using (ExcelPackage excel = new ExcelPackage(new FileInfo(filename)))
                {
                    ExcelWorkbook workbook = excel.Workbook;
                    ExcelWorksheet sheet = workbook.Worksheets[0];
                    int rowCount = sheet.Dimension.Rows;

                    data.date = DateTime.Now.ToString("dd-MMM-yyyy");
                    data.time = DateTime.Now.ToString("hh:mm");
                    data.phase = "Phase " + sheet.Cells[1, 2].Value?.ToString();
                    data.dept = User.FindFirst("dept")?.Value;
                    data.div = User.FindFirst("div")?.Value;
                    data.description = "-";
                    data.req_prepared = new request.Profile
                    {
                        date = DateTime.Now.ToString("yyyy/MM/dd"),
                        empNo = User.FindFirst("username")?.Value,
                        name = User.FindFirst("name")?.Value,
                        dept = User.FindFirst("dept")?.Value,
                        div = User.FindFirst("div")?.Value,
                        tel = sheet.Cells[2, 2].Value?.ToString()
                    };
                    data.div = "-";
                    data.status = "req-prepared";
                    data.year = DateTime.Now.ToString("yyyy");
                    data.month = DateTime.Now.ToString("MMMM");
                    data.description = "-";
                    data.req_checked = new request.Profile();
                    data.req_approved = new request.Profile();
                    data.fae_approved = new request.Profile();
                    data.fae_checked = new request.Profile();
                    data.fae_prepared = new request.Profile();

                    for (int row = 9; row <= rowCount; row += 1)
                    {
                        string no = sheet.Cells[row, 1].Value?.ToString();
                        if (String.IsNullOrEmpty(no))
                        {
                            break;
                        }
                        string biddingType = "-";
                        HazadousItems item = new HazadousItems();
                        for (int col = 1; col <= 24; col += 1)
                        {
                            string value = sheet.Cells[row, col].Value?.ToString();
                            if (col == 5 && value != null)
                            {
                                biddingType = "เศษดีบุกจากการบัดกรี (Dross)";
                            }
                            else if (col == 6 && value != null)
                            {
                                biddingType = "วัสดุดูดซับปนเปื้อน, ฟิวเตอร์ (กล่อง/แท่ง)";
                            }
                            else if (col == 7 && value != null)
                            {
                                biddingType = "หลอดไฟ";
                            }
                            else if (col == 8 && value != null)
                            {
                                biddingType = "แบตเตอรี่";
                            }
                            else if (col == 9 && value != null)
                            {
                                biddingType = "กระป๋องสเปรย์";
                            }
                            else if (col == 10 && value != null)
                            {
                                biddingType = "สารดูดความชื้น";
                            }
                            else if (col == 11 && value != null)
                            {
                                biddingType = "เรซิ่นที่ไม่ใช้แล้ว";
                            }
                            else if (col == 12 && value != null)
                            {
                                biddingType = "ภาชนะปนเปื้อน";
                            }
                            else if (col == 13 && value != null)
                            {
                                biddingType = "คาร์บอนฟิวเตอร์";
                            }
                            else if (col == 14 && value != null)
                            {
                                biddingType = "กระบอกโทนเนอร์";
                            }
                            else if (col == 15 && value != null)
                            {
                                biddingType = "น้ำปนเปื้อนน้ำมัน";
                            }
                            else if (col == 16 && value != null)
                            {
                                biddingType = "น้ำมันที่ใช้แล้ว";
                            }
                            else if (col == 17 && value != null)
                            {
                                biddingType = "ฝุ่นจากระบบบำบัดอากาศ";
                            }
                            else if (col == 18 && value != null)
                            {
                                biddingType = "ถังทำความเย็น";
                            }
                            else if (col == 19 && value != null)
                            {
                                biddingType = "สารเคมีเสื่อมสภาพที่ใช้แล้ว";
                            }
                            else if (col == 20 && value != null)
                            {
                                biddingType = "สารเคมีเสื่อมสภาพ";
                            }
                            else if (col == 21 && value != null)
                            {
                                biddingType = "อื่น ๆ";
                            }
                            switch (col)
                            {
                                case 1: item.no = value; break;
                                case 2: item.wasteName = value; break;
                                case 22: item.containerType = value; break;
                                case 23: item.workCount = value; break;
                                case 24:
                                    item.totalWeight = value;
                                    totalNetweight += Double.Parse(value);
                                    break;
                            }
                            item.howTodestroy = "-";
                            item.biddingType = biddingType;
                        }
                        hazadousList.Add(item);
                    }
                }
                data.netWasteWeight = totalNetweight.ToString();
                data.items = hazadousList.ToArray();
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

            List<HazadousSchema> data = _tb.getByStatus(status, dept);
            return Ok(new { success = true, message = "Hazadous items", data, });
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
            foreach (string id in body.id)
            {
                _tb.updateStatus(id, body.status, user);
            }
            return Ok(new { success = true, message = "Update status success." });
        }
    }
}