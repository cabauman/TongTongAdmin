//using OfficeOpenXml;
//using OfficeOpenXml.Style;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace TongTongAdmin.Views
{
    class Sample1
    {
        public static async Task<string> GetCellValue(string fileName,
           string sheetName,
           string addressName,
           StorageFile file)
        {
            string value = null;
            
            using(var fs = await file.OpenStreamForWriteAsync())
            {
                using(SpreadsheetDocument doc = SpreadsheetDocument.Open(fs, true))
                {
                    WorkbookPart wbPart = doc.WorkbookPart;

                    // Find the sheet with the supplied name, and then use that 
                    // Sheet object to retrieve a reference to the first worksheet.
                    Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().
                      Where(s => s.Name == sheetName).FirstOrDefault();

                    // Throw an exception if there is no sheet.
                    if(theSheet == null)
                    {
                        throw new ArgumentException("sheetName");
                    }

                    // Retrieve a reference to the worksheet part.
                    WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id));

                    var cells = wsPart.Worksheet.Descendants<Cell>();
                    var rows = wsPart.Worksheet.Descendants<Row>();
                    var cols = wsPart.Worksheet.Descendants<Column>();
                    var cellCount = cells.Count();
                    var rowCount = rows.Count();

                    var row = rows.First().RowIndex;

                    // Use its Worksheet property to get a reference to the cell 
                    // whose address matches the address you supplied.
                    Cell theCell = wsPart.Worksheet.Descendants<Cell>().
                      Where(c => c.CellReference == addressName).FirstOrDefault();

                    // If the cell does not exist, return an empty string.
                    if(theCell != null)
                    {
                        value = theCell.InnerText;

                        // If the cell represents an integer number, you are done. 
                        // For dates, this code returns the serialized value that 
                        // represents the date. The code handles strings and 
                        // Booleans individually. For shared strings, the code 
                        // looks up the corresponding value in the shared string 
                        // table. For Booleans, the code converts the value into 
                        // the words TRUE or FALSE.
                        if(theCell.DataType != null)
                        {
                            switch(theCell.DataType.Value)
                            {
                                case CellValues.SharedString:

                                    // For shared strings, look up the value in the
                                    // shared strings table.
                                    var stringTable =
                                        wbPart.GetPartsOfType<SharedStringTablePart>()
                                        .FirstOrDefault();

                                    // If the shared string table is missing, something 
                                    // is wrong. Return the index that is in
                                    // the cell. Otherwise, look up the correct text in 
                                    // the table.
                                    if(stringTable != null)
                                    {
                                        value =
                                            stringTable.SharedStringTable
                                            .ElementAt(int.Parse(value)).InnerText;
                                    }
                                    break;

                                case CellValues.Boolean:
                                    switch(value)
                                    {
                                        case "0":
                                            value = "FALSE";
                                            break;
                                        default:
                                            value = "TRUE";
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

            return value;
        }

        public static void InsertRow(WorksheetPart worksheetPart)
        {
            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            Row lastRow = sheetData.Elements<Row>().LastOrDefault();

            if(lastRow != null)
            {
                sheetData.InsertAfter(new Row() { RowIndex = (lastRow.RowIndex + 1) }, lastRow);
            }
            else
            {
                sheetData.InsertAt(new Row() { RowIndex = 0 }, 0);
            }
        }

        private Cell InsertCell(uint rowIndex, uint columnIndex, Worksheet worksheet)
        {
            Row row = null;
            var sheetData = worksheet.GetFirstChild<SheetData>();

            // Check if the worksheet contains a row with the specified row index.
            row = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex == rowIndex);
            if(row == null)
            {
                row = new Row() { RowIndex = rowIndex };
                sheetData.Append(row);
            }

            // Convert column index to column name for cell reference.
            var columnName = "A";
            var cellReference = columnName + rowIndex;      // e.g. A1

            // Check if the row contains a cell with the specified column name.
            var cell = row.Elements<Cell>()
                       .FirstOrDefault(c => c.CellReference.Value == cellReference);
            if(cell == null)
            {
                cell = new Cell() { CellReference = cellReference };
                if(row.ChildElements.Count < columnIndex)
                    row.AppendChild(cell);
                else
                    row.InsertAt(cell, (int)columnIndex);
            }

            return cell;
        }
    }
}
