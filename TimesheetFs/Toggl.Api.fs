module TogglApi

open System
open System.Threading
open Toggl.Api

let getMe(client: TogglClient) =
    async {
        let! me = client.Me.GetAsync(false, CancellationToken.None) |> Async.AwaitTask
        return me
    }

let getProjects(client: TogglClient) wid =
    async {
        let! projects = client.Projects.GetUsersAsync(wid, Array.Empty<int64>(), System.Nullable(), CancellationToken.None) |> Async.AwaitTask
        return projects //|> List.ofSeq
    }

let getTimeEntries (client: TogglClient) (startDate: DateTime) (endDate: DateTime) =
    async {
        let! timeEntries = client.TimeEntries.GetAsync(true, false, System.Nullable(), System.Nullable(), startDate, endDate, CancellationToken.None) |> Async.AwaitTask

        return timeEntries |> List.ofSeq
    }
