module EPPlusExcel

open System
open OfficeOpenXml
open System.IO


let generateExcelFromTemplate (path: string) (date: System.DateTime) timeEntries =
    let savePath = Path.Combine(path, $"TS-{date:yyyyMM}-Delcoigne-Vincent.xlsx")

    let package = new ExcelPackage(Path.Combine(path, "Timesheet-Template-v10.xlsx"))

    package.Workbook.Worksheets["Configuration"].Cells["D13"].Value <- date

    package.Workbook.Worksheets["Prestations"].Cells["F7"].Value <- TimeSpan.FromHours(8)
    package.Workbook.Worksheets["Prestations"].Cells["G7"].Value <- TimeSpan.FromHours(4)
    package.Workbook.Worksheets["Prestations"].Cells["G8"].Value <- TimeSpan.FromHours(4)
    package.Workbook.Worksheets["Prestations"].Cells["H8"].Value <- TimeSpan.FromHours(1)
    package.Workbook.Worksheets["Prestations"].Cells["H7"].Value <- TimeSpan.FromHours(8)

    package.SaveAs(FileInfo(savePath))

    savePath
