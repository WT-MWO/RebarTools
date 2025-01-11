using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using RebarTools.Utilities;

namespace RebarTools.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class UnobscureRebars : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document document = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            var selector = new RebarSelector(document, uidoc);
            IEnumerable<Element> rebars = selector.GetAllRebars();
            bool x = true;
            View view = document.ActiveView;

            using (Transaction transaction = new Transaction(document, "Unobscure bars"))
            {
                transaction.Start();
                foreach (Rebar rebar in rebars)
                {
                    rebar.SetUnobscuredInView(view, x);
                }
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }
}
