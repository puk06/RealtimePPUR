using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RealtimePPUR.Forms
{
    public partial class ChangePriorityForm : Form
    {
        private int index;
        private readonly int max;

        public ChangePriorityForm()
        {
            InitializeComponent();
            max = sortPriorityList.Items.Count;
        }

        private void SortPriorityList_MouseDown(object o, MouseEventArgs e)
        {
            Point p = MousePosition;
            p = sortPriorityList.PointToClient(p);
            index = sortPriorityList.IndexFromPoint(p);
            if (index > -1) sortPriorityList.DoDragDrop(sortPriorityList.Items[index].ToString(), DragDropEffects.Copy);
        }

        private void SortPriorityList_DragEnter(object o, DragEventArgs e) => e.Effect = DragDropEffects.Copy;

        private void SortPriorityList_DragDrop(object o, DragEventArgs e)
        {
            string str = e.Data.GetData(DataFormats.Text).ToString();
            Point p = MousePosition;
            p = sortPriorityList.PointToClient(p);
            int ind = sortPriorityList.IndexFromPoint(p);
            if (!(ind > -1 && ind < max)) return;
            sortPriorityList.Items[index] = sortPriorityList.Items[ind];
            sortPriorityList.Items[ind] = str;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            var sortPriority = (from string item in sortPriorityList.Items select int.Parse(item.Split(':')[0])).ToList();
            string message = string.Join("/", sortPriority);
            try
            {
                Clipboard.SetText(message);
                MessageBox.Show($"Config.cfgのINGAMEOVERLAYPRIORITYの所をクリップボードに自動保存された文章に書き換えてください！再起動したら反映します！\n\nRewrite the INGAMEOVERLAYPRIORITY section of Config.cfg with the text automatically saved to the clipboard! It will be reflected after rebooting!\n\nコピーされた文章(Copied text): {message}", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show($"テキストのコピーに失敗しました。自分でConfig.cfgのINGAMEOVERLAYPRIORITYの所を下の値に書き換えてください。再起動したら反映します！\n\nFailed to copy the text. Please rewrite INGAMEOVERLAYPRIORITY in Config.cfg by yourself to the text below. It will be reflected after rebooting!\n\nコピーされる予定だった文章(Text that was to be copied): {message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e) => Close();
    }
}
