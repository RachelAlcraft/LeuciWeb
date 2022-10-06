namespace Leucippus.Models
{
    using System;
    public sealed class ViewServer
    {
        // https://csharpindepth.com/Articles/Singleton 4th version implementation of Singleton

        private static readonly ViewServer instance = new ViewServer();
        private ViewServer()
        {
        }

        public static ViewServer Instance
        {
            get
            {
                return instance;
            }
        }

        public string ColorScheme = "RedBlue";
        public double Cap = 3;
        
    }
}
