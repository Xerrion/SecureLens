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
            
            Console.WriteLine("=== SecureLens Console Application ===");

            

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Choose 'cache' or 'online': ");


            if (Console.ReadLine() != "cache")
            {
                Console.WriteLine("You have chosen 'online'");
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

                    // Read input character by character without displaying it
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
                            // Remove the last '*' from the console
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

                // Fetch Inventory Data
                Console.WriteLine("=== Inventory Data ===");
                List<InventoryLogEntry> inventory = await client.FetchInventoryDataAsync();

                if (inventory.Count > 0)
                {
                    Console.WriteLine($"Fetched {inventory.Count} inventory logs.");
                }
                else
                {
                    Console.WriteLine("No inventory data fetched.");
                }

                // Fetch Audit Logs without Pagination
                Console.WriteLine("\n=== Audit Logs ===");

                List<AuditLogEntry> auditLogs = await client.FetchAuditLogsAsync(auditParams);

                if (auditLogs.Count > 0)
                {
                    Console.WriteLine($"Fetched {auditLogs.Count} audit logs.");
                }
                else
                {
                    Console.WriteLine("No audit log data fetched.");
                }

                // Overwrite the API key in memory for security
                for (int i = 0; i < apiKeyChars.Length; i++)
                {
                    apiKeyChars[i] = '\0'; // Overwrite with null char
                }

                apiKeyString = null; // Allow garbage collection
            }
        }
    }
}