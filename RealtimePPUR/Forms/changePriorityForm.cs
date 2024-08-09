using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static RealtimePPUR.Classes.Helper;

namespace RealtimePPUR.Forms
{
    public partial class ChangePriorityForm : Form
    {
        private int index;
        public ChangePriorityForm()
        {
            InitializeComponent();
        }

        private void SortPriorityList_MouseDown(object o, MouseEventArgs e)
        {
            Point p = MousePosition;
            p = sortPriorityList.PointToClient(p);
            index = sortPriorityList.IndexFromPoint(p);
            if (index > -1) sortPriorityList.DoDragDrop(sortPriorityList.Items[index].ToString(), DragDropEffects.Move);
        }

        private void SortPriorityList_DragEnter(object o, DragEventArgs e) => e.Effect = DragDropEffects.Move;

        private void SortPriorityList_DragDrop(object o, DragEventArgs e)
        {
            string str = e.Data.GetData(DataFormats.Text).ToString();
            Point p = MousePosition;
            p = sortPriorityList.PointToClient(p);
            int ind = sortPriorityList.IndexFromPoint(p);

            if (ind == -1 || ind == sortPriorityList.Items.Count - 1)
            {
                sortPriorityList.Items.RemoveAt(index);
                sortPriorityList.Items.Add(str);
            }
            else if (ind != index)
            {
                sortPriorityList.Items.RemoveAt(index);
                sortPriorityList.Items.Insert(ind, str);
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            var sortPriority = (from string item in sortPriorityList.Items select int.Parse(item.Split(':')[0])).ToList();
            string message = string.Join("/", sortPriority);
            try
            {
                const string filePath = "Config.cfg";
                var param = new Dictionary<string, string>
                {
                    { "INGAMEOVERLAYPRIORITY", message }
                };
                WriteConfigFile(filePath, param);
                MessageBox.Show("Config.cfgのINGAMEOVERLAYPRIORITYの値を変更しました。再起動したら反映します！\n\nChanged the value of INGAMEOVERLAYPRIORITY in Config.cfg. It will be reflected after rebooting!", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
