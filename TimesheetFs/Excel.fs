module Excel

open OfficeOpenXml
open System.IO
open Types
open Common

// TODO 
// gérer nom et prénom

let getProjectRows(range: ExcelRange) =
    [ for cell in range do
          (cell.Value |> string, cell.Start.Row) ]
    |> Map.ofList

let generateExcelFromTemplate path date (timeEntries: MyTimeEntry list) =
    let savePath = Path.Combine(path, $"TS-{date:yyyyMM}-Delcoigne-Vincent-WIP.xlsx")

    let package = new ExcelPackage("Timesheet-Template-v10.xlsx")
    package.Workbook.Worksheets["Configuration"].Cells["D13"].Value <- date

    let prestations = package.Workbook.Worksheets["Prestations"]

    let map = prestations.Cells["C:C"] |> getProjectRows

    let dates = date |> generateDates |> Seq.indexed
    let projects = (timeEntries |> List.groupBy (_.ProjectName) |> List.sort)
    
    for projectName, timeEntries in projects do
        for col, date in dates do
            timeEntries
            |> List.tryFind (fun te -> te.Date = date)
            |> Option.iter (fun item ->
                match (map |> Map.tryFind projectName) with
                | None -> ()
                | Some row -> prestations.Cells[row, col + 4].Value <- item.Duration)

    package.SaveAs(FileInfo(savePath))

    savePath
