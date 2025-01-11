using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RebarTools.Utilities;


namespace RebarTools.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class GetMass : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document document = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            RebarSelector selector = new RebarSelector(document, uidoc);
            List<Element> rebars = selector.GetRebars();

            RebarCoG rebarCoG = new RebarCoG(rebars);
            (List<double>, double) results = rebarCoG.GetCoG();
            double mass = Math.Round(results.Item2, 2);
            string msg = "Calculated mass: " + mass.ToString() + " kg.";
            TaskDialog.Show("Total mass", msg);

            return Result.Succeeded;
        }
    }
}
