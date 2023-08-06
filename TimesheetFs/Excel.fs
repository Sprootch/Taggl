module Excel

open System
open System.IO
open ClosedXML.Excel
open FsExcel
open Types

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
    // if not (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) then
    //     LoadOptions.DefaultGraphicEngine <- new ClosedXML.Graphics.DefaultGraphicEngine("Liberation Sans")

    [ Go(Indent 2)
      for day in generateDates date do
          Cell
              [ String(day.ToString("dd/MM"))
                CellSize(ColWidth 10)
                FontEmphasis Bold
                // TODO: active pattern
                if (day.DayOfWeek = DayOfWeek.Saturday || day.DayOfWeek = DayOfWeek.Sunday) then
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
                        if (day.DayOfWeek = DayOfWeek.Saturday || day.DayOfWeek = DayOfWeek.Sunday) then
                            BackgroundColor grey ]
              | Some item ->
                  Cell
                      [ TimeSpan item.Duration
                        FormatCode "hh:mm"
                        if (day.DayOfWeek = DayOfWeek.Saturday || day.DayOfWeek = DayOfWeek.Sunday) then
                            BackgroundColor grey ]

          Go NewRow
          // TODO: ajouter une ligne de sum
      // SizeAll(ColWidth 15)
      // AutoFit AllCols
      ]
    |> Render.AsFile(Path.Combine(path, $"TS-{date:yyyyMM}.xlsx"))
