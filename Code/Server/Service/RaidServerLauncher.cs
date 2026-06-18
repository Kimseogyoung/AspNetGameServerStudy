using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Server.Service
{
    public class RaidServerLauncher : IHostedService
    {
        public RaidServerLauncher(IConfiguration config, ILogger<RaidServerLauncher> logger)
        {
            _workingDir = Path.GetFullPath(config["RaidServer:WorkingDir"] ?? "../RaidServer");
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/k dotnet run",
                    WorkingDirectory = _workingDir,
                    UseShellExecute = true,
                },
            };
            _process.Start();
            _logger.LogInformation("RaidServer 프로세스 시작 (PID: {Pid}, Dir: {Dir})", _process.Id, _workingDir);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_process is { HasExited: false })
            {
                _process.Kill(entireProcessTree: true);
                _logger.LogInformation("RaidServer 프로세스 종료 (PID: {Pid})", _process.Id);
            }
            _process?.Dispose();
            _process = null;
            return Task.CompletedTask;
        }

        private readonly string _workingDir;
        private readonly ILogger<RaidServerLauncher> _logger;
        private Process? _process;
    }
}
