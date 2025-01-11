using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RebarTools.Commands;
using System.Windows.Media.Imaging;


namespace RebarTools
{
    public class Application : IExternalApplication
    {
        static void AddRibbonPanel(UIControlledApplication application)
        {
            // Create a custom ribbon tab
            String tabName = "RebarTools";
            application.CreateRibbonTab(tabName);

            // Add a new ribbon panel
            RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, "View");

            // Get dll assembly path
            string thisAssemblyPath = typeof(Application).Assembly.Location;

            // Create push button for Unobscure
            PushButtonData b1Data = new PushButtonData(
                "cmdUnobscure",
                "Unobscure bars",
                thisAssemblyPath,
                "RebarTools.Commands.UnobscureRebars");
            PushButton pb1 = ribbonPanel.AddItem(b1Data) as PushButton;
            pb1.ToolTip = "Unobscure all rebars";
            BitmapImage pb1Image = new BitmapImage(new Uri("pack://application:,,,/RebarTools;component/Resources/unobscure.png"));
            pb1.LargeImage = pb1Image;

            // Create push button for Obscure
            PushButtonData b2Data = new PushButtonData(
                "cmdObscure",
                "Obscure bars",
                thisAssemblyPath,
                "RebarTools.Commands.ObscureRebars");
            PushButton pb2 = ribbonPanel.AddItem(b2Data) as PushButton;
            pb2.ToolTip = "Obscure bars";
            BitmapImage pb2Image = new BitmapImage(new Uri("pack://application:,,,/RebarTools;component/Resources/obscure.png"));
            pb2.LargeImage = pb2Image;

            // Create push button for GetMass
            PushButtonData b3Data = new PushButtonData(
                "cmdGetMass",
                "Get mass",
                thisAssemblyPath,
                "RebarTools.Commands.GetMass");
            PushButton pb3 = ribbonPanel.AddItem(b3Data) as PushButton;
            pb3.ToolTip = "Get mass of selected bars";
            BitmapImage pb3Image = new BitmapImage(new Uri("pack://application:,,,/RebarTools;component/Resources/get_mass.png"));
            pb3.LargeImage = pb3Image;
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
