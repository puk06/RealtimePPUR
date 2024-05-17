using System;
using System.Windows.Forms;
using System.Globalization;
using System.IO;

namespace RealtimePPUR
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            if (!File.Exists("./src/Fonts/MPLUSRounded1c-ExtraBold.ttf"))
            {
                MessageBox.Show("MPLUSRounded1c-ExtraBoldフォントファイルが存在しません。ソフトをもう一度ダウンロードしてください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists("./src/Fonts/Nexa Light.otf"))
            {
                MessageBox.Show("Nexa Lightフォントファイルが存在しません。ソフトをもう一度ダウンロードしてください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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
