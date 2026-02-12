using DWIS.Client.ReferenceImplementation.OPCFoundation;
using DWIS.RigOS.Common.Worker;
using DWIS.Service.DownholeECD.Model;

namespace DWIS.Service.DownholeECD.DataSource
{
    public class Worker : DWISWorker<Configuration>
    {

        private RealtimeInputsData RealtimeInputsData { get; set; } = new RealtimeInputsData();

        public Worker(ILogger<IDWISWorker<Configuration>> logger, ILogger<DWISClientOPCF>? loggerDWISClient) : base(logger, loggerDWISClient)
        {
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConnectToBlackboard();
            if (_DWISClient != null && _DWISClient.Connected)
            {
                await RegisterToBlackboard(RealtimeInputsData);
                await Loop(stoppingToken);
            }
        }

        protected override async Task Loop(CancellationToken cancellationToken)
        {
            PeriodicTimer timer = new PeriodicTimer(LoopSpan);
            double bitDepth0 = 796;
            double bitTVD0 = 766;
            double incl = 23.0*Math.PI/180;
            double bitDepth = bitDepth0;
            double bitTVD = bitTVD0;
            double rho = 1010;
            double annulusPressure = OSDC.DotnetLibraries.General.Common.Constants.EarthStandardAtmosphericPressure + rho * OSDC.DotnetLibraries.General.Common.Constants.EarthStandardSurfaceGravitationalAcceleration * bitTVD;
            double step = 0.005;
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                bitDepth += step;
                bitTVD += step * Math.Cos(incl);
                annulusPressure = OSDC.DotnetLibraries.General.Common.Constants.EarthStandardAtmosphericPressure + rho * OSDC.DotnetLibraries.General.Common.Constants.EarthStandardSurfaceGravitationalAcceleration * bitTVD;
                if (RealtimeInputsData.BottomOfStringDepth is null)
                {
                    RealtimeInputsData.BottomOfStringDepth = new ScalarProperty();
                }
                RealtimeInputsData.BottomOfStringDepth.Value = bitDepth;
                if (RealtimeInputsData.AnnulusPressure is null)
                {
                    RealtimeInputsData.AnnulusPressure = new ScalarProperty();
                }
                RealtimeInputsData.AnnulusPressure.Value = annulusPressure;
                await PublishBlackboardAsync(RealtimeInputsData, cancellationToken);
                lock (_lock)
                {
                    if (RealtimeInputsData.BottomOfStringDepth is not null && RealtimeInputsData.BottomOfStringDepth.Value is not null)
                    {
                        if (Logger is not null && Logger.IsEnabled(LogLevel.Information))
                        {
                            Logger?.LogInformation("Bit Depth: " + RealtimeInputsData.BottomOfStringDepth.Value.Value.ToString("F3") + " m");
                        }
                    }
                    if (RealtimeInputsData.AnnulusPressure is not null && RealtimeInputsData.AnnulusPressure.Value is not null)
                    {
                        if (Logger is not null && Logger.IsEnabled(LogLevel.Information))
                        {
                            Logger?.LogInformation("Annulus Pressure: " + (RealtimeInputsData.AnnulusPressure.Value.Value/1e5).ToString("F3") + " bar");
                        }
                    }
                }
            }
        }
    }
}
