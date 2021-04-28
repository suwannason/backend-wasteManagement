
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;
using backend.request;
using OfficeOpenXml;
using System.IO;

namespace backend.Controllers
{
    [Authorize, ApiController]
    [Route("fae-part/[controller]")]
    public class companyController : ControllerBase
    {
        private readonly CompanyService _companyService;

        public companyController(CompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        public ActionResult Get()
        {
            List<Companies> data = _companyService.Get();

            List<dynamic> company = new List<dynamic>();

            foreach(Companies item in data) {
                company.Add(
                    new {
                        _id = item._id,
                        companyName = item.companyName
                    }
                );
            }
            

            return Ok(new { success = true, message = "Company", data = company, });
        }

        [HttpGet("{id}", Name = "GetBook")]
        public ActionResult<Companies> Get(string id)
        {
            Companies book = _companyService.Get(id);

            if (book == null)
            {
                return NotFound();
            }
            return book;
        }

        [HttpPost]
        public ActionResult Create(Companies book)
        {
            try
            {
                Companies created = _companyService.Create(book);
                return Ok(created);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, Companies bookIn)
        {
            Companies book = _companyService.Get(id);

            if (book == null)
            {
                return NotFound();
            }
            _companyService.Update(id, bookIn);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            Companies book = _companyService.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            _companyService.Remove(book._id);

            return Ok();
        }

        [HttpPost("upload"), Consumes("multipart/form-data")]
        public ActionResult upload([FromForm] uploadData body) {

            FileInfo existFile = new FileInfo("PATH");
            using(ExcelPackage excel = new ExcelPackage(existFile))
            {
                ExcelWorkbook workbook = excel.Workbook;
                ExcelWorksheet sheet = workbook.Worksheets[0];

                int colCount = sheet.Dimension.End.Column;
                int rowCount = sheet.Dimension.End.Row;

                List<Companies> items = new List<Companies>();

                for (int row = 2; row < rowCount; row += 1)
                {
                    Companies item = new Companies();
                    for (int col = 1; col < colCount; col += 1)
                    {
                        string value = sheet.Cells[row, col].Value?.ToString();

                        switch (col)
                        {
                            case 1: Console.WriteLine(value); break;
                        }
                    }
                    items.Add(item);
                }
            }
            return StatusCode(200);
        }
    }
}