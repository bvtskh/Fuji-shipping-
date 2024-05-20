using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using UMC.OQC.Models;

namespace FujiXeroxShippingSystem
{
    static class Program
    {
        public static PVSService.USERSEntity CurrentUser { get; set; }
        //public static PVSService.BARCODE_RULESEntity CurrentRule { get; set; }
        public static Models CurrentModels { get; set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormLogin());
        }
    }
}
