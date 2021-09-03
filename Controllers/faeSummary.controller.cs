
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.IO;

using OfficeOpenXml;
using backend.Services;
using backend.Models;
using backend.request;
using System.Linq;
using System.Drawing;
using OfficeOpenXml.Drawing;
using System.Net;

namespace backend.Controllers
{

    // [Authorize]
    [ApiController]
    [Route("fae-part/[controller]")]
    public class summaryController : ControllerBase
    {

        private readonly SummaryInvoiceService _services;
        private readonly RecycleService _recycle;
        private readonly requesterUploadServices _requester;
        private readonly faeDBservice _faeDB;

        public summaryController(SummaryInvoiceService summary, RecycleService recycle, requesterUploadServices requester, faeDBservice fae)
        {
            _services = summary;
            _recycle = recycle;
            _requester = requester;
            _faeDB = fae;
        }
        [HttpGet("get/{id}")]
        public ActionResult getByIdSummary(string id)
        {

            SummaryInvoiceSchema data = _services.getById(id);

            return Ok(new { success = true, message = "Summary data.", data, });
        }

        [HttpPost("search")]
        public ActionResult searchPrepare(dataSearch body)
        {
            try
            {

                List<requesterUploadSchema> requester = _requester.faeSummarySearch(body.lotNo, body.startDate, body.endDate, body.wasteName, body.phase);
                List<Waste> waste = _recycle.faeSummary(body.lotNo, body.startDate, body.endDate, body.wasteName, body.phase);
                List<Waste> returnWaste = new List<Waste>();
                foreach (Waste item in waste)
                {
                    item.netWasteWeight = Double.Parse(item.netWasteWeight).ToString("##,###.00");
                    item.totalPrice = Double.Parse(item.totalPrice).ToString("##,###.00");
                    returnWaste.Add(item);
                }

                return Ok(new
                {
                    success = true,
                    // data = new { waste, }
                    data = new { requester, waste = returnWaste, }
                });
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }
        [HttpPost]
        public ActionResult create(createSummary body)
        {
            try
            {
                string empNo = User.FindFirst("username")?.Value;
                string name = User.FindFirst("name")?.Value;
                string dept = User.FindFirst("dept")?.Value;
                SummaryInvoiceSchema createItem = new SummaryInvoiceSchema();

                List<Waste> wasteItems = new List<Waste>();

                List<requesterUploadSchema> requesterItems = new List<requesterUploadSchema>();

                foreach (lotAndboi item in body.requester)
                {
                    requesterItems.AddRange(_requester.getByLotAndBoi(item.lotNo, item.boiType));
                    // _requester.updateStatus(lotNo, "toSummary");
                }

                // Update status
                foreach (requesterUploadSchema item in requesterItems)
                {
                    _requester.updateStatus(item._id, "toSummary");
                }

                foreach (string id in body.recycle)
                {
                    wasteItems.Add(_recycle.Get(id));
                    _recycle.updateStatus(id, "toSummary");
                }
                double sumRequester = 0;
                foreach (requesterUploadSchema item in requesterItems)
                {
                    // Console.WriteLine(item.totalWeight + " ==> " + Double.Parse(item.totalWeight).ToString());
                    sumRequester += Double.Parse(item.netWasteWeight);
                }
                double sumRecycle = 0;
                foreach (Waste item in wasteItems)
                {
                    sumRecycle += Double.Parse(item.netWasteWeight);
                }

                createItem.approve = new Profile { band = "-", date = "-", dept = "-", div = "-", empNo = "-", name = "-", tel = "-" };
                createItem.check = new Profile { band = "-", date = "-", dept = "-", div = "-", empNo = "-", name = "-", tel = "-" };
                createItem.exportRef = true;
                createItem.prepare = new Profile { band = "-", date = DateTime.Now.ToString("yyyy/MM/dd"), dept = dept, div = "-", empNo = empNo, name = name, tel = "-" };
                createItem.recycle = wasteItems.ToArray();
                createItem.requester = requesterItems.ToArray();
                createItem.status = "created";
                createItem.mainInvoice = body.mainInvoice;
                createItem.type = body.type;
                createItem.recycleWeight = sumRecycle;
                createItem.requesterWeight = sumRequester;
                createItem.createDate = DateTime.Now.ToString("yyyy/MM/dd");

                _services.create(createItem);

                return Ok(new { success = true, message = "Create summary success." });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Problem(e.StackTrace);
            }
        }

        [HttpGet("fae/created")]
        public ActionResult getStatusPrepareAndReject()
        {
            List<SummaryInvoiceSchema> data = _services.getPrepareandReject();
            List<dynamic> response = new List<dynamic>();
            foreach (SummaryInvoiceSchema item in data)
            {
                response.Add(new {
                    _id = item._id,
                    approve = item.approve,
                    check = item.check,
                    createDate = item.createDate,
                    exportRef = item.exportRef,
                    mainInvoice = item.mainInvoice,
                    prepare = item.prepare,
                    recycle = item.recycle,
                    recycleWeight = item.recycleWeight.ToString("##,###.00"),
                    rejectBy = item.rejectBy,
                    rejectCommend = item.rejectCommend,
                    requester = item.requester,
                    requesterWeight = item.requesterWeight.ToString("##,###.00"),
                    status = item.status,
                    totalPrice = item.totalPrice,
                    totalWeight = item.totalWeight,
                    type = item.type,
                });
            }
            return Ok(new { success = true, message = "Data for FAE prepare summary", data = response, });
        }
        [HttpGet("{status}")]
        public ActionResult getByStatus(string status)
        {

            try
            {
                List<SummaryInvoiceSchema> data = _services.getByStatus(status);
                List<dynamic> returnData = new List<dynamic>();

                foreach (SummaryInvoiceSchema item in data)
                {
                    returnData.Add(
                        new
                        {
                            _id = item._id,
                            type = item.type,
                            recycleWeight = item.recycleWeight.ToString("##,###.00"),
                            requesterWeight = item.requesterWeight.ToString("##,###.00"),
                            totalPrice = Double.Parse(item.totalPrice).ToString("##,###.00"),
                        }
                    );
                }
                return Ok(new { success = true, message = "Data on this route.", data = returnData, });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Problem(e.StackTrace);
            }
        }
        
        [HttpGet("pmd/{id}")]
        public ActionResult getById(string id)
        {
            try
            {
                SummaryInvoiceSchema data = _services.getById(id);

                double totalWeight = 0.0; double totalPrice = 0.0;

                foreach (Waste item in data.recycle)
                {
                    totalWeight += Double.Parse(item.netWasteWeight);

                    if (item.totalPrice != "-")
                    {
                        totalPrice += Double.Parse(item.totalPrice);
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "Data on this route.",
                    data,
                    total = new
                    {
                        totalWeight = totalWeight.ToString("#,##0.00"),
                        totalPrice = totalPrice.ToString("#,##0.00"),
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Problem(e.StackTrace);
            }
        }

        [HttpGet("imo/{id}")]
        public ActionResult getImodata(string id)
        {
            try
            {
                SummaryInvoiceSchema data = _services.getById(id);
                List<requesterUploadSchema> distinct = data.requester.GroupBy(x => x.biddingType).Select(x => x.First()).ToList();

                // Requester items
                List<dynamic> requesterReturn = new List<dynamic>();
                int i = 1;
                foreach (requesterUploadSchema requester in data.requester)
                {
                    requesterReturn.Add(new
                    {
                        id = i,
                        matrialCode = requester.matrialCode,
                        matrialName = requester.matrialName,
                        privilege = requester.boiType,
                        group = requester.groupBoiNo,
                        common = requester.groupBoiName,
                        unit = requester.unit,
                        qtyOfContainer = requester.qtyOfContainer,
                        netWasteWeight = requester.netWasteWeight,
                        grossWeight = requester.totalWeight,
                        bagWeight = requester.containerWeight,
                        wasteType = requester.biddingType,
                        color = requester.color,
                        unitPrice = requester.unitPrice,
                        totalPrice = requester.totalPrice
                    });
                    i += 1;
                }
                // Requester items
                // FAE items
                List<dynamic> faeReturn = new List<dynamic>();
                int j = 1;

                double totalWeight = 0.0; double totalPrice = 0.0;
                foreach (requesterUploadSchema item in distinct)
                {
                    List<requesterUploadSchema> filtered = data.requester.ToList().FindAll(row => row.biddingType == item.biddingType);
                    double sum = 0.0; double weight = 0.0;

                    foreach (requesterUploadSchema req in filtered)
                    {
                        if (req.totalPrice != "-")
                        {
                            sum += Double.Parse(req.totalPrice);
                            weight += Double.Parse(req.netWasteWeight);
                        }
                        else
                        {
                            sum += 0.0;
                        }
                    }

                    faeDBschema faeDB = _faeDB.getByBiddingType(item.biddingType);
                    if (faeDB != null)
                    {
                        // Console.WriteLine("Add by getByBiddingType");
                        faeReturn.Add(new
                        {
                            id = j,
                            biddingNo = faeDB.biddingNo,
                            biddingType = faeDB.biddingType,
                            color = faeDB.color,
                            weight = weight.ToString("#,##0.00"),
                            unitPrice = faeDB.pricePerUnit,
                            totalPrice = (Math.Round(sum, 2)).ToString("#,##0.00")
                        });
                    }
                    // else
                    // {
                    //     faeDBschema faeDB_kind = _faeDB.getByKind(item.kind);
                    //     if (faeDB_kind != null)
                    //     {
                    //         Console.WriteLine("Add by getByKind");
                    //         faeReturn.Add(new
                    //         {
                    //             id = j,
                    //             biddingNo = faeDB_kind.biddingNo,
                    //             kind = faeDB_kind.kind,
                    //             color = faeDB_kind.color,
                    //             weight = weight.ToString("#,##0.00"),
                    //             unitPrice = faeDB_kind.pricePerUnit,
                    //             totalPrice = (Math.Round(sum, 2)).ToString("#,##0.00")
                    //         });
                    //     }
                    //     else
                    //     {
                    //         Console.WriteLine("Not found: " + item.kind);
                    //     }

                    // }
                    j += 1;
                    totalWeight += weight; totalPrice += Math.Round(sum, 2);
                }
                // FAE items
                return Ok(new
                {
                    success = true,
                    message = "IMO data.",
                    data = new
                    { requester = requesterReturn, fae = faeReturn, total = new { totalWeight = totalWeight.ToString("#,##0.00"), totalPrice = totalPrice.ToString("#,##0.00"), } }
                });
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }

        [HttpGet("boi/{id}")]
        public ActionResult getBOI_non(string id)
        {
            try
            {
                SummaryInvoiceSchema data = _services.getById(id);

                List<dynamic> returnData = new List<dynamic>();

                int no = 1;
                double totalWeight = 0.0; double totalPrice = 0.0;
                foreach (requesterUploadSchema item in data.requester)
                {
                    returnData.Add(new
                    {
                        id = no,
                        biddingType = item.biddingType,
                        color = item.color,
                        weight = Double.Parse(item.netWasteWeight).ToString("#,###.00"),
                        unitPrice = item.unitPrice,
                        totalPrice = Double.Parse(item.totalPrice).ToString("#,###.00")
                    });

                    totalWeight += Double.Parse(item.netWasteWeight);
                    if (item.totalPrice == "-")
                    {
                        totalPrice += 0;
                    }
                    else
                    {
                        totalPrice += Double.Parse(item.totalPrice);
                    }
                    no += 1;
                }
                _services.updateTotal(id, totalPrice.ToString("#,##0.00"), totalWeight.ToString("#,##0.00"));
                return Ok(new
                {
                    success = true,
                    message = "Summary BOI/NON BOI",
                    data = returnData,
                    total = new
                    {
                        lotNo = data.requester[0].lotNo,
                        totalWeight = totalWeight.ToString("#,##0.00"),
                        totalPrice = totalPrice.ToString("#,##0.00")
                    },
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                return Problem(e.StackTrace);
            }
        }

        [HttpGet("boi/print/{id}")]
        public ActionResult getBOI_non_print(string id)
        {
            try
            {
                string rootFolder = Directory.GetCurrentDirectory();
                string pathString2 = @"\API site\files\wastemanagement\download\";
                string serverPath = rootFolder.Substring(0, rootFolder.LastIndexOf(@"\")) + pathString2;

                if (!Directory.Exists(serverPath))
                {
                    Directory.CreateDirectory(serverPath);
                }

                string uuid = System.Guid.NewGuid().ToString();
                string filePath = serverPath + uuid + ".xlsx";

                MemoryStream stream = new MemoryStream();

                using (ExcelPackage excel = new ExcelPackage(stream))
                {
                    excel.Workbook.Worksheets.Add("Data");

                    ExcelWorksheet sheet = excel.Workbook.Worksheets["Data"];

                    FileInfo file = new FileInfo(filePath);

                    sheet.Cells["A1:H1"].Merge = true;
                    sheet.Cells["A1:H80"].Style.Font.Name = "CordiaUPC";
                    sheet.Cells["A1"].Value = "สรุปปริมาณน้ำหนักของเสีย BOI";
                    sheet.Cells["A1"].Style.Font.Bold = true;
                    sheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["A1"].Style.Font.Size = 18;

                    sheet.Cells["A2:H2"].Merge = true;
                    sheet.Cells["A2"].Value = "Summary  of weight  BOI";
                    sheet.Cells["A2"].Style.Font.Bold = true;
                    sheet.Cells["A2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["A2"].Style.Font.Size = 18;

                    sheet.Cells["A3:H3"].Merge = true;
                    sheet.Cells["A3"].Value = "ส่วนที่ 1 บริษัทแคนนอน ปราจีนบุรี (ประเทศไทย) จำกัด";
                    sheet.Cells["A3"].Style.Font.Bold = true;
                    sheet.Cells["A3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["A3"].Style.Font.Size = 16;
                    sheet.Cells["A3"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#C0C0C0"));

                    sheet.Cells["A4:B4"].Merge = true;
                    sheet.Cells["A4"].Value = "Case : ";
                    sheet.Cells["A4"].Style.Font.Bold = true;
                    sheet.Cells["A4"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["A4"].Style.Font.Size = 16;
                    sheet.Cells["A4"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#C0C0C0"));

                    sheet.Cells["C4:D4"].Merge = true;
                    sheet.Cells["C4"].Value = "BOI";
                    sheet.Cells["C4"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["C4"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    sheet.Cells["E4:F4"].Merge = true;
                    sheet.Cells["E4"].Value = "NON BOI";
                    sheet.Cells["E4"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["E4"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    sheet.Cells["A5"].Value = "Lot No.";
                    sheet.Cells["A5"].Style.Font.Bold = true;
                    sheet.Cells["A5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["A5"].Style.Font.Size = 16;
                    sheet.Cells["A5"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#C0C0C0"));

                    sheet.Cells["B5:C5"].Merge = true;

                    sheet.Cells["E5"].Value = "Date move out";
                    sheet.Cells["E5"].Style.Font.Bold = true;
                    sheet.Cells["E5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["E5"].Style.Font.Size = 16;
                    sheet.Cells["E5"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#C0C0C0"));

                    sheet.Cells["A6"].Value = "Contract no.";
                    sheet.Cells["A6"].Style.Font.Bold = true;
                    sheet.Cells["A6"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["A6"].Style.Font.Size = 16;
                    sheet.Cells["A6"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#C0C0C0"));

                    sheet.Cells["C6"].Value = "Contract Start  :";
                    sheet.Cells["C6"].Style.Font.Bold = true;
                    sheet.Cells["C6"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["C6"].Style.Font.Size = 16;
                    sheet.Cells["C6"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#C0C0C0"));

                    sheet.Cells["E6"].Value = "Contract End : ";
                    sheet.Cells["E6"].Style.Font.Bold = true;
                    sheet.Cells["E6"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["E6"].Style.Font.Size = 16;
                    sheet.Cells["E6"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#C0C0C0"));

                    sheet.Cells["A7"].Value = "ลำดับ";
                    sheet.Cells["A7"].Style.Font.Bold = true;
                    sheet.Cells["A7"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["A7"].Style.Font.Size = 14;
                    sheet.Cells["A8"].Value = "No.";
                    sheet.Cells["A8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["A8"].Style.Font.Size = 14;

                    sheet.Cells["B7"].Value = "ประเภทของเสีย";
                    sheet.Cells["B7:C7"].Merge = true;
                    sheet.Cells["B7"].Style.Font.Bold = true;
                    sheet.Cells["B7"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["B7"].Style.Font.Size = 14;
                    sheet.Cells["B8"].Value = "(type of waste)";
                    sheet.Cells["B8:C8"].Merge = true;
                    sheet.Cells["B8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["B8"].Style.Font.Size = 14;

                    sheet.Cells["D7"].Value = "สี";
                    sheet.Cells["D7"].Style.Font.Bold = true;
                    sheet.Cells["D7"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["D7"].Style.Font.Size = 14;
                    sheet.Cells["D8"].Value = "Color";
                    sheet.Cells["D8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["D8"].Style.Font.Size = 14;

                    sheet.Cells["E7"].Value = "น้ำหนัก(Kg.)";
                    sheet.Cells["E7"].Style.Font.Bold = true;
                    sheet.Cells["E7"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["E7"].Style.Font.Size = 14;
                    sheet.Cells["E8"].Value = "Weight (Kg.)";
                    sheet.Cells["E8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["E8"].Style.Font.Size = 14;

                    sheet.Cells["F7:F8"].Merge = true;
                    sheet.Cells["F7"].Value = "ราคา/หน่วย";
                    sheet.Cells["F7"].Style.Font.Size = 14;
                    sheet.Cells["F7"].Style.Font.Bold = true;
                    sheet.Cells["F7"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["F7"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    sheet.Cells["G4:G8"].Merge = true;
                    sheet.Cells["G4"].Value = "รวมราคา";
                    sheet.Cells["G4"].Style.Font.Size = 14;
                    sheet.Cells["G4"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#C0C0C0"));
                    sheet.Cells["G4"].Style.Font.Bold = true;
                    sheet.Cells["G4"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["G4"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    sheet.Cells["H4:H8"].Merge = true;
                    sheet.Cells["H4"].Value = "FAE (J3 up)\nConfirmation Status\nConsistent = OK\nNot consistent = NG\nwith EF-FAE-ENV-056";
                    sheet.Cells["H4"].Style.WrapText = true;
                    sheet.Cells["H4"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#C0C0C0"));
                    sheet.Cells["H4"].Style.Font.Size = 14;
                    sheet.Cells["H4"].Style.Font.Bold = true;
                    sheet.Cells["H4"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["H4"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    sheet.Column(1).Width = 15;
                    sheet.Column(3).Width = 17;
                    sheet.Column(5).Width = 17;
                    sheet.Column(6).Width = 17;
                    sheet.Column(7).Width = 18;
                    sheet.Column(8).Width = 18;

                    SummaryInvoiceSchema data = _services.getById(id);

                    int rowItem = 9; int no = 1;
                    foreach (requesterUploadSchema item in data.requester)
                    {
                        sheet.Cells["A" + rowItem].Value = no;
                        sheet.Cells["B" + rowItem + ":C" + rowItem].Merge = true;
                        sheet.Cells["B" + rowItem].Value = item.biddingType;
                        sheet.Cells["D" + rowItem].Value = item.color;
                        sheet.Cells["E" + rowItem].Value = Double.Parse(item.netWasteWeight).ToString("##,###.00");
                        sheet.Cells["F" + rowItem].Value = item.unitPrice;
                        sheet.Cells["G" + rowItem].Value = Double.Parse(item.totalPrice).ToString("##,###.00");

                        sheet.Cells["A" + rowItem + ":H" + rowItem].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        sheet.Cells["A" + rowItem + ":H" + rowItem].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        sheet.Cells["A" + rowItem + ":H" + rowItem].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        sheet.Cells["A" + rowItem + ":H" + rowItem].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        no += 1; rowItem += 1;
                    }
                    sheet.Cells["A9:H" + rowItem].Style.Font.Size = 14;
                    sheet.Cells["A9:H" + rowItem].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["A9:H" + rowItem].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    sheet.Cells["A3:H8"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["A3:H8"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["A3:H8"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["A3:H8"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    sheet.Cells["C" + rowItem + ":D" + rowItem].Merge = true;
                    sheet.Cells["C" + rowItem].Value = "รวมทั้งหมด";
                    sheet.Cells["C" + rowItem].Style.Font.Bold = true;
                    sheet.Cells["E" + rowItem].Value = data.totalWeight;

                    sheet.Cells["C" + rowItem + ":D" + rowItem].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["C" + rowItem + ":D" + rowItem].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["C" + rowItem + ":D" + rowItem].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    sheet.Cells["E" + rowItem].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["E" + rowItem].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["G" + rowItem].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["G" + rowItem].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["G" + rowItem].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["G" + rowItem].Value = data.totalPrice;

                    WebClient client = new WebClient();
                    Stream stream_img = client.OpenRead("http://cptsvs531:5000/files/middleware_img/signingBox.png");
                    Bitmap bitmap = new Bitmap(stream_img);

                    ExcelPicture picture = sheet.Drawings.AddPicture("image", bitmap);
                    // picture.To.Column = 3;
                    // picture.To.Row = rowItem + 2;
                    picture.SetSize(500, 150);
                    picture.SetPosition(rowItem + 1, 0, 2, 0);

                    // ส่วนที่ 2
                    rowItem += 10;

                    sheet.Cells["A" + rowItem + ":H" + rowItem].Merge = true;
                    sheet.Cells["A" + rowItem + ":H" + rowItem].Style.Fill.SetBackground(ColorTranslator.FromHtml("#C0C0C0"));
                    sheet.Cells["A" + rowItem].Value = "ส่วนที่ 2 ผู้รับกำจัด/บำบัด ของเสีย";
                    sheet.Cells["A" + rowItem].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["A" + rowItem].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells["A" + rowItem].Style.Font.Size = 16;
                    sheet.Cells["A" + rowItem].Style.Font.Bold = true;
                    sheet.Row(rowItem).Height = 23;

                    sheet.Cells["A" + (rowItem + 1) + ":H" + (rowItem + 1)].Merge = true;
                    sheet.Cells["A" + (rowItem + 1)].Value = "ข้าพเจ้าขอยืนยันน้ำหนักที่ระบุตามรายการดังกล่าว I would like to confirm the weight as above-mantioned.";
                    sheet.Cells["A" + (rowItem + 1)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["A" + (rowItem + 1)].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells["A" + (rowItem + 1)].Style.Font.Size = 14;

                    sheet.Cells["D" + (rowItem + 3)].Value = "บริษัท ……………………………………";
                    sheet.Cells["D" + (rowItem + 3)].Style.Font.Size = 14;

                    sheet.Cells["D" + (rowItem + 4)].Value = "Contracter disposal / treatment ";
                    sheet.Cells["D" + (rowItem + 4)].Style.Font.Size = 14;

                    sheet.Cells["D" + (rowItem + 6)].Value = "ลงชื่อ (Sign)...................................................";
                    sheet.Cells["D" + (rowItem + 6)].Style.Font.Size = 14;
                    sheet.Cells["D" + (rowItem + 7)].Value = "             (                                                             )";
                    sheet.Cells["D" + (rowItem + 7)].Style.Font.Size = 14;
                    sheet.Cells["D" + (rowItem + 8)].Value = "ตำแหน่ง (Position) ..................................................";
                    sheet.Cells["D" + (rowItem + 8)].Style.Font.Size = 14;

                    excel.SaveAs(file);
                    stream.Position = 0;

                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filePath);
                }
                // return Ok(new { success = true });
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }

        [HttpGet("recycle/{id}")]
        public ActionResult getRecycleData(string id)
        {
            try
            {
                SummaryInvoiceSchema data = _services.getById(id);

                List<dynamic> returnItems = new List<dynamic>();

                int i = 1; double totalWeight = 0.0; double totalPrice = 0.0;
                foreach (Waste item in data.recycle)
                {
                    returnItems.Add(new
                    {
                        id = i,
                        date = item.moveOutDate,
                        wasteName = item.wasteName,
                        weight = item.netWasteWeight,
                        unitPrice = item.unitPrice,
                        totalPrice = item.totalPrice,
                        // remark = ""
                    });
                    totalWeight += Double.Parse(item.netWasteWeight);
                    if (item.totalPrice != "-")
                    {
                        totalPrice += Double.Parse(item.totalPrice);
                    }
                    i += 1;
                }
                return Ok(new
                {
                    success = true,
                    message = "Recycle item",
                    data = returnItems,
                    total = new
                    {
                        totalWeight = totalWeight.ToString("#,##0.00"),
                        totalPrice = totalPrice.ToString("#,##0.00")
                    }
                });
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }
        [HttpPut("status")]
        public ActionResult updateStatus(updateStatus body)
        {
            try
            {
                Profile user = new Profile
                {
                    empNo = User.FindFirst("username")?.Value,
                    name = User.FindFirst("name")?.Value,
                    dept = User.FindFirst("dept")?.Value,
                    date = DateTime.Now.ToString("yyyy/MM/dd")
                };

                foreach (string id in body.id)
                {
                    SummaryInvoiceSchema data = _services.getById(id);
                    if (data.totalPrice != null)
                    {
                        _services.updateStatus(id, body.status, user);
                    }

                }
                return Ok(new { success = true, message = "Update status success." });
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }

        [HttpPut("total")]
        public ActionResult updateTotal(updateTotal body)
        {
            try
            {
                _services.updateTotal(body.id, body.totalPrice, body.totalWeight);
                return Ok(new { success = true, message = "Update total success." });
            }
            catch (Exception e)
            {

                return Problem(e.StackTrace);
            }
        }

        [HttpGet("type/{id}")]
        public ActionResult getSummaryType(string id)
        {
            try
            {
                SummaryInvoiceSchema data = _services.getById(id);

                return Ok(
                    new { success = true, message = "Summary item", data = new { type = data.type, id = data._id } }
                );
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }

        [HttpGet("reset/{id}")]
        public ActionResult resetToApprove(string id)
        {
            try
            {
                SummaryInvoiceSchema data = _services.getById(id);

                foreach (Waste item in data.recycle)
                {
                    _recycle.updateStatus(item._id, "approve");
                }
                foreach (requesterUploadSchema item in data.requester)
                {
                    _requester.updateStatusById(item._id, "fae-approved");
                }

                _services.delete(id);
                return Ok(new { success = true, message = "Reset data success. " });
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }

        [HttpPatch("reject")]
        public ActionResult rejectSummary(RejectSummary body)
        {

            Profile user = new Profile
            {
                empNo = User.FindFirst("username")?.Value,
                name = User.FindFirst("name")?.Value,
                dept = User.FindFirst("dept")?.Value,
                date = DateTime.Now.ToString("yyyy/MM/dd")
            };
            foreach (string item in body.id)
            {

                _services.rejectSummary(item, body.commend, user);
            }
            return Ok(new { success = true, message = "Reject summary success." });
        }
    }
}