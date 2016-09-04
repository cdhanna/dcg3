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
            using (var game = new TestGame())
            {
                if (args.Length > 0 && args[0] == "server")
                {
                    var server = new NetServer();
                }
                game.Run();
            }
        }
    }
#endif
}
