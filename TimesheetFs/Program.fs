open Excel
open FSharp.SystemCommandLine
open Microsoft.Extensions.Configuration
open Spectre.Console
open System
open System.Globalization
open System.IO
open Timesheet
open Toggl.Api

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
let getSpinner =
    match DateTime.Today.Month with
    | 1 | 12 -> Spinner.Known.Christmas
    | _ -> Spinner.Known.BouncingBar

let generate(dateMaybe: DateTime option, outputDirMaybe: string option) =
    let outputDir = defaultArg outputDirMaybe @"C:\temp"
    let date = defaultArg dateMaybe (DateTime.Today.AddMonths(-1))
    let startDate = DateTime(date.Year, date.Month, 1)
    let generateExcel = generateExcel outputDir startDate

    AnsiConsole.MarkupLine($"""Generating Timesheet for {date.ToString("MMMM", CultureInfo.InvariantCulture)}""")

    let status = AnsiConsole.Status()
    status.SpinnerStyle <- Style.Parse("blue")
    status.Spinner <- getSpinner

    status.Start(
        "Generating Timesheet",
        (fun ctx ->
            ctx.Status <- "Fetching time entries from [bold red]Toggl[/]..."
            let te = date |> getTimeEntries
            // Threading.Thread.Sleep 3000
            ctx.Status <- "Generating [bold green]Excel[/] file..."
            // Threading.Thread.Sleep 3000
            let excel = te |> generateExcel
            excel |> openFile)
    )

    AnsiConsole.MarkupLine($"File generated in {outputDir}")
// Console.ReadKey() |> ignore

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
