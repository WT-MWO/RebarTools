using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RebarUTools.Utilities
{
    internal class RebarSelector(Document doc, UIDocument uidoc)
    {
        private static bool IsRebar(Element obj)
        {
            // Checks if the object is of Rebar type
            return obj.Category.Id.Value == (int)BuiltInCategory.OST_Rebar;
        }

        private static bool IsRebarGroup(Rebar rebarObject)
        {
            int nBars = rebarObject.NumberOfBarPositions;
            return nBars > 1;
        }

        private static List<Element> ReturnRebars(IList<Element> selection)
        {
            List<Element> elements = new List<Element>();

            foreach (Element e in selection)
            {
                if (IsRebar(e))
                {
                    elements.Add(e);
                }
            }
            return elements;
        }

        public List<Element> GetRebars()
        {
            // Gets Rebar type objects from selected elements, prompts user if no objects are selected
            List<Element> selection = uidoc.Selection.GetElementIds()
                .Select(id => doc.GetElement(id))
                .ToList();

            // If no object is selected, prompt the user to select
            if (selection.Count == 0)
            {
                IList<Reference> selectedReferences = uidoc.Selection.PickObjects(ObjectType.Element, "Choose rebars");
                selection = selectedReferences
                    .Select(reference => doc.GetElement(reference.ElementId))
                    .ToList();
            }

            return ReturnRebars(selection);
        }

        public List<Element> GetAllRebars()
        {
            // Gets Rebar type objects from selected elements, or all rebars in the view if none are selected
            List<Element> selection = uidoc.Selection.GetElementIds()
                .Select(id => doc.GetElement(id))
                .ToList();

            if (selection.Count == 0)
            {
                return new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Rebar)
                    .WhereElementIsNotElementType()
                    .ToList();
            }

            return ReturnRebars(selection);
        }

        public List<Element> GetAllModelRebars()
        {
            // Gets all Rebar objects in the view, does not care if anything is selected
            return new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rebar)
                .WhereElementIsNotElementType()
                .ToList();
        }
    }
}