
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
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml.DataValidation.Contracts;

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
        private readonly InvoiceService _invoice;

        public summaryController(SummaryInvoiceService summary, RecycleService recycle, requesterUploadServices requester, faeDBservice fae, InvoiceService invoice)
        {
            _services = summary;
            _recycle = recycle;
            _requester = requester;
            _faeDB = fae;
            _invoice = invoice;
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
                    sumRequester += Double.Parse(item.totalWeight);
                }
                double sumRecycle = 0;
                foreach (Waste item in wasteItems)
                {
                    sumRecycle += Double.Parse(item.totalWeight);
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
                if (createItem.requester.Length > 0)
                {
                    if (createItem.requester[0].boiType == "BOI")
                    {
                        createItem.boiCase = "BOI";
                    }
                    else
                    {
                        createItem.boiCase = null;
                    }
                }
                else
                {
                    createItem.boiCase = null;
                }

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
                response.Add(new
                {
                    _id = item._id,
                    approve = item.approve,
                    check = item.check,
                    createDate = item.createDate,
                    exportRef = item.exportRef,
                    mainInvoice = item.mainInvoice,
                    prepare = item.prepare,
                    recycle = item.recycle,
                    recycleWeight = item.recycleWeight.ToString("##,##0.00"),
                    rejectBy = item.rejectBy,
                    rejectCommend = item.rejectCommend,
                    requester = item.requester,
                    requesterWeight = item.requesterWeight.ToString("##,##0.00"),
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
                            recycleWeight = item.recycleWeight.ToString("##,##0.00"),
                            requesterWeight = item.requesterWeight.ToString("##,##0.00"),
                            totalPrice = Double.Parse(item.totalPrice).ToString("##,###.00"),
                        }
                    );
                }
                return Ok(new { success = true, message = "Data on this route.", data = returnData, });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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

        [HttpPut("pmd/pmdConsistent")]
        public ActionResult changePmdConsistentStatus(request.updateConsistent body)
        {
            try
            {
                string dept = User.FindFirst("dept")?.Value;
                Console.WriteLine(dept);
                if (dept.ToUpper() != "FAE")
                {
                    return BadRequest(new { success = false, message = "Permission denied." });
                }
                _services.updatePMDconsistent(body.id, body.consistent);
                return Ok(new { success = true, message = "Update consistent success." });
            }
            catch (System.Exception e)
            {
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
                        totalPrice = (item.totalPrice != "-") ? Double.Parse(item.totalPrice).ToString("#,###.00") : "0",
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
                        id = item._id,
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

        [HttpGet("invoice/imo/print/{id}"),]
        public ActionResult imoPrintFormat(string id)
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
                // string uuid = "A";
                string filePath = serverPath + uuid + ".xlsx";

                SummaryInvoiceSchema summary = _services.getById(id);
                MemoryStream stream = new MemoryStream();
                List<requesterUploadSchema> distinctBidding = summary.requester.GroupBy(x => x.biddingType).Select(x => x.First()).ToList();


                using (ExcelPackage excel = new ExcelPackage(stream))
                {
                    excel.Workbook.Worksheets.Add("Bank");
                    ExcelWorksheet sheet = excel.Workbook.Worksheets["Bank"];

                    sheet.View.ZoomScale = 70;
                    sheet.View.ShowGridLines = false;
                    FileInfo file = new FileInfo(filePath);

                    sheet.Cells["B8:C8"].Merge = true;
                    sheet.Cells["D8:E8"].Merge = true;
                    sheet.Cells["F8:G8"].Merge = true;
                    sheet.Cells["B8"].Value = "Case: ";
                    sheet.Cells["B8"].Style.Font.Bold = true;
                    sheet.Cells["D8"].Value = "BOI";
                    sheet.Cells["D8"].Style.Font.Bold = true;
                    sheet.Cells["F8"].Value = "NON BOI";
                    sheet.Cells["F8"].Style.Font.Bold = true;
                    sheet.Column(3).Width = 20;
                    sheet.Column(2).Width = 10;
                    sheet.Column(4).Width = 15;
                    sheet.Column(6).Width = 20;
                    sheet.Column(7).Width = 16;
                    sheet.Column(9).Width = 16;
                    sheet.Column(11).Width = 22;
                    sheet.Column(13).Width = 16;
                    sheet.Column(14).Width = 23;
                    sheet.Column(15).Width = 16;
                    sheet.Column(17).Width = 16;
                    sheet.Row(8).Height = 20;
                    sheet.Column(8).Width = 20;
                    sheet.Column(10).Width = 20;
                    sheet.Column(12).Width = 20;
                    sheet.Cells["B8:G8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["B8:G8"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells["B8:M9"].Style.Font.Name = "CordiaUPC";
                    sheet.Cells["B8:M9"].Style.Font.Size = 20;
                    sheet.Cells["B9"].Value = "Lot No.";
                    sheet.Cells["B9"].Style.Font.Bold = true;

                    sheet.Cells["C9:E9"].Merge = true; // Lotno space
                    sheet.Cells["F9"].Value = "Date move out";
                    sheet.Cells["F9"].Style.Font.Bold = true;

                    sheet.Cells["H9"].Value = "Contract no.";
                    sheet.Cells["H9"].Style.Font.Bold = true;

                    sheet.Cells["J9"].Value = "Contract Start";
                    sheet.Cells["J9"].Style.Font.Bold = true;

                    sheet.Cells["L9"].Value = "Contract End";
                    sheet.Cells["L9"].Style.Font.Bold = true;
                    sheet.Cells["B9:M9"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["B9:M9"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    // Header table data
                    sheet.Cells["B11:X11"].Style.Font.Name = "Calibri";

                    sheet.Row(11).Height = 60;
                    sheet.Cells["B11"].Value = "NO.";
                    sheet.Cells["C11"].Value = "MATERIAL CODE";
                    sheet.Cells["D11:E11"].Merge = true;
                    sheet.Cells["D11"].Value = "MATRIAL NAME";
                    sheet.Cells["B11:E11"].Style.Font.Size = 14;

                    sheet.Cells["F11"].Value = "Privilege";
                    sheet.Cells["G11"].Value = "Group";
                    sheet.Cells["H11"].Value = "Common";
                    sheet.Cells["I11"].Value = "Unit";
                    sheet.Cells["F11:I11"].Style.Font.Size = 11;

                    sheet.Cells["J11"].Value = "QUANTITY (BAG)";
                    sheet.Cells["K11"].Value = "NET WEIGHT (KG)";
                    sheet.Cells["L11"].Value = "GROSS WEIGHT (KG)";
                    sheet.Cells["M11"].Value = "BAG Weight (KG)";
                    sheet.Cells["N11"].Value = "ชนิด";
                    sheet.Cells["O11"].Value = "สี";
                    sheet.Cells["P11"].Value = "ราคา";
                    sheet.Cells["Q11"].Value = "รวมราคา";
                    sheet.Cells["J11:Q11"].Style.Font.Size = 14;

                    sheet.Cells["B11:Q11"].Style.WrapText = true;

                    sheet.Cells["B11:Q11"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["B11:Q11"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells["B11:Q11"].Style.Font.Bold = true;
                    sheet.Cells["B11:E11"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#8DB4E2"));
                    sheet.Cells["F11:I11"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#76933C"));
                    sheet.Cells["J11:Q11"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#8DB4E2"));
                    // Header table data


                    // row table data
                    int no = 1; int row = 12;

                    foreach (requesterUploadSchema item in summary.requester)
                    {

                        sheet.Cells["B" + row.ToString()].Value = no.ToString();
                        sheet.Cells["C" + row.ToString()].Value = item.matrialCode;
                        sheet.Cells["D" + row.ToString() + ":E" + row.ToString()].Merge = true;
                        sheet.Cells["D" + row.ToString()].Value = item.matrialName;
                        sheet.Cells["F" + row.ToString()].Value = item.boiType;
                        sheet.Cells["G" + row.ToString()].Value = item.groupBoiNo;
                        sheet.Cells["H" + row.ToString()].Value = item.groupBoiName;
                        sheet.Cells["I" + row.ToString()].Value = item.unit;
                        sheet.Cells["J" + row.ToString()].Value = Double.Parse(item.qtyOfContainer);
                        sheet.Cells["K" + row.ToString()].Value = Double.Parse(item.netWasteWeight);
                        sheet.Cells["L" + row.ToString()].Value = Double.Parse(item.netWasteWeight);
                        sheet.Cells["M" + row.ToString()].Value = Double.Parse(item.containerWeight);
                        sheet.Cells["N" + row.ToString()].Value = item.biddingType;
                        sheet.Cells["O" + row.ToString()].Value = item.color;
                        sheet.Cells["P" + row.ToString()].Value = item.unitPrice;
                        sheet.Cells["Q" + row.ToString()].Value = (item.totalPrice == "-") ? 0 : Double.Parse(item.totalPrice);
                        no += 1; row += 1;
                    }
                    sheet.Cells["B12:Q" + row.ToString()].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["B12:Q" + row.ToString()].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells["B12:Q" + row.ToString()].Style.Font.Size = 14;

                    sheet.Cells["C" + row.ToString()].Value = "Grand Total";
                    sheet.Cells["C" + row.ToString()].Style.Font.Bold = true;
                    sheet.Cells["D" + row.ToString() + ":E" + row.ToString()].Merge = true;
                    sheet.Cells["J" + row.ToString()].Formula = "=SUM(J12:J" + (row - 1).ToString() + ")";
                    sheet.Cells["K" + row.ToString()].Formula = "=SUM(K12:K" + (row - 1).ToString() + ")";
                    sheet.Cells["L" + row.ToString()].Formula = "=SUM(L12:L" + (row - 1).ToString() + ")";
                    sheet.Cells["M" + row.ToString()].Formula = "=SUM(M12:M" + (row - 1).ToString() + ")";
                    sheet.Cells["Q" + row.ToString()].Formula = "=SUM(Q12:Q" + (row - 1).ToString() + ")";
                    sheet.Cells["J12:M" + (row + 1).ToString()].Style.Numberformat.Format = "###,##0.00";
                    sheet.Cells["Q12:Q" + (row + 1).ToString()].Style.Numberformat.Format = "###,##0.00";
                    sheet.Cells["Q" + (row).ToString()].Style.Font.Bold = true;

                    sheet.Cells["B11:Q" + (row).ToString()].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["B11:Q" + (row).ToString()].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["B11:Q" + (row).ToString()].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["B11:Q" + (row).ToString()].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    // row table data

                    // Pricing table
                    sheet.Cells["S11:X11"].Merge = true;
                    sheet.Cells["S11"].Value = "FAE Summary scrap material (BOI)";
                    sheet.Cells["S12"].Value = "ลำดับ";
                    sheet.Cells["T12"].Value = "ชนิดพลาสติก";
                    sheet.Cells["U12"].Value = "สี";
                    sheet.Cells["V12"].Value = "น้ำหนัก";
                    sheet.Cells["W12"].Value = "ราคา";
                    sheet.Cells["X12"].Value = "รวมราคา";

                    int startRow = 13;
                    foreach (requesterUploadSchema item in distinctBidding)
                    {
                        sheet.Cells["S" + startRow].Value = item.biddingNo;
                        sheet.Cells["T" + startRow].Value = item.biddingType;
                        sheet.Cells["U" + startRow].Value = item.color;

                        List<requesterUploadSchema> itemWithBidding = summary.requester.ToList().FindAll(e => e.biddingType == item.biddingType);

                        double sumWeight = 0.0;
                        sheet.Cells["W" + startRow].Value = (item.unitPrice == "-") ? 0 : Double.Parse(item.unitPrice);
                        foreach (requesterUploadSchema biddingItem in itemWithBidding)
                        {
                            sumWeight += Double.Parse(biddingItem.netWasteWeight);
                        }
                        sheet.Cells["V" + startRow].Value = sumWeight;
                        sheet.Cells["X" + startRow].Formula = "=V" + startRow + "*W" + startRow;
                        startRow += 1;
                    }
                    sheet.Column(24).Width = 20;
                    sheet.Column(23).Width = 20;
                    sheet.Column(22).Width = 20;
                    sheet.Column(21).Width = 20;
                    sheet.Column(20).Width = 20;
                    sheet.Column(19).Width = 20;

                    sheet.Cells["W13:W" + startRow].Style.Numberformat.Format = "##,##0.00";
                    sheet.Cells["X13:X" + startRow].Style.Numberformat.Format = "##,##0.00";
                    sheet.Cells["V13:V" + startRow].Style.Numberformat.Format = "##,##0.00";
                    sheet.Cells["S13:S" + startRow].Style.Numberformat.Format = "##,##0";

                    sheet.Cells["S11:X" + (startRow + 1)].Style.Font.Name = "Calibri";
                    sheet.Cells["S11:X11"].Style.Font.Size = 18;
                    sheet.Cells["S11:X11"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#D8E4BC"));
                    sheet.Cells["S11:X11"].Style.Font.Bold = true;
                    sheet.Cells["S12:X" + startRow].Style.Font.Size = 14;
                    sheet.Cells["S12:X12"].Style.Font.Bold = true;
                    sheet.Cells["S" + startRow + ":X" + startRow].Style.Font.Bold = true;
                    sheet.Cells["S12:X12"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#8DB4E2"));

                    sheet.Cells["S" + (startRow)].Value = "Total";
                    sheet.Cells["V" + (startRow)].Formula = "=SUM(V13:V" + (startRow - 1) + ")";
                    sheet.Cells["X" + (startRow)].Formula = "=SUM(X13:X" + (startRow - 1) + ")";

                    sheet.Cells["S11:X" + (startRow)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["S11:X" + (startRow)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["S11:X" + (startRow)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["S11:X" + (startRow)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["S11:X" + (startRow)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["S11:X" + (startRow)].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    // Write heading

                    sheet.Cells["B8:G8"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["B8:G8"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["B8:G8"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["B8:G8"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["B8:G8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["B8:G8"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    sheet.Cells["B9:M9"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["B9:M9"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["B9:M9"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["B9:M9"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["B9:M9"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["B9:M9"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells["C9"].Value = distinctBidding[0].lotNo;

                    Invoices invoiceData = _invoice.getBySummaryId(id);
                    sheet.Cells["G9"].Value = distinctBidding[0].moveOutDate;
                    sheet.Cells["I9"].Value = invoiceData?.company?.contractNo;
                    sheet.Cells["K9"].Value = invoiceData?.company?.contractStartDate;
                    sheet.Cells["M9"].Value = invoiceData?.company?.contractEndDate;

                    if (summary.boiCase == "BOI")
                    {
                        sheet.Cells["D8:E8"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#92D050"));
                    }
                    else
                    {
                        sheet.Cells["F8:G8"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#92D050"));
                    }

                    // Write heading

                    excel.SaveAs(file);
                    stream.Position = 0;
                }
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filePath);
                // return Ok(new { success = true, message = "Export IMO format success.", });
            }
            catch (System.Exception e)
            {
                return Problem(e.StackTrace);
            }
        }

        [HttpGet("invoice/pmd/print/{id}"), AllowAnonymous]
        public ActionResult pmdPrintFormat(string id)
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
                // string uuid = "A";
                string filePath = serverPath + uuid + ".xlsx";

                SummaryInvoiceSchema summary = _services.getById(id);

                List<requesterUploadSchema> distinctBidding = summary.requester.GroupBy(x => x.biddingType).Select(x => x.First()).ToList();

                MemoryStream stream = new MemoryStream();

                using (ExcelPackage excel = new ExcelPackage(stream))
                {
                    excel.Workbook.Worksheets.Add("Bank");
                    ExcelWorksheet sheet = excel.Workbook.Worksheets["Bank"];

                    sheet.View.ZoomScale = 70;
                    sheet.View.ShowGridLines = false;
                    FileInfo file = new FileInfo(filePath);

                    IExcelDataValidationList pmdDept = sheet.DataValidations.AddListValidation("B3");
                    pmdDept.Formula.Values.Add("PMD");
                    pmdDept.Formula.Values.Add("PMD (CLP)");
                    sheet.Cells["B3:D5"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#FFFFCC"));

                    IExcelDataValidationList boi = sheet.DataValidations.AddListValidation("F3");
                    boi.Formula.Values.Add("BOI");
                    boi.Formula.Values.Add("NON BOI");
                    sheet.Cells["F3:H5"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#FFFFCC"));

                    sheet.Cells["A1:H1"].Merge = true;
                    sheet.Cells["A2:H2"].Merge = true;
                    sheet.Row(1).Height = 30;
                    sheet.Cells["A1"].Value = "SUMMARY SCRAPED MATERIAL";
                    sheet.Cells["A2"].Value = "ตารางสรุปน้ำหนักของเสีย";
                    sheet.Cells["A1:H2"].Style.Font.Size = 22;
                    sheet.Cells["A1:H2"].Style.Font.Bold = true;

                    sheet.Cells["A1:H2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["A1:H2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    if (distinctBidding.Count > 0)
                    {
                        sheet.Cells["B4"].Value = distinctBidding[0].lotNo;
                        sheet.Cells["F4"].Value = distinctBidding[0].moveOutDate;
                        sheet.Cells["F3"].Value = distinctBidding[0].boiType;
                    }
                    Invoices invoiceData = _invoice.getBySummaryId(id);
                    if (invoiceData != null)
                    {
                        sheet.Cells["B5"].Value = invoiceData.company.contractNo;
                        sheet.Cells["F5"].Value = invoiceData.company.contractStartDate + " - " + invoiceData.company.contractEndDate;
                    }

                    sheet.Row(2).Height = 30;
                    sheet.Row(3).Height = 30;
                    sheet.Row(4).Height = 30;
                    sheet.Row(5).Height = 30;
                    sheet.Row(6).Height = 70;
                    sheet.Row(7).Height = 70;

                    sheet.Column(1).Width = 30;
                    sheet.Column(2).Width = 17;
                    sheet.Column(3).Width = 18;
                    sheet.Column(4).Width = 20;
                    sheet.Column(5).Width = 20;
                    sheet.Column(6).Width = 20;
                    sheet.Column(7).Width = 20;
                    sheet.Column(8).Width = 30;

                    sheet.Cells["A3"].Value = "Dept :";
                    sheet.Cells["B3:D3"].Merge = true;
                    sheet.Cells["A4"].Value = "Lot No :";
                    sheet.Cells["B4:D4"].Merge = true;
                    sheet.Cells["B5:D5"].Merge = true;
                    sheet.Cells["F3:H3"].Merge = true;
                    sheet.Cells["F4:H4"].Merge = true;
                    sheet.Cells["F5:H5"].Merge = true;
                    sheet.Cells["A5"].Value = "Contract no.";
                    sheet.Cells["A3:A5"].Style.Font.Size = 18;
                    sheet.Cells["A3:A5"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#D9D9D9"));
                    sheet.Cells["B3:D5"].Style.Font.Size = 16;
                    sheet.Cells["E3:E5"].Style.Font.Size = 18;
                    sheet.Cells["E3:E5"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#D9D9D9"));
                    sheet.Cells["F3:H5"].Style.Font.Size = 16;


                    sheet.Cells["E3"].Value = "Privilege Type :";
                    sheet.Cells["E4"].Value = "Date move out :";
                    sheet.Cells["E5"].Value = "Contract Start-End :";

                    sheet.Cells["A6"].Value = "Waste Type";
                    sheet.Cells["B6"].Value = "Box No.";
                    sheet.Cells["C6"].Value = "Box Weight / Track weight (Kg.)";
                    sheet.Cells["D6"].Value = "Gross  Weight / Truck weight+Waste (Kg.)";
                    sheet.Cells["E6"].Value = "NET Weight (Kg.)";
                    sheet.Cells["F6"].Value = "Price (THB)/Kg.";
                    sheet.Cells["G6"].Value = "Total price";

                    sheet.Cells["A7"].Value = "ประเภทของเสีย";
                    sheet.Cells["B7"].Value = "ถังเลขที่";
                    sheet.Cells["C7"].Value = "น้ำหนักรถเปล่า/น้ำหนักถัง (กก.)";
                    sheet.Cells["D7"].Value = "น้ำหนักรถหนัก/น้ำหนักรวมถัง (กก.)";
                    sheet.Cells["E7"].Value = "น้ำหนักของเสีย";
                    sheet.Cells["F7"].Value = "ราคา (บาท)/กก.";
                    sheet.Cells["G7"].Value = "ราคารวม";

                    sheet.Cells["A6:H7"].Style.Font.Size = 14;
                    sheet.Cells["A6:H7"].Style.Font.Bold = true;
                    sheet.Cells["A6:H7"].Style.WrapText = true;
                    sheet.Cells["A6:H7"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["A6:H7"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells["H6:H7"].Merge = true;
                    sheet.Cells["H6"].Value = "FAE (J3 up) Confirmation Status Consistent = OK Not consistent = NG with EF-FAE-ENV-056";

                    int startRow = 8;

                    foreach (Waste item in summary.recycle)
                    {
                        IExcelDataValidationList wasteType = sheet.DataValidations.AddListValidation("A" + startRow);
                        wasteType.Formula.Values.Add("เศษสแตนเลสจากการกลึง");
                        wasteType.Formula.Values.Add("สแตนเลส (แท่ง)");
                        wasteType.Formula.Values.Add("เศษทองเหลืองจากการกลึง");
                        wasteType.Formula.Values.Add("ทองเหลือง (แท่ง)");
                        wasteType.Formula.Values.Add("เหล็ก");
                        sheet.Cells["B" + startRow].Value = "";
                        sheet.Cells["C" + startRow].Value = Double.Parse(item.containerWeight);
                        sheet.Cells["D" + startRow].Value = Double.Parse(item.totalWeight);
                        sheet.Cells["E" + startRow].Value = Double.Parse(item.netWasteWeight);
                        sheet.Cells["F" + startRow].Value = Double.Parse(item.unitPrice);
                        sheet.Cells["G" + startRow].Value = Double.Parse(item.totalPrice);
                        startRow += 1;
                    }
                    sheet.Cells["H8"].Value = summary.pmdConsistent;
                    sheet.Cells["H8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells["H8"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;

                    sheet.Cells["A1:H55"].Style.Font.Name = "CordiaUPC";
                    sheet.Cells["H8:H45"].Merge = true;
                    sheet.Cells["A3:H46"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["A3:H46"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["A3:H46"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    sheet.Cells["A3:H46"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    sheet.Cells["C8:D45"].Style.Numberformat.Format = "###,##0.00";
                    sheet.Cells["E8:G45"].Style.Numberformat.Format = "###,##0.00";
                    sheet.Cells["A8:D45"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#FFFFCC"));

                    excel.SaveAs(file);
                    stream.Position = 0;
                }
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filePath);
                // return Ok(new { success = true, });
            }
            catch (System.Exception e)
            {
                return Problem(e.StackTrace);
            }
        }
    }
}