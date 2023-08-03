open System
open System.Globalization
open Toggl.Api
open Toggl.Api.QueryObjects

type MyProject = { Id: int64; Name: string }

type FindMeAName =
    { Date: DateOnly
      Project: string
      Duration: TimeSpan }

type MyTimeEntry =
    { Date: DateOnly
      Project: MyProject option
      Duration: int64
      Desc: string }

let client = TogglClient("77775ba928442e3ea39bcb4258a52710")

let getProjects =
    async {
        let! projects = client.Projects.ListAsync() |> Async.AwaitTask
        return projects
    }

let projects = getProjects |> Async.RunSynchronously |> List.ofSeq

let getTimeEntries(date: DateTime) =
    let endDate = date.AddMonths(1).AddDays(-1)

    async {
        let param = TimeEntryParams(StartDate = date, EndDate = endDate)

        let! timeEntries = client.TimeEntries.GetAllAsync(param) |> Async.AwaitTask
        return timeEntries
    }

let timeEntries = (getTimeEntries (DateTime(2023, 7, 1))) |> Async.RunSynchronously

let valueOrDefault(nullable: Nullable<int64>) =
    if nullable.HasValue then nullable.Value else 0

// let parseDuration(duration: int64) =
//     if (duration < 0) then
//         TimeSpan.Zero
//     else
//         TimeSpan.FromSeconds(duration |> float)
let parseDuration(duration: int64) = if (duration < 0) then 0L else duration

let parseDate(dateStr: string) =
    DateOnly.FromDateTime(DateTime.Parse(dateStr, CultureInfo.InvariantCulture))

let parseProject(id: int64) =
    projects
    |> List.tryFind (fun prj -> prj.Id = id)
    |> Option.map (fun prj ->
        { Id = prj.Id |> valueOrDefault
          Name = prj.Name })

let x =
    timeEntries
    |> List.ofSeq
    |> List.filter (fun te -> te.ProjectId.HasValue)
    |> List.map (fun te ->
        { Date = te.Start |> parseDate
          Project = te.ProjectId |> valueOrDefault |> parseProject
          Duration = te.Duration |> valueOrDefault |> parseDuration
          Desc = te.Description })

let grp =
    x
    |> List.groupBy (fun te -> te.Date, te.Project)
    |> List.map (fun (key, list) ->
        { Date = fst key
          Project = snd key |> Option.map (fun prj -> prj.Name) |> Option.defaultValue "No Project"
          Duration = TimeSpan.FromSeconds(list |> List.sumBy (fun te -> te.Duration |> float)) })

grp |> List.iter (fun te -> printfn $"{te.Date} - {te.Project} : {te.Duration}")
