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

    private readonly itcDBservice _itcdb;
    private readonly faeDBservice _faeDB;

    public handleUpload(itcDBservice itcdb, faeDBservice fae)
    {
        _itcdb = itcdb;
        _faeDB = fae;
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
                        // rowData.moveOutDate = value.Trim();
                        DateTime parsed = DateTime.ParseExact(value, "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                        rowData.moveOutDate = parsed.ToString("dd-MMMM-yyyy");
                        rowData.moveOutMonth = parsed.ToString("MMMM");
                        rowData.moveOutYear = parsed.ToString("yyyy");
                    }
                    else if (col == 5)
                    {
                        rowData.phase = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 6)
                    {
                        rowData.lotNo = prepare.dept.ToUpper() + "-" + sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 7)
                    {
                        matrialCode = sheet.Cells[row, col].Value?.ToString();
                        rowData.matrialCode = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 8)
                    {
                        matrialName = sheet.Cells[row, col].Value?.ToString();
                        rowData.matrialName = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 9)
                    {
                        rowData.totalWeight = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 10)
                    {
                        rowData.containerWeight = Math.Round(Double.Parse(sheet.Cells[row, col].Value?.ToString()), 4).ToString();
                    }
                    else if (col == 11)
                    {
                        rowData.qtyOfContainer = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 12)
                    {
                        rowData.netWasteWeight = sheet.Cells[row, col].Value?.ToString();
                    }
                    else if (col == 13)
                    {
                        rowData.unit = sheet.Cells[row, col].Value?.ToString();

                        ITCDB itc = _itcdb.matCode_name(matrialCode);

                        // ITC
                        if (itc != null)
                        {
                            Console.WriteLine("---------> " + itc.groupBoiNo);
                            rowData.boiType = itc.privilegeType;
                            rowData.groupBoiNo = itc.groupBoiNo;
                            rowData.groupBoiName = itc.groupBoiName;
                        }
                        else
                        {
                            Console.WriteLine("DATA NULL: " + matrialCode);
                            rowData.boiType = "-";
                            rowData.groupBoiNo = "-";
                            rowData.groupBoiName = "-";
                        }
                    }
                    // ITC

                    rowData.invoiceRef = false;
                    // FAE
                    // Console.WriteLine(row + " --> " + sheet.Cells[row, 1].Value?.ToString());
                    if (col == 12)
                    {
                        faeDBschema faeDB = _faeDB.getByWasteName(rowData.matrialCode, rowData.kind);

                        if (faeDB != null)
                        {
                            rowData.biddingType = faeDB.biddingType;
                            rowData.wasteName = faeDB.wasteName;
                            rowData.biddingNo = faeDB.biddingNo;
                            rowData.wasteName = faeDB.wasteName;
                            rowData.color = faeDB.color;
                            rowData.unitPrice = faeDB.pricePerUnit;
                            rowData.totalPrice = Math.Round(Double.Parse(faeDB.pricePerUnit) * Double.Parse(rowData.netWasteWeight), 4).ToString(); // ??
                        }
                        else
                        {
                            rowData.biddingType = "-";
                            rowData.wasteName = "-";
                            rowData.biddingNo = "-";
                            rowData.color = "-";
                            rowData.unitPrice = "-";
                            rowData.totalPrice = "-";
                        }
                    }
                    // FAE


                    rowData.date = DateTime.Now.ToString("yyyy-MM-dd");
                    rowData.dept = prepare.dept;
                    rowData.div = prepare.div;
                    rowData.requestYear = DateTime.Now.ToString("yyyy");
                    rowData.requestMonth = DateTime.Now.ToString("MMMM");
                    rowData.requestTime = DateTime.Now.ToString("HH:mm");

                    rowData.status = "req-prepared";
                    rowData.req_prepared = prepare;
                    rowData.req_checked = emptyUser;
                    rowData.req_approved = emptyUser;
                    rowData.itc_checked = emptyUser;
                    rowData.itc_approved = emptyUser;
                    rowData.fae_prepared = emptyUser;
                    rowData.fae_checked = emptyUser;
                    rowData.fae_approved = emptyUser;
                    rowData.pdc_prepared = emptyUser;
                    rowData.pdc_checked = emptyUser;
                    rowData.pdc_approved = emptyUser;
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

    public List<requesterUploadSchema> uploadData(string pathFile, string fileName, Profile prepare, Profile emptyUser)
    {

        List<requesterUploadSchema> data = new List<requesterUploadSchema>();

        using (ExcelPackage package = new ExcelPackage(new FileInfo(pathFile)))
        {
            ExcelWorkbook Workbook = package.Workbook;
            ExcelWorksheet sheet = Workbook.Worksheets[0];

            int colCount = sheet.Dimension.Columns;
            int rowCount = sheet.Dimension.Rows;

            bool isEmptyRow = false;
            for (int row = 4; row <= rowCount; row++)
            {
                requesterUploadSchema item = new requesterUploadSchema();
                // isEmptyRow = false;

                for (int col = 1; col <= 25; col++)
                {
                    string value = sheet.Cells[row, col].Value?.ToString();
                    if (value == null)
                    {
                        value = "-";
                    }
                    switch (col)
                    {
                        case 1:
                            if (value == null)
                            {
                                isEmptyRow = true;
                                break;
                            }
                            else
                            {
                                item.no = value;
                            }
                            break;
                        case 2: item.extraWorkNo = value; break;
                        case 3: item.matrialCode = value; break;
                        case 4: item.matrialName = value; break;
                        case 5: item.childPart = value; break;
                        case 6: item.blockCodeExtraWork = value; break;
                        case 7: item.supOnMacro = value; break;
                        case 8: item.fact = value; break;
                        case 9: item.boiType = value; break;
                        case 10: item.groupBoiNo = value; break;
                        case 11: item.groupBoiName = value; break;
                        case 12: item.boiUnit = value; break;
                        case 13: item.qtyOfContainer = value; break;
                        case 14: item.dateSupplierConfirm = value; break;
                        case 15: item.phase = value; break;
                        case 16: item.kind = value; break;
                        case 17:
                            if (value != "-")
                            {
                                item.moveOutDate = DateTime.FromOADate(Double.Parse(value)).ToString("dd-MMM-yyyy");
                                item.moveOutMonth = DateTime.FromOADate(Double.Parse(value)).ToString("MMMM");
                                item.moveOutYear = DateTime.FromOADate(Double.Parse(value)).ToString("yyyy");

                            }
                            break;
                        case 18: item.lotNo = value; break;
                        case 19: item.totalWeight = value; break;
                        case 20: item.containerWeight = value; break;
                        case 21: item.qtyOfContainer = value; break;
                        case 22: item.unit = value; break;
                        case 23: item.netWasteWeight = value; break;
                        case 24: item.KG_G = value; break;
                        case 25: item.remark = value; break;
                    }
                    if (col == 25)
                    {
                        faeDBschema faeDB = null;

                        if (item.kind == "-") {
                            break;
                        }
                        if (item.matrialCode == "-") {
                            faeDB = _faeDB.getByKind(item.kind);
                        } else {
                            faeDB = _faeDB.getByMatcodeAndKind(item.matrialCode, item.kind);
                        }

                        if (faeDB != null)
                        {
                            item.biddingType = faeDB.biddingType;
                            item.wasteName = faeDB.wasteName;
                            item.biddingNo = faeDB.biddingNo;
                            item.wasteName = faeDB.wasteName;
                            item.color = faeDB.color;
                            item.unitPrice = faeDB.pricePerUnit;
                            item.totalPrice = Math.Round(Double.Parse(faeDB.pricePerUnit) * Double.Parse(item.netWasteWeight), 4).ToString(); // ??
                        }
                        else
                        {
                            item.biddingType = "-";
                            item.wasteName = "-";
                            item.biddingNo = "-";
                            item.color = "-";
                            item.unitPrice = "-";
                            item.totalPrice = "-";
                        }
                    }

                    item.requestMonth = DateTime.Now.ToString("MMMM");
                    item.requestYear = DateTime.Now.ToString("yyyy");
                    item.requestTime = DateTime.Now.ToString("hh:mm");
                    item.dept = prepare.dept;
                    item.div = prepare.div;
                    item.date = DateTime.Now.ToString("dd-MMM-yyyy");
                    item.status = "req-prepared";
                    item.req_prepared = prepare;
                    item.req_checked = emptyUser;
                    item.req_approved = emptyUser;
                    item.pdc_checked = emptyUser;
                    item.itc_checked = emptyUser;
                    item.itc_approved = emptyUser;
                    item.fae_approved = emptyUser;
                    item.fileUploadName = fileName;
                    item.rejectCommend = "-";
                }
                if (isEmptyRow == false && item.totalWeight != "-")
                {
                    data.Add(item);
                }

            }
            Console.WriteLine(data.Count);
        }

        return data;
    }

}