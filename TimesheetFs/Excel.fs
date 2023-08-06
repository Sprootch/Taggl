module Excel

open System
open System.IO
open FsExcel
open Types

let genDates (startDate: System.DateTime) =
    let endDate = startDate.AddMonths(1).AddDays(-1)

    startDate |> Seq.unfold
        (fun date ->
            if date <= endDate then
                Some(date, date.AddDays(1.0))
            else
                None)
        |> Seq.map (DateOnly.FromDateTime)

let generateExcel (date: System.DateTime) (timeEntries: MyTimeEntry2 list) =
    let savePath = "/home/lapin"

    [
      Go(Indent 2)
      for date in genDates date do
          Cell [ DateTime (date.ToDateTime TimeOnly.MinValue) ]
      Go NewRow
      Go(Indent 1)
      for prjName, list in (timeEntries |> List.groupBy (fun te -> te.ProjectName)) do
          Cell [ String prjName ]

          for date in genDates date do
              match list |> List.tryFind (fun te -> te.Date = date) with
              | None -> Cell [ String "" ]
              | Some item -> Cell [ TimeSpan item.Duration ]
              
          Go NewRow
          SizeAll (ColWidth 15)
    ]
    
    |> Render.AsFile(Path.Combine(savePath, $"TS_{date:yyyy_MM}.xlsx"))
