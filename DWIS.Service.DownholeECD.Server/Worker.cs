using DWIS.Client.ReferenceImplementation.OPCFoundation;
using DWIS.RigOS.Common.Worker;
using DWIS.Service.DownholeECD.Model;
using System.Reflection;
using System.Text.Json;

namespace DWIS.Service.DownholeECD.Server
{
    public class Worker : DWISWorker<ConfigurationForDownholeECD>
    {
        private RealtimeInputsData RealtimeInputsData { get; set; } = new RealtimeInputsData();
        private RealtimeOutputsData RealtimeOutputsData { get; set; } = new RealtimeOutputsData();

        private WellBoreArchitectureData ArchitectureData { get; set; } = new WellBoreArchitectureData();

        private TrajectoryData TrajectoryData { get; set; } = new TrajectoryData();

        private BHADrillStringData BHADrillStringData { get; set; } = new BHADrillStringData();
        private readonly List<RealtimeDataSample> _processLog = new();
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
        private DateTimeOffset? _nextDumpUtc;


        public Worker(ILogger<IDWISWorker<ConfigurationForDownholeECD>> logger, ILogger<DWISClientOPCF>? loggerDWISClient) : base(logger, loggerDWISClient)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConnectToBlackboard();
            if (Configuration is not null && _DWISClient != null && _DWISClient.Connected)
            {
                await RegisterQueries(RealtimeInputsData);
                await RegisterQueries(ArchitectureData);
                await RegisterQueries(TrajectoryData);
                await RegisterQueries(BHADrillStringData);
                await RegisterToBlackboard(RealtimeOutputsData);
                await Loop(stoppingToken);
            }
        }

        protected override async Task Loop(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new PeriodicTimer(LoopSpan);
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    try
                    {
                        await ReadBlackboardAsync(BHADrillStringData, stoppingToken);
                        await ReadBlackboardAsync(TrajectoryData, stoppingToken);
                        await ReadBlackboardAsync(ArchitectureData, stoppingToken);
                        await ReadBlackboardAsync(RealtimeInputsData, stoppingToken);
                        if (TrajectoryData.Trajectory is not null &&
                            TrajectoryData.Trajectory.Trajectory is not null &&
                            BHADrillStringData.BHADrillString is not null &&
                            BHADrillStringData.BHADrillString.BHADrillString is not null &&
                            ArchitectureData.WellBoreArchitecture is not null &&
                            ArchitectureData.WellBoreArchitecture.WellBoreArchitecture is not null)
                        {
                            DownholePressureConverter.Process(ArchitectureData.WellBoreArchitecture.WellBoreArchitecture, TrajectoryData.Trajectory.Trajectory, BHADrillStringData.BHADrillString.BHADrillString, RealtimeInputsData, RealtimeOutputsData);
                        }
                        await PublishBlackboardAsync(RealtimeOutputsData, stoppingToken);
                        lock (_lock)
                        {
                            if (Logger is not null && Logger.IsEnabled(LogLevel.Information))
                            {
                                if (RealtimeInputsData.BottomOfStringDepth is not null &&
                                    RealtimeInputsData.BottomOfStringDepth.Value is not null)
                                {
                                    Logger.LogInformation("Bit Depth: " + RealtimeInputsData.BottomOfStringDepth.Value.Value.ToString("F3") + " m");
                                }
                                if (RealtimeInputsData.AnnulusPressure is not null &&
                                    RealtimeInputsData.AnnulusPressure.Value is not null)
                                {
                                    Logger.LogInformation("Downhole Pressure: " + (RealtimeInputsData.AnnulusPressure.Value.Value / 1e5).ToString("F3") + " bar");

                                }
                                if (RealtimeOutputsData.DownholeEquivalentCirculationDensity is not null &&
                                    RealtimeOutputsData.DownholeEquivalentCirculationDensity.Value is not null)
                                {
                                    Logger.LogInformation("Downhole ECD: " + RealtimeOutputsData.DownholeEquivalentCirculationDensity.Value.Value.ToString("F3") + " kg/m^3");
                                }
                                if (RealtimeOutputsData.TopLiquidLevelReferencePressure is not null &&
                                    RealtimeOutputsData.TopLiquidLevelReferencePressure.Value is not null)
                                {
                                    Logger.LogInformation("Top level liquid reference pressure: " + (RealtimeOutputsData.TopLiquidLevelReferencePressure.Value.Value / 1e5).ToString("F3") + " bar");
                                }
                                if (RealtimeOutputsData.TopLiquidLevelReferenceTVD is not null &&
                                    RealtimeOutputsData.TopLiquidLevelReferenceTVD.Value is not null)
                                {
                                    Logger.LogInformation("Top level liquid reference TVD: " + RealtimeOutputsData.TopLiquidLevelReferenceTVD.Value.Value.ToString("F3") + " m");
                                }
                            }
                        }

                        await TryDumpProcessLogIfDueAsync(stoppingToken);
                    }
                    catch (Exception e)
                    {
                        Logger?.LogError(e.ToString());
                    }
                    ConfigurationUpdater<ConfigurationForDownholeECD>.Instance.UpdateConfiguration(this);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }

            await ForceDumpProcessLogAsync();
        }

        private async Task TryDumpProcessLogIfDueAsync(CancellationToken cancellationToken)
        {
            if (Configuration is null || !Configuration.EnableRealtimeDataDump)
            {
                return;
            }

            TimeSpan interval = GetValidatedDumpInterval(Configuration.RealtimeDataDumpInterval);
            DateTimeOffset now = DateTimeOffset.UtcNow;
            _processLog.Add(CreateSample(now));

            if (_nextDumpUtc is null)
            {
                _nextDumpUtc = GetNextBoundary(now, interval);
            }

            if (now < _nextDumpUtc)
            {
                return;
            }

            await DumpProcessLogAsync(interval, _nextDumpUtc.Value, cancellationToken);
            _processLog.Clear();
            _nextDumpUtc = GetNextBoundary(now, interval);
        }

        private async Task ForceDumpProcessLogAsync()
        {
            if (_processLog.Count == 0 || Configuration is null || !Configuration.EnableRealtimeDataDump)
            {
                return;
            }

            TimeSpan interval = GetValidatedDumpInterval(Configuration.RealtimeDataDumpInterval);
            DateTimeOffset dumpBoundary = _nextDumpUtc ?? DateTimeOffset.UtcNow;
            await DumpProcessLogAsync(interval, dumpBoundary, CancellationToken.None);
            _processLog.Clear();
        }

        private async Task DumpProcessLogAsync(TimeSpan interval, DateTimeOffset dumpBoundary, CancellationToken cancellationToken)
        {
            if (Configuration is null)
            {
                return;
            }

            string dumpDirectory = string.IsNullOrWhiteSpace(Configuration.RealtimeDataDumpDirectory) ? "/home" : Configuration.RealtimeDataDumpDirectory;
            Directory.CreateDirectory(dumpDirectory);

            var payload = new RealtimeDataDumpPayload
            {
                DumpTimestampUtc = DateTimeOffset.UtcNow,
                DumpInterval = interval,
                Samples = _processLog.ToArray()
            };

            string fileName = $"downholeecd-realtime-{dumpBoundary:yyyyMMddTHHmmssZ}.json";
            string filePath = Path.Combine(dumpDirectory, fileName);
            string jsonPayload = JsonSerializer.Serialize(payload, _jsonSerializerOptions);
            await File.WriteAllTextAsync(filePath, jsonPayload, cancellationToken);

            Logger?.LogInformation("Realtime input/output samples dumped to {FilePath} ({Count} samples).", filePath, payload.Samples.Length);
        }

        private static TimeSpan GetValidatedDumpInterval(TimeSpan interval)
        {
            if (interval <= TimeSpan.Zero)
            {
                return TimeSpan.FromHours(1);
            }

            return interval;
        }

        private static DateTimeOffset GetNextBoundary(DateTimeOffset now, TimeSpan interval)
        {
            long ticks = interval.Ticks;
            long nextTicks = ((now.UtcTicks / ticks) + 1) * ticks;
            return new DateTimeOffset(nextTicks, TimeSpan.Zero);
        }

        private RealtimeDataSample CreateSample(DateTimeOffset timestampUtc)
        {
            return new RealtimeDataSample
            {
                TimestampUtc = timestampUtc,
                RealtimeInputsData = ExtractScalarSnapshot(RealtimeInputsData),
                RealtimeOutputsData = ExtractScalarSnapshot(RealtimeOutputsData)
            };
        }

        private static Dictionary<string, double?> ExtractScalarSnapshot(object data)
        {
            var snapshot = new Dictionary<string, double?>();
            PropertyInfo[] properties = data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var property in properties)
            {
                object? propertyValue = property.GetValue(data);
                if (propertyValue is null)
                {
                    snapshot[property.Name] = null;
                    continue;
                }

                PropertyInfo? valueProperty = propertyValue.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
                if (valueProperty is null)
                {
                    continue;
                }

                object? rawValue = valueProperty.GetValue(propertyValue);
                if (rawValue is null)
                {
                    snapshot[property.Name] = null;
                    continue;
                }

                if (rawValue is double valueAsDouble)
                {
                    snapshot[property.Name] = valueAsDouble;
                    continue;
                }

                if (double.TryParse(rawValue.ToString(), out double parsedValue))
                {
                    snapshot[property.Name] = parsedValue;
                }
            }

            return snapshot;
        }

        private sealed class RealtimeDataDumpPayload
        {
            public DateTimeOffset DumpTimestampUtc { get; set; }
            public TimeSpan DumpInterval { get; set; }
            public RealtimeDataSample[] Samples { get; set; } = Array.Empty<RealtimeDataSample>();
        }

        private sealed class RealtimeDataSample
        {
            public DateTimeOffset TimestampUtc { get; set; }
            public Dictionary<string, double?> RealtimeInputsData { get; set; } = new Dictionary<string, double?>();
            public Dictionary<string, double?> RealtimeOutputsData { get; set; } = new Dictionary<string, double?>();
        }

        protected override void AssignValue(DWISData data, object propValue, object? value)
        {
            if (propValue is WellBoreArchitectureProperty archProp)
            {
                if (value is string sval)
                {
                    ModelShared.WellBoreArchitecture? arch = System.Text.Json.JsonSerializer.Deserialize<ModelShared.WellBoreArchitecture>(sval);
                    if (arch != null)
                    {
                        archProp.WellBoreArchitecture = arch;
                    }
                }
            }
            if (propValue is TrajectoryProperty trajProp)
            {
                if (value is string sval)
                {
                    ModelShared.Trajectory? traj = System.Text.Json.JsonSerializer.Deserialize<ModelShared.Trajectory>(sval);
                    if (traj != null)
                    {
                        trajProp.Trajectory = traj;
                    }
                }
            }
            if (propValue is BHADrillStringProperty stringProp)
            {
                if (value is string sval)
                {
                    ModelShared.DrillString? ds = System.Text.Json.JsonSerializer.Deserialize<ModelShared.DrillString>(sval);
                    if (ds != null)
                    {
                        stringProp.BHADrillString = ds;
                    }
                }
            }
            else
            {
                base.AssignValue(data, propValue, value);
            }
        }

        protected override void SetDefaultProperty(PropertyInfo property, DWISData data)
        {
            if (property.PropertyType == typeof(WellBoreArchitectureData))
            {
                WellBoreArchitectureData archProp = new WellBoreArchitectureData();
                property.SetValue(data, archProp);
            }
            else if (property.PropertyType == typeof(TrajectoryData))
            {
                TrajectoryData trajProp = new TrajectoryData();
                property.SetValue(data, trajProp);
            }
            else if (property.PropertyType == typeof(BHADrillStringData))
            {
                BHADrillStringData ds = new BHADrillStringData();
                property.SetValue(data, ds);
            }
            else
            {
                base.SetDefaultProperty(property, data);
            }
        }
    }
}
