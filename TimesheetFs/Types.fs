module Types

open System

type FindMeAName =
    { Date: DateOnly
      Project: string
      Duration: TimeSpan }

type MyTimeEntry =
    { Date: DateOnly
      ProjectId: int64 
      Duration: TimeSpan }
type MyTimeEntry2 =
    { Date: DateOnly
      ProjectName: string
      Duration: TimeSpan }
