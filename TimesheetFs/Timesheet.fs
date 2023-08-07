module Timesheet

open System
open System.Globalization
open Toggl.Api
open Toggl.Api.DataObjects
open Types

// TODO: check how to hide methos other than private
let private valueOrDefault(nullable: Nullable<int64>) =
    if nullable.HasValue then nullable.Value else 0L

let private tryFindProject (projects: Project list) (id: int64) =
    projects
    |> List.tryFind (fun prj -> prj.Id = id)
    |> Option.map (fun prj -> prj.Name)

let private roundSeconds(ts: TimeSpan) =
    if (ts.Seconds <= 30) then
        ts.Subtract(TimeSpan.FromSeconds(ts.Seconds))
    else
        ts.Add(TimeSpan.FromSeconds((60 - ts.Seconds) |> float))

let private sumDuration timeEntries =
    let rec add dict (te: MyTimeEntry list) =
        match te with
        | [] -> dict
        | head :: tail ->
            let key = (head.Date, head.ProjectId)

            match dict |> Map.tryFind key with
            | None -> add (dict |> Map.add key head.Duration) tail
            | Some value -> add (dict |> Map.add key (value + head.Duration)) tail

    add Map.empty timeEntries
    |> Map.map(fun _ duration -> duration |> roundSeconds)
    |> Map.toList

let getTimeEntries (client: TogglClient) (date: DateTime) =
    let startDate = DateTime(date.Year, date.Month, 1)
    let endDate = startDate.AddMonths(1).AddDays(-1)

    let projects = TogglApi.getProjects client |> Async.RunSynchronously

    // TODO: in one shot
    let myTimeEntries =
        TogglApi.getTimeEntries client startDate endDate
        |> Async.RunSynchronously
        |> List.map (fun te ->
            { Date = DateOnly.FromDateTime(DateTime.Parse(te.Start, CultureInfo.InvariantCulture))
              ProjectId = te.ProjectId |> valueOrDefault
              Duration = TimeSpan.FromSeconds(te.Duration |> valueOrDefault |> float) })

    myTimeEntries
    |> sumDuration
    |> List.map (fun ((date, prjId), duration) ->
        { Date = date
          ProjectName = (tryFindProject projects prjId) |> Option.defaultValue "No Project"
          Duration = duration })
