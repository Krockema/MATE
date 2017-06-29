using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SharpSimExamples
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Edit this part to run different examples
            Application.Run(new Example5());
        }
    }
}
