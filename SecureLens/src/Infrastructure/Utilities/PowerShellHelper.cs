using System.Diagnostics;
using System.Text;

namespace SecureLens.Infrastructure.Utilities;

public abstract class PowerShellHelper
{
    private const string _powerShellExe = "pwsh";

    private static string GeneratePowerShellCommand(string command)
    {
        return $"-NoProfile -Command \"{command}\"";
    }

    public static ProcessStartInfo CreatePowerShellProcessStartInfo(string command)
    {
        return new ProcessStartInfo
        {
            FileName = _powerShellExe,
            Arguments = GeneratePowerShellCommand(command),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8
        };
    }
}