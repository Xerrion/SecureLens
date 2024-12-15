using System;
using System.Threading.Tasks;
using DefaultNamespace;
using SecureLens;
using System.Collections.Generic;

namespace DefaultNamespace
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var auditParams = new Dictionary<string, string>
            {
                { "take", "1000" }, // Adjust as needed, maximum is 10000
                { "wantscandetails", "1" }, // Include scan details
                { "startdate", "2023-01-01" },
                { "enddate", "2025-12-31" },
                { "status", "Finished" },
                { "type", "app" }
            };

            // Define the settings and their corresponding AD groups
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
            
            var mySettings = new List<AdminByRequestSetting>();
            foreach (var kv in settingsGroups)
            {
                mySettings.Add(new AdminByRequestSetting(kv.Key, kv.Value));
            }
            
            Console.WriteLine("=== SecureLens Console Application ===");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Choose 'cache' or 'online': ");
            Console.ResetColor();
            
            string mode = Console.ReadLine()?.Trim().ToLower();

            if (mode == "cache")
            {
                Console.WriteLine("You have chosen 'cache' mode. Loading from local JSON...");

                // Initialize AdminByRequestClient with an empty/dummy API key, since we won't call the real API
                var client = new AdminByRequestClient("");
                foreach (var entry in settingsGroups)
                {
                    string settingName = entry.Key;
                    List<string> activeDirectoryGroups = entry.Value;
                    client.CreateSetting(settingName, activeDirectoryGroups);
                }

                // Paths to your cached JSON files
                string cachedInventoryPath  = @"../../../../MockData/cached_inventory.json";
                string cachedAuditLogsPath  = @"../../../../MockData/cached_auditlogs.json";
                // AD cache files
                string cachedAdGroupsPath   = @"../../../../MockData/cached_adgroup_queries.json";
                string cachedAdMembersPath  = @"../../../../MockData/cached_admember_queries.json";

                // Load Cached Inventory
                List<InventoryLogEntry> inventory = client.LoadCachedInventoryData(cachedInventoryPath);

                // Load Cached Audit Logs
                List<AuditLogEntry> auditLogs = client.LoadCachedAuditLogs(cachedAuditLogsPath);

                // Now load AD cache data
                var adClient = new ActiveDirectoryClient();
                adClient.LoadCachedGroupData(cachedAdGroupsPath);
                adClient.LoadCachedAdData(cachedAdMembersPath);

                // ---------------------------------------------------------------------
                // Incorporate DataHandler to combine everything into CompletedUser list
                // ---------------------------------------------------------------------
                var dataHandler = new DataHandler(auditLogs, inventory, adClient);
                List<CompletedUser> completedUsers = dataHandler.BuildCompletedUsers();

                Console.WriteLine($"[CACHE] Built {completedUsers.Count} CompletedUsers from the data.");

                // Example: print details for the first user
                if (completedUsers.Count > 0)
                {
                    var first = completedUsers[0];
                    Console.WriteLine($"\nFirst CompletedUser: {first.AccountName}");
                    Console.WriteLine($"  Audit Log Entries: {first.AuditLogEntries.Count}");
                    Console.WriteLine($"  Inventory Entries: {first.InventoryLogEntries.Count}");
                    Console.WriteLine($"  AD User: {first.ActiveDirectoryUser?.DistinguishedName}");
                    Console.WriteLine($"  AD Groups: {string.Join(", ", first.ActiveDirectoryUser?.Groups.Select(g => g.Name))}");
                }
                
                // Now create the Analyzer with your CompletedUsers & mySettings
                var analyzer = new Analyzer(completedUsers, mySettings);

                // Compute & print the "overall statistics"
                Analyzer.OverallStatisticsResult stats = analyzer.ComputeOverallStatistics();
                List<Analyzer.UnusedAdGroupResult> unusedGroups = analyzer.ComputeUnusedAdGroups(adClient);
                // 1) Compute Application Statistics
                var appStats = analyzer.ComputeApplicationStatistics();
                analyzer.PrintOverallStatistics(stats);
                analyzer.PrintApplicationStatistics(appStats);
                analyzer.PrintUnusedAdGroups(unusedGroups);
                
            }
            else
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
                char[] apiKeyChars = apiKeyString.ToCharArray();

                // Create client with the entered API key
                var client = new AdminByRequestClient(apiKeyString);

                // Fetch Inventory Data (online)
                Console.WriteLine("=== Inventory Data ===");
                List<InventoryLogEntry> inventory = await client.FetchInventoryDataAsync();
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
                List<AuditLogEntry> auditLogs = await client.FetchAuditLogsAsync(auditParams);
                if (auditLogs.Count > 0)
                {
                    Console.WriteLine($"Fetched {auditLogs.Count} audit logs (online).");
                }
                else
                {
                    Console.WriteLine("No audit log data fetched (online).");
                }

                // Overwrite the API key in memory for security
                for (int i = 0; i < apiKeyChars.Length; i++)
                {
                    apiKeyChars[i] = '\0'; // Overwrite with null char
                }
                apiKeyString = null; // Allow GC
                
                Console.WriteLine("\n=== Data Fetched ===");

                
            }
            
            Console.WriteLine("\n=== Program Finished ===");
        }
    }
}
