using System.Drawing.Drawing2D;

namespace RealtimePPUR.Utils;

internal class FormUtils
{
    internal static void ShowErrorMessageBox(string message) =>
        MessageBox.Show(message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

    internal static void ShowInformationMessageBox(string message) =>
        MessageBox.Show(message, "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
    internal static void ToggleChecked(object? sender, EventArgs? e)
    {
        if (sender is not ToolStripMenuItem menuItem) return;

        menuItem.Checked = !menuItem.Checked;
        LogUtils.DebugLogger($"{menuItem.Text} is now {menuItem.Checked}");

        ShowParentToolStrip(sender, e);
    }

    internal static Region RoundCorners(int width, int height)
    {
        const int radius = 11;
        const int diameter = radius * 2;

        GraphicsPath gp = new();
        gp.AddPie(0, 0, diameter, diameter, 180, 90);
        gp.AddPie(width - diameter, 0, diameter, diameter, 270, 90);
        gp.AddPie(0, height - diameter, diameter, diameter, 90, 90);
        gp.AddPie(width - diameter, height - diameter, diameter, diameter, 0, 90);
        gp.AddRectangle(new Rectangle(radius, 0, width - diameter, height));
        gp.AddRectangle(new Rectangle(0, radius, radius, height - diameter));
        gp.AddRectangle(new Rectangle(width - radius, radius, radius, height - diameter));

        return new Region(gp);
    }

    /// <summary>
    /// 親のToolStripを表示します。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal static void ShowParentToolStrip(object? sender, EventArgs? e)
    {
        if (sender == null) return;
        if (((ToolStripMenuItem)sender).GetCurrentParent() is ToolStripDropDownMenu dropDown)
        {
            var ownerItem = dropDown.OwnerItem;
            if (ownerItem == null) return;
            dropDown.Show(ownerItem.Bounds.Location);
        }
    }
}
