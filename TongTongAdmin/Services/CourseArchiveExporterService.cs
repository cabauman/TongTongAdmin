using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TongTongAdmin.Models;
using Windows.Storage;

namespace TongTongAdmin.Services
{
    public class CourseArchiveExporterService
    {
        public const string DATE_COLUMN = "A";
        public const string STUDENT_COLUMN = "B";
        public const string TEACHER_COLUMN = "C";
        public const string CLASS_DURATION_COLUMN = "D";
        public const string HOURLY_RATE_COLUMN = "E";
        public const string TUITION_COLUMN = "F";       // Formula
        public const string AMOUNT_PAID_COLUMN = "G";
        public const string AMOUNT_UNPAID_COLUMN = "H"; // Formula

        public const string DATE_COLUMN2 = "J";
        public const string STUDENT_COLUMN2 = "K";
        public const string TEACHER_COLUMN2 = "L";
        public const string AGENDA_COLUMN = "M";
        public const string HOMEWORK_COLUMN = "N";

        // The first 2 rows are headers.
        public const uint START_ROW_IDX = 3;

        public static async Task ExportExcelFile(
            List<ICourse> courses,
            string sheetName,
            StorageFile file)
        {
            using(var fs = await file.OpenStreamForWriteAsync())
            {
                using(SpreadsheetDocument doc = SpreadsheetDocument.Open(fs, true))
                {
                    WorkbookPart wbPart = doc.WorkbookPart;

                    // Find the sheet with the supplied name, and then use that 
                    // Sheet object to retrieve a reference to the first worksheet.
                    Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().
                        Where(s => s.Name == sheetName).FirstOrDefault();

                    // Retrieve a reference to the worksheet part.
                    WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id));

                    uint rowIdx = START_ROW_IDX;
                    foreach(var course in courses)
                    {
                        if(course.HourlyRate == 0)
                        {
                            continue;
                        }
                        
                        foreach(var student in course.StudentUsers)
                        {
                            uint totalAmountPaid = course.GetPaidTuition(student.Uid);

                            foreach(var syllabusItem in course.SyllabusItems)
                            {
                                InsertDate(wbPart, wsPart, DATE_COLUMN, rowIdx, syllabusItem.FromDate);
                                InsertText(wbPart, wsPart, STUDENT_COLUMN, rowIdx, student.Name);
                                InsertText(wbPart, wsPart, TEACHER_COLUMN, rowIdx, course.Teachers);
                                InsertNumber(wbPart, wsPart, CLASS_DURATION_COLUMN, rowIdx, syllabusItem.ClassDuration);
                                InsertNumber(wbPart, wsPart, HOURLY_RATE_COLUMN, rowIdx, course.HourlyRate);

                                uint tuitionForThisSyllabusItem = (uint)(course.HourlyRate * syllabusItem.ClassDuration);
                                InsertNumber(wbPart, wsPart, TUITION_COLUMN, rowIdx, tuitionForThisSyllabusItem);
                                uint amountPaidForThisSyllabusItem = Math.Min(totalAmountPaid, tuitionForThisSyllabusItem);
                                InsertNumber(wbPart, wsPart, AMOUNT_PAID_COLUMN, rowIdx, amountPaidForThisSyllabusItem);
                                totalAmountPaid -= amountPaidForThisSyllabusItem;
                                uint amountUnpaidForThisSyllabusItem = tuitionForThisSyllabusItem - amountPaidForThisSyllabusItem;
                                InsertNumber(wbPart, wsPart, AMOUNT_UNPAID_COLUMN, rowIdx, amountUnpaidForThisSyllabusItem);

                                InsertDate(wbPart, wsPart, DATE_COLUMN2, rowIdx, syllabusItem.FromDate);
                                InsertText(wbPart, wsPart, STUDENT_COLUMN2, rowIdx, student.Name);
                                InsertText(wbPart, wsPart, TEACHER_COLUMN2, rowIdx, course.Teachers);
                                InsertText(wbPart, wsPart, AGENDA_COLUMN, rowIdx, syllabusItem.Agenda);
                                InsertText(wbPart, wsPart, HOMEWORK_COLUMN, rowIdx, syllabusItem.Homework);

                                ++rowIdx;
                            }
                        }
                    }

                    wsPart.Worksheet.Save();
                    wbPart.Workbook.Save();
                }
            }
        }

        // Given a column name, a row index, and a WorksheetPart, inserts a cell into the worksheet. 
        // If the cell already exists, returns it. 
        private static Cell InsertCellInWorksheet(string columnName, uint rowIndex, WorksheetPart worksheetPart)
        {
            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();
            string cellReference = columnName + rowIndex;

            // If the worksheet does not contain a row with the specified row index, insert one.
            Row row;
            if(sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).Count() != 0)
            {
                row = sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).First();
            }
            else
            {
                row = new Row() { RowIndex = rowIndex };
                //var refRow = sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex - 1).First();
                //row = (Row)refRow.CloneNode(true);
                sheetData.Append(row);
            }

            // If there is not a cell with the specified column name, insert one.  
            if(row.Elements<Cell>().Where(c => c.CellReference.Value == columnName + rowIndex).Count() > 0)
            {
                return row.Elements<Cell>().Where(c => c.CellReference.Value == cellReference).First();
            }
            else
            {
                // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
                Cell refCell = null;
                foreach(Cell cell in row.Elements<Cell>())
                {
                    if(string.Compare(cell.CellReference.Value, cellReference, true) > 0)
                    {
                        refCell = cell;
                        break;
                    }
                }

                Cell newCell = new Cell() { CellReference = cellReference };
                row.InsertBefore(newCell, refCell);

                return newCell;
            }
        }

        public static void InsertText(WorkbookPart wbPart, WorksheetPart wsPart, string columnName, uint rowIndex, string text)
        {
            // Get the SharedStringTablePart. If it does not exist, create a new one.
            SharedStringTablePart shareStringPart;
            if(wbPart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
            {
                shareStringPart = wbPart.GetPartsOfType<SharedStringTablePart>().First();
            }
            else
            {
                shareStringPart = wbPart.AddNewPart<SharedStringTablePart>();
            }

            // Insert the text into the SharedStringTablePart.
            int index = InsertSharedStringItem(text, shareStringPart);

            Cell cell = InsertCellInWorksheet(columnName, rowIndex, wsPart);
            cell.CellValue = new CellValue(index.ToString());
            cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
        }

        public static void InsertNumber(WorkbookPart wbPart, WorksheetPart wsPart, string columnName, uint rowIndex, float number)
        {
            Cell cell = InsertCellInWorksheet(columnName, rowIndex, wsPart);
            cell.CellValue = new CellValue(number.ToString());
            cell.DataType = new EnumValue<CellValues>(CellValues.Number);
        }

        public static void InsertFormula(WorkbookPart wbPart, WorksheetPart wsPart, string columnName, uint rowIndex, string formula)
        {
            Cell cell = InsertCellInWorksheet(columnName, rowIndex, wsPart);
            cell.CellFormula = new CellFormula(formula);
            cell.DataType = new EnumValue<CellValues>(CellValues.Number);
        }

        public static void InsertDate(WorkbookPart wbPart, WorksheetPart wsPart, string columnName, uint rowIndex, DateTime date)
        {
            Cell cell = InsertCellInWorksheet(columnName, rowIndex, wsPart);
            cell.CellValue = new CellValue(date.ToOADate().ToString(CultureInfo.InvariantCulture));
            cell.DataType = new EnumValue<CellValues>(CellValues.Number);

            //cell.CellValue = new CellValue(date.ToString("s"));
            //cell.DataType = new EnumValue<CellValues>(CellValues.Date);
            //cell.DataType = CellValues.Date;
            //cell.StyleIndex = 0;
        }

        // Given text and a SharedStringTablePart, creates a SharedStringItem with the specified text 
        // and inserts it into the SharedStringTablePart. If the item already exists, returns its index.
        private static int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart)
        {
            // If the part does not contain a SharedStringTable, create one.
            if(shareStringPart.SharedStringTable == null)
            {
                shareStringPart.SharedStringTable = new SharedStringTable();
            }

            int i = 0;

            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach(SharedStringItem item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
            {
                if(item.InnerText == text)
                {
                    return i;
                }

                i++;
            }

            // The text does not exist in the part. Create the SharedStringItem and return its index.
            shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(text)));
            shareStringPart.SharedStringTable.Save();

            return i;
        }
    }
}