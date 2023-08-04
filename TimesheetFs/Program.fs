open System
open Toggl.Api
open Timesheet

let client = TogglClient("77775ba928442e3ea39bcb4258a52710")

let getTimeEntries = getTimeEntries client 

let timeEntries = getTimeEntries (DateTime(2023, 7, 1))
timeEntries |> List.iter (printfn "%A")


