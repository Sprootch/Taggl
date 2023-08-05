module Excel

open System.IO
open FsExcel
open Types

let genDates (startDate: System.DateTime) =
    let endDate = startDate.AddMonths(1).AddDays(-1)

    Seq.unfold
        (fun date ->
            if date <= endDate then
                Some(date, date.AddDays(1.0))
            else
                None)
        startDate

let generateExcel (date: System.DateTime) (timeEntries: MyTimeEntry2 list) =
    let savePath = "/home/lapin"

    [
      Go(Indent 2)
      for date in genDates date do
          Cell [ DateTime date ]
      Go NewRow
      Go(Indent 1)
      for prjName, list in (timeEntries |> List.groupBy (fun te -> te.ProjectName)) do
          Cell [ String prjName ]

          for item in list do
              Cell [ TimeSpan item.Duration ]

          Go NewRow
          SizeAll (ColWidth 15)
    ]
    
    |> Render.AsFile(Path.Combine(savePath, $"TS_{date:yyyy_MM}.xlsx"))
