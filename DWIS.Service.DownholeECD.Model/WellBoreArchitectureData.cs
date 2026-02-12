using DWIS.API.DTO;
using DWIS.RigOS.Common.Model;
using DWIS.RigOS.Common.Worker;
using DWIS.Vocabulary.Schemas;
using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using System.Reflection;

namespace DWIS.Service.DownholeECD.Model
{
    public class WellBoreArchitectureProperty : SemanticInfo
    {
        public ModelShared.WellBoreArchitecture? WellBoreArchitecture { get; set; } = null;
    }
    public class WellBoreArchitectureData : DWISData
    {
        private static readonly Lazy<IReadOnlyDictionary<PropertyInfo, Dictionary<string, QuerySpecification>>> LocalSparQLQueries = new(BuildSparQLQueries(typeof(WellBoreArchitectureData)));
        private static readonly Lazy<IReadOnlyDictionary<PropertyInfo, ManifestFile>> LocalManifests = new(BuildManifests(typeof(WellBoreArchitectureData), "DownholeECDDataManifest", "DWIS", "DWISService"));
        public override Lazy<IReadOnlyDictionary<PropertyInfo, Dictionary<string, QuerySpecification>>> SparQLQueries { get => LocalSparQLQueries; }
        public override Lazy<IReadOnlyDictionary<PropertyInfo, ManifestFile>> Manifests { get => LocalManifests; }

        [AccessToVariable(CommonProperty.VariableAccessType.Assignable)]
        [Mandatory(CommonProperty.MandatoryType.General)]
        [SemanticStringVariable("WellBoreArchitectureDescription")]
        [SemanticFact("WellBoreArchitectureDescription", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("WellBoreArchitectureDescription#01", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("WellBoreArchitectureDescription#01", Nouns.Enum.WellboreArchitectureDescription)]
        [SemanticFact("WellBoreArchitectureDescription#01", Nouns.Enum.JSonDataType)]
        [SemanticFact("WellBoreArchitectureDescription#01", Verbs.Enum.HasDynamicValue, "WellBoreArchitectureDescription")]
        [SemanticFact("Current#01", Nouns.Enum.Current)]
        [SemanticFact("WellBoreArchitectureDescription#01", Verbs.Enum.IsCharacterizedBy, "Current#01")]
        [SemanticFact("Advisor#01", Nouns.Enum.Advisor)]
        [SemanticFact("WellBoreArchitectureDescription#01", Verbs.Enum.IsProvidedTo, "Advisor#01")]
        [SemanticFact("DataProvider#01", Nouns.Enum.DataProvider)]
        [SemanticFact("WellBoreArchitectureDescription#01", Verbs.Enum.IsProvidedTo, "DataProvider#01")]
        [SemanticFact("contextualDataBuilder#01", Nouns.Enum.DWISContextualDataBuilder)]
        [SemanticFact("WellBoreArchitectureDescription#01", Verbs.Enum.IsProvidedBy, "contextualDataBuilder#01")]
        public WellBoreArchitectureProperty? WellBoreArchitecture { get; set; } = null;
    }
}
