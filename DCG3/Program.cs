using DCG.Framework.Net;
using System;

namespace DCG3
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                using (var game = new TestNetGame())
                {
                    game.Run();
                }
            } else if (args[0] == "server")
            {
                using (var game = new TestNetGame(new NetServer()))
                {
                    game.Run();
                }
            }
            else if (args.Length > 0)
            {
                using (var game = new TestNetGame(args[0], args[1]))
                {
                    game.Run();
                }
            } else
            {
                using (var game = new TestNetGame())
                {
                    game.Run();
                }
            }

            
        }
    }
#endif
}
