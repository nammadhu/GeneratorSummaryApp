namespace GeneratorSummaryApp
{
    public partial class Program
    {
        // Helper methods for colored console output
        private static void PrintTitle(string message)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"===== {message} =====");
            Console.ResetColor();
        }

        private static void PrintInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void PrintTerminate(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.WriteLine("Application is exiting!");
            Console.ResetColor();
        }

        private static void PrintColor(string message, ConsoleColor consoleColor = ConsoleColor.Cyan,
            ConsoleColor? backgroundColor = null)
        {
            Console.ForegroundColor = consoleColor;
            if (backgroundColor != null)
                Console.BackgroundColor = backgroundColor ?? ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}