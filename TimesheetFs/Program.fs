module Program

open FSharp.SystemCommandLine
open Microsoft.Extensions.Configuration
open Spectre.Console
open System
open System.Globalization
open System.IO
open Timesheet
open Email
open Common
open Toggl.Api

// TODO:
// - Set :Thread & ThreadUI
// translation
let settings =
    ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false)
        .AddUserSecrets("e5ec099c-f0d8-49cf-8a1c-e3f0c5715645")
        .Build()

// let firstName = settings["Firstname"]
// let lastName = settings["Lastname"]
let client = new TogglClient(TogglClientOptions(Key = settings["Toggl:ApiKey"]))
let getTimeEntries = getTimeEntries client

let generate (lastName: string, firstName: string, dateMaybe: DateTime option, outputDirMaybe: string option) =
    let outputDir = defaultArg outputDirMaybe Environment.CurrentDirectory
    let date = defaultArg dateMaybe (DateTime.Today.AddMonths(-1))

    let outputFile =
        Path.Combine(outputDir, $"TS-{date:yyyyMM}-{lastName}-{firstName}.xlsx")

    let generateExcel = Excel.generateExcel outputFile date (lastName, firstName)

    AnsiConsole.MarkupLine(
        $"""Generating Timesheet for {date.ToString("MMMM", CultureInfo.InvariantCulture)} {date.Year}"""
    )

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
            ctx.Status <- "Generating [bold green]Excel[/] file"
            timeEntries |> generateExcel
            ctx.Status <- "Update your timesheet if needed. [bold dodgerblue1]Outlook[/] will be opened afterwards."
            outputFile |> openFile
            openEmail date outputFile)
    )

    AnsiConsole.MarkupLine("Done 🙂")

[<EntryPoint>]
let main argv =
    if String.IsNullOrWhiteSpace(settings["Toggl:ApiKey"]) then
        printfn "Please add the Toggl api key (Toggl:ApiKey) in user secrets"
        Console.ReadKey() |> ignore
        exit -1

    rootCommand argv {
        description "Generates an Actiris Timesheet"

        inputs (
            Input.Argument<string>("Lastname", "Enter your lastname"),
            Input.Argument<string>("Firstname", "Enter your firstname"),
            Input.OptionMaybe<DateTime>([ "--date"; "-d" ], "The timesheet date. By default previous month"),
            Input.OptionMaybe<string>([ "--output"; "-o" ], "The output directory. By default C:\\temp")
        )

        setHandler generate
    }
