using Terminal.Gui.App;
using Terminal.Gui.Configuration;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Toggl.Api;

namespace myproj;

public class ExampleWindow : Window
{
    private readonly TogglClient _client = new(new TogglClientOptions
    {
        Key = "77775ba928442e3ea39bcb4258a52710"
    });
    private Label label { get; set; }
    private readonly ProgressBar _progress;
    private SpinnerView _spinner;

    public ExampleWindow()
    {
        Title = $"Taggl ({Application.QuitKey} to quit)";

        // Create input components and labels
        var firstNameLabel = new Label
        {
            Text = "First Name:"
        };

        var firstNameText = new TextField
        {
            // Position text field adjacent to the label
            X = Pos.Right(firstNameLabel) + 1,

            // Fill remaining horizontal space
            Width = Dim.Fill()
        };

        var lastNameLabel = new Label
        {
            Text = "Last Name:",
            X = Pos.Left(firstNameLabel),
            Y = Pos.Bottom(firstNameLabel) + 1
        };

        var lastNameText = new TextField
        {
            // Position text field adjacent to the label
            X = Pos.Right(lastNameLabel) + 1,
            Y = Pos.Top(lastNameLabel),

            // Fill remaining horizontal space
            Width = Dim.Fill()
        };

        var passwordLabel = new Label
        {
            Text = "Password:",
            X = Pos.Left(firstNameLabel),
            Y = Pos.Bottom(lastNameLabel) + 1
        };

        var passwordText = new TextField
        {
            Secret = true,

            // align with the text box above
            X = Pos.Left(firstNameText),
            Y = Pos.Top(passwordLabel),
            Width = Dim.Fill()
        };

        label = new Label
        {
            Text = "Hello World",
            Height = Dim.Auto(),
            Width = Dim.Auto(),
            X = Pos.Center(),
            Y = Pos.Center()
        };
        // Create login button
        var startButton = new Button
        {
            Text = "Start",
            Y = Pos.Bottom(label) + 1,

            // center the login button horizontally
            X = Pos.Center(),
            IsDefault = true
        };
        _spinner = new SpinnerView
        {
            X = Pos.Right(label),
            Y = Pos.Center(),
            Style = new SpinnerStyle.Points(),
            AutoSpin = true,
            Visible = false
        };

        // When login button is clicked display a message popup
        startButton.Accepting += async (_, e) =>
        {
            e.Handled = true;
            startButton.Visible = false;
            try
            {
                // var cts = new CancellationTokenSource();
                // var t = Pulse(cts.Token);
                _spinner.Visible = true;
                await GenerateTimesheet();

                // await cts.CancelAsync();
                // await t;
            }
            finally
            {
                startButton.Visible = true;
                _spinner.Visible = false;
            }
        };

        _progress = new ProgressBar
        {
            ProgressBarFormat = ProgressBarFormat.Simple,
            ProgressBarStyle = ProgressBarStyle.Continuous,
            Y = Pos.AnchorEnd(),
            Width = Dim.Fill()
        };
        // Add the views to the Window
        Add(firstNameLabel, firstNameText, lastNameLabel, lastNameText, passwordLabel, passwordText, label, startButton, _spinner);
    }

    private async Task Pulse(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            _progress.Pulse();
            await Task.Delay(10, token);
            await Task.Yield();
        }
    }

    public override void EndInit()
    {
        base.EndInit();
        ThemeManager.Theme = ThemeManager.GetThemeNames().FirstOrDefault(x => x == "Anders") ?? "Default";
    }

    private async Task GenerateTimesheet()
    {
        var date = DateTime.Today.AddMonths(-1);
        var lastName = "Delcoigne";
        var firstName = "Vincent";

        var outputFile = Path.Combine(Environment.CurrentDirectory, $"TS-{date:yyyyMM}-{lastName}-{firstName}.xlsx");
        label.Text = "Fetching time entries from Toggl";

        var timeEntries = await Timesheet.Csharp.GetTimeEntriesAsync(_client, date);
        label.Text = "Generating Excel file";

        // Excel.generateExcel(@"c:\temp\TS_VD.xlsx", date, "Vincent", "Delcoigne", timeEntries);
        await Task.Run(() => Excel.generateExcel(outputFile, date, lastName, firstName, timeEntries));
        _spinner.Visible = false;

        Common.openFile(outputFile);
        var i = MessageBox.Query("Outlook", "Send email ?", "Ok", "Cancel");

        if (i == 0)
        {
            Email.openEmail("delcoignevincent@gmail.com", date, outputFile);
        }
        label.Text = "Done";
    }
}
