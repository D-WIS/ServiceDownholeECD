using DWIS.API.DTO;
using DWIS.Vocabulary.Schemas;
using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using System.Reflection;
using DWIS.RigOS.Common.Worker;

namespace DWIS.Service.DownholeECD.Model
{
    public class RealtimeOutputsData : DWISData
    {
        private static readonly Lazy<IReadOnlyDictionary<PropertyInfo, Dictionary<string, QuerySpecification>>> LocalSparQLQueries = new(BuildSparQLQueries(typeof(RealtimeOutputsData)));
        private static readonly Lazy<IReadOnlyDictionary<PropertyInfo, ManifestFile>> LocalManifests = new(BuildManifests(typeof(RealtimeOutputsData), "DownholeECDDataManifest", "DWIS", "DWISService"));
        public override Lazy<IReadOnlyDictionary<PropertyInfo, Dictionary<string, QuerySpecification>>> SparQLQueries { get => LocalSparQLQueries; }
        public override Lazy<IReadOnlyDictionary<PropertyInfo, ManifestFile>> Manifests { get => LocalManifests; }

        [AccessToVariable(CommonProperty.VariableAccessType.Assignable)]
        [Mandatory(CommonProperty.MandatoryType.General)]
        [SemanticDiracVariable("downholeECD")]
        [SemanticFact("downholeECD", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("downholeECD#01", Nouns.Enum.Measurement)]
        [SemanticFact("downholeECD#01", Nouns.Enum.ContinuousDataType)]
        [SemanticFact("downholeECD#01", Verbs.Enum.HasDynamicValue, "downholeECD")]
        [SemanticFact("downholeECD#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.MassDensityDrilling)]
        [SemanticFact("movingAverageDownholeECD", Nouns.Enum.MovingAverage)]
        [SemanticFact("downholeECD#01", Verbs.Enum.IsTransformationOutput, "movingAverageDownholeECD")]
        [SemanticFact("downholePressure#01", Nouns.Enum.Measurement)]
        [SemanticFact("downholePressure#01", Nouns.Enum.ContinuousDataType)]
        [SemanticFact("downholePressure#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.PressureDrilling)]
        [SemanticFact("movingAverageDownholePressure", Nouns.Enum.MovingAverage)]
        [SemanticFact("downholePressure#01", Verbs.Enum.IsTransformationOutput, "movingAverageDownholePressure")]
        [SemanticFact("pressureToDownholeECD", Nouns.Enum.PressureToEquivalentDensityTransformation)]
        [SemanticFact("downholePressure#01", Verbs.Enum.IsTransformationInput, "pressureToDownholeECD")]
        [SemanticFact("downholeECD#01", Verbs.Enum.IsTransformationOutput, "pressureToDownholeECD")]
        [SemanticFact("downholeNetwork", Nouns.Enum.DownholeHydraulicNetwork)]
        [SemanticFact("bhaAnnulus", Nouns.Enum.BHAAnnular)]
        [SemanticFact("downholeNetwork", Verbs.Enum.HasBranchComponent, "bhaAnnulus")]
        [SemanticFact("downholeECD#01", Verbs.Enum.IsEquivalentCirculationDensityAt, "bhaAnnulus")]
        [SemanticFact("downholePressure#01", Verbs.Enum.IsPressureAt, "bhaAnnulus")]
        [SemanticFact("circulating", Nouns.Enum.HydraulicCirculationCondition)]
        [SemanticFact("circulating", Verbs.Enum.IsHydraulicConditionFor, "bhaAnnulus")]
        [SemanticFact("drillingLiquid", Nouns.Enum.DrillingLiquidType)]
        [SemanticFact("drillingLiquid", Verbs.Enum.IsFluidTypeLocatedAt, "bhaAnnulus")]
        [SemanticFact("air", Nouns.Enum.AirType)]
        [SemanticFact("air", Verbs.Enum.IsFluidTypeLocatedAt, "bhaAnnulus")]
        [SemanticFact("airDrillingLiquidInterface", Nouns.Enum.FluidInterface)]
        [SemanticFact("air", Verbs.Enum.IsUpstreamOf, "airDrillingLiquidInterface")]
        [SemanticFact("drillingLiquid", Verbs.Enum.IsDownstreamOf, "airDrillingLiquidInterface")]
        [SemanticFact("topLiquidLevelTVD#01", Nouns.Enum.Measurement)]
        [SemanticFact("topLiquidLevelTVD#01", Nouns.Enum.ContinuousDataType)]
        [SemanticFact("topLiquidLevelTVD#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.DepthDrilling)]
        [SemanticFact("verticalDepthFrame#01", Nouns.Enum.VerticalDepthFrame)]
        [SemanticFact("topLiquidLevelTVD#01", Verbs.Enum.HasReferenceFrame, "verticalDepthFrame#01")]
        [SemanticFact("airDrillingLiquidInterface", Verbs.Enum.IsObservableFrom, "topLiquidLevelTVD#01")]
        [SemanticFact("topLiquidLevelReferencePressure#01", Nouns.Enum.Measurement)]
        [SemanticFact("topLiquidLevelReferencePressure#01", Nouns.Enum.ContinuousDataType)]
        [SemanticFact("topLiquidLevelReferencePressure#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.PressureDrilling)]
        [SemanticFact("absolutePressure#01", Nouns.Enum.AbsolutePressureReference)]
        [SemanticFact("topLiquidLevelReferencePressure#01", Verbs.Enum.HasPressureReferenceType, "absolutePressure#01")]
        [SemanticFact("airDrillingLiquidInterface", Verbs.Enum.IsObservableFrom, "topLiquidLevelReferencePressure#01")]
        [SemanticFact("topLiquidLevelTVD#01", Verbs.Enum.IsTransformationInput, "pressureToDownholeECD")]
        [SemanticFact("topLiquidLevelReferencePressure#01", Verbs.Enum.IsTransformationInput, "pressureToDownholeECD")]
        [SemanticFact("downholeECD#01", Verbs.Enum.IsDependentOn, "topLiquidLevelTVD#01")]
        [SemanticFact("downholeECD#01", Verbs.Enum.IsDependentOn, "topLiquidLevelReferencePressure#01")]
        [SemanticFact("downholeECD#01", Nouns.Enum.DownholeECD)]
        [SemanticFact("DWISService#01", Nouns.Enum.DWISInternalService)]
        [SemanticFact("downholeECD#01", Verbs.Enum.IsProvidedBy, "DWISService#01")]
        public ScalarProperty? DownholeEquivalentCirculationDensity { get; set; } = null;

        [AccessToVariable(CommonProperty.VariableAccessType.Assignable)]
        [Mandatory(CommonProperty.MandatoryType.General)]
        [SemanticDiracVariable("topLiquidLevelTVD")]
        [SemanticFact("topLiquidLevelTVD", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("topLiquidLevelTVD#01", Nouns.Enum.Measurement)]
        [SemanticFact("topLiquidLevelTVD#01", Nouns.Enum.ContinuousDataType)]
        [SemanticFact("topLiquidLevelTVD#01", Verbs.Enum.HasDynamicValue, "topLiquidLevelTVD")]
        [SemanticFact("topLiquidLevelTVD#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.DepthDrilling)]
        [SemanticFact("verticalDepthFrame#01", Nouns.Enum.VerticalDepthFrame)]
        [SemanticFact("topLiquidLevelTVD#01", Verbs.Enum.HasReferenceFrame, "verticalDepthFrame#01")]
        [SemanticFact("downholeNetwork", Nouns.Enum.DownholeHydraulicNetwork)]
        [SemanticFact("bhaAnnulus", Nouns.Enum.BHAAnnular)]
        [SemanticFact("downholeNetwork", Verbs.Enum.HasBranchComponent, "bhaAnnulus")]
        [SemanticFact("drillingLiquid", Nouns.Enum.DrillingLiquidType)]
        [SemanticFact("drillingLiquid", Verbs.Enum.IsFluidTypeLocatedAt, "bhaAnnulus")]
        [SemanticFact("air", Nouns.Enum.AirType)]
        [SemanticFact("air", Verbs.Enum.IsFluidTypeLocatedAt, "bhaAnnulus")]
        [SemanticFact("airDrillingLiquidInterface", Nouns.Enum.FluidInterface)]
        [SemanticFact("air", Verbs.Enum.IsUpstreamOf, "airDrillingLiquidInterface")]
        [SemanticFact("drillingLiquid", Verbs.Enum.IsDownstreamOf, "airDrillingLiquidInterface")]
        [SemanticFact("airDrillingLiquidInterface", Verbs.Enum.IsObservableFrom, "topLiquidLevelTVD#01")]
        public ScalarProperty? TopLiquidLevelReferenceTVD { get; set; } = null;

        [AccessToVariable(CommonProperty.VariableAccessType.Assignable)]
        [Mandatory(CommonProperty.MandatoryType.General)]
        [SemanticDiracVariable("topLiquidLevelReferencePressure")]
        [SemanticFact("topLiquidLevelReferencePressure", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("topLiquidLevelReferencePressure#01", Nouns.Enum.Measurement)]
        [SemanticFact("topLiquidLevelReferencePressure#01", Nouns.Enum.ContinuousDataType)]
        [SemanticFact("topLiquidLevelReferencePressure#01", Verbs.Enum.HasDynamicValue, "topLiquidLevelReferencePressure")]
        [SemanticFact("topLiquidLevelReferencePressure#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.PressureDrilling)]
        [SemanticFact("absolutePressure#01", Nouns.Enum.AbsolutePressureReference)]
        [SemanticFact("topLiquidLevelReferencePressure#01", Verbs.Enum.HasPressureReferenceType, "absolutePressure#01")]
        [SemanticFact("downholeNetwork", Nouns.Enum.DownholeHydraulicNetwork)]
        [SemanticFact("bhaAnnulus", Nouns.Enum.BHAAnnular)]
        [SemanticFact("downholeNetwork", Verbs.Enum.HasBranchComponent, "bhaAnnulus")]
        [SemanticFact("drillingLiquid", Nouns.Enum.DrillingLiquidType)]
        [SemanticFact("drillingLiquid", Verbs.Enum.IsFluidTypeLocatedAt, "bhaAnnulus")]
        [SemanticFact("air", Nouns.Enum.AirType)]
        [SemanticFact("air", Verbs.Enum.IsFluidTypeLocatedAt, "bhaAnnulus")]
        [SemanticFact("airDrillingLiquidInterface", Nouns.Enum.FluidInterface)]
        [SemanticFact("air", Verbs.Enum.IsUpstreamOf, "airDrillingLiquidInterface")]
        [SemanticFact("drillingLiquid", Verbs.Enum.IsDownstreamOf, "airDrillingLiquidInterface")]
        [SemanticFact("airDrillingLiquidInterface", Verbs.Enum.IsObservableFrom, "topLiquidLevelReferencePressure#01")]
        public ScalarProperty? TopLiquidLevelReferencePressure { get; set; } = null;
    }
}
