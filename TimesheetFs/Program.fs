open System
open System.Globalization
open System.IO
open Microsoft.Extensions.Configuration
open Toggl.Api
open FSharp.SystemCommandLine
open Timesheet
open Excel
open Spectre.Console
open FsSpectre

let settings =
    ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false)
        .Build()

let client = TogglClient(settings["Toggl:ApiKey"])
let getTimeEntries = getTimeEntries client

// TODO:
// Try to go with real Actiris template.
// Verbose param to debug print ?
// Spectre.Console

let generate(dateMaybe: DateTime option, outputDirMaybe: string option) =
    let outputDir = defaultArg outputDirMaybe @"C:\temp"
    let date = defaultArg dateMaybe (DateTime.Today.AddMonths(-1))
    let startDate = DateTime(date.Year, date.Month, 1)
    let generateExcel = generateExcel outputDir startDate

    AnsiConsole.MarkupLine($"""Generating Timesheet for {date.ToString("MMMM", CultureInfo.InvariantCulture)} in {outputDir} ...""")
    let status = AnsiConsole.Status()
    status.Spinner <- Spinner.Known.Star
    status.SpinnerStyle <- Style.Parse("green")
    status.Start("desc", (fun ctx ->
            ctx.Status <- "Fetching time entries from Toggl"
            let te = date |> getTimeEntries
            // Threading.Thread.Sleep 1000
            ctx.Status <- "Generating Excel file"
            te |> generateExcel |> openFile
            ))
    AnsiConsole.MarkupLine("Done !")

// status {
//     label $"""Generating Timesheet for {date.ToString("MMMM", CultureInfo.InvariantCulture)} in {outputDir} ..."""
//     date |> getTimeEntries |> generateExcel |> openFile
// } |> AnsiConsole.Write
// printfn $"""Generating Timesheet for {date.ToString("MMMM", CultureInfo.InvariantCulture)} in {outputDir} ..."""


[<EntryPoint>]
let main argv =
    if String.IsNullOrWhiteSpace(settings["Toggl:ApiKey"]) then
        printfn "Please provide the Toggl api key in appsettings.json"
        exit -1

    rootCommand argv {
        description "Generates an Actiris Timesheet"

        inputs (
            Input.OptionMaybe<DateTime>([ "--date"; "-d" ], "The timesheet date. By default previous month"),
            Input.OptionMaybe<string>([ "--output"; "-o" ], "The output directory. By default C:\\temp")
        )

        setHandler generate
    }
