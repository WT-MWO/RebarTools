using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RebarUTools.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class UnobscureRebars : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document document = commandData.Application.ActiveUIDocument.Document;
            IEnumerable<Element> rebars= new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Rebar).WhereElementIsNotElementType();
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
