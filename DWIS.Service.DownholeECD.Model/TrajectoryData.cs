using DWIS.API.DTO;
using DWIS.RigOS.Common.Model;
using DWIS.RigOS.Common.Worker;
using DWIS.Vocabulary.Schemas;
using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using System.Reflection;

namespace DWIS.Service.DownholeECD.Model
{
    public class TrajectoryProperty : SemanticInfo
    {
        public ModelShared.Trajectory? Trajectory { get; set; } = null;
    }
    public class TrajectoryData : DWISData
    {
        private static readonly Lazy<IReadOnlyDictionary<PropertyInfo, Dictionary<string, QuerySpecification>>> LocalSparQLQueries = new(BuildSparQLQueries(typeof(TrajectoryData)));
        private static readonly Lazy<IReadOnlyDictionary<PropertyInfo, ManifestFile>> LocalManifests = new(BuildManifests(typeof(TrajectoryData), "DownholeECDDataManifest", "DWIS", "DWISService"));
        public override Lazy<IReadOnlyDictionary<PropertyInfo, Dictionary<string, QuerySpecification>>> SparQLQueries { get => LocalSparQLQueries; }
        public override Lazy<IReadOnlyDictionary<PropertyInfo, ManifestFile>> Manifests { get => LocalManifests; }

        [AccessToVariable(CommonProperty.VariableAccessType.Assignable)]
        [Mandatory(CommonProperty.MandatoryType.General)]
        [SemanticStringVariable("TrajectoryDescription")]
        [SemanticFact("TrajectoryDescription", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("TrajectoryDescription#01", Nouns.Enum.DrillingDataPoint)]
        [SemanticFact("TrajectoryDescription#01", Nouns.Enum.TrajectoryDescription)]
        [SemanticFact("TrajectoryDescription#01", Nouns.Enum.JSonDataType)]
        [SemanticFact("TrajectoryDescription#01", Verbs.Enum.HasDynamicValue, "TrajectoryDescription")]
        [SemanticFact("Current#01", Nouns.Enum.Current)]
        [SemanticFact("TrajectoryDescription#01", Verbs.Enum.IsCharacterizedBy, "Current#01")]
        [SemanticFact("Advisor#01", Nouns.Enum.Advisor)]
        [SemanticFact("TrajectoryDescription#01", Verbs.Enum.IsProvidedTo, "Advisor#01")]
        [SemanticFact("DataProvider#01", Nouns.Enum.DataProvider)]
        [SemanticFact("TrajectoryDescription#01", Verbs.Enum.IsProvidedTo, "DataProvider#01")]
        [SemanticFact("contextualDataBuilder#01", Nouns.Enum.DWISContextualDataBuilder)]
        [SemanticFact("TrajectoryDescription#01", Verbs.Enum.IsProvidedBy, "contextualDataBuilder#01")]
        public TrajectoryProperty? Trajectory { get; set; } = null;
    }
}
