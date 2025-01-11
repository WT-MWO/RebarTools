using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;

namespace RebarTools.Utilities
{
    internal class RebarCoG(List<Element> rebarCollector)
    {
        //private const double FEET_TO_MM = 304.8; // Conversion from feet to mm
        private const double steel_density = 222.2872457472; // kg per feet^3 equivalent to 7850 kg/m^3
        private readonly MultiplanarOption multiplanarOption = MultiplanarOption.IncludeAllMultiplanarCurves;

        private static bool IsRebarGroup(Rebar rebar)
        {
            return rebar.NumberOfBarPositions > 1;
        }

        private static (List<double>, double) ComputeSegmentCentroid(Curve curve, double diameter, int index)
        {
            //double density = 7.85e-6; // kg/mm3
            double radius = diameter / 2; // ft
            double area = Math.PI * Math.Pow(radius, 2); // ft^2
            double length = curve.Length; // ft
            double volume = area * length; // ft^3
            double mass = volume * steel_density; // kg

            if (curve is Line line)
            {
                double spX = line.GetEndPoint(0).X;
                double spY = line.GetEndPoint(0).Y;
                double spZ = line.GetEndPoint(0).Z;
                double epX = line.GetEndPoint(1).X;
                double epY = line.GetEndPoint(1).Y;
                double epZ = line.GetEndPoint(1).Z;

                double cpX = (spX + epX) / 2;
                double cpY = (spY + epY) / 2;
                double cpZ = (spZ + epZ) / 2;

                return (new List<double> { cpX, cpY, cpZ }, mass);
            }

            if (curve is Arc arc)
            {
                double arcRadius = arc.Radius;
                double arcCenterX = arc.Center.X;
                double arcCenterY = arc.Center.Y;
                double arcCenterZ = arc.Center.Z;
                //calculating midpoint of Arc
                XYZ midpoint = arc.Evaluate(0.5, true);
                double mpX = midpoint.X;
                double mpY = midpoint.Y;
                double mpZ = midpoint.Z;
                //calculate 'x' coordinate of centroid for arc
                double theta = length / (2 * Math.PI * arcRadius) * 360;
                double alpha = Math.PI * theta / 360;
                double p = (2 * Math.Sin(alpha) / (3 * alpha)) * (Math.Pow(arcRadius + radius, 3) - Math.Pow(arcRadius - radius, 3)) / (Math.Pow(arcRadius + radius, 2) - Math.Pow(arcRadius - radius, 2));
                //calculate a line vector and unit vector between center and midpoint
                double vectorX = mpX - arcCenterX;
                double vectorY = mpY - arcCenterY;
                double vectorZ = mpZ - arcCenterZ;
                double vectorLength = Math.Sqrt(vectorX * vectorX + vectorY * vectorY + vectorZ * vectorZ);

                if (vectorLength == 0) throw new DivideByZeroException();

                double unitX = vectorX / vectorLength;
                double unitY = vectorY / vectorLength;
                double unitZ = vectorZ / vectorLength;
                //centroid point
                double cpX = arcCenterX + p * unitX;
                double cpY = arcCenterY + p * unitY;
                double cpZ = arcCenterZ + p * unitZ;

                return (new List<double> { cpX, cpY, cpZ }, mass);
            }

            throw new ArgumentException("Unsupported curve type");
        }

        private static (List<double>, double) ComputeCentroid(List<List<double>> centroids, List<double> masses)
        {
            var weightedCoords = centroids.Select((centroid, i) => centroid.Select(coord => coord * masses[i]).ToList()).ToList();

            double sumXM = weightedCoords.Sum(c => c[0]);
            double sumYM = weightedCoords.Sum(c => c[1]);
            double sumZM = weightedCoords.Sum(c => c[2]);
            double totalMass = masses.Sum();

            if (totalMass == 0) throw new DivideByZeroException();

            return (new List<double> { sumXM / totalMass, sumYM / totalMass, sumZM / totalMass }, totalMass);
        }

        private static double GetRebarDiameter(Rebar rebar)
        {
            Parameter diamParam = rebar.LookupParameter("Bar Diameter");
            return diamParam.AsDouble(); //ft
        }

        private static List<int> GetExistingBarIndexes(Rebar rebar)
        {
            return Enumerable.Range(0, rebar.NumberOfBarPositions).Where(rebar.DoesBarExistAtPosition).ToList();
        }

        private List<Curve> GetCurveFromGroup(Rebar rebar, int index)
        {
            if (rebar.HasVariableLengthBars) //This needs to be tested if fully substitutes UNIFORM distribution type
            {
                return (List<Curve>)rebar.GetTransformedCenterlineCurves(false, false, false, multiplanarOption, index);
            }
            else
            { 
                return (List<Curve>)rebar.GetCenterlineCurves(false, false, false, multiplanarOption, index);
            }

        }

        public (List<double>, double) GetCoG()
        {
            var allBarCentroids = new List<List<double>>();
            var allBarMasses = new List<double>();

            foreach (Rebar rebar in rebarCollector)
            {
                double rebarDiameter = GetRebarDiameter(rebar);

                if (IsRebarGroup(rebar))
                {
                    foreach (int index in GetExistingBarIndexes(rebar))
                    {
                        var curves = GetCurveFromGroup(rebar, index);

                        var centroids = new List<List<double>>();
                        var masses = new List<double>();

                        foreach (Curve curve in curves)
                        {
                            var (centroid, mass) = ComputeSegmentCentroid(curve, rebarDiameter, index);
                            centroids.Add(centroid);
                            masses.Add(mass);
                        }

                        var (cog, massSum) = ComputeCentroid(centroids, masses);
                        allBarCentroids.Add(cog);
                        allBarMasses.Add(massSum);
                    }
                }
                else
                {
                    var curves = rebar.GetCenterlineCurves(false, false, false, multiplanarOption, 0);

                    var centroids = new List<List<double>>();
                    var masses = new List<double>();

                    foreach (Curve curve in curves)
                    {
                        var (centroid, mass) = ComputeSegmentCentroid(curve, rebarDiameter, 0);
                        centroids.Add(centroid);
                        masses.Add(mass);
                    }

                    var (cog, massSum) = ComputeCentroid(centroids, masses);
                    allBarCentroids.Add(cog);
                    allBarMasses.Add(massSum);
                }
            }

            var (finalCoG, totalMass) = ComputeCentroid(allBarCentroids, allBarMasses);
            return (finalCoG, totalMass);
        }
    }
}
