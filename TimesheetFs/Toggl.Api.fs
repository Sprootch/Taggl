module TogglApi

open System
open System.Threading
open Toggl.Api

let getMyProjects (client: TogglClient) =
    async {
        let! projects =
            client.Me.GetProjectsAsync(false, System.Nullable(), CancellationToken.None)
            |> Async.AwaitTask

        return projects |> List.ofSeq
    }

let getTimeEntries (client: TogglClient) startDate endDate =
    async {
        let! timeEntries =
            client.TimeEntries.GetAsync(
                true,
                true,
                System.Nullable(),
                System.Nullable(),
                DateTimeOffset(startDate),
                DateTimeOffset(endDate),
                CancellationToken.None
            )
            |> Async.AwaitTask

        return timeEntries |> List.ofSeq
    }
