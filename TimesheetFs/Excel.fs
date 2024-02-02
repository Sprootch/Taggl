module Excel

open OfficeOpenXml
open System.IO
open Types
open Common

// todo : add dynamically the projects.
// gérer autre

let getProjectRows(range: ExcelRange) =
    range
    |> Seq.map (fun cell -> (cell.Value |> string, cell.Start.Row))
    |> Map.ofSeq

let findProjectCode (package: ExcelPackage) (name: string) =
    let projectsSheet = package.Workbook.Worksheets["Codes projet"]
    let range = projectsSheet.Cells["B:B"]

    range
    |> Seq.tryFind (fun cell -> cell.Value = name)
    |> Option.map (fun cell -> projectsSheet.Cells[cell.Start.Row, 1].Value |> string)

let generateExcel path date timeEntries =
    let package = new ExcelPackage("Timesheet-Template-v10.xlsx")
    package.Workbook.Worksheets["Configuration"].Cells["D13"].Value <- date

    let prestations = package.Workbook.Worksheets["Prestations"]

    let days = date |> generateDaysOfMonth |> Seq.indexed

    let mutable row = 7

    let projectCodes =
        timeEntries
        |> List.groupBy (_.ProjectName)
        |> List.map (fun (name, _) -> (name, findProjectCode package name))

    for name, code in projectCodes do
        match code with
        | None ->
            prestations.Cells[row, 2].Value <- "AUTRE"
            prestations.Cells[row, 3].Value <- name
        | Some code -> prestations.Cells[row, 2].Value <- code

        row <- row + 1

    // prestations.Cells["C:C"]
    // |> getProjectRows
    // |> Map.iter (fun project row ->
    //     for col, date in days do
    //         timeEntries
    //         |> List.tryFind (fun te -> te.Date = date && te.ProjectName = project)
    //         |> Option.iter (fun timeEntry -> prestations.Cells[row, col + 4].Value <- timeEntry.Duration))

    let savePath =
        Path.Combine(path, $"TS-{date:yyyyMM}-Delcoigne-Vincent.xlsx") |> FileInfo

    package.SaveAs(savePath)

    savePath
