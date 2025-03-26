namespace RealtimePPUR.Forms
{
    partial class UnstableRateGraph
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
            unstableLateGraph = new OxyPlot.WindowsForms.PlotView();
            ValueLabel = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // unstableLateGraph
            // 
            unstableLateGraph.Location = new System.Drawing.Point(0, 25);
            unstableLateGraph.Name = "unstableLateGraph";
            unstableLateGraph.PanCursor = System.Windows.Forms.Cursors.Hand;
            unstableLateGraph.Size = new System.Drawing.Size(801, 240);
            unstableLateGraph.TabIndex = 0;
            unstableLateGraph.Text = "unstableLateGraph";
            unstableLateGraph.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            unstableLateGraph.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            unstableLateGraph.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // ValueLabel
            // 
            ValueLabel.AutoSize = true;
            ValueLabel.Font = new System.Drawing.Font(this.mainForm.GuiFont, 15F);
            ValueLabel.Location = new System.Drawing.Point(0, 0);
            ValueLabel.Name = "ValueLabel";
            ValueLabel.Size = new System.Drawing.Size(59, 28);
            ValueLabel.TabIndex = 1;
            ValueLabel.Text = "Value";
            // 
            // UnstableRateGraph
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 265);
            Controls.Add(ValueLabel);
            Controls.Add(unstableLateGraph);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "UnstableRateGraph";
            Text = "UnstableRateGraph by RealtimePPUR";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private OxyPlot.WindowsForms.PlotView unstableLateGraph;
        private System.Windows.Forms.Label ValueLabel;
    }
}