module TogglApi

open System
open Toggl.Api
open Toggl.Api.QueryObjects

let getProjects(client: TogglClient) =
    async {
        let! projects = client.Projects.GetAsync() |> Async.AwaitTask
        return projects |> List.ofSeq
    }

let getTimeEntries (client: TogglClient) (startDate: DateTime) (endDate: DateTime) =
    async {
        let param = TimeEntryParams(StartDate = startDate, EndDate = endDate)

        let! timeEntries = client.TimeEntries.GetAsync(false, false,  startDate) |> Async.AwaitTask

        return timeEntries |> List.ofSeq
    }
