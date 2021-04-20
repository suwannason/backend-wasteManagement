using System;
using System.IO;
using OfficeOpenXml;
using backend.request;

using backend.Models;
using backend.Services;
using System.Collections.Generic;
using System.Globalization;

public class handleUpload
{

    private readonly ITC_IMO_DB_service _itc_IMO;

    public handleUpload(ITC_IMO_DB_service itc_imo)
    {
        _itc_IMO = itc_imo;
    }

    public List<requesterUploadSchema> Upload(string pathFile, Profile prepare, Profile emptyUser)
    {

        using (ExcelPackage package = new ExcelPackage(new FileInfo(pathFile)))
        {
            ExcelWorkbook Workbook = package.Workbook;
            ExcelWorksheet sheet = Workbook.Worksheets[0];

            List<requesterUploadSchema> data = new List<requesterUploadSchema>();

            int colCount = sheet.Dimension.Columns;
            int rowCount = sheet.Dimension.Rows;
            Console.WriteLine("rowCount: " + rowCount);

            bool isEmptyRow = false;
            string matrialCode = ""; string matrialName = "";
            for (int row = 5; row <= rowCount; row++)
            {
                requesterUploadSchema rowData = new requesterUploadSchema();
                isEmptyRow = false;
                for (int col = 1; col <= colCount; col++)
                {
                    if (col == 1)
                    {
                        // rowData.summaryType = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 2)
                    {
                        if (sheet.Cells[row, col].Value?.ToString().Trim() == "" || sheet.Cells[row, col].Value?.ToString() == null)
                        {
                            isEmptyRow = true;
                            break;
                        }
                        rowData.no = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 3)
                    {
                        rowData.kind = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 4)
                    {
                        string value = sheet.Cells[row, col].Value?.ToString();
                        Console.WriteLine(value);
                        int lastIndex = (int)sheet.Cells[row, col].Value?.ToString().IndexOf(" ");
                        rowData.moveOutDate = DateTime.ParseExact(value.Substring(0, lastIndex), "m/d/yyyy", CultureInfo.InvariantCulture).ToString("yyyy/mm/dd");
                    }
                    else if (col == 5)
                    {
                        rowData.lotNo = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 6)
                    {
                        matrialCode = sheet.Cells[row, col].Value?.ToString();
                        rowData.matrialCode = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 7)
                    {
                        matrialName = sheet.Cells[row, col].Value?.ToString();
                        rowData.matrialName = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 8)
                    {
                        rowData.totalWeight = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 9)
                    {
                        rowData.containerWeight = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 10)
                    {
                        rowData.qtyOfContainer = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 11)
                    {
                        rowData.netWasteWeight = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 12)
                    {
                        rowData.unit = sheet.Cells[row, col].Value?.ToString();

                        ITC_IMO_SCHEMA imo = _itc_IMO.matCode_name(matrialCode, matrialName);

                        // ITC
                        if (imo != null)
                        {
                            Console.WriteLine("---------> " + imo.groupBoiNo);
                            rowData.boiType = imo.boiType;
                            rowData.groupBoiNo = imo.groupBoiNo;
                            rowData.groupBoiName = imo.groupBoiName;
                        }
                        else
                        {
                            Console.WriteLine("DATA NULL");
                            rowData.boiType = "-";
                            rowData.groupBoiNo = "-";
                            rowData.groupBoiName = "-";
                        }
                    }
                    // ITC

                    rowData.invoiceRef = false;
                    // FAE
                    rowData.biddingType = "-";
                    rowData.biddingNo = "-";
                    rowData.color = "-";
                    rowData.unitPrice = "-";
                    rowData.totalPrice = "-"; // ??
                    // FAE


                    rowData.date = DateTime.Now.ToString("yyyy-MM-dd");
                    rowData.dept = prepare.dept;
                    rowData.div = prepare.div;
                    rowData.year = DateTime.Now.ToString("yyyy");

                    rowData.status = "req-prepared";
                    rowData.req_prepared = prepare;
                    rowData.req_checked = emptyUser;
                    rowData.req_approved = emptyUser;
                    rowData.itc_checked = emptyUser;
                    rowData.itc_approved = emptyUser;
                    rowData.fae_checked = emptyUser;
                    rowData.fae_approved = emptyUser;
                }
                if (isEmptyRow == false)
                {
                    data.Add(rowData);
                }
            }
            Console.WriteLine(data.ToArray().Length);
            return data;
        }
    }

    public void PMDupload(string pathFile, Profile prepare, Profile emptyUser)
    {

    }

    // public List<ITC_PMD_DB> ITCupload(string pathFile)
    // {

    //     Console.WriteLine(pathFile);
    //     using (ExcelPackage package = new ExcelPackage(new FileInfo(pathFile)))
    //     {
    //         ExcelWorkbook Workbook = package.Workbook;
    //         ExcelWorksheet sheet = Workbook.Worksheets[0];

    //         int colCount = sheet.Dimension.Columns;
    //         int rowCount = sheet.Dimension.Rows;

    //         Console.WriteLine(sheet.Name);
    //         Console.WriteLine(rowCount);
    //         List<ITC_PMD_DB> data = new List<ITC_PMD_DB>();
    //         for (int row = 2; row <= rowCount; row++)
    //         {
    //             ITC_PMD_DB rowData = new ITC_PMD_DB();
    //             for (int col = 1; col <= colCount; col++)
    //             {
    //                 if (col == 1)
    //                 {
    //                     // rowData.summaryType = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 2)
    //                 {

    //                 }
    //                 else if (col == 3)
    //                 {
    //                     rowData.matrialCode = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 4)
    //                 {
    //                     rowData.dim = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 5)
    //                 {
    //                     rowData.matrial = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 6)
    //                 {
    //                     rowData.matrialName = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 7)
    //                 {
    //                     rowData.partForCheck = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 8)
    //                 {
    //                     rowData.partNo = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 9)
    //                 {
    //                     rowData.unitNo = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 10)
    //                 {
    //                     rowData.partName = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 11)
    //                 {
    //                     rowData.part_matrial_name = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 12)
    //                 {
    //                     rowData.sizeT = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 13)
    //                 {
    //                     rowData.sizeW = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 14)
    //                 {
    //                     rowData.sizeL = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 15)
    //                 {
    //                     rowData.sizePitch = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 16)
    //                 {
    //                     rowData.CTsystem = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 17)
    //                 {
    //                     rowData.mc = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 18)
    //                 {
    //                     rowData.vendor = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 19)
    //                 {
    //                     rowData.remark = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 20)
    //                 {
    //                     rowData.privilegeType = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 21)
    //                 {
    //                     rowData.bioGroup = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 22)
    //                 {
    //                     rowData.bioName = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 23)
    //                 {
    //                     rowData.unit = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 24)
    //                 {
    //                     rowData.supplier = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //                 else if (col == 25)
    //                 {
    //                     rowData.remark_itc = sheet.Cells[row, col].Value?.ToString();
    //                 }
    //             }
    //             data.Add(rowData);
    //         }
    //         return data;
    //     }
    // }
}