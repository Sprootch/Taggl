module Excel

open OfficeOpenXml
open System.IO
open Types
open Common

let getProjectRows(range: ExcelRange) =
    [ for cell in range do
          (cell.Value |> string, cell.Start.Row) ]
    |> Map.ofList

let generateExcel path date timeEntries =
    let package = new ExcelPackage("Timesheet-Template-v10.xlsx")
    package.Workbook.Worksheets["Configuration"].Cells["D13"].Value <- date

    let prestations = package.Workbook.Worksheets["Prestations"]

    let days = date |> generateDaysOfMonth |> Seq.indexed

    prestations.Cells["C:C"]
    |> getProjectRows
    |> Map.iter (fun project row ->
        for col, date in days do
            timeEntries
            |> List.tryFind (fun te -> te.Date = date && te.ProjectName = project)
            |> Option.iter (fun timeEntry -> prestations.Cells[row, col + 4].Value <- timeEntry.Duration))

    let savePath =
        Path.Combine(path, $"TS-{date:yyyyMM}-Delcoigne-Vincent.xlsx") |> FileInfo

    package.SaveAs(savePath)

    savePath
