module Common

open System
open System.Diagnostics

let firstDayOfMonth(date: DateTime) = DateTime(date.Year, date.Month, 1)

let isWeekend(date: DateOnly) =
    match date.DayOfWeek with
    | DayOfWeek.Saturday
    | DayOfWeek.Sunday -> true
    | _ -> false

let generateDates(startDate: DateTime) =
    let endDate = startDate.AddMonths(1).AddDays(-1)

    startDate
    |> Seq.unfold (fun date ->
        if date <= endDate then
            Some(date, date.AddDays(1.0))
        else
            None)
    |> Seq.map DateOnly.FromDateTime
    
let openFile(filename: string) =
    let psi = ProcessStartInfo(filename)
    psi.UseShellExecute <- true
    Process.Start(psi) |> ignore
