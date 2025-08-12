namespace RealtimePPUR.Forms
{
    partial class ChangeTargetForm
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
            targetPercentageText = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            button1 = new Button();
            SuspendLayout();
            // 
            // targetPercentageText
            // 
            targetPercentageText.Font = new Font("Yu Gothic UI", 12F);
            targetPercentageText.Location = new Point(238, 126);
            targetPercentageText.Name = "targetPercentageText";
            targetPercentageText.PlaceholderText = "90";
            targetPercentageText.Size = new Size(127, 29);
            targetPercentageText.TabIndex = 0;
            targetPercentageText.TextAlign = HorizontalAlignment.Center;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI", 15F);
            label1.Location = new Point(12, 123);
            label1.Name = "label1";
            label1.Size = new Size(156, 28);
            label1.TabIndex = 1;
            label1.Text = "目標パーセンテージ";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Yu Gothic UI", 11F);
            label2.Location = new Point(12, 50);
            label2.Name = "label2";
            label2.Size = new Size(332, 60);
            label2.TabIndex = 2;
            label2.Text = "Taiko、Mania限定で、ターゲットという機能があります。\r\nTaiko: 300の数と現在の総ノーツで比較します。\r\nMania: 300G/300の数と現在の総ノーツで比較します。";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Yu Gothic UI", 18F, FontStyle.Bold);
            label3.Location = new Point(12, 9);
            label3.Name = "label3";
            label3.Size = new Size(170, 32);
            label3.TabIndex = 3;
            label3.Text = "ターゲットの設定";
            // 
            // button1
            // 
            button1.Font = new Font("Yu Gothic UI", 14F);
            button1.Location = new Point(132, 170);
            button1.Name = "button1";
            button1.Size = new Size(121, 38);
            button1.TabIndex = 4;
            button1.Text = "決定";
            button1.UseVisualStyleBackColor = true;
            button1.Click += Button1_Click;
            // 
            // ChangeTargetForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(383, 220);
            Controls.Add(button1);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(targetPercentageText);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "ChangeTargetForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ターゲットの設定";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox targetPercentageText;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button button1;
    }
}