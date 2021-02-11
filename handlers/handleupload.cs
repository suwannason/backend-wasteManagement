using System;
using System.IO;
using OfficeOpenXml;
using backend.request;

using backend.Models;
using System.Collections.Generic;

public class handleUpload
{
    public List<ScrapMatrialimoSchema> IMOupload(string pathFile, Profile prepare, Profile emptyUser)
    {

        using (ExcelPackage package = new ExcelPackage(new FileInfo(pathFile)))
        {
            ExcelWorkbook Workbook = package.Workbook;
            ExcelWorksheet sheet = Workbook.Worksheets[0];

            List<ScrapMatrialimoSchema> data = new List<ScrapMatrialimoSchema>();

            int colCount = sheet.Dimension.Columns;
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                ScrapMatrialimoSchema rowData = new ScrapMatrialimoSchema();
                for (int col = 1; col <= colCount; col++)
                {
                    if (col == 1)
                    {
                        rowData.summaryType = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 2)
                    {
                        rowData.moveOutDate = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 3)
                    {
                        rowData.no = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 4)
                    {
                        rowData.matrialCode = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 5)
                    {
                        rowData.matrialName = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 6)
                    {
                        rowData.totalWeight = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 7)
                    {
                        rowData.containerWeight = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 8)
                    {
                        rowData.qtyOfContainer = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 9)
                    {
                        rowData.containerType = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 10)
                    {
                        rowData.netWasteWeight = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 11)
                    {
                        rowData.imoLotNo = sheet.Cells[row, col].Value?.ToString();
                    }

                    rowData.date = DateTime.Now.ToString("yyyy-MM-dd");
                    rowData.time = DateTime.Now.ToString("hh:mm:ss");

                    rowData.year = DateTime.Now.ToString("yyyy");
                    rowData.biddingNo = "-";
                    rowData.biddingType = "-";
                    rowData.color = "-";
                    rowData.unitPrice = "-";
                    rowData.totalPrice = "-";
                    rowData.status = "req-prepared";
                    rowData.req_prepared = prepare;
                    rowData.req_checked = emptyUser;
                    rowData.req_approved = emptyUser;


                    rowData.fae_checked = emptyUser;
                    rowData.fae_approved = emptyUser;
                }
                data.Add(rowData);
            }
            return data;
        }
    }

    public void PMDupload(string pathFile, Profile prepare, Profile emptyUser) {

    }
}