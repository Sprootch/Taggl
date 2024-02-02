module EPPlusExcel

open OfficeOpenXml
open System.IO
open Types
open Common

// TODO : around 8h +- 10mn = 8h
// find project row
// utiliser user secret

let generateExcelFromTemplate path date (timeEntries: MyTimeEntry list) =
    let savePath = Path.Combine(path, $"TS-{date:yyyyMM}-Delcoigne-Vincent-WIP.xlsx")

    let dates = date |> generateDates |> Seq.toList
    let projects = (timeEntries |> List.groupBy (_.ProjectName) |> List.sort)
    let package = new ExcelPackage(Path.Combine(path, "Timesheet-Template-v10.xlsx"))

    package.Workbook.Worksheets["Configuration"].Cells["D13"].Value <- date

    let prestations = package.Workbook.Worksheets["Prestations"]

    let excelColumns =
        [ "AA"; "AB"; "AC"; "AD"; "AE"; "AF"; "AH"; "AG" ]
        |> List.append ([ 'D' .. 'Z' ] |> List.map string)

    let mutable row = 7
    for projectName, te in projects do
        for i, date in dates |> List.indexed do
            match te |> List.tryFind (fun te -> te.Date = date) with
            | None -> ()
            | Some item ->
                let col = excelColumns[i]
                prestations.Cells[$"{col}{row}"].Value <- item.Duration
        row <- row + 1

    package.SaveAs(FileInfo(savePath))

    savePath
