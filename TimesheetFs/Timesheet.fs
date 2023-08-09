module Timesheet

open System
open System.Globalization
open Toggl.Api
open Toggl.Api.DataObjects
open Types

let private valueOrDefault(nullable: Nullable<int64>) =
    if nullable.HasValue then nullable.Value else 0L

let private tryFindProject (projects: Project list) (id: int64 option) =
    projects
    |> List.tryFind (fun prj -> (prj.Id |> Option.ofNullable) = id)
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
    |> Map.map (fun _ duration -> duration |> roundSeconds)
    |> Map.toList

let x (projects: Project list) (te: TimeEntry list) =
    let grp = te |> List.groupBy (fun te -> te.ProjectId)

    for prj, te in grp do
        let name =
            prj
            |> Option.ofNullable
            |> tryFindProject projects
            |> Option.defaultValue "No Project"

        printfn "%s" name

        let dates =
            te |> List.groupBy (fun te -> te.Start |> DateTime.Parse |> (fun d -> d.Date))

        for date, te in dates do
            printfn "%A" date

            let duration =
                te
                |> List.sumBy (fun te -> te.Duration |> Option.ofNullable |> Option.defaultValue 0)

            let zz = duration |> float |> TimeSpan.FromSeconds
            printfn "%A" zz

    []

let getTimeEntries (client: TogglClient) (date: DateTime) =
    let startDate = DateTime(date.Year, date.Month, 1)
    let endDate = startDate.AddMonths(1).AddDays(-1)

    let projects = TogglApi.getProjects client |> Async.RunSynchronously

    let timeEntries =
        TogglApi.getTimeEntries client startDate endDate |> Async.RunSynchronously

    let res = x projects timeEntries

    []
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
