using System;

namespace AIGame
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (AIGame game = new AIGame())
            {
                game.Run();
            }
        }
    }
}
