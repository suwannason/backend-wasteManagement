
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Authorization;

using backend.Services;
using backend.Models;
using backend.request;
using System.Linq;

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

        [HttpPost("search")]
        public ActionResult searchPrepare(dataSearch body)
        {
            try
            {
                List<requesterUploadSchema> requester = _requester.faeSummarySearch(body.moveOutMonth, body.moveOutYear, body.wasteName, body.lotNo);
                List<Waste> waste = _recycle.faeSummary(body.moveOutMonth, body.moveOutYear, body.wasteName, body.phase);
                return Ok(new
                {
                    success = true,
                    data = new { requester, waste, }
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

                foreach (string lotNo in body.requester)
                {
                    requesterItems.AddRange(_requester.getByLotno(lotNo));
                    _requester.updateStatus(lotNo, "toSummary");
                }

                foreach (string id in body.recycle)
                {
                    wasteItems.Add(_recycle.Get(id));
                    _recycle.updateStatus(id, "toSummary");
                }
                double sumRequester = 0;
                foreach (requesterUploadSchema item in requesterItems)
                {
                    Console.WriteLine(item.totalWeight + " ==> " + Double.Parse(item.totalWeight).ToString());
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
                createItem.status = "prepared";
                createItem.type = body.type;
                createItem.recycleWeight = sumRecycle;
                createItem.requesterWeight = sumRequester;

                _services.create(createItem);

                return Ok(new { success = true, message = "Create summary success." });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Problem(e.StackTrace);
            }
        }

        [HttpGet("{status}")]
        public ActionResult getByStatus(string status)
        {

            try
            {
                List<SummaryInvoiceSchema> data = _services.getByStatus(status);

                return Ok(new { success = true, message = "Data on this route.", data, });
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

                return Ok(new { success = true, message = "Data on this route.", data, });
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
                List<requesterUploadSchema> distinct = data.requester.GroupBy(x => x.kind).Select(x => x.First()).ToList();

                List<requesterUploadSchema> kind = new List<requesterUploadSchema>();

                foreach (requesterUploadSchema item in distinct)
                {
                    kind.Add(new requesterUploadSchema
                    {
                        kind = item.kind,
                        matrialCode = item.matrialCode
                    });
                }

                foreach (requesterUploadSchema item in kind) {

                }
                return Ok(new { success = true, message = "IMO data.", data = kind });
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }
    }
}