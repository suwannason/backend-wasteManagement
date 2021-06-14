
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.response;
using backend.Models;
using backend.request;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;

namespace backend.Controllers
{

    [Authorize]
    [ApiController]
    [Route("fae-part/[controller]")]

    public class invoiceController : ControllerBase
    {

        private readonly InvoiceService _invoiceService;
        private readonly RecycleService _wasteService;
        private readonly requesterUploadServices _requester;
        private readonly SummaryInvoiceService _summary;
        private readonly InvoicePrintedService _invoicePrinting;
        private readonly faeDBservice _faeDB;

        InvoiceResponse res = new InvoiceResponse();

        public invoiceController(InvoiceService invoiceService, RecycleService wasteService, requesterUploadServices requester, faeDBservice fae, SummaryInvoiceService summary, InvoicePrintedService invoicePrinting)
        {
            _invoiceService = invoiceService;
            _wasteService = wasteService;
            _requester = requester;
            _faeDB = fae;
            _summary = summary;
            _invoicePrinting = invoicePrinting;
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, Invoices body)
        {
            try
            {
                var data = _invoiceService.GetById(id);

                if (data == null)
                {
                    return NotFound();
                }
                _invoiceService.Update(id, body);
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
        [HttpPost("prepared")]
        public IActionResult prepared(createInvoice body)
        {
            try
            {
                // PREMISSION CHECKING
                // PREMISSION CHECKING
                Profile user = new Profile();

                user.empNo = User.FindFirst("username")?.Value;
                user.band = User.FindFirst("band")?.Value;
                user.dept = User.FindFirst("dept")?.Value;
                user.div = User.FindFirst("div")?.Value;
                user.name = User.FindFirst("name")?.Value;
                user.date = DateTime.Now.ToString("yyyy/MM/dd");

                Profile user_tmp = new Profile();

                user_tmp.empNo = "-";
                user_tmp.band = "-";
                user_tmp.dept = "-";
                user_tmp.div = "-";
                user_tmp.name = "-";
                user_tmp.date = "-";

                string currentDate = DateTime.Now.ToString("yyyy/MM/dd");

                Invoices data = new Invoices();

                data.fae_prepared = user;
                data.fae_checked = user_tmp;
                data.fae_approved = user_tmp;
                data.gm_approved = user_tmp;
                data.acc_check = user_tmp;
                data.acc_prepare = user_tmp;
                data.acc_approve = user_tmp;
                data.summaryId = body.summaryId;
                data.status = "fae-prepared";
                data.month = DateTime.Now.ToString("MMM");
                data.year = DateTime.Now.ToString("yyyy");
                data.invoiceDate = DateTime.ParseExact(body.invoiceDate, "yyyy/MM/dd", CultureInfo.InvariantCulture).ToString("dd-MMM-yyyy");
                data.company = body.company;
                data.createDate = DateTime.Now.ToString("yyyy/MM/dd");

                double totalPrice = 0.0;
                foreach (string item in body.summaryId)
                {
                    _summary.updateToInvoice(item);

                    SummaryInvoiceSchema summary = _summary.getById(item);

                    totalPrice += Double.Parse(summary.totalPrice);

                }
                data.invoiceNo = "-";
                data.termsOfPayment = "-";
                data.dueDate = "-";
                data.customerCode = "-";
                data.poNo = "-";
                data.attnRef = "-";
                data.totalPrice = totalPrice.ToString("#,##0.00");
                _invoiceService.Create(data);

                // Set pricing DB to requester
                return Ok(new { success = true, message = "Create invoices success." });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Conflict();
            }
        }

        [HttpPatch("status")]
        public IActionResult approve(RequestInvoiceUpdateStatus body)
        {
            try
            {
                // PREMISSION CHECKING

                // PREMISSION CHECKING
                Profile user = new Profile();

                user.empNo = User.FindFirst("username")?.Value;
                user.band = User.FindFirst("band")?.Value;
                user.dept = User.FindFirst("dept")?.Value;
                user.div = User.FindFirst("div")?.Value;
                user.name = User.FindFirst("name")?.Value;
                user.date = DateTime.Now.ToString("yyyy/MM/dd");

                foreach (string id in body.id)
                {
                    _invoiceService.updateStatus(id, body.status, user);
                }
                res.success = true;
                res.message = "Update to " + body.status + " success";
                return Ok(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Conflict();
            }
        }

        [HttpGet("{status}")]
        public IActionResult getInvoicse(string status)
        {
            try
            {
                Console.WriteLine(status);
                List<Invoices> data = _invoiceService.getByStatus(status);
                res.success = true;
                res.data = data.ToArray();

                if (res.data.Length == 0)
                {
                    res.message = "No data.";
                    return NotFound(res);
                }
                res.message = "Get invoice success";
                return Ok(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Conflict();
            }
        }

        [HttpDelete("{id}")]
        public IActionResult deleteInvoice(string id)
        {

            Invoices data = _invoiceService.GetById(id);


            if (data == null)
            {
                res.success = false;
                res.message = "Not found record.";
                return NotFound(res);
            }

            _invoiceService.deleteInvoice(id);
            res.success = true;
            res.message = "Delete invoice success";

            Profile user = new Profile();

            user.empNo = User.FindFirst("username")?.Value;
            user.band = User.FindFirst("band")?.Value;
            user.dept = User.FindFirst("dept")?.Value;
            user.div = User.FindFirst("div")?.Value;
            user.name = User.FindFirst("name")?.Value;

            return Ok(res);
        }

        [HttpPatch("search")]
        public ActionResult getLotOnSearch(startEndDate body)
        {

            List<requesterUploadSchema> requester = _requester.searchToInvoice(body.startDate, body.endDate);
            // List<Waste> waste = _wasteService.searchToInvoice(body.startDate, body.endDate);

            return Ok(new
            {
                success = true,
                message = "Waste data",
                data = new
                {
                    requester = requester,
                    // fae = waste,
                }
            });
        }

        [HttpPatch("print")]
        public ActionResult print(getInvoice body)
        {

            List<dynamic> lotDataReturn = new List<dynamic>();

            Invoices invoice = _invoiceService.getById(body.id);

            decimal subTotal = 0; decimal gradTotal = 0; decimal vat = 0;

            // List<requesterUploadSchema> lotData = _requester.getByLotno(body.lotNo);

            List<requesterUploadSchema> lotData = new List<requesterUploadSchema>();

            foreach (string lotNo in invoice.summaryId)
            {
                lotData.AddRange(_requester.getByLotno(lotNo));
            }

            subTotal = 0;
            foreach (requesterUploadSchema item in lotData)
            {
                subTotal = subTotal + decimal.Round(Decimal.Parse(item.totalPrice), 3);
                lotDataReturn.Add(new
                {
                    description = item.biddingType,
                    quantity = item.totalWeight,
                    unit = item.unit,
                    unitPrice = item.unitPrice,
                    amount = decimal.Round(Decimal.Parse(item.totalPrice), 3).ToString()
                });
            }
            vat = (subTotal * 7) / 100;

            gradTotal = subTotal + vat;
            return Ok(new
            {
                success = true,
                message = "Invoice printing",
                data = new
                {
                    detail = new
                    {
                        company = invoice.company,
                        invoiceNo = "CPT-2021-abc",
                        invoiceDate = invoice.createDate,
                        invoiceDue = "Due date"
                    },
                    records = lotDataReturn,
                    summary = new
                    {
                        subTotal = subTotal.ToString(),
                        vat = vat.ToString(),
                        grandTotal = Decimal.Round(gradTotal, 2).ToString()
                    },
                    textMessage = ThaiBahtText((subTotal + vat).ToString())
                }
            });
        }

        public static string ThaiBahtText(string strNumber, bool IsTrillion = false)
        {
            string BahtText = "";
            string strTrillion = "";
            string[] strThaiNumber = { "ศูนย์", "หนึ่ง", "สอง", "สาม", "สี่", "ห้า", "หก", "เจ็ด", "แปด", "เก้า", "สิบ" };
            string[] strThaiPos = { "", "สิบ", "ร้อย", "พัน", "หมื่น", "แสน", "ล้าน" };

            decimal decNumber = 0;
            decimal.TryParse(strNumber, out decNumber);

            if (decNumber == 0)
            {
                return "ศูนย์บาทถ้วน";
            }

            strNumber = decNumber.ToString("0.00");
            string strInteger = strNumber.Split('.')[0];
            string strSatang = strNumber.Split('.')[1];

            if (strInteger.Length > 13)
                throw new Exception("รองรับตัวเลขได้เพียง ล้านล้าน เท่านั้น!");

            bool _IsTrillion = strInteger.Length > 7;
            if (_IsTrillion)
            {
                strTrillion = strInteger.Substring(0, strInteger.Length - 6);
                BahtText = ThaiBahtText(strTrillion, _IsTrillion);
                strInteger = strInteger.Substring(strTrillion.Length);
            }

            int strLength = strInteger.Length;
            for (int i = 0; i < strInteger.Length; i++)
            {
                string number = strInteger.Substring(i, 1);
                if (number != "0")
                {
                    if (i == strLength - 1 && number == "1" && strLength != 1)
                    {
                        BahtText += "เอ็ด";
                    }
                    else if (i == strLength - 2 && number == "2" && strLength != 1)
                    {
                        BahtText += "ยี่";
                    }
                    else if (i != strLength - 2 || number != "1")
                    {
                        BahtText += strThaiNumber[int.Parse(number)];
                    }

                    BahtText += strThaiPos[(strLength - i) - 1];
                }
            }

            if (IsTrillion)
            {
                return BahtText + "ล้าน";
            }

            if (strInteger != "0")
            {
                BahtText += "บาท";
            }

            if (strSatang == "00")
            {
                BahtText += "ถ้วน";
            }
            else
            {
                strLength = strSatang.Length;
                for (int i = 0; i < strSatang.Length; i++)
                {
                    string number = strSatang.Substring(i, 1);
                    if (number != "0")
                    {
                        if (i == strLength - 1 && number == "1" && strSatang[0].ToString() != "0")
                        {
                            BahtText += "เอ็ด";
                        }
                        else if (i == strLength - 2 && number == "2" && strSatang[0].ToString() != "0")
                        {
                            BahtText += "ยี่";
                        }
                        else if (i != strLength - 2 || number != "1")
                        {
                            BahtText += strThaiNumber[int.Parse(number)];
                        }

                        BahtText += strThaiPos[(strLength - i) - 1];
                    }
                }

                BahtText += "สตางค์";
            }

            return BahtText;
        }

        [HttpGet("acc/print/{id}")]
        public ActionResult getById(string id)
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

                // preparing acc record
                Invoices data = _invoiceService.getById(id);

                List<SummaryInvoiceSchema> summary = new List<SummaryInvoiceSchema>();

                foreach (string summaryId in data.summaryId)
                {
                    summary.Add(_summary.getById(summaryId));
                }

                List<Waste> wasteData = new List<Waste>();

                // merge all recycle data summary
                foreach (SummaryInvoiceSchema item in summary)
                {
                    wasteData.AddRange(item.recycle);
                }
                // merge all recycle data summary

                List<Waste> distinct = wasteData.GroupBy(x => x.wasteName).Select(x => x.First()).ToList();
                // preparing acc record


                List<InvoiceprintingItems> dataItems = new List<InvoiceprintingItems>();

                double subTotal = 0.0;

                foreach (Waste wastename in distinct)
                {
                    string name = wastename.wasteName;
                    double totalWeight = 0.0; double totalPrice = 0.0;
                    int no = 1;
                    List<Waste> searchBywasteName = wasteData.FindAll(item => item.wasteName == name);
                    foreach (Waste item in searchBywasteName)
                    {
                        totalWeight += Double.Parse(item.netWasteWeight);
                        totalPrice += Double.Parse(item.totalPrice);
                    }
                    dataItems.Add(new InvoiceprintingItems
                    {
                        no = no,
                        wastename = wastename.wasteName,
                        quantity = totalWeight.ToString("#,###.00"),
                        unit = wastename.unit,
                        unitPrice = wastename.unitPrice,
                        totalPrice = totalPrice.ToString("#,###.00")
                    });
                    no += 1;

                    subTotal += totalPrice;
                }
                double vat = Math.Round(((subTotal * 7) / 100), 2);

                string bathString = ThaiBahtText((subTotal + vat).ToString("#,###,###.00"));

                InvoicePrintedSchema printingData = new InvoicePrintedSchema
                {
                    company = data.company,
                    invoice = new InvoiceprintingDetail
                    {
                        invoiceNo = data.invoiceNo,
                        invoiceDate = data.invoiceDate,
                        termOfPayment = data.termsOfPayment,
                        dueDate = data.dueDate,
                        customerCode = data.customerCode,
                        poNo = data.poNo,
                        address = data.company.address,
                        attnRef = data.attnRef,
                        customerName = data.company.companyName
                    },
                    detail = dataItems.ToArray(),
                    total = new totalPrintingDetail
                    {
                        subTotal = Math.Round(subTotal, 2).ToString("#,###,###.00"),
                        vat = vat.ToString("#,###,###.00"),
                        grandTotal = (subTotal + vat).ToString("#,###,###.00"),
                        bathString = bathString,
                    },
                    printingDate = DateTime.Now.ToString("dd-MMM-yyyy"),
                    printedBy = new Profile
                    {
                        empNo = User.FindFirst("username")?.Value,
                        name = User.FindFirst("name")?.Value,
                        dept = User.FindFirst("dept")?.Value,
                        date = DateTime.Now.ToString("yyyy/MM/dd")
                    }

                };
                accCreatePrintFile();
                return Ok(new
                {
                    success = true,
                    message = "Invoice detail",
                    data = printingData
                });
            }
            catch (Exception e)
            {

                return Problem(e.StackTrace);
            }
        }


        private void accCreatePrintFile()
        {

            Console.WriteLine("accCreatePrintFile");
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

            WebClient client = new WebClient();
            Stream canon_img = client.OpenRead("http://cptsvs531:5000/files/middleware_img/canon.png");
            Bitmap bitmap = new Bitmap(canon_img);

            using (ExcelPackage excel = new ExcelPackage(stream))
            {
                Console.WriteLine(filePath);
                excel.Workbook.Worksheets.Add("Data");

                ExcelWorksheet sheet = excel.Workbook.Worksheets["Data"];

                FileInfo file = new FileInfo(filePath);

                ExcelPicture picture = sheet.Drawings.AddPicture("image", bitmap);
                picture.SetSize(109, 41);
                picture.SetPosition(0, 0, 1, 0);

                sheet.Row(3).Height = 9;
                sheet.Column(1).Width = 1;
                sheet.Column(4).Width = 30;
                sheet.Cells["B4"].Value = "บริษัท แคนนอน ปราจีนบุรี (ประเทศไทย) จำกัด";
                sheet.Cells["B5"].Value = "Canon Prachinburi (Thailand) Ltd.";
                sheet.Cells["B6"].Value = "550  Moo  7  T.Thatoom A.Srimahaphote";
                sheet.Cells["B7"].Value = "เลขที่ 550 หมู่ที่ 7 ตำบล ท่าตูม อำเภอ ศรีมหาโพธิ";
                sheet.Cells["B8"].Value = "Prachinburi  25140, Thailand";
                sheet.Cells["B9"].Value = "จังหวัดปราจีนบุรี 25140 ประเทศไทย  สำนักงานใหญ่";
                sheet.Cells["B10"].Value = "โทรศัพท์ +66-372-84500  Phone +66-372-84500";
                sheet.Cells["B11"].Value = "เลขประจำตัวผู้เสียภาษี 0145554002624  Tax ID. 0145554002624";

                sheet.Cells["B4:F11"].Style.Font.Name = "HGP創英角ｺﾞｼｯｸUB";
                sheet.Cells["B4:F11"].Style.Font.Size = 8;
                sheet.Cells["B4:F11"].Style.Font.Color.SetColor(ColorTranslator.FromHtml("#939393"));

                sheet.Cells["B12:K12"].Merge = true;
                sheet.Cells["B12"].Value = "TAX INVOICE / INVOICE";
                sheet.Cells["B12"].Style.Font.Bold = true;
                sheet.Cells["B12"].Style.Font.Size = 16;
                sheet.Cells["B12"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Cells["B12"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                sheet.Cells["B13:K13"].Merge = true;
                sheet.Cells["B13"].Value = "ต้นฉบับใบกำกับภาษี / ต้นฉบับใบแจ้งหนี้";
                sheet.Cells["B13"].Style.Font.Bold = true;
                sheet.Cells["B13"].Style.Font.Size = 11;
                sheet.Cells["B12:B13"].Style.Font.Name = "Century";
                sheet.Cells["B13"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Cells["B13"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                sheet.Cells["B15:I26"].Style.Font.Color.SetColor(ColorTranslator.FromHtml("#939393"));
                sheet.Cells["B15:I26"].Style.Font.Name = "HGP創英角ｺﾞｼｯｸUB";
                sheet.Cells["B15:B26"].Style.Font.Size = 10;
                sheet.Cells["I15:I26"].Style.Font.Size = 9;

                sheet.Cells["B24"].Value = "CUSTOMER CODE / รหัสลูกค้า";
                sheet.Cells["B26"].Value = "P/O No. / เลขที่ใบสั่งซื้อของลูกค้า";

                sheet.Cells["B15"].Value = "ชื่อลูกค้า CUSTOMER NAME:";
                sheet.Cells["B16"].Value = "ที่อยู่ ADDRESS:";
                sheet.Cells["B21"].Value = "อ้างอิง ATTN:";
                sheet.Cells["I15"].Value = "INVOICE NO:";
                sheet.Cells["I16"].Value = "เลขที่ใบกำกับภาษี";
                sheet.Cells["I17"].Value = "INVOICE DATE:";
                sheet.Cells["I18"].Value = "วันที่";
                sheet.Cells["I21"].Value = "TERMS OF PAYMENT:";
                sheet.Cells["I22"].Value = "เงื่อนไขการชำระเงิน";
                sheet.Cells["I23"].Value = "DUE DATE:";
                sheet.Cells["I24"].Value = "วันครบกำหนด";

                ExcelShape shape = sheet.Drawings.AddShape("customer shape", eShapeStyle.RoundRect);
                shape.Fill.Color = Color.White;
                shape.Fill.Transparancy = 100;
                // shape.Fill.PatternFill.PatternType = eFillPatternStyle.DashDnDiag;
                sheet.Cells["B28"].Value = "ITEM NO.";
                sheet.Cells["B29"].Value = "ลำดับที่";

                sheet.Cells["C28:F28"].Merge = true;
                sheet.Cells["C28"].Value = "DESCRIPTIONS";
                sheet.Cells["C29:F29"].Merge = true;
                sheet.Cells["C29"].Value = "รายการ";
                sheet.Cells["G28"].Value = "QUANTITY";
                sheet.Cells["G29"].Value = "จำนวน";
                sheet.Cells["H28"].Value = "UNIT";
                sheet.Cells["H29"].Value = "หน่วย";
                sheet.Cells["I28"].Value = "UNIT PRICE (THB)";
                sheet.Cells["I29"].Value = "ราคาต่อหน่วย (บาท)";
                sheet.Cells["J28"].Value = "Amount (THB)";
                sheet.Cells["J29"].Value = "จำนวนเงิน (บาท)";
                
                shape.SetPosition(13, 0, 1, 0);
                shape.SetSize(590, 168);
                excel.SaveAs(file);
                stream.Position = 0;
            }
            // return file path when gennerated
            // return "";
        }
        [HttpPut("acc/prepare")]
        public ActionResult AccPrepare(AccPrepareInvoice body)
        {

            foreach (string id in body.id)
            {
                _invoiceService.accPrepare(
                    id,
                    body.attnRef,
                    body.customerCode,
                    body.dueDate,
                    body.invoiceNo,
                    body.poNo,
                    body.termsOfPayment
                );
            }
            return Ok(new { success = true, message = "Prepared success." });
        }
    }
}