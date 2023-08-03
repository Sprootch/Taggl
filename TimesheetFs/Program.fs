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

let rec aggregateDuration (dict:Map<DateOnly, int64>) (te:MyTimeEntry list) =
    match te with
    | [] -> dict
    | head::tail ->
        match dict |> Map.tryFind head.Date with
        | None -> aggregateDuration (dict |> Map.add head.Date head.Duration) tail
        | Some value -> aggregateDuration (dict |> Map.add head.Date (value + head.Duration)) tail
        // if (dict |> Map.exists (fun k v -> k = head.Date))
        // then
        //     printfn "found %A" head.Date
        //     aggregateDuration (dict |> Map.add head.Date 0L) tail
        // else
        //     aggregateDuration (dict |> Map.add head.Date head.Duration) tail
    
let z = timeEntries |> aggregateDuration Map.empty
z |> Map.iter (fun k v -> printfn "%A : %A" k (TimeSpan.FromSeconds(v |> float)))
// printfn "%A" z

// let grp =
//     timeEntries
//     |> List.groupBy (fun te -> te.Date, te.ProjectName)
//     |> List.map (fun (key, list) ->
//         { Date = fst key
//           Project = snd key 
//           Duration = TimeSpan.FromSeconds(list |> List.sumBy (fun te -> te.Duration |> float)) })
//
// grp |> List.iter (fun te -> printfn $"{te.Date} - {te.Project} : {te.Duration}")
