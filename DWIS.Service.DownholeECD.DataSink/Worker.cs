using DWIS.Client.ReferenceImplementation.OPCFoundation;
using DWIS.RigOS.Common.Worker;
using DWIS.Service.DownholeECD.Model;

namespace DWIS.Service.DownholeECD.DataSink
{
    public class Worker : DWISWorker<Configuration>
    {
        private RealtimeOutputsData RealtimeOutputsData { get; set; } = new RealtimeOutputsData();

        public Worker(ILogger<IDWISWorker<Configuration>> logger, ILogger<DWISClientOPCF>? loggerDWISClient) : base(logger, loggerDWISClient)
        {
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConnectToBlackboard();
            if (_DWISClient != null && _DWISClient.Connected)
            {
                await RegisterQueries(RealtimeOutputsData);
                await Loop(stoppingToken);
            }
        }

        protected override async Task Loop(CancellationToken cancellationToken)
        {
            PeriodicTimer timer = new PeriodicTimer(LoopSpan);
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await ReadBlackboardAsync(RealtimeOutputsData, cancellationToken);
                lock (_lock)
                {
                    if (RealtimeOutputsData.DownholeEquivalentCirculationDensity is not null && RealtimeOutputsData.DownholeEquivalentCirculationDensity.Value is not null)
                    {
                        if (Logger is not null && Logger.IsEnabled(LogLevel.Information))
                        {
                            Logger?.LogInformation("Downhole ECD: " + RealtimeOutputsData.DownholeEquivalentCirculationDensity.Value.Value.ToString("F3") + " kg/m^3");
                        }
                    }
                    if (RealtimeOutputsData.TopLiquidLevelReferencePressure is not null && RealtimeOutputsData.TopLiquidLevelReferencePressure.Value is not null)
                    {
                        if (Logger is not null && Logger.IsEnabled(LogLevel.Information))
                        {
                            Logger?.LogInformation("Reference Pressure: " + (RealtimeOutputsData.TopLiquidLevelReferencePressure.Value.Value / 1e5).ToString("F3") + " bar");
                        }
                    }
                    if (RealtimeOutputsData.TopLiquidLevelReferenceTVD is not null && RealtimeOutputsData.TopLiquidLevelReferenceTVD.Value is not null)
                    {
                        if (Logger is not null && Logger.IsEnabled(LogLevel.Information))
                        {
                            Logger?.LogInformation("Reference TVD: " + RealtimeOutputsData.TopLiquidLevelReferenceTVD.Value.Value.ToString("F3") + " m");
                        }
                    }
                }
            }
        }
    }
}
