open System
open Toggl.Api
open Timesheet
open Excel

let client = TogglClient("77775ba928442e3ea39bcb4258a52710")

let getTimeEntries = getTimeEntries client

// TODO: take first day of last month by default.
// TODO: Arrondir les timespans
let date = DateTime(2023, 7, 1)
let timeEntries = getTimeEntries date

timeEntries
|> generateExcel "C:\\temp" date

// timeEntries
// |> List.iter (fun te ->
//     printfn "%A" te.Date
//     printfn "%s : %A" te.ProjectName te.Duration)
