module TogglApi

open System
open System.Globalization
open Toggl.Api
open Toggl.Api.QueryObjects
open Types

let getProjects(client: TogglClient) =
    async {
        let! projects = client.Projects.ListAsync() |> Async.AwaitTask
        return projects |> List.ofSeq
    }

let getTimeEntries (client: TogglClient) (startDate: DateTime) (endDate: DateTime) =
    let parseDuration(duration: int64) = if (duration < 0) then 0L else duration

    let parseDate(dateStr: string) =
        DateOnly.FromDateTime(DateTime.Parse(dateStr, CultureInfo.InvariantCulture))

    async {
        let param = TimeEntryParams(StartDate = startDate, EndDate = endDate)

        let! timeEntries = client.TimeEntries.GetAllAsync(param) |> Async.AwaitTask

        return
            timeEntries
            |> List.ofSeq
            |> List.map (fun te ->
                { Date = te.Start |> parseDate
                  ProjectId = te.ProjectId |> Option.ofNullable
                  Duration =
                    te.Duration
                    |> Option.ofNullable
                    |> Option.map parseDuration
                    |> Option.defaultValue 0L })
    }
