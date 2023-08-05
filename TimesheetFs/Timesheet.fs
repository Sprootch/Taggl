module Timesheet

open System
open System.Globalization
open Toggl.Api
open Toggl.Api.DataObjects
open Types

let private tryFindProject (projects: Project list) (id: int64) =
    projects
    |> List.tryFind (fun prj -> prj.Id = id)
    |> Option.map (fun prj -> prj.Name)

let parseDuration(duration: int64) = if (duration < 0) then 0L else duration

let parseDate(dateStr: string) =
    DateOnly.FromDateTime(DateTime.Parse(dateStr, CultureInfo.InvariantCulture))

let getTimeEntries (client: TogglClient) (date: DateTime) =
    let startDate = DateTime(date.Year, date.Month, 1)
    let endDate = startDate.AddMonths(1).AddDays(-1)

    let projects = TogglApi.getProjects client |> Async.RunSynchronously

    let timeEntries =
        TogglApi.getTimeEntries client startDate endDate |> Async.RunSynchronously

    timeEntries
    // |> List.map (fun te ->
    //     { Date = parseDate(te.Start)
    //       ProjectName = "TODO"//te.ProjectId
    //       Duration = TimeSpan.FromSeconds(te.Duration |> float) }
    //     : MyTimeEntry2)
// Aggréger les durées..
// Consolider avec le projet
// Retourner un MyTimeEntry



// let private timeEntries =
//     getTimeEntries (DateTime(2023, 7, 1))
//     // |> List.filter (fun te -> te.ProjectId.HasValue)
//     |> List.map (fun te ->
//         { Date = te.Start |> parseDate
//           ProjectName = "TODO"
//             // te.ProjectId
//             // |> Option.ofNullable
//             // |> Option.bind tryFindProject
//             // |> Option.defaultValue "No Project"
//
//           Duration =
//             te.Duration
//             |> Option.ofNullable
//             |> Option.map parseDuration
//             |> Option.defaultValue 0L
//           Desc = te.Description })
// let rec aggregateDuration (dict:Map<DateOnly * string, int64>) (te:MyTimeEntry list) =
//     match te with
//     | [] -> dict
//     | head::tail ->
//         let key = (head.Date, head.ProjectName)
//         match dict |> Map.tryFind (head.Date, head.ProjectName) with
//         | None -> aggregateDuration (dict |> Map.add key head.Duration) tail
//         | Some value -> aggregateDuration (dict |> Map.add key (value + head.Duration)) tail
// if (dict |> Map.exists (fun k v -> k = head.Date))
// then
//     printfn "found %A" head.Date
//     aggregateDuration (dict |> Map.add head.Date 0L) tail
// else
//     aggregateDuration (dict |> Map.add head.Date head.Duration) tail

// let z = timeEntries |> aggregateDuration Map.empty
// z |> Map.iter (fun k v -> printfn $"{k} : {TimeSpan.FromSeconds(v |> float)}")
//
// let zz = z |> Map.toList
// printfn "%A" zz

// let grp =
//     timeEntries
//     |> List.groupBy (fun te -> te.Date, te.ProjectName)
//     |> List.map (fun (key, list) ->
//         { Date = fst key
//           Project = snd key
//           Duration = TimeSpan.FromSeconds(list |> List.sumBy (fun te -> te.Duration |> float)) })
//
// grp |> List.iter (fun te -> printfn $"{te.Date} - {te.Project} : {te.Duration}")
