using DWIS.Client.ReferenceImplementation.OPCFoundation;
using DWIS.RigOS.Common.Worker;
using DWIS.Service.DownholeECD.Model;
using System.Reflection;

namespace DWIS.Service.DownholeECD.Server
{
    public class Worker : DWISWorker<Configuration>
    {
        private RealtimeInputsData RealtimeInputsData { get; set; } = new RealtimeInputsData();
        private RealtimeOutputsData RealtimeOutputsData { get; set; } = new RealtimeOutputsData();

        private WellBoreArchitectureData ArchitectureData { get; set; } = new WellBoreArchitectureData();

        private TrajectoryData TrajectoryData { get; set; } = new TrajectoryData();

        private BHADrillStringData BHADrillStringData { get; set; } = new BHADrillStringData();


        public Worker(ILogger<IDWISWorker<Configuration>> logger, ILogger<DWISClientOPCF>? loggerDWISClient) : base(logger, loggerDWISClient)
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
            PeriodicTimer timer = new PeriodicTimer(LoopSpan);
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
                }
                catch (Exception e)
                {
                    Logger?.LogError(e.ToString());
                }
                ConfigurationUpdater<Configuration>.Instance.UpdateConfiguration(this);
            }
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
