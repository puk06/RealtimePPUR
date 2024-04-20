using System;
using System.Windows.Forms;
using System.Globalization;

namespace RealtimePPUR
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("en-us");
                CultureInfo.CurrentUICulture = new CultureInfo("en-us");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new RealtimePpur());
            }
            catch (Exception softwareError)
            {
                MessageBox.Show($"ソフトの起動に失敗しました。\nエラー内容: {softwareError.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
