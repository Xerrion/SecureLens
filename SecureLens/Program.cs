using System.Net.Http.Headers;
using SecureLens.Data;
using SecureLens.Logging;
using SecureLens.Models;
using SecureLens.Services;

namespace SecureLens
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var logger = new ConsoleLogger();

            var auditParams = new Dictionary<string, string>
            {
                { "take", "1000" }, // Adjust as needed
                { "wantscandetails", "1" },
                { "startdate", "2023-01-01" },
                { "enddate", "2025-12-31" },
                { "status", "Finished" },
                { "type", "app" }
            };

            // Hardcoded settings groups for developer cache mode - anonymized data
            Dictionary<string, List<string>> settingsGroups = new Dictionary<string, List<string>>
            {
                { "Technology", new List<string> { "Technology" } },
                {
                    "Elevate Terminal Rights", new List<string>
                    {
                        "Technology", "Servicedesk", "Tooling", "Cloud Developer", "Infrastructure",
                        "Production Support", "Cloud Admin", "Content Technology", "Data Science",
                        "Developers", "Access Management", "Ad & Sales", "Business Services", "Content Metadata", "Management",
                        "Entertainment", "Economics", "Finance"
                    }
                },
                {
                    "Global", new List<string>
                    {
                        "Journalism", "Sport", "Graphical Designer",  
                        "Advertisement", "HR", "Legal", "Marketing"
                    }
                }
            };
            
            // Convert settingsGroups dictionary to a list of AdminByRequestSetting
            var mySettings = settingsGroups
                .Select(kv => new AdminByRequestSetting(kv.Key, kv.Value))
                .ToList();

            Console.WriteLine("=== SecureLens Console Application ===");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Choose 'cache' or 'online': ");
            Console.ResetColor();
            
            string mode = Console.ReadLine()?.Trim().ToLower();

            if (mode == "cache")
            {
                Console.WriteLine("You have chosen 'cache' mode. Loading from local JSON...");

                try
                {
                    // Opret ABR repository + service
                    var abrRepo = new AdminByRequestRepository("", logger);
                    var abrService = new AdminByRequestService(abrRepo);

                    foreach (var entry in settingsGroups)
                    {
                        string settingName = entry.Key;
                        List<string> activeDirectoryGroups = entry.Value;
                        abrService.CreateSetting(settingName, activeDirectoryGroups);
                        Console.WriteLine($"Created setting: {settingName} containing {activeDirectoryGroups.Count} AD-groups");
                    }

                    // Paths cached JSON files
                    string cachedInventoryPath  = @"../../../../MockData/cached_inventory.json";
                    string cachedAuditLogsPath  = @"../../../../MockData/cached_auditlogs.json";
                    string cachedAdGroupsPath   = @"../../../../MockData/cached_adgroup_queries.json";
                    string cachedAdMembersPath  = @"../../../../MockData/cached_admember_queries.json";

                    // Load Cached Inventory
                    Console.WriteLine($"Loading cached inventory data from file: {cachedInventoryPath}");
                    List<InventoryLogEntry> inventory = abrRepo.LoadCachedInventoryData(cachedInventoryPath);
                    Console.WriteLine($"Loaded {inventory.Count} cached inventory records.");

                    // Load Cached Audit Logs
                    Console.WriteLine($"Loading cached audit logs from file: {cachedAuditLogsPath}");
                    List<AuditLogEntry> auditLogs = abrRepo.LoadCachedAuditLogs(cachedAuditLogsPath);
                    Console.WriteLine($"Loaded {auditLogs.Count} cached audit log records.");

                    // Load AD cache data
                    Console.WriteLine($"Loading cached Active Directory groups from {cachedAdGroupsPath}");
                    var adRepo = new ActiveDirectoryRepository(logger);
                    adRepo.LoadGroupDataFromFile(cachedAdGroupsPath);
                    adRepo.LoadUserDataFromFile(cachedAdMembersPath);

                    // Combine everything into CompletedUser list
                    Console.WriteLine("Building Completed Users to prepare analysis...");
                    var dataHandler = new DataHandler(auditLogs, inventory, adRepo, logger);
                    List<CompletedUser> completedUsers = dataHandler.BuildCompletedUsers();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[CACHE] Built {completedUsers.Count} CompletedUsers from the data.");
                    Console.ResetColor();
                    
                    // Initialize Analyzer with CompletedUsers and Settings
                    var analyzer = new Analyzer(completedUsers, mySettings);

                    // Compute everything
                    var overallStats = analyzer.ComputeOverallStatistics();
                    var unusedGroups = analyzer.ComputeUnusedAdGroups(adRepo);  // <<< Instead of adClient
                    var appStats = analyzer.ComputeApplicationStatistics();
                    List<Analyzer.TerminalStatisticsRow> terminalStats = analyzer.ComputeTerminalStatistics();

                    // Generate HTML report
                    var htmlWriter = new HtmlReportWriter();
                    string htmlContent = htmlWriter.BuildHtmlReport(
                        overallStats,
                        appStats,
                        terminalStats,
                        unusedGroups,
                        mySettings
                    );

                    // Write to local file
                    string outputHtmlFile = @"C:\Users\jeppe\Desktop\report.html";
                    File.WriteAllText(outputHtmlFile, htmlContent);
                    Console.WriteLine($"[INFO] HTML report successfully written to '{outputHtmlFile}'.");
                }
                catch (ArgumentNullException ex)
                {
                    // Handle specific exceptions from Analyzer or other classes
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    // Handle any unexpected exceptions
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                    Console.ResetColor();
                }
            }
            else if (mode == "online")
            {
                Console.WriteLine("You have chosen 'online' mode.");

                string apiKeyString = string.Empty;
                bool isValid = false;

                // Loop until a valid API key is entered
                do
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Please enter your API key: ");
                    Console.ResetColor();

                    List<char> keyChars = new List<char>();
                    ConsoleKeyInfo keyInfo;

                    // Read input character-by-character without displaying it
                    do
                    {
                        keyInfo = Console.ReadKey(true);
                        if (!char.IsControl(keyInfo.KeyChar))
                        {
                            keyChars.Add(keyInfo.KeyChar);
                            Console.Write("*"); // Mask the input
                        }
                        else if (keyInfo.Key == ConsoleKey.Backspace && keyChars.Count > 0)
                        {
                            keyChars.RemoveAt(keyChars.Count - 1);
                            Console.Write("\b \b");
                        }
                    } while (keyInfo.Key != ConsoleKey.Enter);

                    Console.WriteLine(); // Move to the next line after input

                    char[] apiKeyArray = keyChars.ToArray();
                    apiKeyString = new string(apiKeyArray);

                    // Validate the API key
                    isValid = ApiKeyValidator.IsValid(apiKeyString);

                    if (!isValid)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid API key format. Please try again.");
                        Console.ResetColor();
                    }
                } while (!isValid);

                // Proceed with the valid API key
                char[] finalApiKeyChars = apiKeyString.ToCharArray();
                try
                {
                    var loggerOnline = new ConsoleLogger();
                    var abrRepo = new AdminByRequestRepository(apiKeyString, loggerOnline);
                    
                    // Fetch Inventory Data (online)
                    Console.WriteLine("=== Inventory Data ===");
                    List<InventoryLogEntry> inventory = await abrRepo.FetchInventoryDataAsync();
                    if (inventory.Count > 0)
                    {
                        Console.WriteLine($"Fetched {inventory.Count} inventory logs (online).");
                    }
                    else
                    {
                        Console.WriteLine("No inventory data fetched (online).");
                    }

                    // Fetch Audit Logs (online)
                    Console.WriteLine("\n=== Audit Logs ===");
                    List<AuditLogEntry> auditLogs = await abrRepo.FetchAuditLogsAsync(auditParams);
                    if (auditLogs.Count > 0)
                    {
                        Console.WriteLine($"Fetched {auditLogs.Count} audit logs (online).");
                    }
                    else
                    {
                        Console.WriteLine("No audit log data fetched (online).");
                    }

                    // Overwrite the API key in memory for security
                    for (int i = 0; i < finalApiKeyChars.Length; i++)
                    {
                        finalApiKeyChars[i] = '\0';
                    }
                    apiKeyString = null;

                    Console.WriteLine("\n=== Data Fetched ===");

                    // Combine everything into CompletedUser list
                    // AD repository (live AD or no AD?), 
                    // For demo just instantiate, but no file loading if purely online
                    var adRepo = new ActiveDirectoryRepository(loggerOnline);

                    var dataHandler = new DataHandler(auditLogs, inventory, adRepo, loggerOnline);
                    List<CompletedUser> completedUsers = dataHandler.BuildCompletedUsers();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[ONLINE] Built {completedUsers.Count} CompletedUsers from the data.");
                    Console.ResetColor();

                    // Initialize Analyzer with CompletedUsers and Settings
                    var analyzer = new Analyzer(completedUsers, mySettings);

                    // Compute everything
                    var overallStats = analyzer.ComputeOverallStatistics();
                    var unusedGroups = analyzer.ComputeUnusedAdGroups(adRepo); // pass IActiveDirectoryRepository
                    var appStats = analyzer.ComputeApplicationStatistics();
                    List<Analyzer.TerminalStatisticsRow> terminalStats = analyzer.ComputeTerminalStatistics();

                    // Generate HTML report
                    var htmlWriter = new HtmlReportWriter();
                    string htmlContent = htmlWriter.BuildHtmlReport(
                        overallStats,
                        appStats,
                        terminalStats,
                        unusedGroups,
                        mySettings
                    );

                    // Write to local file
                    string outputHtmlFile = @"C:\Users\jeppe\Desktop\report.html";
                    File.WriteAllText(outputHtmlFile, htmlContent);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[INFO] HTML report successfully written to '{outputHtmlFile}'.");
                    Console.ResetColor();
                }
                catch (ArgumentNullException ex)
                {
                    // Handle specific exceptions from Analyzer or other classes
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    // Handle any unexpected exceptions
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                    Console.ResetColor();
                }
                finally
                {
                    // Overwrite the API key in memory for security in case of exceptions
                    for (int i = 0; i < finalApiKeyChars.Length; i++)
                    {
                        finalApiKeyChars[i] = '\0';
                    }
                    apiKeyString = null;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid mode selected. Please choose 'cache' or 'online'.");
                Console.ResetColor();
            }

            Console.WriteLine("\n=== Program Finished ===");
        }
    }
}
