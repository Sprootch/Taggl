open System
open Toggl.Api
open Timesheet
open Excel
open FSharp.SystemCommandLine
open System.IO

let client = TogglClient("77775ba928442e3ea39bcb4258a52710")

let getTimeEntries = getTimeEntries client

// TODO:
// take first day of last month by default.
// Verbose to debug print ?
// No sum for weekends.
// Refacto ts generation.
let date = DateTime(2023, 7, 1)

let generate(outputDirMaybe: DirectoryInfo option) =
    let outputDir = defaultArg outputDirMaybe (DirectoryInfo @"C:\temp")
    printfn $"Generating Timesheet in {outputDir} ..."

    let timeEntries = getTimeEntries date
    timeEntries |> generateExcel "C:\\temp" date |> openFile

let outputDirMaybe =
    Input.OptionMaybe<DirectoryInfo>([ "--output"; "-o" ], "The output directory")

[<EntryPoint>]
let main argv =
    rootCommand argv {
        description "Generate an Excel Timesheet"
        inputs outputDirMaybe
        setHandler generate
    }

// timeEntries
// |> List.iter (fun te ->
//     printfn "%A" te.Date
//     printfn "%s : %A" te.ProjectName te.Duration)
