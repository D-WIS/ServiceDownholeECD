using DWIS.Service.DownholeECD.ModelShared;
using OSDC.DotnetLibraries.General.Math;
using OSDC.DotnetLibraries.General.Common;

namespace DWIS.Service.DownholeECD.Model
{
    public class DownholePressureConverter
    {
        private static List<OSDC.DotnetLibraries.Drilling.Surveying.SurveyPoint> _surveyPoints = new List<OSDC.DotnetLibraries.Drilling.Surveying.SurveyPoint>();
        public static void Process(ModelShared.WellBoreArchitecture architecture, ModelShared.Trajectory trajectory, ModelShared.DrillString drillString, RealtimeInputsData inputs, RealtimeOutputsData outputs)
        {
            if (outputs.DownholeEquivalentCirculationDensity is null)
            {
                outputs.DownholeEquivalentCirculationDensity = new RigOS.Common.Worker.ScalarProperty();
            }
            if (outputs.TopLiquidLevelReferencePressure is null)
            {
                outputs.TopLiquidLevelReferencePressure = new RigOS.Common.Worker.ScalarProperty();
            }
            if (outputs.TopLiquidLevelReferenceTVD is null)
            {
                outputs.TopLiquidLevelReferenceTVD = new RigOS.Common.Worker.ScalarProperty();
            }
            outputs.DownholeEquivalentCirculationDensity.Value = null;
            double? sensorDistanceToBit = null;
            // search for the sensor in the drill string and get the distance to bit, if not found use default value
            double dist = 0;
            foreach (var section in drillString.DrillStringSectionList)
            {
                if (section is not null && section.SectionComponentList is not null)
                {
                    double dist2 = 0;
                    foreach (var component in section.SectionComponentList)
                    {
                        if (component is not null && component.PartList is not null)
                        {
                            double dist3 = 0;
                            foreach (var part in component.PartList)
                            {
                                if (part is not null)
                                {
                                    if (false)
                                    {
                                        dist += dist2 + dist3 + 0;
                                        sensorDistanceToBit = dist;
                                        break;
                                    }
                                    dist3 += part.TotalLength;
                                }
                            }
                            if (sensorDistanceToBit is not null)
                            {
                                break;
                            }
                            dist2 += dist3;
                        }
                    }
                    if (sensorDistanceToBit is not null)
                    {
                        break;
                    }
                    dist += section.Count * dist2;
                }
            }
            if (sensorDistanceToBit is null)
            {
                sensorDistanceToBit = 2.0;
            }
            if (inputs.AnnulusPressure is not null && inputs.AnnulusPressure.Value is not null &&
                inputs.BottomOfStringDepth is not null && inputs.BottomOfStringDepth.Value is not null &&
                trajectory.SurveyStationList is not null)
            {
                double p = inputs.AnnulusPressure.Value.Value;
                // search for the TVD at the sensor depth using the trajectory
                CurvilinearPoint3D interpolated = new CurvilinearPoint3D();
                _surveyPoints.Clear();
                foreach (var sv in trajectory.SurveyStationList)
                {
                    if (sv is not null)
                    {
                        OSDC.DotnetLibraries.Drilling.Surveying.SurveyPoint sp = new OSDC.DotnetLibraries.Drilling.Surveying.SurveyPoint()
                        {
                            Abscissa = sv.Abscissa,
                            Inclination = sv.Inclination,
                            Azimuth = sv.Azimuth,
                            X = sv.X,
                            Y = sv.Y,
                            Z = sv.Z,
                            Curvature = sv.Curvature,
                            Toolface = sv.Toolface,
                            BUR = sv.BUR,
                            TUR = sv.TUR,
                        };
                        _surveyPoints.Add(sp);
                    }
                }
                if (OSDC.DotnetLibraries.Drilling.Surveying.SurveyPoint.InterpolateAtAbscissa(_surveyPoints, inputs.BottomOfStringDepth.Value.Value - sensorDistanceToBit.Value, interpolated) &&
                    interpolated.Z is not null)
                {
                    double z = interpolated.Z.Value;
                    Result result = TryGetTopLiquidLevelTvdMean(architecture, OperationMode.Auto);
                    if (result is not null)
                    {
                        if (result.Success && result.TopLiquidLevelTvdMean is not null)
                        {
                            double z0 = result.TopLiquidLevelTvdMean.Value;
                            double p0 = Constants.EarthStandardAtmosphericPressure;
                            if (!Numeric.EQ(z, z0))
                            {
                                double ecd = (p - p0) / (Constants.EarthStandardSurfaceGravitationalAcceleration * (z - z0));
                                outputs.DownholeEquivalentCirculationDensity.Value = ecd;
                                outputs.TopLiquidLevelReferenceTVD.Value = z0;
                                outputs.TopLiquidLevelReferencePressure.Value = p0;
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Operational context (paper-driven). If Auto, use a heuristic based on the architecture.
        /// </summary>
        public enum OperationMode
        {
            Auto = 0,
            Conventional,
            Riserless,
            DualGradientDrilling,
            LowLevelAnnulusReturn
        }

        public sealed record Result(
            bool Success,
            double? TopLiquidLevelTvdMean,
            string Rationale);

        /// <summary>
        /// Extracts a best-estimate TVD(mean) for the top of the liquid level in the annulus, as a reference TVD.
        /// This is a "reference selection" problem, not a hydraulics computation.
        /// </summary>
        public static Result TryGetTopLiquidLevelTvdMean(
            WellBoreArchitecture wba,
            OperationMode mode = OperationMode.Auto)
        {
            if (wba == null)
                return new Result(false, null, "WellBoreArchitecture is null.");

            var wellHeadTvd = MeanOf(wba.WellHead?.Depth);
            if (!wellHeadTvd.HasValue)
                return new Result(false, null, "Missing WellHead.Depth.GaussianValue.Mean (mudline / wellhead TVD).");

            // Surface equipment
            var surfaceSections = (wba.SurfaceSections ?? new List<SurfaceSection>()).ToList();
            bool hasMarineRiser = surfaceSections.Any(s => s.Type == SurfaceSectionType.MarineRiser
                                                       || s.Type == SurfaceSectionType.HighPressureRiser
                                                       || s.Type == SurfaceSectionType.LowPressureRiser);

            bool hasBellNipple = surfaceSections.Any(s => s.Type == SurfaceSectionType.BellNipple);
            bool hasDiverter = surfaceSections.Any(s => s.Type == SurfaceSectionType.Diverter);
            bool hasRcd = surfaceSections.Any(s => s.Type == SurfaceSectionType.RotatingControlDevice);

            // Side elements (lift pump etc.)
            var sideElements = EnumerateAllSideElements(surfaceSections).ToList();
            var pumpsWithTvd = sideElements
                .Where(e => e.Type == SideElementType.Pump && MeanOf(e.TopVerticalDepth).HasValue)
                .Select(e => new SideElementTVD(e, MeanOf(e.TopVerticalDepth)!.Value))
                .ToList();

            // Heuristic operation mode if Auto:
            var selectedMode = mode == OperationMode.Auto
                ? InferMode(hasMarineRiser, pumpsWithTvd.Count, hasRcd, wba)
                : mode;

            // Compute candidate TVDs for "top-of-column" reference based on paper logic.
            // Note: TVD is assumed positive downward.
            switch (selectedMode)
            {
                case OperationMode.Conventional:
                    {
                        // Paper: conventional annulus liquid level goes up to bell nipple or diverter just below drill floor. :contentReference[oaicite:1]{index=1}
                        var topFromRig = TryGetSurfaceSectionTopTvdMean(
                            wellHeadTvd.Value,
                            surfaceSections,
                            preferred: new[] { SurfaceSectionType.BellNipple, SurfaceSectionType.Diverter });

                        if (topFromRig.HasValue)
                            return new Result(true, topFromRig.Value,
                                "Conventional mode: using BellNipple/Diverter top TVD computed from WellHead.Depth minus cumulative SurfaceSections lengths.");

                        // Fallback: if those are absent, use shallowest computed top of any surface section.
                        var shallowest = TryGetShallowestSurfaceTopTvdMean(wellHeadTvd.Value, surfaceSections);
                        if (shallowest.HasValue)
                            return new Result(true, shallowest.Value,
                                "Conventional mode: BellNipple/Diverter not found; using shallowest computed surface section top TVD.");

                        // Last resort: mudline/wellhead
                        return new Result(true, wellHeadTvd.Value,
                            "Conventional mode: could not compute surface section tops; falling back to WellHead.Depth (mudline/wellhead TVD).");
                    }

                case OperationMode.Riserless:
                    {
                        // Paper: riserless references can be mudline or sea level; architecture typically provides mudline (WellHead.Depth). :contentReference[oaicite:2]{index=2}
                        // If a mud-lift pump is present, paper indicates lift pump depth is a natural reference. :contentReference[oaicite:3]{index=3}
                        var liftPump = SelectLiftPumpCandidate(pumpsWithTvd, wellHeadTvd.Value);
                        if (liftPump != null)
                            return new Result(true, liftPump.Value.TVD,
                                $"Riserless mode: using lift pump TopVerticalDepth as reference (selected pump: '{liftPump.Value.Element.Name ?? "(unnamed)"}').");

                        return new Result(true, wellHeadTvd.Value,
                            "Riserless mode: no lift pump found; using WellHead.Depth (mudline/wellhead TVD) as top-of-column reference.");
                    }

                case OperationMode.DualGradientDrilling:
                    {
                        // Paper: DGD commonly uses mudline as reference depth and sea bottom pressure at that depth. 
                        // The question here is only TVD of liquid level in annulus; in DGD that top-of-drilling-fluid column is typically at mudline.
                        return new Result(true, wellHeadTvd.Value,
                            "DualGradientDrilling mode: using WellHead.Depth (mudline/wellhead TVD) as drilling fluid top reference.");
                    }

                case OperationMode.LowLevelAnnulusReturn:
                    {
                        // Paper: low-level annulus return uses the liquid level in annulus as reference depth; that level is time-dependent. 
                        // Architecture alone cannot provide this dynamic level; it requires runtime pressures (e.g., bottom of riser) and a model.
                        // So return a "best structural reference" (lift pump if present; else mudline) plus an explicit rationale.
                        var liftPump = SelectLiftPumpCandidate(pumpsWithTvd, wellHeadTvd.Value);
                        if (liftPump != null)
                            return new Result(true, liftPump.Value.TVD,
                                "LowLevelAnnulusReturn mode: true top liquid level TVD is dynamic and not derivable from architecture alone; returning lift pump TopVerticalDepth as the closest structural reference.");

                        return new Result(true, wellHeadTvd.Value,
                            "LowLevelAnnulusReturn mode: true top liquid level TVD is dynamic and not derivable from architecture alone; returning WellHead.Depth (mudline/wellhead TVD) as structural fallback.");
                    }

                default:
                    return new Result(false, null, $"Unhandled mode: {selectedMode}.");
            }
        }

        // -------------------------------
        // Mode inference (heuristic)
        // -------------------------------

        private static OperationMode InferMode(
            bool hasMarineRiser,
            int pumpCount,
            bool hasRcd,
            WellBoreArchitecture wba)
        {
            // Heuristic intent:
            // - If no riser modeled -> likely riserless (unless land rig, but then you'd have bell nipple/diverter and wellhead depth ~0).
            // - If pumps exist with TopVerticalDepth near wellhead -> likely mud-lift / DGD-related surface.
            // - If riser exists and bell nipple/diverter exists -> conventional.
            // - If RCD exists, could be MPD; but MPD does not change "liquid level" concept; still typically rig-top references unless low-level return is specifically used.
            //
            // If you have an explicit "method" elsewhere in your system, pass it instead of Auto.
            if (!hasMarineRiser)
            {
                // Without a marine riser, offshore operations are often riserless.
                return pumpCount > 0 ? OperationMode.DualGradientDrilling : OperationMode.Riserless;
            }

            // With riser: default to conventional unless strong hints of lift-pump system
            if (pumpCount > 0)
                return OperationMode.DualGradientDrilling; // conservative assumption: pump suggests DGD/mud-lift presence

            return OperationMode.Conventional;
        }

        // -------------------------------
        // Surface section TVD reconstruction
        // -------------------------------

        private static double? TryGetSurfaceSectionTopTvdMean(
            double wellHeadTvdMean,
            IList<SurfaceSection> sections,
            SurfaceSectionType[] preferred)
        {
            // Assumption: SurfaceSections are ordered from WellHead upward.
            // We compute the "top TVD" after subtracting cumulative lengths (upwards).
            double current = wellHeadTvdMean;

            double? best = null;
            foreach (var s in sections)
            {
                var len = MeanOf(s.SectionLength);
                if (len.HasValue)
                    current -= len.Value;

                if (preferred.Contains(s.Type))
                    best = current;
            }

            return best;
        }

        private static double? TryGetShallowestSurfaceTopTvdMean(
            double wellHeadTvdMean,
            IList<SurfaceSection> sections)
        {
            double current = wellHeadTvdMean;
            double? shallowest = null;

            foreach (var s in sections)
            {
                var len = MeanOf(s.SectionLength);
                if (len.HasValue)
                    current -= len.Value;

                if (!shallowest.HasValue || current < shallowest.Value)
                    shallowest = current;
            }

            return shallowest;
        }

        // -------------------------------
        // Side element traversal
        // -------------------------------

        private static IEnumerable<SideElement> EnumerateAllSideElements(IEnumerable<SurfaceSection> surfaceSections)
        {
            foreach (var sec in surfaceSections)
            {
                if (sec.SideConnectors == null) continue;

                foreach (var sc in sec.SideConnectors)
                {
                    if (sc == null) continue;

                    if (sc.FirstSideElement != null)
                        yield return sc.FirstSideElement;

                    if (sc.ElementConnectivities == null) continue;

                    foreach (var c in sc.ElementConnectivities)
                    {
                        if (c?.UpstreamElement != null) yield return c.UpstreamElement;
                        if (c?.DownstreamElement != null) yield return c.DownstreamElement;
                    }
                }
            }
        }

        private static SideElementTVD? SelectLiftPumpCandidate(List<SideElementTVD> pumpsWithTvd, double wellHeadTvdMean)
        {
            // Choose the pump whose TopVerticalDepth is closest to WellHead.Depth (mudline/wellhead),
            // which matches the "mud-lift pump near mudline" intuition.
            if (pumpsWithTvd == null || pumpsWithTvd.Count == 0)
                return null;

            var best = pumpsWithTvd
                .Select(p => new SideElementTVD(p.Element, p.TVD))
                .OrderBy(p => Math.Abs(p.TVD - wellHeadTvdMean))
                .First();

            return best;
        }

        // -------------------------------
        // Gaussian helpers
        // -------------------------------

        private static double? MeanOf(GaussianDrillingProperty? prop)
            => prop?.GaussianValue?.Mean;

        private struct SideElementTVD
        {
            public SideElement Element { get; }
            public double TVD { get; }
            public SideElementTVD(SideElement element, double tvd)
            {
                Element = element;
                TVD = tvd;
            }
        }

    }
}
