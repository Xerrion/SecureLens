using System.Text;

namespace SecureLens
{
    /// <summary>
    /// A class responsible for generating an HTML report from Analyzer results.
    /// </summary>
    public class HtmlReportWriter
    {
        /// <summary>
        /// Generates a single HTML document containing 
        /// Overall Stats, App Stats, Terminal Stats, and Unused AD Groups.
        /// </summary>
        /// <param name="overallStats">OverallStatisticsResult from Analyzer</param>
        /// <param name="appStats">Application statistics dictionary</param>
        /// <param name="terminalStats">List of TerminalStatisticsRow</param>
        /// <param name="unusedAdGroups">List of unused AD groups</param>
        /// <param name="settings">AdminByRequestSettings list (to correlate columns or naming)</param>
        /// <returns>An HTML string representing the entire report</returns>
        public string BuildHtmlReport(
            Analyzer.OverallStatisticsResult overallStats,
            Dictionary<string, Analyzer.ApplicationStatisticsResult> appStats,
            List<Analyzer.TerminalStatisticsRow> terminalStats,
            List<Analyzer.UnusedAdGroupResult> unusedAdGroups,
            List<AdminByRequestSetting> settings
        )
        {
            // Use a StringBuilder to build your HTML document:
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("  <meta charset='UTF-8'>");
            sb.AppendLine("  <title>SecureLens Report</title>");
            // Basic styling - you can add more advanced styling if you want
            sb.AppendLine("  <style>");
            sb.AppendLine("    body { font-family: Arial, sans-serif; margin: 20px; }");
            sb.AppendLine("    h1, h2 { color: #2F4F4F; }");
            sb.AppendLine("    table { border-collapse: collapse; width: 100%; margin-bottom: 30px; }");
            sb.AppendLine("    th, td { border: 1px solid #ccc; padding: 8px; text-align: left; }");
            sb.AppendLine("    th { background-color: #f2f2f2; }");
            sb.AppendLine("  </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            sb.AppendLine("  <h1>SecureLens HTML Report</h1>");

            // 1) Overall Statistics
            sb.AppendLine("  <h2>Overall Statistics</h2>");
            sb.AppendLine(GenerateOverallStatisticsTable(overallStats, settings));

            // 2) Application Statistics
            sb.AppendLine("  <h2>Application Statistics</h2>");
            sb.AppendLine(GenerateApplicationStatisticsTable(appStats));

            // 3) Terminal Statistics
            sb.AppendLine("  <h2>Terminal Statistics</h2>");
            sb.AppendLine(GenerateTerminalStatisticsTable(terminalStats));

            // 4) Unused AD Groups
            sb.AppendLine("  <h2>Unused AD Groups</h2>");
            sb.AppendLine(GenerateUnusedAdGroupsTable(unusedAdGroups));

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        #region HTML Generators

        private string GenerateOverallStatisticsTable(
            Analyzer.OverallStatisticsResult stats, 
            List<AdminByRequestSetting> settings)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table>");
            sb.AppendLine("  <thead>");
            sb.AppendLine("    <tr><th>Description</th><th>Count</th></tr>");
            sb.AppendLine("  </thead>");
            sb.AppendLine("  <tbody>");

            // Basic rows
            sb.AppendLine($"    <tr><td>Total unique users in environment:</td><td>{stats.TotalUniqueUsers}</td></tr>");
            sb.AppendLine($"    <tr><td>Total unique workstations:</td><td>{stats.TotalUniqueWorkstations}</td></tr>");

            // For each setting
            foreach (var setting in settings)
            {
                if (stats.MembersPerSetting.TryGetValue(setting.Name, out int memberCount))
                {
                    sb.AppendLine($"    <tr><td>Total members able to use '{setting.Name}':</td><td>{memberCount}</td></tr>");
                }
            }

            sb.AppendLine($"    <tr><td>Users with at least 5 elevations in period:</td><td>{stats.UsersWith5Elevations}</td></tr>");

            sb.AppendLine("  </tbody>");
            sb.AppendLine("</table>");
            return sb.ToString();
        }

        private string GenerateApplicationStatisticsTable(
            Dictionary<string, Analyzer.ApplicationStatisticsResult> appStats)
        {
            if (appStats == null || appStats.Count == 0)
            {
                return "<p>No application statistics available.</p>";
            }

            var sortedApps = appStats.OrderByDescending(kvp => kvp.Value.TotalCount).ToList();
            var sb = new StringBuilder();

            sb.AppendLine("<table>");
            sb.AppendLine("  <thead>");
            sb.AppendLine("    <tr>");
            sb.AppendLine("      <th>Total Count</th>");
            sb.AppendLine("      <th>Application</th>");
            sb.AppendLine("      <th>Vendor</th>");
            sb.AppendLine("      <th>Pre-approved</th>");
            sb.AppendLine("      <th>'Technology' Count</th>");
            sb.AppendLine("      <th>'Elevate Terminal Rights' Count</th>");
            sb.AppendLine("      <th>'Global' Count</th>");
            sb.AppendLine("    </tr>");
            sb.AppendLine("  </thead>");
            sb.AppendLine("  <tbody>");

            foreach (var kvp in sortedApps)
            {
                string appName = kvp.Key;
                var stats = kvp.Value;
                sb.AppendLine("    <tr>");
                sb.AppendLine($"      <td>{stats.TotalCount}</td>");
                sb.AppendLine($"      <td>{HtmlEncode(appName)}</td>");
                sb.AppendLine($"      <td>{HtmlEncode(stats.Vendor)}</td>");
                sb.AppendLine($"      <td>{(stats.Preapproved ? "True" : "False")}</td>");
                sb.AppendLine($"      <td>{stats.TechnologyCount}</td>");
                sb.AppendLine($"      <td>{stats.ElevateTerminalRightsCount}</td>");
                sb.AppendLine($"      <td>{stats.GlobalCount}</td>");
                sb.AppendLine("    </tr>");
            }

            sb.AppendLine("  </tbody>");
            sb.AppendLine("</table>");
            return sb.ToString();
        }

        private string GenerateTerminalStatisticsTable(List<Analyzer.TerminalStatisticsRow> terminalStats)
        {
            if (terminalStats == null || terminalStats.Count == 0)
            {
                return "<p>No terminal usage found.</p>";
            }

            var sb = new StringBuilder();

            sb.AppendLine("<table>");
            sb.AppendLine("  <thead>");
            sb.AppendLine("    <tr>");
            sb.AppendLine("      <th>User</th>");
            sb.AppendLine("      <th>Application(s)</th>");
            sb.AppendLine("      <th>Count</th>");
            sb.AppendLine("      <th>Department</th>");
            sb.AppendLine("      <th>Title</th>");
            sb.AppendLine("      <th>Settings(s) used</th>");
            sb.AppendLine("      <th>Types</th>");
            sb.AppendLine("      <th>Source AD Group</th>");
            sb.AppendLine("    </tr>");
            sb.AppendLine("  </thead>");
            sb.AppendLine("  <tbody>");

            foreach (var row in terminalStats)
            {
                sb.AppendLine("    <tr>");
                sb.AppendLine($"      <td>{HtmlEncode(row.User)}</td>");
                sb.AppendLine($"      <td>{HtmlEncode(row.Applications)}</td>");
                sb.AppendLine($"      <td>{row.Count}</td>");
                sb.AppendLine($"      <td>{HtmlEncode(row.Department)}</td>");
                sb.AppendLine($"      <td>{HtmlEncode(row.Title)}</td>");
                sb.AppendLine($"      <td>{HtmlEncode(row.SettingsUsed)}</td>");
                sb.AppendLine($"      <td>{HtmlEncode(row.Types)}</td>");
                sb.AppendLine($"      <td>{HtmlEncode(row.SourceADGroups)}</td>");
                sb.AppendLine("    </tr>");
            }

            sb.AppendLine("  </tbody>");
            sb.AppendLine("</table>");

            return sb.ToString();
        }

        private string GenerateUnusedAdGroupsTable(List<Analyzer.UnusedAdGroupResult> unusedGroups)
        {
            if (unusedGroups == null || unusedGroups.Count == 0)
            {
                return "<p>All AD groups under these settings are involved in elevations. No unused groups found.</p>";
            }

            var sorted = unusedGroups
                .OrderBy(u => u.Setting, StringComparer.OrdinalIgnoreCase)
                .ThenBy(u => u.ADGroup, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("<table>");
            sb.AppendLine("  <thead>");
            sb.AppendLine("    <tr>");
            sb.AppendLine("      <th>AD Group</th>");
            sb.AppendLine("      <th>Setting</th>");
            sb.AppendLine("      <th>Number of Users</th>");
            sb.AppendLine("    </tr>");
            sb.AppendLine("  </thead>");
            sb.AppendLine("  <tbody>");

            foreach (var item in sorted)
            {
                sb.AppendLine("    <tr>");
                sb.AppendLine($"      <td>{HtmlEncode(item.ADGroup)}</td>");
                sb.AppendLine($"      <td>{HtmlEncode(item.Setting)}</td>");
                sb.AppendLine($"      <td>{item.NumberOfUsers}</td>");
                sb.AppendLine("    </tr>");
            }

            sb.AppendLine("  </tbody>");
            sb.AppendLine("</table>");
            return sb.ToString();
        }

        #endregion

        /// <summary>
        /// HTML encoding to avoid issues with special chars in app names, etc.
        /// </summary>
        private string HtmlEncode(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return System.Net.WebUtility.HtmlEncode(text);
        }
    }
}
