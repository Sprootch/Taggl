module Timesheet

open System
open Toggl.Api
open Toggl.Api.Models
open Types
open Common

[<Literal>]
let NoProject = "! No project !"

let valueOrDefault = Option.ofNullable >> Option.defaultValue 0L

let private toTimespan = float >> TimeSpan.FromSeconds >> roundSeconds >> roundHours

let private transform (projects: Project list) (timeEntries: TimeEntry list) =
    let getProjectName id =
        projects
        |> List.tryFind (fun prj -> prj.Id = id)
        |> Option.map _.Name
        |> Option.defaultValue NoProject

    let getDuration (te: TimeEntry list) =
        te |> List.sumBy _.Duration |> toTimespan

    timeEntries
    |> List.filter _.Stop.HasValue
    |> List.groupBy (fun te -> te.ProjectId |> valueOrDefault)
    |> List.collect (fun (prjId, te) ->
        te
        |> List.groupBy _.Start.Value.Date.Date
        |> List.map (fun (date, te) ->
            { ProjectName = getProjectName prjId
              Date = DateOnly.FromDateTime(date.Date)
              Duration = getDuration te }))

let getTimeEntries client date =
    let startDate = date |> firstDayOfMonth
    let endDate = startDate |> lastDayOfMonth

    let projects = TogglApi.getMyProjects client |> Async.RunSynchronously

    let timeEntries =
        TogglApi.getTimeEntries client startDate endDate |> Async.RunSynchronously

    timeEntries |> transform projects

let getTimeEntriesAsync client date =
    async {
        let startDate = date |> firstDayOfMonth
        let endDate = startDate |> lastDayOfMonth

        let! projects = TogglApi.getMyProjects client
        let! timeEntries = TogglApi.getTimeEntries client startDate endDate

        return timeEntries |> transform projects
    }

type Csharp =
    static member GetTimeEntriesAsync (client: TogglClient) date =
        Async.StartAsTask(getTimeEntriesAsync client date)
