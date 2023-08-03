module TogglApi

open System
open Toggl.Api
open Toggl.Api.QueryObjects

let getProjects (client:TogglClient) =
    async {
        let! projects = client.Projects.ListAsync() |> Async.AwaitTask
        return projects |> List.ofSeq
    }
    
let getTimeEntries (client:TogglClient) (startDate: DateTime) (endDate:DateTime) =
    // let endDate = date.AddMonths(1).AddDays(-1)

    async {
        let param = TimeEntryParams(StartDate = startDate, EndDate = endDate)

        let! timeEntries = client.TimeEntries.GetAllAsync(param) |> Async.AwaitTask
        return timeEntries |> List.ofSeq
    }
