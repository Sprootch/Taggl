module Types

open System

type MyTimeEntry =
    { Date: DateOnly
      ProjectName: string
      Duration: TimeSpan }
