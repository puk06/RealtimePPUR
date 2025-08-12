namespace RealtimePPUR.Forms
{
    partial class StrainGraphForm
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
            StrainGraphPlot = new OxyPlot.WindowsForms.PlotView();
            RhythmCheckBox = new CheckBox();
            ReadingCheckBox = new CheckBox();
            ColourCheckBox = new CheckBox();
            Stamina1CheckBox = new CheckBox();
            Stamina2CheckBox = new CheckBox();
            label1 = new Label();
            progressLine = new Label();
            SuspendLayout();
            // 
            // StrainGraphPlot
            // 
            StrainGraphPlot.BackColor = SystemColors.Control;
            StrainGraphPlot.Location = new Point(0, 0);
            StrainGraphPlot.Name = "StrainGraphPlot";
            StrainGraphPlot.PanCursor = Cursors.Hand;
            StrainGraphPlot.Size = new Size(951, 279);
            StrainGraphPlot.TabIndex = 0;
            StrainGraphPlot.Text = "plotView1";
            StrainGraphPlot.ZoomHorizontalCursor = Cursors.SizeWE;
            StrainGraphPlot.ZoomRectangleCursor = Cursors.SizeNWSE;
            StrainGraphPlot.ZoomVerticalCursor = Cursors.SizeNS;
            StrainGraphPlot.MouseMove += plotView1_MouseMove;
            // 
            // RhythmCheckBox
            // 
            RhythmCheckBox.AutoSize = true;
            RhythmCheckBox.BackColor = SystemColors.Control;
            RhythmCheckBox.Checked = true;
            RhythmCheckBox.CheckState = CheckState.Checked;
            RhythmCheckBox.Font = new Font("Yu Gothic UI", 16F);
            RhythmCheckBox.ForeColor = SystemColors.ControlText;
            RhythmCheckBox.Location = new Point(12, 285);
            RhythmCheckBox.Name = "RhythmCheckBox";
            RhythmCheckBox.Size = new Size(123, 34);
            RhythmCheckBox.TabIndex = 1;
            RhythmCheckBox.Text = "Unknown";
            RhythmCheckBox.UseVisualStyleBackColor = false;
            RhythmCheckBox.CheckedChanged += CheckChanged;
            // 
            // ReadingCheckBox
            // 
            ReadingCheckBox.AutoSize = true;
            ReadingCheckBox.BackColor = SystemColors.Control;
            ReadingCheckBox.Checked = true;
            ReadingCheckBox.CheckState = CheckState.Checked;
            ReadingCheckBox.Font = new Font("Yu Gothic UI", 16F);
            ReadingCheckBox.ForeColor = SystemColors.ControlText;
            ReadingCheckBox.Location = new Point(159, 285);
            ReadingCheckBox.Name = "ReadingCheckBox";
            ReadingCheckBox.Size = new Size(123, 34);
            ReadingCheckBox.TabIndex = 2;
            ReadingCheckBox.Text = "Unknown";
            ReadingCheckBox.UseVisualStyleBackColor = false;
            ReadingCheckBox.CheckedChanged += CheckChanged;
            // 
            // ColourCheckBox
            // 
            ColourCheckBox.AutoSize = true;
            ColourCheckBox.BackColor = SystemColors.Control;
            ColourCheckBox.Checked = true;
            ColourCheckBox.CheckState = CheckState.Checked;
            ColourCheckBox.Font = new Font("Yu Gothic UI", 16F);
            ColourCheckBox.Location = new Point(306, 285);
            ColourCheckBox.Name = "ColourCheckBox";
            ColourCheckBox.Size = new Size(123, 34);
            ColourCheckBox.TabIndex = 3;
            ColourCheckBox.Text = "Unknown";
            ColourCheckBox.UseVisualStyleBackColor = false;
            ColourCheckBox.CheckedChanged += CheckChanged;
            // 
            // Stamina1CheckBox
            // 
            Stamina1CheckBox.AutoSize = true;
            Stamina1CheckBox.BackColor = SystemColors.Control;
            Stamina1CheckBox.Checked = true;
            Stamina1CheckBox.CheckState = CheckState.Checked;
            Stamina1CheckBox.Font = new Font("Yu Gothic UI", 16F);
            Stamina1CheckBox.Location = new Point(453, 285);
            Stamina1CheckBox.Name = "Stamina1CheckBox";
            Stamina1CheckBox.Size = new Size(123, 34);
            Stamina1CheckBox.TabIndex = 4;
            Stamina1CheckBox.Text = "Unknown";
            Stamina1CheckBox.UseVisualStyleBackColor = false;
            Stamina1CheckBox.CheckedChanged += CheckChanged;
            // 
            // Stamina2CheckBox
            // 
            Stamina2CheckBox.AutoSize = true;
            Stamina2CheckBox.BackColor = SystemColors.Control;
            Stamina2CheckBox.Checked = true;
            Stamina2CheckBox.CheckState = CheckState.Checked;
            Stamina2CheckBox.Font = new Font("Yu Gothic UI", 16F);
            Stamina2CheckBox.Location = new Point(600, 285);
            Stamina2CheckBox.Name = "Stamina2CheckBox";
            Stamina2CheckBox.Size = new Size(123, 34);
            Stamina2CheckBox.TabIndex = 5;
            Stamina2CheckBox.Text = "Unknown";
            Stamina2CheckBox.UseVisualStyleBackColor = false;
            Stamina2CheckBox.CheckedChanged += CheckChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI", 16F);
            label1.Location = new Point(751, 286);
            label1.Name = "label1";
            label1.Size = new Size(111, 30);
            label1.TabIndex = 6;
            label1.Text = "Time: N/A";
            // 
            // progressLine
            // 
            progressLine.BorderStyle = BorderStyle.FixedSingle;
            progressLine.ForeColor = SystemColors.Control;
            progressLine.Location = new Point(13, 17);
            progressLine.Name = "progressLine";
            progressLine.Size = new Size(5, 225);
            progressLine.TabIndex = 9;
            // 
            // StrainGraph
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(950, 321);
            Controls.Add(progressLine);
            Controls.Add(label1);
            Controls.Add(Stamina2CheckBox);
            Controls.Add(Stamina1CheckBox);
            Controls.Add(ColourCheckBox);
            Controls.Add(ReadingCheckBox);
            Controls.Add(RhythmCheckBox);
            Controls.Add(StrainGraphPlot);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "StrainGraph";
            Text = "StrainGraph by RealtimePPUR";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private OxyPlot.WindowsForms.PlotView StrainGraphPlot;
        private CheckBox RhythmCheckBox;
        private CheckBox ReadingCheckBox;
        private CheckBox ColourCheckBox;
        private CheckBox Stamina1CheckBox;
        private CheckBox Stamina2CheckBox;
        private Label label1;
        private Label progressLine;
    }
}