open System
open System.IO
open Microsoft.Extensions.Configuration
open OfficeOpenXml
open Terminal.Gui.App
open Terminal.Gui.Views
open Toggl.Api

// let settings =
//     ConfigurationBuilder()
//         .SetBasePath(Directory.GetCurrentDirectory())
//         .AddJsonFile("appsettings.json", false)
//         .AddUserSecrets("e5ec099c-f0d8-49cf-8a1c-e3f0c5715645")
//         .Build()
//
let client = new TogglClient(TogglClientOptions(Key = "77775ba928442e3ea39bcb4258a52710"))
// let getTimeEntries = TogglApi.getTimeEntries client
// let openEmail = Email.openEmail (settings["MailRecipients"])


type MainWindow() as this =
    inherit Window()

    do
        this.Title <- sprintf "Taggl (%O to quit)" Application.QuitKey

        // Create input components and labels
        // let usernameLabel = new Label(Text = "Username:")
        // let pb = new ProgressBar()
        // pb.Pulse()

        // let pb = new ProgressBar(X = Pos.Right(usernameLabel) + Pos.op_Implicit(1), Width = Dim.Fill())
        // pb.Fraction <- (1 |> float32)
        // pb.Accepting.Add(fun _ -> pb.Pulse())
        // pb.Pulse()

        let startButton = new Button (Text = "Start")
        let spinner = new SpinnerView ()

        // let passwordText = new TextField(Secret = true, X = Pos.Left(userNameText), Y = Pos.Top(passwordLabel), Width = Dim.Fill())

        // Create login button
        // let btnLogin = new Button(Text = "Login", Y = Pos.Bottom(passwordLabel) +  Pos.op_Implicit(1), X = Pos.Center(), IsDefault = true)
        let date = DateTime.Today.AddMonths(-1);
        let generateExcel = Excel.generateExcel "c:\\temp\\TS_VD.xlsx" date ("Delcoigne", "Vincent")
        // When login button is clicked display a message popup
        startButton.Accepting.Add(fun _ ->
            spinner.AutoSpin <- true
            startButton.Visible <- false

            // _currentStatus = "Fetching time entries from Toggl";

            let timeEntries = Timesheet.getTimeEntries client date
            // _currentStatus = "Generating Excel file";

            timeEntries |> generateExcel

            Common.openFile(@"c:\temp\TS_VD.xlsx");
            )
        //     if userNameText.Text = "admin" && passwordText.Text = "password" then
        //         MessageBox.Query("Logging In", "Login Successful", "Ok") |> ignore
        //         ExampleWindow.UserName <- userNameText.Text.ToString()
        //         Application.RequestStop()
        //     else
        //         MessageBox.ErrorQuery("Logging In", "Incorrect username or password", "Ok") |> ignore
        // )

        // Add the views to the Window
        this.Add(startButton, spinner)

    // static member val UserName = "" with get, set

[<EntryPoint>]
let main argv =
    ExcelPackage.License.SetNonCommercialPersonal("Delcoigne Vincent")

    Application.Init()
    Application.Run<MainWindow>().Dispose()

    // Before the application exits, reset Terminal.Gui for clean shutdown
    Application.Shutdown()

    // To see this output on the screen it must be done after shutdown,
    // which restores the previous screen.
    // printfn "Username: %s" ExampleWindow.UserName

    0
