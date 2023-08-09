module Timesheet

open System
open System.Globalization
open Toggl.Api
open Toggl.Api.DataObjects
open Types

let private roundSeconds(ts: TimeSpan) =
    if (ts.Seconds <= 30) then
        ts.Subtract(TimeSpan.FromSeconds(ts.Seconds))
    else
        ts.Add(TimeSpan.FromSeconds((60 - ts.Seconds) |> float))

let x (projects: Project list) (timeEntries: TimeEntry list) =
    let getProjectName id =
        projects
        |> List.tryFind (fun prj -> prj.Id = id)
        |> Option.map (fun prj -> prj.Name)
        |> Option.defaultValue "No Project"

    let getDuration(te: TimeEntry list) =
        te
        |> List.sumBy (fun te -> te.Duration |> Option.ofNullable |> Option.defaultValue 0)
        |> float
        |> TimeSpan.FromSeconds
        |> roundSeconds

    timeEntries
    |> List.groupBy (fun te -> te.ProjectId)
    |> List.collect (fun (prjId, te) ->
        te
        |> List.groupBy (fun te ->
            te.Start
            |> (fun d -> DateTime.Parse(d, CultureInfo.InvariantCulture))
            |> (fun d -> d.Date))
        |> List.map (fun (date, te) ->
            { ProjectName = getProjectName prjId
              Date = DateOnly.FromDateTime(date)
              Duration = getDuration te }))

let getTimeEntries (client: TogglClient) (date: DateTime) =
    let startDate = DateTime(date.Year, date.Month, 1)
    let endDate = startDate.AddMonths(1).AddDays(-1)

    let projects = TogglApi.getProjects client |> Async.RunSynchronously

    let timeEntries =
        TogglApi.getTimeEntries client startDate endDate |> Async.RunSynchronously

    let res = x projects timeEntries

    res
// timeEntries
// |> List.map (fun te ->
//     { Date = DateOnly.FromDateTime(DateTime.Parse(te.Start, CultureInfo.InvariantCulture))
//       ProjectId = te.ProjectId |> valueOrDefault
//       Duration = TimeSpan.FromSeconds(te.Duration |> valueOrDefault |> float) })
// |> sumDuration
// |> List.map (fun ((date, prjId), duration) ->
//     { Date = date
//       ProjectName = (tryFindProject projects prjId) |> Option.defaultValue "No Project"
//       Duration = duration })
