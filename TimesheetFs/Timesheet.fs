module Timesheet

open System
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
        |> Option.map (_.Name)
        |> Option.defaultValue NoProject

    let getDuration(te: TimeEntry list) =
        te |> List.sumBy (_.Duration) |> toTimespan

    timeEntries
    |> List.filter (fun te -> te.Stop.HasValue |> not)
    |> List.groupBy (fun te -> te.ProjectId |> valueOrDefault)
    |> List.collect (fun (prjId, te) ->
        te
        |> List.groupBy (fun te ->
            te.Start)
            // |> (fun date -> DateTime.Parse(date, CultureInfo.InvariantCulture))
            // |> (_.Date))
        |> List.map (fun (date, te) ->
            { ProjectName = getProjectName prjId
              Date = DateOnly.FromDateTime(date.Value.Date)
              Duration = getDuration te }))

let getTimeEntries client date =
    let startDate = date |> firstDayOfMonth
    let endDate = startDate |> lastDayOfMonth

    let me = TogglApi.getMe client |> Async.RunSynchronously
    let projects = TogglApi.getProjects client me.DefaultWorkspaceId |> Async.RunSynchronously

    let timeEntries = TogglApi.getTimeEntries client startDate endDate |> Async.RunSynchronously
    ()
    // timeEntries |> transform projects
