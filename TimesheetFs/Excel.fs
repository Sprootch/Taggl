module Excel

open System
open System.Diagnostics
open System.IO
open ClosedXML.Excel
open FsExcel
open Microsoft.FSharp.Core
open Types
open Common

[<Literal>]
let TimeFormat = "h \h mm"

module Color =
    let grey = XLColor.FromArgb(0, 169, 169, 169)
    let red = XLColor.FromArgb(0, 255, 0, 0)

let generateExcel (path: string) (date: System.DateTime) timeEntries =
    let savePath = Path.Combine(path, $"TS-{date:yyyyMM}.xlsx")
    let dates = date |> generateDates |> Seq.toList
    let projects = (timeEntries |> List.groupBy (fun te -> te.ProjectName) |> List.sort)

    let excelColumns =
        [ "AA"; "AB"; "AC"; "AD"; "AE"; "AF" ]
        |> List.append ([ 'B' .. 'Z' ] |> List.map string)

    [ Go(Indent 2)
      for date in dates do
          Cell
              [ String(date.ToString("dd/MM"))
                CellSize(ColWidth 08)
                FontEmphasis Bold
                if (date |> isWeekend) then
                    BackgroundColor Color.grey ]

      Go NewRow
      Go(Indent 1)

      for projectName, te in projects do
          Cell
              [ String projectName
                CellSize(ColWidth 25)
                FontEmphasis Bold
                if projectName = Timesheet.NoProject then
                    FontColor Color.red ]

          for date in dates do
              if (date |> isWeekend) then
                  Cell [ BackgroundColor Color.grey ]
              else
                  match te |> List.tryFind (fun te -> te.Date = date) with
                  | None -> Cell []
                  | Some item -> Cell [ TimeSpan item.Duration; FormatCode TimeFormat ]

          Go NewRow

      Go(Indent 2)
      // Empty line before sum
      for date in dates do
          if (date |> isWeekend) then
              Cell [ BackgroundColor Color.grey ]
          else
              Cell []
      Go NewRow
      Go(Indent 2)

      FreezePanes FirstColumn

      for idx, date in dates |> List.indexed do
          if (date |> isWeekend) then
              Cell [ BackgroundColor Color.grey ]
          else
              let duration =
                  timeEntries
                  |> List.filter (fun te -> te.Date = date)
                  |> List.sumBy (fun te -> te.Duration.TotalSeconds)
                  |> TimeSpan.FromSeconds

              if (duration = TimeSpan.Zero) then
                  Cell []
              else
                  let column = excelColumns[idx]
                  let sumEnd = 1 + (projects |> List.length)

                  Cell
                      [ FormulaA1 $"=SUM({column}2:{column}{sumEnd})"
                        FormatCode TimeFormat
                        FontEmphasis Bold ] ]
    |> Render.AsFile(savePath)

    savePath
