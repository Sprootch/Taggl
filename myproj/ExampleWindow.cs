using Terminal.Gui.App;
using Terminal.Gui.Configuration;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace myproj;

public class ExampleWindow : Window
{
  public static string UserName { get; set; }

    public ExampleWindow ()
    {
        Title = $"Taggl ({Application.QuitKey} to quit)";

        // Create input components and labels
        var firstNameLabel = new Label { Text = "First Name:" };

        var firstNameText = new TextField
        {
            // Position text field adjacent to the label
            X = Pos.Right (firstNameLabel) + 1,

            // Fill remaining horizontal space
            Width = Dim.Fill ()
        };

        var lastNameLabel = new Label
        {
            Text = "Last Name:" ,
            X = Pos.Left (firstNameLabel), Y = Pos.Bottom (firstNameLabel) + 1
        };

        var lastNameText = new TextField
        {
            // Position text field adjacent to the label
            X = Pos.Right (lastNameLabel) + 1,
            Y = Pos.Top (lastNameLabel),

            // Fill remaining horizontal space
            Width = Dim.Fill ()
        };

        var passwordLabel = new Label
        {
            Text = "Password:",
            X = Pos.Left (firstNameLabel), Y = Pos.Bottom (lastNameLabel) + 1
        };

        var passwordText = new TextField
        {
            Secret = true,

            // align with the text box above
            X = Pos.Left (firstNameText),
            Y = Pos.Top (passwordLabel),
            Width = Dim.Fill ()
        };

        // Create login button
        var startButton = new Button
        {
            Text = "Start",
            Y = Pos.Bottom (passwordLabel) + 1,

            // center the login button horizontally
            X = Pos.Center (),
            IsDefault = true
        };

        // When login button is clicked display a message popup
        startButton.Accepting += (s, e) =>
                           {
                               e.Handled = true;
                           };

        // Add the views to the Window
        Add (firstNameLabel, firstNameText, lastNameLabel, lastNameText, passwordLabel, passwordText, startButton);
    }

    public override void EndInit ()
    {
        base.EndInit ();
        ThemeManager.Theme = ThemeManager.GetThemeNames ().FirstOrDefault (x => x == "Anders") ?? "Default";
    }
}
