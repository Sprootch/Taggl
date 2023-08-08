open System
open System.Globalization
open System.IO
open Microsoft.Extensions.Configuration
open Toggl.Api
open FSharp.SystemCommandLine
open Timesheet
open Excel

let settings =
    ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false)
        .Build()
        
let client = TogglClient(settings["Toggl:ApiKey"])
let getTimeEntries = getTimeEntries client

// TODO:
// have an Excel SUM
// Refacto ts generation.
// Verbose param to debug print ?
// Spectre.Console

let generate(dateMaybe: DateTime option, outputDirMaybe: string option) =
    let outputDir = defaultArg outputDirMaybe @"C:\temp"
    let date = defaultArg dateMaybe (DateTime.Today.AddMonths(-1))
    let startDate = DateTime(date.Year, date.Month, 1)
    
    printfn $"""Generating Timesheet for {date.ToString("MMMM", CultureInfo.InvariantCulture)} in {outputDir} ..."""
    
    getTimeEntries date |> generateExcel outputDir startDate |> openFile


[<EntryPoint>]
let main argv =
    rootCommand argv {
        description "Generates an Actiris Timesheet"

        inputs (
            Input.OptionMaybe<DateTime>([ "--date"; "-d" ], "The timesheet date"),
            Input.OptionMaybe<string>([ "--output"; "-o" ], "The output directory")
        )

        setHandler generate
    }
