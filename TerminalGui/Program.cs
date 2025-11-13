using TerminalGui;
using Terminal.Gui;

Application.Init();

try
{
    Application.Run(new ExampleWindow());
}
finally
{
    Application.Shutdown();
}
