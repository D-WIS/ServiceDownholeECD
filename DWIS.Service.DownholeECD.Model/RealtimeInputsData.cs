using DWIS.API.DTO;
using DWIS.Vocabulary.Schemas;
using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using System.Reflection;
using DWIS.RigOS.Common.Worker;

namespace DWIS.Service.DownholeECD.Model
{
    public class RealtimeInputsData : DWISData
    {
        private static readonly Lazy<IReadOnlyDictionary<PropertyInfo, Dictionary<string, QuerySpecification>>> LocalSparQLQueries = new(BuildSparQLQueries(typeof(RealtimeInputsData)));
        private static readonly Lazy<IReadOnlyDictionary<PropertyInfo, ManifestFile>> LocalManifests = new(BuildManifests(typeof(RealtimeInputsData), "DownholeECDDataManifest", "DWIS", "DWISService"));
        public override Lazy<IReadOnlyDictionary<PropertyInfo, Dictionary<string, QuerySpecification>>> SparQLQueries { get => LocalSparQLQueries; }
        public override Lazy<IReadOnlyDictionary<PropertyInfo, ManifestFile>> Manifests { get => LocalManifests; }

        [AccessToVariable(CommonProperty.VariableAccessType.Readable)]
        [Mandatory(CommonProperty.MandatoryType.General)]
        [SemanticExclusiveOr(1, 2)]
        [SemanticDiracVariable("BOS_depth")]
        [SemanticFact("BOS_depth", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("BOS_depth#01", Nouns.Enum.Measurement)]
        [SemanticFact("BOS_depth#01", Nouns.Enum.ContinuousDataType)]
        [SemanticFact("BOS_depth#01", Verbs.Enum.HasDynamicValue, "BOS_depth")]
        [SemanticFact("BOS_depth#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.DepthDrilling)]
        [SemanticFact("movingAverageBOS_depth", Nouns.Enum.MovingAverage)]
        [SemanticFact("BOS_depth#01", Verbs.Enum.IsTransformationOutput, "movingAverageBOS_depth")]
        [SemanticFact("curvilinearAbscissaFrame#01", Nouns.Enum.OneDimensionalCurviLinearReferenceFrame)]
        [SemanticFact("BOS_depth#01", Verbs.Enum.HasReferenceFrame, "curvilinearAbscissaFrame#01")]
        [SemanticFact("bos#01", Nouns.Enum.BottomOfStringReferenceLocation)]
        [SemanticFact("BOS_depth#01", Verbs.Enum.IsPhysicallyLocatedAt, "bos#01")]
        [SemanticFact("BOS_depth#01", Nouns.Enum.BitDepth)]
        public ScalarProperty? BottomOfStringDepth { get; set; } = null;

        [AccessToVariable(CommonProperty.VariableAccessType.Readable)]
        [Mandatory(CommonProperty.MandatoryType.General)]
        [SemanticDiracVariable("DownholeMeasuredAnnulusPressure")]
        [SemanticFact("DownholeMeasuredAnnulusPressure", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("DownholeMeasuredAnnulusPressure#01", Nouns.Enum.DerivedMeasurement)]
        [SemanticFact("DownholeMeasuredAnnulusPressure#01", Nouns.Enum.ContinuousDataType)]
        [SemanticFact("DownholeMeasuredAnnulusPressure#01", Verbs.Enum.HasDynamicValue, "DownholeMeasuredAnnulusPressure")]
        [SemanticFact("DownholeMeasuredAnnulusPressure#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.PressureDrilling)]
        [SemanticFact("absolutePressure#02", Nouns.Enum.AbsolutePressureReference)]
        [SemanticFact("downholePressure#01", Verbs.Enum.HasPressureReferenceType, "absolutePressure#02")]
        [SemanticFact("MovingAverageDownholeMeasuredAnnulusPressure", Nouns.Enum.MovingAverage)]
        [SemanticFact("DownholeMeasuredAnnulusPressure#01", Verbs.Enum.IsTransformationOutput, "MovingAverageDownholeMeasuredAnnulusPressure")]
        [SemanticFact("GaussianUncertaintyDownholeMeasuredAnnulusPressure#01", Nouns.Enum.GaussianUncertainty)]
        [SemanticFact("DownholeMeasuredAnnulusPressure#01", Verbs.Enum.HasUncertainty, "GaussianUncertaintyDownholeMeasuredAnnulusPressure#01")]
        [SemanticFact("GaussianUncertaintyDownholeMeasuredAnnulusPressure#01", Verbs.Enum.HasUncertaintyMean, "DownholeMeasuredAnnulusPressure#01")]
        [SemanticFact("mudPulseTelemetry", Nouns.Enum.MudPulseTelemetry)]
        [SemanticFact("DownholeMeasuredAnnulusPressure#01", Verbs.Enum.IsTransmittedBy, "mudPulseTelemetry")]
        [SemanticFact("annulusOutletJunction#01", Nouns.Enum.AnnulusOutletJunction)]
        [SemanticFact("outletHydraulicBranch#01", Nouns.Enum.HydraulicBranch)]
        [SemanticFact("annulusOutletJunction#01", Verbs.Enum.HasUpstreamBranch, "outletHydraulicBranch#01")]
        [SemanticFact("DownholeMeasuredAnnulusPressure#01", Verbs.Enum.IsAssociatedToHydraulicBranch, "outletHydraulicBranch#01")]
        [SemanticFact("BHA#01", Nouns.Enum.BHAMechanicalLogicalElement)]
        [SemanticFact("DownholeMeasuredAnnulusPressure#01", Verbs.Enum.IsMechanicallyLocatedAt, "BHA#01")]
        public ScalarProperty? AnnulusPressure { get; set; } = null;

    }
}
