using System;
using System.Windows.Forms;

namespace RealtimePPUR
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
            this.LabelText = new System.Windows.Forms.Label();
            this.sortPriorityList = new System.Windows.Forms.ListBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LabelText
            // 
            this.LabelText.AutoSize = true;
            this.LabelText.Font = new System.Drawing.Font("メイリオ", 10F);
            this.LabelText.Location = new System.Drawing.Point(8, 11);
            this.LabelText.Name = "LabelText";
            this.LabelText.Size = new System.Drawing.Size(363, 42);
            this.LabelText.TabIndex = 0;
            this.LabelText.Text = "並び替えたい順に項目を動かしてください！\r\nMove the items in the order you wish to sort them!\r\n";
            this.LabelText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // sortPriorityList
            // 
            this.sortPriorityList.AllowDrop = true;
            this.sortPriorityList.Font = new System.Drawing.Font("メイリオ", 15F);
            this.sortPriorityList.FormattingEnabled = true;
            this.sortPriorityList.ItemHeight = 30;
            this.sortPriorityList.Items.AddRange(new object[] { "1: SR: 1.23", "2: SSPP: 300pp", "3: PP: 100 / 200pp", "4: ACC: 100%", "5: Hits: 1/2/3/4", "6: ifFCHits: 5/2/3/0", "7: UR: 100", "8: OffsetHelp: -2", "9: ManiaScore: 995000", "10: AvgOffset: 1.86", "11: Progress: 50%", "12: HP: 65.7%", "13: Position: #1", "14: HigherDiff: 123456", "15: HighestDiff: 1234567", "16: Score: 12345" });
            this.sortPriorityList.Location = new System.Drawing.Point(12, 65);
            this.sortPriorityList.Name = "sortPriorityList";
            this.sortPriorityList.Size = new System.Drawing.Size(361, 484);
            this.sortPriorityList.TabIndex = 1;
            this.sortPriorityList.DragDrop += new System.Windows.Forms.DragEventHandler(this.sortPriorityList_DragDrop);
            this.sortPriorityList.DragEnter += new System.Windows.Forms.DragEventHandler(this.sortPriorityList_DragEnter);
            this.sortPriorityList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.sortPriorityList_MouseDown);
            // 
            // okButton
            // 
            this.okButton.Font = new System.Drawing.Font("メイリオ", 12F);
            this.okButton.Location = new System.Drawing.Point(253, 561);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(118, 32);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Font = new System.Drawing.Font("メイリオ", 12F);
            this.cancelButton.Location = new System.Drawing.Point(8, 561);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(118, 31);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // ChangePriorityForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(385, 604);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.sortPriorityList);
            this.Controls.Add(this.LabelText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChangePriorityForm";
            this.Text = "Change InGameOverlay Priority";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListBox sortPriorityList;
        private Label LabelText;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
    }
}