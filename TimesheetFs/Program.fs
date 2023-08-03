open System
open System.Globalization
open Toggl.Api
open Types

let client = TogglClient("77775ba928442e3ea39bcb4258a52710")

let projects = TogglApi.getProjects client |> Async.RunSynchronously

let getTimeEntries(date: DateTime) =
    let startDate = DateTime(date.Year, date.Month, 1)
    let endDate = startDate.AddMonths(1).AddDays(-1)

    TogglApi.getTimeEntries client startDate endDate |> Async.RunSynchronously

let parseDuration(duration: int64) = if (duration < 0) then 0L else duration

let parseDate(dateStr: string) =
    DateOnly.FromDateTime(DateTime.Parse(dateStr, CultureInfo.InvariantCulture))

let tryFindProject(id: int64) =
    projects
    |> List.tryFind (fun prj -> prj.Id = id)
    |> Option.map (fun prj -> prj.Name)

let timeEntries =
    getTimeEntries (DateTime(2023, 7, 1))
    // |> List.filter (fun te -> te.ProjectId.HasValue)
    |> List.map (fun te ->
        { Date = te.Start |> parseDate
          ProjectName =
            te.ProjectId
            |> Option.ofNullable
            |> Option.bind tryFindProject
            |> Option.defaultValue "No Project"

          Duration =
            te.Duration
            |> Option.ofNullable
            |> Option.map parseDuration
            |> Option.defaultValue 0L
          Desc = te.Description })

let grp =
    timeEntries
    |> List.groupBy (fun te -> te.Date, te.ProjectName)
    |> List.map (fun (key, list) ->
        { Date = fst key
          Project = snd key 
          Duration = TimeSpan.FromSeconds(list |> List.sumBy (fun te -> te.Duration |> float)) })

grp |> List.iter (fun te -> printfn $"{te.Date} - {te.Project} : {te.Duration}")
