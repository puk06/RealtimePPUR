using RealtimePPUR.Utils;

namespace RealtimePPUR.Forms;

public partial class ChangePriorityForm : Form
{
    internal event EventHandler? PriorityChanged;


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
        if (index > -1) sortPriorityList.DoDragDrop(sortPriorityList.Items[index]?.ToString() ?? "", DragDropEffects.Move);
    }

    private void SortPriorityList_DragEnter(object o, DragEventArgs e) => e.Effect = DragDropEffects.Move;

    private void SortPriorityList_DragDrop(object o, DragEventArgs e)
    {
        string str = e.Data?.GetData(DataFormats.Text)?.ToString() ?? "";
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

        PriorityChanged?.Invoke(message, null!);

        try
        {
            const string filePath = "Config.cfg";
            var param = new Dictionary<string, string>
            {
                { "INGAMEOVERLAYPRIORITY", message }
            };
            ConfigUtils.WriteConfigFile(filePath, param);
            MessageBox.Show("値の変更を反映し、保存しました。", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("値の自動保存に失敗しました。手動でConfig保存してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
    }

    private void CancelButton_Click(object sender, EventArgs e)
        => Close();
}
