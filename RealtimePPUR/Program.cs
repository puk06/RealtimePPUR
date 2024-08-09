using System;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using RealtimePPUR.Classes;
using RealtimePPUR.Forms;
using File = System.IO.File;

namespace RealtimePPUR
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            try
            {
                CheckFiles().Wait();
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

        private static async Task CheckFiles()
        {
            if (!Directory.Exists("Updater") || !File.Exists("config.cfg") || !Directory.Exists("src"))
            {
                MessageBox.Show("起動に必要なファイルをダウンロードします。", "ダウンロード", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await new SourceDownloader().DownloadFiles();
                MessageBox.Show("ダウンロードが完了しました！ソフトを起動します！", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
