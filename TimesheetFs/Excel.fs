module Excel

open System
open System.Diagnostics
open System.IO
open ClosedXML.Excel
open FsExcel
open Types

let IsWeekend(date: DateOnly) =
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

let addSumLine date =
    let columns =
        [ "AA"; "AB"; "AC"; "AD"; "AE"; "AF" ]
        |> List.append ([ 'B' .. 'Z' ] |> List.map string)

    seq {
        for c in columns do
            // if (day |> IsWeekend) then
            //     yield Cell [ String "" ]
            // else
            // let column = char (65 + 1 + idx)
            // Cell [ String $"{char(65 + 1 + idx)}{idx + 1}" ]
            yield Cell [ FormulaA1 $"=SUM({c}2:{c}5)"; FormatCode "hh:mm" ]
    }

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

      for prjName, list in (timeEntries |> List.groupBy (fun te -> te.ProjectName)) do
          Cell [ String prjName; CellSize(ColWidth 25); FontEmphasis Bold ]

          for day in generateDates date do
              match list |> List.tryFind (fun te -> te.Date = day) with
              | None ->
                  Cell
                      [ String ""
                        if (day |> IsWeekend) then
                            BackgroundColor grey ]
              | Some item ->
                  Cell
                      [ TimeSpan item.Duration
                        FormatCode "hh:mm"
                        if (day |> IsWeekend) then
                            BackgroundColor grey ]

          Go NewRow

      Go NewRow
      Go(Indent 2)

      for cell in addSumLine date do
          cell

      ]
    |> Render.AsFile(savePath)

    savePath

let openFile(filename: string) =
    let psi = ProcessStartInfo(filename)
    psi.UseShellExecute <- true
    let proc = Process.Start(psi)
    proc.WaitForExit()
