
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
        private readonly faeDBservice _faeDB;

        InvoiceResponse res = new InvoiceResponse();

        public invoiceController(InvoiceService invoiceService, RecycleService wasteService, requesterUploadServices requester, faeDBservice fae, SummaryInvoiceService summary)
        {
            _invoiceService = invoiceService;
            _wasteService = wasteService;
            _requester = requester;
            _faeDB = fae;
            _summary = summary;
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


                List<dynamic> dataItems = new List<dynamic>();

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
                    dataItems.Add(new
                    {
                        no,
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
                // Console.WriteLine(vat);
                return Ok(new
                {
                    success = true,
                    message = "Invoice detail",
                    data = new
                    {
                        company = data.company,
                        invoice = new
                        {
                            invoiceNo = data.invoiceNo,
                            invoiceDate = data.invoiceDate,
                            termOfPayment = data.termsOfPayment,
                            dueDate = data.dueDate,
                            customerCode = data.customerCode,
                            poNo = data.poNo
                        },
                        detail = dataItems
                    },
                    total = new
                    {
                        subTotal = Math.Round(subTotal, 2).ToString("#,###,###.00"),
                        vat = vat.ToString("#,###,###.00"),
                        grandTotal = (subTotal + vat).ToString("#,###,###.00"),
                        bathString,
                    }
                });
            }
            catch (Exception e)
            {

                return Problem(e.StackTrace);
            }
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