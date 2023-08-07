open System
open Toggl.Api
open Timesheet
open Excel

let client = TogglClient("77775ba928442e3ea39bcb4258a52710")

let getTimeEntries = getTimeEntries client

// TODO:
// take first day of last month by default.
// Takes an optional output dir.
// Verbose to debug print ?
// No sum for weekends.
// Refacto ts generation.
let date = DateTime(2023, 6, 1)
let timeEntries = getTimeEntries date

timeEntries
|> generateExcel "C:\\temp" date
|> openFile

// timeEntries
// |> List.iter (fun te ->
//     printfn "%A" te.Date
//     printfn "%s : %A" te.ProjectName te.Duration)
