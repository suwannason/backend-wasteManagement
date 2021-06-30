
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.response;
using backend.Models;
using backend.request;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;

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
        private readonly ITC_invoiceService _itc_invoice;

        InvoiceResponse res = new InvoiceResponse();

        public invoiceController(InvoiceService invoiceService, RecycleService wasteService, requesterUploadServices requester, faeDBservice fae, SummaryInvoiceService summary, InvoicePrintedService invoicePrinting, ITC_invoiceService itcInvoice)
        {
            _invoiceService = invoiceService;
            _wasteService = wasteService;
            _requester = requester;
            _faeDB = fae;
            _summary = summary;
            _invoicePrinting = invoicePrinting;
            _itc_invoice = itcInvoice;
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
                List<dynamic> ITCdata = new List<dynamic>();
                List<ITCinvoiceSchema> itc_invoice = new List<ITCinvoiceSchema>();
                List<Invoices> invoice = new List<Invoices>();

                if (status == "acc-prepared")
                {
                    invoice = _invoiceService.getByStatus(status);
                    itc_invoice = _itc_invoice.getByStatus("approved");
                }
                else if (status == "acc-checked")
                {
                    invoice = _invoiceService.getByStatus(status);
                    itc_invoice = _itc_invoice.getByStatus("acc-checked");
                }
                else if (status == "acc-approved")
                {
                    invoice = _invoiceService.getByStatus(status);
                    // itc_invoice = _itc_invoice.getByStatus("acc-checked");
                }
                else if (status == "fae-approved")
                {
                    invoice = _invoiceService.getByStatus(status);
                    // itc_invoice = _itc_invoice.getByStatus("checked");
                }

                else if (status == "fae-prepared")
                {
                    invoice = _invoiceService.getByStatus(status);
                    // itc_invoice = _itc_invoice.getByStatus("checked");
                }
                else if (status == "fae-checked")
                {
                    invoice = _invoiceService.getByStatus(status);
                    // itc_invoice = _itc_invoice.getByStatus("checked");
                }
                else if (status == "makingApproved")
                {
                    invoice = _invoiceService.getByStatus(status);
                    // itc_invoice = _itc_invoice.getByStatus("checked");
                }


                int i = 1;
                foreach (ITCinvoiceSchema item in itc_invoice)
                {
                    SummaryInvoiceSchema summary = _summary.getById(item.summaryId);
                    ITCdata.Add(
                        new
                        {
                            id = item._id,
                            no = i,
                            summaryId = item.summaryId,
                            faeCreateBy = summary.prepare.name,
                            itcPrepareBy = item.prepare.name,
                            fileName = item.files,
                            createDate = item.createDate
                        }
                    );
                    i += 1;
                }
                return Ok(
                    new
                    {
                        success = true,
                        message = "ACC data for approve",
                        data = new
                        {
                            invoiceFAE = invoice,
                            invoiceITC = ITCdata,
                        }
                    }
                );
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
            // Console.WriteLine(strNumber);
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

                if (subTotal + vat < 0)
                {
                    return BadRequest(new { success = false, message = "Total price from summary invalid." });
                }

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
                    },
                    invoiceId = id
                };
                _invoicePrinting.create(printingData);

                string filePathcreated = accCreatePrintFile(printingData);
                string filename = System.IO.Path.GetFileName(filePathcreated);
                string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                FileStream stream = System.IO.File.OpenRead(filePathcreated);
                return new FileStreamResult(stream, mimeType) { FileDownloadName = filename };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return Problem(e.Message);
            }
        }


        private string accCreatePrintFile(InvoicePrintedSchema data)
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

            WebClient client = new WebClient();
            Stream canon_img = client.OpenRead("http://cptsvs531:5000/files/middleware_img/canon.png");
            Bitmap bitmap = new Bitmap(canon_img);

            using (ExcelPackage excel = new ExcelPackage())
            {
                Console.WriteLine(filePath);
                excel.Workbook.Worksheets.Add("Data");

                ExcelWorksheet sheet = excel.Workbook.Worksheets["Data"];
                sheet.View.ShowGridLines = false;

                FileInfo file = new FileInfo(filePath);

                ExcelPicture picture = sheet.Drawings.AddPicture("image", bitmap);
                picture.SetSize(109, 41);
                picture.SetPosition(0, 0, 1, 0);

                sheet.Row(3).Height = 9;
                sheet.Column(1).Width = 2;
                sheet.Column(3).Width = 18;
                sheet.Column(4).Width = 30;
                sheet.Column(7).Width = 12;
                sheet.Column(8).Width = 8;
                sheet.Column(9).Width = 20;
                sheet.Column(11).Width = 20;

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
                sheet.Cells["D24"].Value = data.invoice.customerCode;
                sheet.Cells["D24"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                sheet.Cells["B26"].Value = "P/O No. / เลขที่ใบสั่งซื้อของลูกค้า";
                sheet.Cells["D26"].Value = data.invoice.invoiceNo;
                sheet.Cells["D26"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                sheet.Cells["D15:D26"].Style.Font.Bold = true;
                sheet.Cells["D15:D26"].Style.Font.Size = 10;
                sheet.Cells["D15:D26"].Style.Font.Color.SetColor(Color.Black);

                sheet.Cells["B15"].Value = "ชื่อลูกค้า CUSTOMER NAME:";
                sheet.Cells["D15"].Value = data.company.companyName;

                sheet.Cells["B16"].Value = "ที่อยู่ ADDRESS:";
                sheet.Cells["D16"].Value = data.company.address;
                sheet.Cells["D16"].Style.WrapText = true;

                sheet.Cells["B21"].Value = "อ้างอิง ATTN:";
                sheet.Cells["D21"].Value = data.invoice.attnRef;

                sheet.Cells["I15"].Value = "INVOICE NO:";
                sheet.Cells["J15:K16"].Merge = true;
                sheet.Cells["J15"].Value = data.invoice.invoiceNo;
                sheet.Cells["J15:K16"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Cells["J15:K16"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                sheet.Cells["I16"].Value = "เลขที่ใบกำกับภาษี";
                sheet.Cells["I17"].Value = "INVOICE DATE:";
                sheet.Cells["I18"].Value = "วันที่";
                sheet.Cells["J17:K18"].Merge = true;
                sheet.Cells["J17"].Value = data.invoice.invoiceDate;
                sheet.Cells["J17:K18"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Cells["J17:K18"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                sheet.Cells["I21"].Value = "TERMS OF PAYMENT:";
                sheet.Cells["I22"].Value = "เงื่อนไขการชำระเงิน";
                sheet.Cells["J21:K22"].Merge = true;
                sheet.Cells["J23"].Value = data.invoice.dueDate;
                sheet.Cells["J21:K22"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Cells["J21:K22"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                sheet.Cells["I23"].Value = "DUE DATE:";
                sheet.Cells["I24"].Value = "วันครบกำหนด";
                sheet.Cells["J23:K24"].Merge = true;
                sheet.Cells["J21"].Value = data.invoice.invoiceDate;
                sheet.Cells["J23:K24"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Cells["J23:K24"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                sheet.Cells["J15:K24"].Style.Font.Bold = true;
                sheet.Cells["J15:K24"].Style.Font.Size = 9;
                sheet.Cells["J15:K24"].Style.Font.Name = "Century";


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
                sheet.Cells["J28:K28"].Merge = true;
                sheet.Cells["J29:K29"].Merge = true;

                sheet.Cells["B28:K29"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Cells["B28:K29"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Cells["B28:K52"].Style.Font.Name = "HGP創英角ｺﾞｼｯｸUB";
                sheet.Cells["B28:K29"].Style.Font.Size = 9;
                sheet.Cells["B28:K29"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#C2C2C2"));
                sheet.Cells["B28:K52"].Style.Font.Bold = true;

                sheet.Cells["B28:K28"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.Cells["B29:K29"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                int startRow = 30;

                foreach (InvoiceprintingItems item in data.detail)
                {
                    sheet.Cells["B" + startRow].Value = item.no;
                    sheet.Cells["C" + startRow].Value = item.wastename;
                    sheet.Cells["G" + startRow].Value = item.quantity;
                    sheet.Cells["H" + startRow].Value = item.unit;
                    sheet.Cells["I" + startRow].Value = item.unitPrice;
                    sheet.Cells["K" + startRow].Value = item.totalPrice;

                    startRow += 1;
                }
                sheet.Cells["B30:B" + startRow].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Cells["H30:H" + startRow].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Cells["G30:G" + startRow].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                sheet.Cells["I30:I" + startRow].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                sheet.Cells["K30:K51"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                sheet.Cells["B" + startRow + ":K" + startRow].Style.Font.Size = 10;

                sheet.Cells["B52:C52"].Merge = true;
                sheet.Cells["B52"].Value = "AMOUNT จำนวนเงินตัวอักษร";
                sheet.Cells["B52"].Style.Font.Size = 9;

                sheet.Cells["I48"].Value = "SUB TOTAL จำนวนเงิน";
                sheet.Cells["I49"].Value = "VAT 7% ภาษีมูลค่าเพิ่ม";
                sheet.Cells["I50"].Value = "GRAND TOTAL";
                sheet.Cells["I51"].Value = "จำนวนเงินรวมทั้งสิ้น";
                sheet.Cells["B51"].Style.Font.Size = 9;

                sheet.Cells["I48:I51"].Style.Font.Size = 9;
                sheet.Cells["K48:K51"].Style.Font.Size = 10;
                sheet.Cells["I48:I50"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Cells["K48"].Value = data.total.subTotal;
                sheet.Cells["K49"].Value = data.total.vat;
                sheet.Cells["K51"].Value = data.total.grandTotal;

                sheet.Cells["D52:K52"].Merge = true;
                // sheet.Cells["I50:I51"].Merge = true;
                sheet.Row(52).Height = 13;
                sheet.Cells["D52"].Value = data.total.bathString;
                sheet.Cells["D52"].Style.Font.Size = 9;
                sheet.Cells["D52"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // vertical line
                sheet.Cells["A28:A52"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.Cells["B28:B51"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.Cells["F28:F51"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.Cells["G28:G51"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.Cells["H28:H51"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.Cells["I28:I51"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.Cells["K28:K52"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.Cells["C52"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                // Horizontal line
                sheet.Cells["I48:K48"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.Cells["I49:K49"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.Cells["I50:K50"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.Cells["I51"].Style.WrapText = true;
                sheet.Cells["I51"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Cells["I51"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                sheet.Cells["B52:K52"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.Cells["B52:K52"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;

                // green color space
                sheet.Cells["I48:K49"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#CFF7AC"));
                sheet.Cells["I50:K51"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#A4E769"));
                sheet.Cells["D52:K52"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#CFF7AC"));
                sheet.Cells["B52:C52"].Style.Fill.SetBackground(ColorTranslator.FromHtml("#A4E769"));

                // Remark space
                sheet.Cells["B54"].Value = "REMARK / หมายเหตุ : ";
                sheet.Cells["B56"].Value = "IMPORTANT CONDITION / เงื่อนไข";
                sheet.Cells["B57"].Value = "1. PAY BY CHEQUE OR DRAFT. PLEASE ISSUE PAYEE ONLY TO \"CANON PRACHINBURI (THAILAND) LTD.\"";
                sheet.Cells["B58"].Value = "2. PAY BY TRANSFER BANK \"SIAM COMMERCIAL BANK\"  Account No.849-2482662";
                sheet.Cells["B59"].Value = "1.เมื่อชำระด้วยเช็ค ดร๊าฟ โปรดสั่งจ่ายในนาม บริษัท แคนนอน ปราจีนบุรี (ประเทศไทย) จำกัด ขีดฆ่าคำว่า \"หรือผู้ถือ\" และขีดคร่อมเช็คด้วย";
                sheet.Cells["B60"].Value = "2.เมื่อชำระด้วยการโอนเงิน โปรดโอนผ่านธนาคารไทยพาณิชย์ เลขที่บัญชี 849-2482662";
                sheet.Cells["B54:B60"].Style.Font.Size = 8;
                sheet.Cells["B54:B60"].Style.Font.Name = "HGP創英角ｺﾞｼｯｸUB";
                sheet.Cells["B54:B60"].Style.Font.Color.SetColor(ColorTranslator.FromHtml("#939393"));
                // Signature space
                sheet.Cells["I65"].Value = "___________________________";
                sheet.Cells["I66"].Value = "                          Sign / ลงชื่อ";
                sheet.Cells["I67"].Value = "DATE/วันที่______________________";
                sheet.Cells["I69"].Value = "HIDEAKI  TAKATORI";
                sheet.Cells["I70"].Value = "            General Manager";
                sheet.Cells["I71"].Value = "  Personnel & General Affairs Div.";
                sheet.Cells["I72"].Value = " Canon Prachinburi (Thailand) Ltd.";
                sheet.Cells["I65:I72"].Style.Font.Size = 10;
                sheet.Cells["I65:I72"].Style.Font.Name = "HGP創英角ｺﾞｼｯｸUB";
                sheet.Cells["I65:I72"].Style.Font.Color.SetColor(ColorTranslator.FromHtml("#939393"));


                shape.Border.Fill.Color = Color.Black;
                shape.SetPosition(13, 0, 1, 1);
                shape.SetSize(590, 168);

                excel.SaveAs(file);


                return filePath;
            }
            // return file path when gennerated
        }
        [HttpPut("acc/prepare")]
        public ActionResult AccPrepare(AccPrepareInvoice body)
        {

            foreach (string id in body.id)
            {
                _invoiceService.accPrepare(
                    id,
                    body.dueDate,
                    body.invoiceNo,
                    body.termsOfPayment
                );
            }
            return Ok(new { success = true, message = "Prepared success." });
        }
    }
}