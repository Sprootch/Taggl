module Common

open System

let firstDayOfMonth (date:DateTime) =
    DateTime(date.Year, date.Month, 1)

