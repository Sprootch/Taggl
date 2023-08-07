open System
open System.Globalization
open Toggl.Api
open FSharp.SystemCommandLine
open Timesheet
open Excel

let client = TogglClient("77775ba928442e3ea39bcb4258a52710")
let getTimeEntries = getTimeEntries client

// TODO:
// Appsettings for Api key
// Refacto ts generation.
// Verbose param to debug print ?
// Spectre.Console


let generate(dateMaybe: DateTime option, outputDirMaybe: string option) =
    let outputDir = defaultArg outputDirMaybe @"C:\temp"
    let date = defaultArg dateMaybe (DateTime.Today.AddMonths(-1))
    let startDate = DateTime(date.Year, date.Month, 1)

    printfn $"""Generating Timesheet for {date.ToString("MMMM", CultureInfo.InvariantCulture)} in {outputDir} ..."""

    getTimeEntries date |> generateExcel outputDir startDate |> openFile

let outputDirMaybe =
    Input.OptionMaybe<string>([ "--output"; "-o" ], "The output directory")

let dateMaybe =
    Input.OptionMaybe<DateTime>([ "--date"; "-d" ], "The timesheet date")

[<EntryPoint>]
let main argv =
    rootCommand argv {
        description "Generates an Actiris Timesheet"
        inputs (dateMaybe, outputDirMaybe)
        setHandler generate
    }
