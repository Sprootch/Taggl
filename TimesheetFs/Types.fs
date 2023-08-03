module Types

open System

type FindMeAName =
    { Date: DateOnly
      Project: string
      Duration: TimeSpan }

type MyTimeEntry =
    { Date: DateOnly
      ProjectName : string
      Duration: int64 
      Desc: string }

