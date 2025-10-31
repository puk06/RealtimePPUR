namespace RealtimePPUR.Forms
{
    partial class UnstableRateBar
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
            SuspendLayout();
            // 
            // UnstableRateBar
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(731, 115);
            Name = "UnstableRateBar";
            Text = "UnstableRateBar";
            FormBorderStyle = FormBorderStyle.None;
            TransparencyKey = SystemColors.Control;
            MouseDown += URBar_MouseDown;
            MouseMove += URBar_MouseMove;
            StartPosition = FormStartPosition.CenterScreen;

            ResumeLayout(false);
        }

        #endregion
    }
}