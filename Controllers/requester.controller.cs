
using backend.Models;
using backend.Services;
using backend.response;
using backend.request;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using OfficeOpenXml;
using System.IO;

namespace backend.Controllers
{

    [Route("fae-part/[controller]")]
    [ApiController]
    [Authorize]
    public class requesterController : ControllerBase
    {
        private readonly HazadousService _hazadous;
        private readonly InfectionsService _infections;
        private readonly ScrapMatrialImoService _scrapImo;
        private readonly ScrapMatrialPMDService _scrapPmd;
        private readonly ITC_IMO_DB_service _itc_imo;

        RequesterResponse res = new RequesterResponse();

        public requesterController(HazadousService req, InfectionsService infect, ScrapMatrialImoService scrapImo, ScrapMatrialPMDService pmd, ITC_IMO_DB_service itc_imo)
        {
            _hazadous = req;
            _infections = infect;
            _scrapImo = scrapImo;
            _scrapPmd = pmd;
            _itc_imo = itc_imo;
        }

        [HttpPost]
        public async Task<ActionResult<RequesterResponse>> create(ReuqesterREQ body)
        { // create by web form
            Profile req_prepare = new Profile();

            req_prepare.empNo = User.FindFirst("username")?.Value;
            req_prepare.band = User.FindFirst("band")?.Value;
            req_prepare.dept = User.FindFirst("dept")?.Value;
            req_prepare.div = User.FindFirst("div")?.Value;
            req_prepare.name = User.FindFirst("name")?.Value;
            req_prepare.tel = User.FindFirst("tel")?.Value;

            body.div = User.FindFirst("div")?.Value;
            body.dept = User.FindFirst("dept")?.Value;

            List<Task> onCreate = new List<Task>();
            if (body.hazardous.Length > 0)
            {
                onCreate.Add(Task.Run(() => { _hazadous.create(body, req_prepare); }));
            }
            if (body.infections.Length > 0)
            {
                onCreate.Add(Task.Run(() => { _infections.create(body, req_prepare); }));
            }
            if (body.scrapImo.Length > 0)
            {
                onCreate.Add(Task.Run(() => { _scrapImo.create(body, req_prepare); }));
            }

            Task created = Task.WhenAll(onCreate.ToArray());
            await created;
            res.success = true;
            res.message = "create requester data success";
            return Ok(res);
        }

        [HttpGet]
        public ActionResult<RequesterResponse> getAll()
        {
            res.success = true;
            res.message = "Get data success";

            return Ok(res);
        }
        [HttpPut("status")]
        public ActionResult<RequesterResponse> updateStatus(UpdateStatusFormRequester body)
        {

            Parallel.ForEach(body.lotNo, item =>
            {
                _hazadous.updateStatus(item, body.status);
                _scrapImo.updateStatus(item, body.status);
                _scrapPmd.updateStatus(item, body.status);
            });
            res.success = true;
            res.message = "Update status to " + body.status + " success.";
            return Ok(res);
        }

        [HttpGet("{status}")]
        // [Obsolete]
        public ActionResult getByStatus(string status)
        {
            try
            {
                string dept = User.FindFirst("dept")?.Value;

                Console.WriteLine(dept);
                typeItem type = new typeItem();
                res.message = "Item on : " + status;
                // req-prepared, req-checked, req-approved --> fae-prepared, fae-checked, fae-approved
                if (dept.ToUpper() == "FAE" || dept.ToUpper() == "ITC")
                {
                    List<InfectionSchema> infecs = _infections.getByStatus_fae(status);
                    List<HazadousSchema> hazas = _hazadous.getByStatus_fae(status);
                    List<ScrapMatrialimoSchema> scrapImo = _scrapImo.getByStatus_fae(status);

                    type.infectionsWaste = infecs.ToArray();
                    type.hazadousWaste = hazas.ToArray();
                    type.scrapImo = scrapImo.ToArray();

                    res.success = true;
                    res.data = type;
                }
                else
                {
                    List<InfectionSchema> infecs = _infections.getByStatus(status, dept);
                    List<HazadousSchema> hazas = _hazadous.getByStatus(status, dept);
                    List<ScrapMatrialimoSchema> scrapImo = _scrapImo.getByStatus(status, dept);

                    type.infectionsWaste = infecs.ToArray();
                    type.hazadousWaste = hazas.ToArray();
                    type.scrapImo = scrapImo.ToArray();

                    res.success = true;
                    res.data = type;
                }
                return Ok(res);
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }

        [HttpPatch("getByLotNo")]
        public ActionResult getByLotNo(getByStatus body)
        {
            try
            {
                List<HazadousSchema> hazadous = _hazadous.getByLotnoIdAndStatus(body.lotNo, body.status);
                List<InfectionSchema> infections = _infections.getByStatus(body.status);
                List<ScrapMatrialimoSchema> scraps = _scrapImo.getByLotNoAndStatus(body.lotNo, body.status);

                typeItem data = new typeItem();
                data.hazadousWaste = hazadous.ToArray();
                data.infectionsWaste = infections.ToArray();
                data.scrapImo = scraps.ToArray();

                res.success = true;
                res.message = "Get item success";
                res.data = data;

                return Ok(res);
            }
            catch (Exception e)
            {
                return Problem(e.StackTrace);
            }
        }


        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public ActionResult uploadData([FromForm] uploadData body)
        {
            string rootFolder = Directory.GetCurrentDirectory();

            string pathString2 = @"\API site\files\wastemanagement\upload\";
            string serverPath = rootFolder.Substring(0, rootFolder.LastIndexOf(@"\")) + pathString2;

            Console.WriteLine(serverPath);
            if (!System.IO.Directory.Exists(serverPath))
            {
                Directory.CreateDirectory(serverPath);
            }
            Profile req_prepare = new Profile();

            req_prepare.empNo = User.FindFirst("username")?.Value;
            req_prepare.band = User.FindFirst("band")?.Value;
            req_prepare.dept = User.FindFirst("dept")?.Value;
            req_prepare.div = User.FindFirst("div")?.Value;
            req_prepare.name = User.FindFirst("name")?.Value;
            req_prepare.tel = User.FindFirst("tel")?.Value;

            if (req_prepare.dept.ToUpper() != body.form.ToUpper())
            {
                return Forbid();
            }

            using (FileStream strem = System.IO.File.Create($"{serverPath}{body.file.FileName}"))
            {
                body.file.CopyTo(strem);
            }

            Profile usertmp = new Profile();
            usertmp.band = "-";
            usertmp.dept = "-";
            usertmp.empNo = "-";
            usertmp.name = "-";
            usertmp.div = "-";
            usertmp.tel = "-";

            handleUpload action = new handleUpload(_itc_imo);

            if (body.form.ToUpper() == "IMO")
            {
                List<ScrapMatrialimoSchema> data = action.IMOupload($"{serverPath}{body.file.FileName}", req_prepare, usertmp);

                Console.WriteLine("==================================");
                _scrapImo.handleUpload(data);
            }
            else if (body.form.ToUpper() == "PMD")
            {
                action.PMDupload($"{serverPath}{body.file.FileName}", req_prepare, usertmp);
            }
            else
            {
                return BadRequest(new { success = false, message = "Form invalid" });
            }
            return Ok(new { success = true, message = "Upload data success." });
        }

        [HttpPatch("history")]
        public ActionResult history(startEndDate body)
        {
            Console.WriteLine("HISTORY REUESTER");

            if (User.FindFirst("dept")?.Value.ToLower() == "imo")
            {
                List<ScrapMatrialimoSchema> data = _scrapImo.getHistory(body.startDate, body.endDate);
                return Ok(new { success = true, message = "Get imo history success", data, });
            }

            return Unauthorized(new { success = false, message = "User login department DB not match" });
        }
    }

}