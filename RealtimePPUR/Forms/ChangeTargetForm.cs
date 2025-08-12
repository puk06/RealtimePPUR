using RealtimePPUR.Utils;

namespace RealtimePPUR.Forms;

public partial class ChangeTargetForm : Form
{
    public ChangeTargetForm(int currentValue)
    {
        InitializeComponent();
        targetPercentageText.Text = currentValue.ToString();
    }

    internal int selectedValue = 90;

    private void Button1_Click(object sender, EventArgs e)
    {
        if (!int.TryParse(targetPercentageText.Text, out selectedValue))
        {
            FormUtils.ShowErrorMessageBox("値が正しくありません。80や90などの数字を入力してください");
            return;
        }

        Close();
    }
}
