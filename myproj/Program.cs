using myproj;
using OfficeOpenXml;
using Terminal.Gui.App;

ExcelPackage.License.SetNonCommercialPersonal("Delcoigne Vincent");

Application.Init();

try
{
    Application.Run(new ExampleWindow());
}
finally
{
    Application.Shutdown();
}
