open Terminal.Gui
open Fun.SunUI


Application.Init()

Application.Begin(
#if DEBUG
    let dispatcher (fn: unit -> unit) = fn()
    UI.hotreload("App1.Entry.top", (fun () -> App1.Entry.top), (), dispatcher).Build(null)
#else
    top.Build(null))
#endif
)
|> ignore

Application.Run()
Application.Shutdown()