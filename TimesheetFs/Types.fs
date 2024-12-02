module Types

open System

type MyTimeEntry =
    { Date: DateOnly
      ProjectName: string
      Description: string option
      Duration: TimeSpan }
