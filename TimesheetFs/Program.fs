module Program

open FSharp.SystemCommandLine
open Microsoft.Extensions.Configuration
open OfficeOpenXml
open Spectre.Console
open System
open System.Globalization
open System.IO
open Timesheet
open Email
open Common
open Toggl.Api

// TODO:
// Si c'est projet autre, prendre le libellé du pointage;

let askOpenEmail recipients date outputFile  =
    let confirmation =
        AnsiConsole.Prompt(
            TextPrompt<bool>("Open [bold dodgerblue1]Outlook[/] ?")
                .AddChoice(true)
                .AddChoice(false)
                .DefaultValue(true)
                .WithConverter(fun choice -> if choice then "y" else "n")
        )

    if confirmation then openEmail recipients date outputFile else ()

let generate
    (
        lastName: string,
        firstName: string,
        dateMaybe: DateTime option,
        outputDirMaybe: string option,
        forceMaybe: bool option
    ) =

    let settings =
        ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false)
            .AddUserSecrets("e5ec099c-f0d8-49cf-8a1c-e3f0c5715645")
            .Build()

    if String.IsNullOrWhiteSpace(settings["Toggl:ApiKey"]) then
        printfn "Please add the Toggl api key in appsettings.json"
        printfn "Press any key to exit..."
        Console.ReadKey() |> ignore
        exit -1

    ExcelPackage.License.SetNonCommercialPersonal($"{lastName} {firstName}")
    let outputDir = defaultArg outputDirMaybe Environment.CurrentDirectory
    let date = defaultArg dateMaybe (DateTime.Today.AddMonths(-1))
    let forceRegen = defaultArg forceMaybe false

    let outputFile =
        Path.Combine(outputDir, $"TS-{date:yyyyMM}-{lastName}-{firstName}.xlsx")

    let generateExcel = Excel.generateExcel outputFile date (lastName, firstName)

    AnsiConsole.MarkupLine(
        $"""Generating Timesheet for [bold slateblue1]{date.ToString("MMMM", CultureInfo.InvariantCulture)} {date.Year}[/]"""
    )

    let client = new TogglClient(TogglClientOptions(Key = settings["Toggl:ApiKey"]))
    let getTimeEntries = getTimeEntries client
    let recipients = settings["MailRecipients"]
    let askOpenEmail = askOpenEmail recipients date

    if File.Exists outputFile && not forceRegen then
        AnsiConsole.MarkupLine($"Reusing existing [bold green]Excel[/] file ({outputFile})")
        outputFile |> openFile
        outputFile |> askOpenEmail
        AnsiConsole.MarkupLine("Done")
    else
        AnsiConsole.MarkupLine("Fetching time entries from [bold red]Toggl[/]")
        let timeEntries = date |> getTimeEntries
        AnsiConsole.MarkupLine("Generating [bold green]Excel[/] file")
        timeEntries |> generateExcel

        AnsiConsole.MarkupLine(
            "Update your timesheet if needed. [bold dodgerblue1]Outlook[/] will be opened afterwards."
        )

        outputFile |> openFile
        outputFile |> askOpenEmail

        AnsiConsole.MarkupLine("Done")

[<EntryPoint>]
let main argv =
    rootCommand argv {
        description "Generates an Actiris Timesheet"

        inputs (
            Input.Argument<string>("Lastname", "Enter your lastname"),
            Input.Argument<string>("Firstname", "Enter your firstname"),
            Input.OptionMaybe<DateTime>([ "--date"; "-d" ], "The timesheet date. By default previous month"),
            Input.OptionMaybe<string>([ "--output"; "-o" ], "The output directory. By default the current directory"),
            Input.OptionMaybe<bool>([ "--force"; "-f" ], "Force the regeneration even if the Excel file exists.")
        )

        setHandler generate
    }
