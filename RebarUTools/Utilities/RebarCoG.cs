using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;

namespace RebarUTools.Utilities
{
    internal class RebarCoG(FilteredElementCollector rebarCollector)
    {
        private const double FEET_TO_MM = 304.8; // Conversion from feet to mm
        private readonly MultiplanarOption multiplanarOption = MultiplanarOption.IncludeAllMultiplanarCurves;

        private static bool IsRebarGroup(Rebar rebar)
        {
            return rebar.NumberOfBarPositions > 1;
        }

        private static (List<double>, double) ComputeSegmentCentroid(Curve curve, double diameter, int index)
        {
            double density = 7.85e-6; // kg/mm3
            double radius = diameter / 2; //mm
            double area = Math.PI * Math.Pow(radius, 2);
            double length = curve.Length * FEET_TO_MM;
            double volume = area * length;
            double mass = volume * density;

            if (curve is Line line)
            {
                double spX = line.GetEndPoint(0).X * FEET_TO_MM;
                double spY = line.GetEndPoint(0).Y * FEET_TO_MM;
                double spZ = line.GetEndPoint(0).Z * FEET_TO_MM;
                double epX = line.GetEndPoint(1).X * FEET_TO_MM;
                double epY = line.GetEndPoint(1).Y * FEET_TO_MM;
                double epZ = line.GetEndPoint(1).Z * FEET_TO_MM;

                double cpX = (spX + epX) / 2;
                double cpY = (spY + epY) / 2;
                double cpZ = (spZ + epZ) / 2;

                return (new List<double> { cpX, cpY, cpZ }, mass);
            }

            if (curve is Arc arc)
            {
                double arcRadius = arc.Radius * FEET_TO_MM;
                double arcCenterX = arc.Center.X * FEET_TO_MM;
                double arcCenterY = arc.Center.Y * FEET_TO_MM;
                double arcCenterZ = arc.Center.Z * FEET_TO_MM;

                XYZ midpoint = arc.Evaluate(0.5, true);
                double mpX = midpoint.X * FEET_TO_MM;
                double mpY = midpoint.Y * FEET_TO_MM;
                double mpZ = midpoint.Z * FEET_TO_MM;

                double theta = length / (2 * Math.PI * arcRadius) * 360;
                double alpha = Math.PI * theta / 360;
                double p = (2 * Math.Sin(alpha) / (3 * alpha)) * (Math.Pow(arcRadius + radius, 3) - Math.Pow(arcRadius - radius, 3)) / (Math.Pow(arcRadius + radius, 2) - Math.Pow(arcRadius - radius, 2));

                double vectorX = mpX - arcCenterX;
                double vectorY = mpY - arcCenterY;
                double vectorZ = mpZ - arcCenterZ;
                double vectorLength = Math.Sqrt(vectorX * vectorX + vectorY * vectorY + vectorZ * vectorZ);

                if (vectorLength == 0) throw new DivideByZeroException();

                double unitX = vectorX / vectorLength;
                double unitY = vectorY / vectorLength;
                double unitZ = vectorZ / vectorLength;

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
            return diamParam.AsDouble() * FEET_TO_MM;
        }

        private static List<int> GetExistingBarIndexes(Rebar rebar)
        {
            return Enumerable.Range(0, rebar.NumberOfBarPositions).Where(rebar.DoesBarExistAtPosition).ToList();
        }

        private List<Curve> GetCurveFromGroup(Rebar rebar, int index)
        {
            return rebar.DistributionType == DistributionType.Uniform || rebar.DistributionType == DistributionType.VaryingLength
                ? rebar.GetTransformedCenterlineCurves(false, false, false, multiplanarOption, index)
                : rebar.GetCenterlineCurves(false, false, false, multiplanarOption, index);
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
