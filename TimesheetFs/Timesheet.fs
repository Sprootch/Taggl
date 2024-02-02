module Timesheet

open System
open System.Globalization
open Toggl.Api
open Toggl.Api.DataObjects
open Types
open Common

[<Literal>]
let NoProject = "! No project !"

let valueOrDefault(value: Nullable<int64>) =
    value |> Option.ofNullable |> Option.defaultValue 0

let private toTimespan = float >> TimeSpan.FromSeconds >> roundSeconds >> roundHours

let private transform (projects: Project list) (timeEntries: TimeEntry list) =
    let getProjectName(id: int64) =
        projects
        |> List.tryFind (fun prj -> prj.Id = Nullable<int64> id)
        |> Option.map (_.Name)
        |> Option.defaultValue NoProject

    let getDuration(te: TimeEntry list) =
        te |> List.sumBy (fun te -> te.Duration |> valueOrDefault) |> toTimespan

    timeEntries
    |> List.filter (fun te -> String.IsNullOrWhiteSpace(te.Stop) |> not)
    |> List.groupBy (fun te -> te.ProjectId |> valueOrDefault)
    |> List.collect (fun (prjId, te) ->
        te
        |> List.groupBy (fun te ->
            te.Start
            |> (fun date -> DateTime.Parse(date, CultureInfo.InvariantCulture))
            |> (_.Date))
        |> List.map (fun (date, te) ->
            { ProjectName = getProjectName prjId
              Date = DateOnly.FromDateTime(date)
              Duration = getDuration te }))

let getTimeEntries (client: TogglClient) (date: DateTime) =
    let startDate = date |> firstDayOfMonth
    let endDate = startDate |> lastDayOfMonth

    let projects = TogglApi.getProjects client |> Async.RunSynchronously

    let timeEntries =
        TogglApi.getTimeEntries client startDate endDate |> Async.RunSynchronously

    timeEntries |> transform projects
