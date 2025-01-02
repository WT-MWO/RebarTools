using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RebarUTools.Commands;
using System.Windows.Media.Imaging;


namespace RebarUTools
{
    public class Application : IExternalApplication
    {
        static void AddRibbonPanel(UIControlledApplication application)
        {
            // Create a custom ribbon tab
            String tabName = "RebarUTools";
            application.CreateRibbonTab(tabName);

            // Add a new ribbon panel
            RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, "View");

            // Get dll assembly path
            string thisAssemblyPath = typeof(Application).Assembly.Location;

            // Create push button for Unobscure
            PushButtonData b1Data = new PushButtonData(
                "cmdUnobscure",
                "Unobscure all rebars",
                thisAssemblyPath,
                "RebarUTools.Commands.UnobscureRebars");

            PushButton pb1 = ribbonPanel.AddItem(b1Data) as PushButton;
            pb1.ToolTip = "Unobscure all rebars";
            BitmapImage pb1Image = new BitmapImage(new Uri("pack://application:,,,/RebarUTools;component/Resources/unobscure.png"));
            pb1.LargeImage = pb1Image;
        }
        public Result OnStartup(UIControlledApplication application)
        {
            AddRibbonPanel(application);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
