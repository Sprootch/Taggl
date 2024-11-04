module Excel

open OfficeOpenXml
open System.IO
open Types
open Common

let private isBillable =
    function
    | "Jour férié"
    | "Autre absence"
    | "Congé ou fermeture" -> false
    | _ -> true

let setupProjects timeEntries (package: ExcelPackage) =
    let getProjectCode name =
        let projectsSheet = package.Workbook.Worksheets["Codes projet"]

        projectsSheet.Cells["B:B"]
        |> Seq.tryFind (fun cell -> cell.Value = name)
        |> Option.map (fun cell -> projectsSheet.Cells[cell.Start.Row, 1].Value |> string)
        |> Option.defaultValue "AUTRE"

    timeEntries
    |> List.map (_.ProjectName)
    |> List.distinct
    |> List.filter isBillable
    |> List.map (fun name -> (name, name |> getProjectCode))
    |> List.iteri (fun row (name, code) ->
        let prestations = package.Workbook.Worksheets["Prestations"]
        prestations.Cells[7 + row, 2].Value <- code
        prestations.Cells[7 + row, 3].Value <- name)

    package

let setupDate date (package: ExcelPackage) =
    package.Workbook.Worksheets["Configuration"].Cells["D13"].Value <- date
    package

let setupName (last, first) (package: ExcelPackage) =
    package.Workbook.Worksheets["Configuration"].Cells["D14"].Value <- last
    package.Workbook.Worksheets["Configuration"].Cells["D15"].Value <- first
    package

let addTimeEntries date timeEntries (package: ExcelPackage) =
    let prestations = package.Workbook.Worksheets["Prestations"]

    let days = date |> generateDaysOfMonth |> Seq.indexed

    prestations.Cells["C:C"] // colonne Intitulé
    |> Seq.map (fun cell -> (cell.Value |> string, cell.Start.Row))
    |> Seq.iter (fun (project, row) ->
        for col, date in days do
            timeEntries
            |> List.tryFind (fun timeEntry -> timeEntry.Date = date && timeEntry.ProjectName = project)
            |> Option.iter (fun timeEntry -> prestations.Cells[row, col + 4].Value <- timeEntry.Duration))

    package

let save path (package: ExcelPackage) = package.SaveAs(path |> FileInfo)

let generateExcel outputFile date name timeEntries =
    let date = date |> firstDayOfMonth

    new ExcelPackage("Timesheet-Template-v10.xlsx")
    |> setupDate date
    |> setupName name
    |> setupProjects timeEntries
    |> addTimeEntries date timeEntries
    |> save outputFile
