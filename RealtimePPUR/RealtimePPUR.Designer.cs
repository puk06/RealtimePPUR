using System.Drawing;

namespace RealtimePPUR
{
    sealed partial class RealtimePpur
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RealtimePpur));
            _currentPp = new System.Windows.Forms.Label();
            _sr = new System.Windows.Forms.Label();
            _sspp = new System.Windows.Forms.Label();
            _good = new System.Windows.Forms.Label();
            _ok = new System.Windows.Forms.Label();
            _miss = new System.Windows.Forms.Label();
            _avgoffset = new System.Windows.Forms.Label();
            _ur = new System.Windows.Forms.Label();
            _avgoffsethelp = new System.Windows.Forms.Label();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            modeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            realtimePPURToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            offsetHelperToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            realtimePPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            osuModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            inGameOverlayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            sRToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            sSPPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            currentPPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            currentACCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            hitsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ifFCHitsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            uRToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            offsetHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            expectedManiaScoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            avgOffsetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            progressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ifFCPPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            healthPercentageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            currentPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            higherScoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            highestScoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            userScoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            changePriorityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            changeFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            resetFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            inGameValue = new System.Windows.Forms.Label();
            discordRichPresenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // _currentPp
            // 
            _currentPp.BackColor = Color.Transparent;
            _currentPp.ForeColor = Color.White;
            _currentPp.Location = new Point(249, 18);
            _currentPp.Name = "_currentPp";
            _currentPp.Size = new Size(30, 39);
            _currentPp.TabIndex = 0;
            _currentPp.Text = "0";
            _currentPp.TextAlign = ContentAlignment.MiddleRight;
            _currentPp.MouseDown += RealtimePPUR_MouseDown;
            _currentPp.MouseMove += RealtimePPUR_MouseMove;
            // 
            // _sr
            // 
            _sr.BackColor = Color.Transparent;
            _sr.ForeColor = Color.White;
            _sr.Location = new Point(40, -1);
            _sr.Name = "_sr";
            _sr.Size = new Size(30, 29);
            _sr.TabIndex = 0;
            _sr.Text = "0";
            _sr.MouseDown += RealtimePPUR_MouseDown;
            _sr.MouseMove += RealtimePPUR_MouseMove;
            // 
            // _sspp
            // 
            _sspp.BackColor = Color.Transparent;
            _sspp.ForeColor = Color.White;
            _sspp.Location = new Point(138, -1);
            _sspp.Name = "_sspp";
            _sspp.Size = new Size(30, 29);
            _sspp.TabIndex = 0;
            _sspp.Text = "0";
            _sspp.MouseDown += RealtimePPUR_MouseDown;
            _sspp.MouseMove += RealtimePPUR_MouseMove;
            // 
            // _good
            // 
            _good.BackColor = Color.Transparent;
            _good.ForeColor = Color.White;
            _good.Location = new Point(22, 25);
            _good.Name = "_good";
            _good.Size = new Size(30, 29);
            _good.TabIndex = 0;
            _good.Text = "0";
            _good.TextAlign = ContentAlignment.MiddleCenter;
            _good.MouseDown += RealtimePPUR_MouseDown;
            _good.MouseMove += RealtimePPUR_MouseMove;
            // 
            // _ok
            // 
            _ok.BackColor = Color.Transparent;
            _ok.ForeColor = Color.White;
            _ok.Location = new Point(82, 25);
            _ok.Name = "_ok";
            _ok.Size = new Size(30, 29);
            _ok.TabIndex = 0;
            _ok.Text = "0";
            _ok.TextAlign = ContentAlignment.MiddleCenter;
            _ok.MouseDown += RealtimePPUR_MouseDown;
            _ok.MouseMove += RealtimePPUR_MouseMove;
            // 
            // _miss
            // 
            _miss.BackColor = Color.Transparent;
            _miss.ForeColor = Color.White;
            _miss.Location = new Point(140, 25);
            _miss.Name = "_miss";
            _miss.Size = new Size(30, 29);
            _miss.TabIndex = 0;
            _miss.Text = "0";
            _miss.TextAlign = ContentAlignment.MiddleCenter;
            _miss.MouseDown += RealtimePPUR_MouseDown;
            _miss.MouseMove += RealtimePPUR_MouseMove;
            // 
            // _avgoffset
            // 
            _avgoffset.AutoSize = true;
            _avgoffset.BackColor = Color.Transparent;
            _avgoffset.ForeColor = Color.White;
            _avgoffset.Location = new Point(37, 105);
            _avgoffset.Name = "_avgoffset";
            _avgoffset.Size = new Size(28, 15);
            _avgoffset.TabIndex = 0;
            _avgoffset.Text = "0ms";
            _avgoffset.MouseDown += RealtimePPUR_MouseDown;
            _avgoffset.MouseMove += RealtimePPUR_MouseMove;
            // 
            // _ur
            // 
            _ur.AutoSize = true;
            _ur.BackColor = Color.Transparent;
            _ur.ForeColor = Color.White;
            _ur.Location = new Point(217, 70);
            _ur.Name = "_ur";
            _ur.RightToLeft = System.Windows.Forms.RightToLeft.No;
            _ur.Size = new Size(13, 15);
            _ur.TabIndex = 0;
            _ur.Text = "0";
            _ur.MouseDown += RealtimePPUR_MouseDown;
            _ur.MouseMove += RealtimePPUR_MouseMove;
            // 
            // _avgoffsethelp
            // 
            _avgoffsethelp.AutoSize = true;
            _avgoffsethelp.BackColor = Color.Transparent;
            _avgoffsethelp.ForeColor = Color.White;
            _avgoffsethelp.Location = new Point(82, 70);
            _avgoffsethelp.Name = "_avgoffsethelp";
            _avgoffsethelp.Size = new Size(13, 15);
            _avgoffsethelp.TabIndex = 0;
            _avgoffsethelp.Text = "0";
            _avgoffsethelp.TextAlign = ContentAlignment.MiddleCenter;
            _avgoffsethelp.MouseDown += RealtimePPUR_MouseDown;
            _avgoffsethelp.MouseMove += RealtimePPUR_MouseMove;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { modeToolStripMenuItem, osuModeToolStripMenuItem, inGameOverlayToolStripMenuItem, changePriorityToolStripMenuItem, changeFontToolStripMenuItem, loadFontToolStripMenuItem, resetFontToolStripMenuItem, discordRichPresenceToolStripMenuItem, closeToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(191, 224);
            // 
            // modeToolStripMenuItem
            // 
            modeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { realtimePPURToolStripMenuItem, offsetHelperToolStripMenuItem, realtimePPToolStripMenuItem });
            modeToolStripMenuItem.Name = "modeToolStripMenuItem";
            modeToolStripMenuItem.Size = new Size(190, 22);
            modeToolStripMenuItem.Text = "Mode";
            // 
            // realtimePPURToolStripMenuItem
            // 
            realtimePPURToolStripMenuItem.Name = "realtimePPURToolStripMenuItem";
            realtimePPURToolStripMenuItem.Size = new Size(148, 22);
            realtimePPURToolStripMenuItem.Text = "RealtimePPUR";
            realtimePPURToolStripMenuItem.Click += realtimePPURToolStripMenuItem_Click;
            // 
            // offsetHelperToolStripMenuItem
            // 
            offsetHelperToolStripMenuItem.Name = "offsetHelperToolStripMenuItem";
            offsetHelperToolStripMenuItem.Size = new Size(148, 22);
            offsetHelperToolStripMenuItem.Text = "Offset Helper";
            offsetHelperToolStripMenuItem.Click += offsetHelperToolStripMenuItem_Click;
            // 
            // realtimePPToolStripMenuItem
            // 
            realtimePPToolStripMenuItem.Name = "realtimePPToolStripMenuItem";
            realtimePPToolStripMenuItem.Size = new Size(148, 22);
            realtimePPToolStripMenuItem.Text = "RealtimePP";
            realtimePPToolStripMenuItem.Click += realtimePPToolStripMenuItem_Click;
            // 
            // osuModeToolStripMenuItem
            // 
            osuModeToolStripMenuItem.Name = "osuModeToolStripMenuItem";
            osuModeToolStripMenuItem.Size = new Size(190, 22);
            osuModeToolStripMenuItem.Text = "osu! mode";
            osuModeToolStripMenuItem.Click += osuModeToolStripMenuItem_Click;
            // 
            // inGameOverlayToolStripMenuItem
            // 
            inGameOverlayToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { sRToolStripMenuItem, sSPPToolStripMenuItem, currentPPToolStripMenuItem, currentACCToolStripMenuItem, hitsToolStripMenuItem, ifFCHitsToolStripMenuItem, uRToolStripMenuItem, offsetHelpToolStripMenuItem, expectedManiaScoreToolStripMenuItem, avgOffsetToolStripMenuItem, progressToolStripMenuItem, ifFCPPToolStripMenuItem, healthPercentageToolStripMenuItem, currentPositionToolStripMenuItem, higherScoreToolStripMenuItem, highestScoreToolStripMenuItem, userScoreToolStripMenuItem });
            inGameOverlayToolStripMenuItem.Name = "inGameOverlayToolStripMenuItem";
            inGameOverlayToolStripMenuItem.Size = new Size(190, 22);
            inGameOverlayToolStripMenuItem.Text = "InGameOverlay";
            // 
            // sRToolStripMenuItem
            // 
            sRToolStripMenuItem.Name = "sRToolStripMenuItem";
            sRToolStripMenuItem.Size = new Size(184, 22);
            sRToolStripMenuItem.Text = "SR";
            sRToolStripMenuItem.Click += sRToolStripMenuItem_Click;
            // 
            // sSPPToolStripMenuItem
            // 
            sSPPToolStripMenuItem.Name = "sSPPToolStripMenuItem";
            sSPPToolStripMenuItem.Size = new Size(184, 22);
            sSPPToolStripMenuItem.Text = "SSPP";
            sSPPToolStripMenuItem.Click += sSPPToolStripMenuItem_Click;
            // 
            // currentPPToolStripMenuItem
            // 
            currentPPToolStripMenuItem.Name = "currentPPToolStripMenuItem";
            currentPPToolStripMenuItem.Size = new Size(184, 22);
            currentPPToolStripMenuItem.Text = "CurrentPP";
            currentPPToolStripMenuItem.Click += currentPPToolStripMenuItem_Click;
            // 
            // currentACCToolStripMenuItem
            // 
            currentACCToolStripMenuItem.Name = "currentACCToolStripMenuItem";
            currentACCToolStripMenuItem.Size = new Size(184, 22);
            currentACCToolStripMenuItem.Text = "CurrentACC";
            currentACCToolStripMenuItem.Click += currentACCToolStripMenuItem_Click;
            // 
            // hitsToolStripMenuItem
            // 
            hitsToolStripMenuItem.Name = "hitsToolStripMenuItem";
            hitsToolStripMenuItem.Size = new Size(184, 22);
            hitsToolStripMenuItem.Text = "Hits";
            hitsToolStripMenuItem.Click += hitsToolStripMenuItem_Click;
            // 
            // ifFCHitsToolStripMenuItem
            // 
            ifFCHitsToolStripMenuItem.Name = "ifFCHitsToolStripMenuItem";
            ifFCHitsToolStripMenuItem.Size = new Size(184, 22);
            ifFCHitsToolStripMenuItem.Text = "ifFCHits";
            ifFCHitsToolStripMenuItem.Click += ifFCHitsToolStripMenuItem_Click;
            // 
            // uRToolStripMenuItem
            // 
            uRToolStripMenuItem.Name = "uRToolStripMenuItem";
            uRToolStripMenuItem.Size = new Size(184, 22);
            uRToolStripMenuItem.Text = "UR";
            uRToolStripMenuItem.Click += uRToolStripMenuItem_Click;
            // 
            // offsetHelpToolStripMenuItem
            // 
            offsetHelpToolStripMenuItem.Name = "offsetHelpToolStripMenuItem";
            offsetHelpToolStripMenuItem.Size = new Size(184, 22);
            offsetHelpToolStripMenuItem.Text = "OffsetHelp";
            offsetHelpToolStripMenuItem.Click += offsetHelpToolStripMenuItem_Click;
            // 
            // expectedManiaScoreToolStripMenuItem
            // 
            expectedManiaScoreToolStripMenuItem.Name = "expectedManiaScoreToolStripMenuItem";
            expectedManiaScoreToolStripMenuItem.Size = new Size(184, 22);
            expectedManiaScoreToolStripMenuItem.Text = "ExpectedManiaScore";
            expectedManiaScoreToolStripMenuItem.Click += expectedManiaScoreToolStripMenuItem_Click;
            // 
            // avgOffsetToolStripMenuItem
            // 
            avgOffsetToolStripMenuItem.Name = "avgOffsetToolStripMenuItem";
            avgOffsetToolStripMenuItem.Size = new Size(184, 22);
            avgOffsetToolStripMenuItem.Text = "AvgOffset";
            avgOffsetToolStripMenuItem.Click += avgOffsetToolStripMenuItem_Click;
            // 
            // progressToolStripMenuItem
            // 
            progressToolStripMenuItem.Name = "progressToolStripMenuItem";
            progressToolStripMenuItem.Size = new Size(184, 22);
            progressToolStripMenuItem.Text = "Progress";
            progressToolStripMenuItem.Click += progressToolStripMenuItem_Click;
            // 
            // ifFCPPToolStripMenuItem
            // 
            ifFCPPToolStripMenuItem.Name = "ifFCPPToolStripMenuItem";
            ifFCPPToolStripMenuItem.Size = new Size(184, 22);
            ifFCPPToolStripMenuItem.Text = "ifFCPP";
            ifFCPPToolStripMenuItem.Click += ifFCPPToolStripMenuItem_Click;
            // 
            // healthPercentageToolStripMenuItem
            // 
            healthPercentageToolStripMenuItem.Name = "healthPercentageToolStripMenuItem";
            healthPercentageToolStripMenuItem.Size = new Size(184, 22);
            healthPercentageToolStripMenuItem.Text = "Health Percentage";
            healthPercentageToolStripMenuItem.Click += healthPercentageToolStripMenuItem_Click;
            // 
            // currentPositionToolStripMenuItem
            // 
            currentPositionToolStripMenuItem.Name = "currentPositionToolStripMenuItem";
            currentPositionToolStripMenuItem.Size = new Size(184, 22);
            currentPositionToolStripMenuItem.Text = "CurrentPosition";
            currentPositionToolStripMenuItem.Click += currentPositionToolStripMenuItem_Click;
            // 
            // higherScoreToolStripMenuItem
            // 
            higherScoreToolStripMenuItem.Name = "higherScoreToolStripMenuItem";
            higherScoreToolStripMenuItem.Size = new Size(184, 22);
            higherScoreToolStripMenuItem.Text = "HigherScoreDiff";
            higherScoreToolStripMenuItem.Click += higherScoreToolStripMenuItem_Click;
            // 
            // highestScoreToolStripMenuItem
            // 
            highestScoreToolStripMenuItem.Name = "highestScoreToolStripMenuItem";
            highestScoreToolStripMenuItem.Size = new Size(184, 22);
            highestScoreToolStripMenuItem.Text = "HighestScoreDiff";
            highestScoreToolStripMenuItem.Click += highestScoreToolStripMenuItem_Click;
            // 
            // userScoreToolStripMenuItem
            // 
            userScoreToolStripMenuItem.Name = "userScoreToolStripMenuItem";
            userScoreToolStripMenuItem.Size = new Size(184, 22);
            userScoreToolStripMenuItem.Text = "UserScore";
            userScoreToolStripMenuItem.Click += userScoreToolStripMenuItem_Click;
            // 
            // changePriorityToolStripMenuItem
            // 
            changePriorityToolStripMenuItem.Name = "changePriorityToolStripMenuItem";
            changePriorityToolStripMenuItem.Size = new Size(190, 22);
            changePriorityToolStripMenuItem.Text = "Change Priority";
            changePriorityToolStripMenuItem.Click += changePriorityToolStripMenuItem_Click;
            // 
            // changeFontToolStripMenuItem
            // 
            changeFontToolStripMenuItem.Name = "changeFontToolStripMenuItem";
            changeFontToolStripMenuItem.Size = new Size(190, 22);
            changeFontToolStripMenuItem.Text = "Change Font";
            changeFontToolStripMenuItem.Click += changeFontToolStripMenuItem_Click;
            // 
            // loadFontToolStripMenuItem
            // 
            loadFontToolStripMenuItem.Name = "loadFontToolStripMenuItem";
            loadFontToolStripMenuItem.Size = new Size(190, 22);
            loadFontToolStripMenuItem.Text = "Load Font";
            loadFontToolStripMenuItem.Click += loadFontToolStripMenuItem_Click;
            // 
            // resetFontToolStripMenuItem
            // 
            resetFontToolStripMenuItem.Name = "resetFontToolStripMenuItem";
            resetFontToolStripMenuItem.Size = new Size(190, 22);
            resetFontToolStripMenuItem.Text = "Reset Font";
            resetFontToolStripMenuItem.Click += resetFontToolStripMenuItem_Click_1;
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.Size = new Size(190, 22);
            closeToolStripMenuItem.Text = "Close";
            closeToolStripMenuItem.Click += closeToolStripMenuItem_Click;
            // 
            // inGameValue
            // 
            inGameValue.AutoSize = true;
            inGameValue.ForeColor = Color.White;
            inGameValue.Location = new Point(0, 0);
            inGameValue.Name = "inGameValue";
            inGameValue.Size = new Size(0, 15);
            inGameValue.TabIndex = 1;
            inGameValue.Visible = false;
            // 
            // discordRichPresenceToolStripMenuItem
            // 
            discordRichPresenceToolStripMenuItem.Name = "discordRichPresenceToolStripMenuItem";
            discordRichPresenceToolStripMenuItem.Size = new Size(190, 22);
            discordRichPresenceToolStripMenuItem.Text = "Discord Rich Presence";
            discordRichPresenceToolStripMenuItem.Click += discordRichPresenceToolStripMenuItem_Click;
            // 
            // RealtimePpur
            // 

            this._avgoffsethelp.Font = new System.Drawing.Font(_fontCollection.Families[1], 20F, System.Drawing.FontStyle.Bold);
            this._ur.Font = new System.Drawing.Font(_fontCollection.Families[1], 25F, System.Drawing.FontStyle.Bold);
            this._avgoffset.Font = new System.Drawing.Font(_fontCollection.Families[1], 13F, System.Drawing.FontStyle.Bold);
            this._miss.Font = new System.Drawing.Font(_fontCollection.Families[1], 15F, System.Drawing.FontStyle.Bold);
            this._ok.Font = new System.Drawing.Font(_fontCollection.Families[1], 15F, System.Drawing.FontStyle.Bold);
            this._good.Font = new System.Drawing.Font(_fontCollection.Families[1], 15F, System.Drawing.FontStyle.Bold);
            this._sspp.Font = new System.Drawing.Font(_fontCollection.Families[1], 13F, System.Drawing.FontStyle.Bold);
            this._sr.Font = new System.Drawing.Font(_fontCollection.Families[1], 13F, System.Drawing.FontStyle.Bold);
            this._currentPp.Font = new System.Drawing.Font(_fontCollection.Families[1], 20F, System.Drawing.FontStyle.Bold);
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.PPUR;
            ClientSize = new Size(316, 130);
            ContextMenuStrip = contextMenuStrip1;
            Controls.Add(inGameValue);
            Controls.Add(_currentPp);
            Controls.Add(_sr);
            Controls.Add(_sspp);
            Controls.Add(_good);
            Controls.Add(_ok);
            Controls.Add(_miss);
            Controls.Add(_avgoffset);
            Controls.Add(_ur);
            Controls.Add(_avgoffsethelp);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4);
            MaximizeBox = false;
            Name = "RealtimePpur";
            Text = "RealtimePPUR";
            TransparencyKey = SystemColors.Control;
            Closed += RealtimePPUR_Closed;
            Shown += RealtimePpur_Shown;
            MouseDown += RealtimePPUR_MouseDown;
            MouseMove += RealtimePPUR_MouseMove;
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
            RoundCorners();
        }

        private System.Windows.Forms.ToolStripMenuItem healthPercentageToolStripMenuItem;

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem modeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem offsetHelperToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem realtimePPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem realtimePPURToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem osuModeToolStripMenuItem;
        private System.Windows.Forms.Label inGameValue;
        private System.Windows.Forms.ToolStripMenuItem inGameOverlayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sRToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem currentPPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sSPPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hitsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uRToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem offsetHelpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem avgOffsetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem currentACCToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem progressToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeFontToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetFontToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadFontToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ifFCPPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ifFCHitsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expectedManiaScoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changePriorityToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem currentPositionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem higherScoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem highestScoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem userScoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem discordRichPresenceToolStripMenuItem;
    }
}

