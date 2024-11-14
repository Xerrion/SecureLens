using SharpFuzz;
using Xunit;

namespace SecureLens.Tests
{
    public class FuzzingTests
    {
        [Fact]
        public void FuzzContainsSecure()
        {
            // Kør fuzz testing på ContainsSecure metoden.
            Fuzzer.LibFuzzer.Run(input =>
            {
                // Konverter input til en streng
                var text = System.Text.Encoding.UTF8.GetString(input);
                bool result = SecureLens.StringOperations.ContainsSecure(text);

                // Vi antager, at ContainsSecure ikke skal kaste undtagelser.
                // Hvis en undtagelse kastes, vil SharpFuzz fange det og rapportere det.
            });
        }
    }
}