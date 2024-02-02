module Program

open FSharp.SystemCommandLine
open Microsoft.Extensions.Configuration
open Spectre.Console
open System
open System.Globalization
open System.IO
open Timesheet
open Common

let settings =
    ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false)
        .AddUserSecrets("e5ec099c-f0d8-49cf-8a1c-e3f0c5715645")
        .Build()

let client = Toggl.Api.TogglClient(settings["Toggl:ApiKey"])
let getTimeEntries = getTimeEntries client

let generate(dateMaybe: DateTime option, outputDirMaybe: string option) =
    let outputDir = defaultArg outputDirMaybe @"C:\temp"
    let date = defaultArg dateMaybe (DateTime.Today.AddMonths(-1))

    let generateExcel =
        EPPlusExcel.generateExcelFromTemplate outputDir (date |> firstDayOfMonth)

    AnsiConsole.MarkupLine($"""Generating Timesheet for {date.ToString("MMMM", CultureInfo.InvariantCulture)}""")

    let status = AnsiConsole.Status()
    status.SpinnerStyle <- Style.Parse("blue")

    status.Spinner <-
        match DateTime.Today.Month with
        | 1
        | 12 -> Spinner.Known.Christmas
        | _ -> Spinner.Known.BouncingBar

    status.Start(
        "Fetching time entries from [bold red]Toggl[/]",
        (fun ctx ->
            let timeEntries = date |> getTimeEntries
            // Threading.Thread.Sleep 3000
            ctx.Status <- "Generating [bold green]Excel[/] file"
            // Threading.Thread.Sleep 3000
            let excel = timeEntries |> generateExcel
            excel |> openFile)
    )

    AnsiConsole.MarkupLine($"File generated in {outputDir}")

[<EntryPoint>]
let main argv =
    if String.IsNullOrWhiteSpace(settings["Toggl:ApiKey"]) then
        printfn "Please add the Toggl api key (Toggl:ApiKey) in user secrets"
        Console.ReadKey() |> ignore
        exit -1

    rootCommand argv {
        description "Generates an Actiris Timesheet"

        inputs (
            Input.OptionMaybe<DateTime>([ "--date"; "-d" ], "The timesheet date. By default previous month"),
            Input.OptionMaybe<string>([ "--output"; "-o" ], "The output directory. By default C:\\temp")
        )

        setHandler generate
    }
