module Common

open System
open System.Diagnostics

let firstDayOfMonth(date: DateTime) = DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Local)

let lastDayOfMonth(date: DateTime) = date.AddMonths(1).AddSeconds(-1)

let isWeekend(date: DateOnly) =
    match date.DayOfWeek with
    | DayOfWeek.Saturday
    | DayOfWeek.Sunday -> true
    | _ -> false

let generateDaysOfMonth(startDate: DateTime) =
    let endDate = startDate |> lastDayOfMonth |> (_.Date)

    startDate
    |> Seq.unfold (fun date ->
        if date <= endDate then
            Some(date, date.AddDays(1.0))
        else
            None)
    |> Seq.map DateOnly.FromDateTime

let round(ts: TimeSpan) =
    TimeSpan.FromMinutes(Math.Round(ts.TotalMinutes / 15., 0, MidpointRounding.ToEven) * 15.)

let roundSeconds(ts: TimeSpan) =
    if (ts.Seconds <= 30) then
        ts.Subtract(TimeSpan.FromSeconds(ts.Seconds |> int64))
    else
        ts.Add(TimeSpan.FromSeconds((60 - ts.Seconds) |> float))

let roundHours(ts: TimeSpan) =
    if (ts.Hours = 8) then
        TimeSpan.FromHours(8)
    else if (ts.Add(TimeSpan.FromMinutes(10L)).Hours = 8) then
        TimeSpan.FromHours(8)
    else
        ts

let openFile filename =
    let psi = ProcessStartInfo(filename)
    psi.UseShellExecute <- true
    Process.Start(psi).WaitForExit()
