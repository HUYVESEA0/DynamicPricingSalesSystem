using DynamicPricingSalesSystem;

namespace DynamicPricingSalesSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var mainMenu = new MainMenu();
                mainMenu.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
