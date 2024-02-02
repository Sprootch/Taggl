module EPPlusExcel

open OfficeOpenXml
open System.IO
open Types
open Common

// TODO : 
// find project row
// utiliser user secret

let generateExcelFromTemplate path date (timeEntries: MyTimeEntry list) =
    let savePath = Path.Combine(path, $"TS-{date:yyyyMM}-Delcoigne-Vincent-WIP.xlsx")

    let dates = date |> generateDates |> Seq.toList
    let projects = (timeEntries |> List.groupBy (_.ProjectName) |> List.sort)
    let package = new ExcelPackage(Path.Combine(path, "Timesheet-Template-v10.xlsx"))

    package.Workbook.Worksheets["Configuration"].Cells["D13"].Value <- date

    let prestations = package.Workbook.Worksheets["Prestations"]

    let mutable row = 7

    for _, timeEntries in projects do
        for col, date in dates |> List.indexed do
            timeEntries
            |> List.tryFind (fun te -> te.Date = date)
            |> Option.iter (fun item -> prestations.Cells[row, col + 4].Value <- item.Duration)

        row <- row + 1

    package.SaveAs(FileInfo(savePath))

    savePath
