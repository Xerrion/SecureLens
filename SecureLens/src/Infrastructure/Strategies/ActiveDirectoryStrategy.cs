using System.Diagnostics;
using System.Text;
using SecureLens.Infrastructure.Interfaces;
using SecureLens.Infrastructure.Logging;
using SecureLens.Infrastructure.Utilities;

namespace SecureLens.Infrastructure.Strategies;

public class ActiveDirectoryStrategy(ILogger logger) : IActiveDirectoryStrategy
{
    public List<string> QueryAdGroup(string groupName)
    {
        try
        {
            var cmd =
                $"Get-ADGroupMember -Identity \"{groupName}\" -Recursive | Select-Object -ExpandProperty SamAccountName";

            logger.LogInfo("Running command: " + cmd);

            ProcessStartInfo psi = PowerShellHelper.CreatePowerShellProcessStartInfo(cmd);

            using var proc = new Process();
            proc.StartInfo = psi;
            proc.Start();
            
            var stdout = proc.StandardOutput.ReadToEnd();
            var stderr = proc.StandardError.ReadToEnd();
            
            proc.WaitForExit();

            if (!string.IsNullOrWhiteSpace(stdout))
            {
                var lines = stdout.Trim().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                return lines.ToList();
            }
            else
            {
                var errorMsg = stderr.Trim();
                if (errorMsg.Contains("Cannot find an object with identity", StringComparison.OrdinalIgnoreCase))
                    logger.LogWarning($"AD group '{groupName}' not found in AD.");
                else
                    logger.LogError($"Failed to get AD group details for '{groupName}'. Error: {errorMsg}");
                return new List<string>();
            }
        }
        catch (Exception e)
        {
            logger.LogError($"Error querying AD group '{groupName}': {e}");
            return new List<string>();
        }
    }

    public HashSet<string> QueryAdGroupMembers(IEnumerable<string> groupNames)
    {
        var allMembers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var groupsNotFound = 0;

        foreach (var group in groupNames)
        {
            List<string>? members = QueryAdGroup(group);
            if (members.Count == 0) groupsNotFound++;
            foreach (var m in members) allMembers.Add(m);
        }

        if (groupsNotFound > 0) logger.LogWarning($"{groupsNotFound} groups not found or had errors.");
        return allMembers;
    }
}