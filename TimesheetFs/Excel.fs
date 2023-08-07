module Excel

open System
open System.Diagnostics
open System.IO
open ClosedXML.Excel
open FsExcel
open Microsoft.FSharp.Core
open Types

[<Literal>]
let TimeFormat = "h \h mm"

let private IsWeekend(date: DateOnly) =
    match date.DayOfWeek with
    | DayOfWeek.Saturday
    | DayOfWeek.Sunday -> true
    | _ -> false

let generateDates(startDate: System.DateTime) =
    let endDate = startDate.AddMonths(1).AddDays(-1)

    startDate
    |> Seq.unfold (fun date ->
        if date <= endDate then
            Some(date, date.AddDays(1.0))
        else
            None)
    |> Seq.map DateOnly.FromDateTime

let generateExcel (path: string) (date: System.DateTime) (timeEntries: MyTimeEntry2 list) =
    let grey = XLColor.FromArgb(0, 169, 169, 169)
    let savePath = Path.Combine(path, $"TS-{date:yyyyMM}.xlsx")

    [ Go(Indent 2)
      for day in generateDates date do
          Cell
              [ String(day.ToString("dd/MM"))
                CellSize(ColWidth 10)
                FontEmphasis Bold
                if (day |> IsWeekend) then
                    BackgroundColor grey ]

      Go NewRow
      Go(Indent 1)

      for projectName, te in (timeEntries |> List.groupBy (fun te -> te.ProjectName) |> List.sort) do
          Cell [ String projectName; CellSize(ColWidth 25); FontEmphasis Bold ]

          for day in generateDates date do
              if (day |> IsWeekend) then
                  Cell [ BackgroundColor grey ]
              else
                  match te |> List.tryFind (fun te -> te.Date = day) with
                  | None -> Cell []
                  | Some item -> Cell [ TimeSpan item.Duration; FormatCode TimeFormat ]

          Go NewRow

      Go(Indent 2)
      // Empty line
      for day in generateDates date do
          if (day |> IsWeekend) then
              Cell [ BackgroundColor grey ]
          else
              Cell []
      Go NewRow
      Go(Indent 2)

      for day in generateDates date do
          if (day |> IsWeekend) then
              Cell [ BackgroundColor grey ]
          else
              let duration =
                  timeEntries
                  |> List.filter (fun te -> te.Date = day)
                  |> List.sumBy (fun te -> te.Duration.TotalSeconds)
                  |> TimeSpan.FromSeconds

              if (duration = TimeSpan.Zero) then
                  Cell []
              else
                  Cell [ TimeSpan duration; FormatCode TimeFormat ] ]
    |> Render.AsFile(savePath)

    savePath

let openFile(filename: string) =
    let psi = ProcessStartInfo(filename)
    psi.UseShellExecute <- true
    let proc = Process.Start(psi)
    proc.WaitForExit()
