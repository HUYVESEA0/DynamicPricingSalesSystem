using DynamicPricingSalesSystem;

try
{
    var application = new Application();
    await application.RunAsync();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Fatal error: {ex.Message}");
    Console.ResetColor();
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}
