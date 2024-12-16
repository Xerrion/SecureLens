// UserInterface.cs
namespace SecureLens.UI
{
    public class UserInterface
    {
        public string GetMode()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Choose 'cache' or 'online': ");
            Console.ResetColor();

            string mode = Console.ReadLine()?.Trim().ToLower();
            return mode ?? "cache"; // Standard til 'cache' hvis null
        }

        public string GetApiKey()
        {
            string apiKey = string.Empty;
            bool isValid = false;

            do
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Please enter your API key: ");
                Console.ResetColor();

                List<char> keyChars = new List<char>();
                ConsoleKeyInfo keyInfo;

                // Læs input karakter-for-karakter uden at vise det
                do
                {
                    keyInfo = Console.ReadKey(true);
                    if (!char.IsControl(keyInfo.KeyChar))
                    {
                        keyChars.Add(keyInfo.KeyChar);
                        Console.Write("*"); // Masker input
                    }
                    else if (keyInfo.Key == ConsoleKey.Backspace && keyChars.Count > 0)
                    {
                        keyChars.RemoveAt(keyChars.Count - 1);
                        Console.Write("\b \b");
                    }
                } while (keyInfo.Key != ConsoleKey.Enter);

                Console.WriteLine(); // Gå til næste linje efter input

                apiKey = new string(keyChars.ToArray());

                // Valider API-nøglen
                isValid = ApiKeyValidator.IsValid(apiKey);

                if (!isValid)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid API key format. Please try again.");
                    Console.ResetColor();
                }
            } while (!isValid);

            return apiKey;
        }
    }
}
