namespace Leucippus.Models
{

    using System;
    using System.Runtime.InteropServices;

    
    


    public class DllInterface
    {
        [DllImport("LeuciDll.dll")]
        public static extern uint fibonacci_init(uint x, uint y);
        
        [DllImport("LeuciDll.dll")]
        public static extern bool fibonacci_next();
        [DllImport("LeuciDll.dll")]
        public static extern uint fibonacci_current();
        
        [DllImport("LeuciDll.dll")]
        public static extern uint fibonacci_index();

        public DllInterface()
        {
            uint a = fibonacci_init(1, 4);
            bool b = fibonacci_next();
            a = fibonacci_current();
            b = fibonacci_next();
            a = fibonacci_current();
            b = fibonacci_next();
            a = fibonacci_current();
            b = fibonacci_next();
        }




    }
    
}
