using System.Windows.Forms;

namespace RealtimePPUR.Forms
{
    partial class ChangePriorityForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangePriorityForm));
            LabelText = new Label();
            sortPriorityList = new ListBox();
            okButton = new Button();
            cancelButton = new Button();
            SuspendLayout();
            // 
            // LabelText
            // 
            LabelText.AutoSize = true;
            LabelText.Font = new System.Drawing.Font("メイリオ", 10F);
            LabelText.Location = new System.Drawing.Point(41, 18);
            LabelText.Margin = new Padding(4, 0, 4, 0);
            LabelText.Name = "LabelText";
            LabelText.Size = new System.Drawing.Size(363, 42);
            LabelText.TabIndex = 0;
            LabelText.Text = "並び替えたい順に項目を動かしてください！\r\nMove the items in the order you wish to sort them!\r\n";
            LabelText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // sortPriorityList
            // 
            sortPriorityList.AllowDrop = true;
            sortPriorityList.Font = new System.Drawing.Font("メイリオ", 15F);
            sortPriorityList.FormattingEnabled = true;
            sortPriorityList.ItemHeight = 30;
            sortPriorityList.Items.AddRange(new object[] { "1: SR: 1.23", "2: SSPP: 300pp", "3: PP: 100 / 200pp", "4: ACC: 99.2 / 99.82%", "5: Hits: 1/2/3/4", "6: IFFCHits: 5/2/3/0", "7: UR: 100", "8: Offset: -2", "9: ManiaScore: 995000", "10: AvgOffset: 1.86", "11: Progress: 50%", "12: HP: 65.7%", "13: Position: #1", "14: HigherDiff: 123456", "15: HighestDiff: 1234567", "16: Score: 12345", "17: BPM: 200.6", "18: Rank: B / A", "19: Notes: 1020" });
            sortPriorityList.Location = new System.Drawing.Point(14, 81);
            sortPriorityList.Margin = new Padding(4);
            sortPriorityList.Name = "sortPriorityList";
            sortPriorityList.Size = new System.Drawing.Size(420, 604);
            sortPriorityList.TabIndex = 1;
            sortPriorityList.DragDrop += SortPriorityList_DragDrop;
            sortPriorityList.DragEnter += SortPriorityList_DragEnter;
            sortPriorityList.MouseDown += SortPriorityList_MouseDown;
            // 
            // okButton
            // 
            okButton.Font = new System.Drawing.Font("メイリオ", 12F);
            okButton.Location = new System.Drawing.Point(295, 701);
            okButton.Margin = new Padding(4);
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(138, 40);
            okButton.TabIndex = 2;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += OkButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Font = new System.Drawing.Font("メイリオ", 12F);
            cancelButton.Location = new System.Drawing.Point(9, 701);
            cancelButton.Margin = new Padding(4);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(138, 39);
            cancelButton.TabIndex = 3;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += CancelButton_Click;
            // 
            // ChangePriorityForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(449, 755);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            Controls.Add(sortPriorityList);
            Controls.Add(LabelText);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ChangePriorityForm";
            Text = "Change InGameOverlay Priority";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListBox sortPriorityList;
        private Label LabelText;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
    }
}