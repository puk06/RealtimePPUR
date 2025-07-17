using RealtimePPUR.Classes;
using RealtimePPUR.Forms;
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
                CheckFiles().Wait();
                CultureInfo.CurrentCulture = new CultureInfo("en-us");
                CultureInfo.CurrentUICulture = new CultureInfo("en-us");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.Run(new RealtimePpur());
            }
            catch (Exception softwareError)
            {
                MessageBox.Show($"ソフトの起動に失敗しました。\nエラー内容: {softwareError}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static async Task CheckFiles()
        {
            if (!Directory.Exists("Updater") || !File.Exists("config.cfg") || !Directory.Exists("src") || !File.Exists("./src/Fonts/MPLUSRounded1c-ExtraBold.ttf") || !File.Exists("./src/Fonts/IBMPlexSans-Light.ttf"))
            {
                MessageBox.Show("起動に必要なファイルをダウンロードします。", "ダウンロード", MessageBoxButtons.OK, MessageBoxIcon.Information);
#pragma warning disable CS0618 // 型またはメンバーが旧型式です
                await SourceDownloader.DownloadFiles();
#pragma warning restore CS0618
                MessageBox.Show("ダウンロードが完了しました！ソフトを起動します！", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
