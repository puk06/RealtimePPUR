using RealtimePPUR.Utils;

namespace RealtimePPUR.Forms
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
            currentPp = new Label();
            sr = new Label();
            iffc = new Label();
            good = new Label();
            ok = new Label();
            miss = new Label();
            avgoffset = new Label();
            ur = new Label();
            avgoffsethelp = new Label();
            contextMenuStrip1 = new ContextMenuStrip(components);
            modeToolStripMenuItem = new ToolStripMenuItem();
            realtimePPURToolStripMenuItem = new ToolStripMenuItem();
            offsetHelperToolStripMenuItem = new ToolStripMenuItem();
            realtimePPToolStripMenuItem = new ToolStripMenuItem();
            osuModeToolStripMenuItem = new ToolStripMenuItem();
            calculationOptionToolStripMenuItem = new ToolStripMenuItem();
            pPLossModeToolStripMenuItem = new ToolStripMenuItem();
            calculateFirstToolStripMenuItem = new ToolStripMenuItem();
            inGameOverlayToolStripMenuItem = new ToolStripMenuItem();
            sRToolStripMenuItem = new ToolStripMenuItem();
            sSPPToolStripMenuItem = new ToolStripMenuItem();
            currentPPToolStripMenuItem = new ToolStripMenuItem();
            currentACCToolStripMenuItem = new ToolStripMenuItem();
            hitsToolStripMenuItem = new ToolStripMenuItem();
            ifFCHitsToolStripMenuItem = new ToolStripMenuItem();
            uRToolStripMenuItem = new ToolStripMenuItem();
            offsetHelpToolStripMenuItem = new ToolStripMenuItem();
            expectedManiaScoreToolStripMenuItem = new ToolStripMenuItem();
            avgOffsetToolStripMenuItem = new ToolStripMenuItem();
            progressToolStripMenuItem = new ToolStripMenuItem();
            ifFCPPToolStripMenuItem = new ToolStripMenuItem();
            healthPercentageToolStripMenuItem = new ToolStripMenuItem();
            currentPositionToolStripMenuItem = new ToolStripMenuItem();
            higherScoreToolStripMenuItem = new ToolStripMenuItem();
            highestScoreToolStripMenuItem = new ToolStripMenuItem();
            userScoreToolStripMenuItem = new ToolStripMenuItem();
            currentBPMToolStripMenuItem = new ToolStripMenuItem();
            currentRankToolStripMenuItem = new ToolStripMenuItem();
            remainingNotesToolStripMenuItem = new ToolStripMenuItem();
            overlayOptionToolStripMenuItem = new ToolStripMenuItem();
            changePriorityToolStripMenuItem1 = new ToolStripMenuItem();
            changeFontToolStripMenuItem1 = new ToolStripMenuItem();
            loadFontToolStripMenuItem1 = new ToolStripMenuItem();
            resetFontToolStripMenuItem1 = new ToolStripMenuItem();
            graphToolStripMenuItem = new ToolStripMenuItem();
            uRGraphToolStripMenuItem1 = new ToolStripMenuItem();
            strainGraphToolStripMenuItem1 = new ToolStripMenuItem();
            discordRichPresenceToolStripMenuItem = new ToolStripMenuItem();
            saveConfigToolStripMenuItem = new ToolStripMenuItem();
            closeToolStripMenuItem = new ToolStripMenuItem();
            inGameValue = new PictureBox();
            toolStripMenuItem2 = new ToolStripMenuItem();
            toolStripMenuItem3 = new ToolStripMenuItem();
            toolStripMenuItem4 = new ToolStripMenuItem();
            contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)inGameValue).BeginInit();
            SuspendLayout();
            // 
            // currentPp
            // 
            currentPp.BackColor = Color.Transparent;
            currentPp.ForeColor = Color.White;
            currentPp.Location = new Point(249, 18);
            currentPp.Name = "currentPp";
            currentPp.Size = new Size(30, 39);
            currentPp.TabIndex = 0;
            currentPp.Text = "0";
            currentPp.TextAlign = ContentAlignment.MiddleRight;
            currentPp.MouseDown += RealtimePPUR_MouseDown;
            currentPp.MouseMove += RealtimePPUR_MouseMove;
            // 
            // sr
            // 
            sr.BackColor = Color.Transparent;
            sr.ForeColor = Color.White;
            sr.Location = new Point(38, -1);
            sr.Name = "sr";
            sr.Size = new Size(30, 29);
            sr.TabIndex = 0;
            sr.Text = "0";
            sr.MouseDown += RealtimePPUR_MouseDown;
            sr.MouseMove += RealtimePPUR_MouseMove;
            // 
            // iffc
            // 
            iffc.BackColor = Color.Transparent;
            iffc.ForeColor = Color.White;
            iffc.Location = new Point(133, -1);
            iffc.Name = "iffc";
            iffc.Size = new Size(30, 29);
            iffc.TabIndex = 0;
            iffc.Text = "0";
            iffc.MouseDown += RealtimePPUR_MouseDown;
            iffc.MouseMove += RealtimePPUR_MouseMove;
            // 
            // good
            // 
            good.BackColor = Color.Transparent;
            good.ForeColor = Color.White;
            good.Location = new Point(23, 25);
            good.Name = "good";
            good.Size = new Size(30, 29);
            good.TabIndex = 0;
            good.Text = "0";
            good.TextAlign = ContentAlignment.MiddleCenter;
            good.MouseDown += RealtimePPUR_MouseDown;
            good.MouseMove += RealtimePPUR_MouseMove;
            // 
            // ok
            // 
            ok.BackColor = Color.Transparent;
            ok.ForeColor = Color.White;
            ok.Location = new Point(82, 25);
            ok.Name = "ok";
            ok.Size = new Size(30, 29);
            ok.TabIndex = 0;
            ok.Text = "0";
            ok.TextAlign = ContentAlignment.MiddleCenter;
            ok.MouseDown += RealtimePPUR_MouseDown;
            ok.MouseMove += RealtimePPUR_MouseMove;
            // 
            // miss
            // 
            miss.BackColor = Color.Transparent;
            miss.ForeColor = Color.White;
            miss.Location = new Point(140, 25);
            miss.Name = "miss";
            miss.Size = new Size(30, 29);
            miss.TabIndex = 0;
            miss.Text = "0";
            miss.TextAlign = ContentAlignment.MiddleCenter;
            miss.MouseDown += RealtimePPUR_MouseDown;
            miss.MouseMove += RealtimePPUR_MouseMove;
            // 
            // avgoffset
            // 
            avgoffset.AutoSize = true;
            avgoffset.BackColor = Color.Transparent;
            avgoffset.ForeColor = Color.White;
            avgoffset.Location = new Point(37, 105);
            avgoffset.Name = "avgoffset";
            avgoffset.Size = new Size(28, 15);
            avgoffset.TabIndex = 0;
            avgoffset.Text = "0ms";
            avgoffset.MouseDown += RealtimePPUR_MouseDown;
            avgoffset.MouseMove += RealtimePPUR_MouseMove;
            // 
            // ur
            // 
            ur.AutoSize = true;
            ur.BackColor = Color.Transparent;
            ur.ForeColor = Color.White;
            ur.Location = new Point(217, 70);
            ur.Name = "ur";
            ur.RightToLeft = RightToLeft.No;
            ur.Size = new Size(13, 15);
            ur.TabIndex = 0;
            ur.Text = "0";
            ur.MouseDown += RealtimePPUR_MouseDown;
            ur.MouseMove += RealtimePPUR_MouseMove;
            // 
            // avgoffsethelp
            // 
            avgoffsethelp.AutoSize = true;
            avgoffsethelp.BackColor = Color.Transparent;
            avgoffsethelp.ForeColor = Color.White;
            avgoffsethelp.Location = new Point(82, 70);
            avgoffsethelp.Name = "avgoffsethelp";
            avgoffsethelp.Size = new Size(13, 15);
            avgoffsethelp.TabIndex = 0;
            avgoffsethelp.Text = "0";
            avgoffsethelp.TextAlign = ContentAlignment.MiddleCenter;
            avgoffsethelp.MouseDown += RealtimePPUR_MouseDown;
            avgoffsethelp.MouseMove += RealtimePPUR_MouseMove;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { modeToolStripMenuItem, calculationOptionToolStripMenuItem, toolStripMenuItem2, osuModeToolStripMenuItem, inGameOverlayToolStripMenuItem, overlayOptionToolStripMenuItem, toolStripMenuItem3, graphToolStripMenuItem, discordRichPresenceToolStripMenuItem, saveConfigToolStripMenuItem, toolStripMenuItem4, closeToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(200, 290);
            // 
            // modeToolStripMenuItem
            // 
            modeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { realtimePPURToolStripMenuItem, offsetHelperToolStripMenuItem, realtimePPToolStripMenuItem });
            modeToolStripMenuItem.Name = "modeToolStripMenuItem";
            modeToolStripMenuItem.Size = new Size(199, 22);
            modeToolStripMenuItem.Text = "Software Mode";
            // 
            // realtimePPURToolStripMenuItem
            // 
            realtimePPURToolStripMenuItem.Checked = true;
            realtimePPURToolStripMenuItem.CheckState = CheckState.Checked;
            realtimePPURToolStripMenuItem.Name = "realtimePPURToolStripMenuItem";
            realtimePPURToolStripMenuItem.Size = new Size(148, 22);
            realtimePPURToolStripMenuItem.Text = "RealtimePPUR";
            realtimePPURToolStripMenuItem.Click += RealtimePPURToolStripMenuItem_Click;
            // 
            // offsetHelperToolStripMenuItem
            // 
            offsetHelperToolStripMenuItem.Name = "offsetHelperToolStripMenuItem";
            offsetHelperToolStripMenuItem.Size = new Size(148, 22);
            offsetHelperToolStripMenuItem.Text = "Offset Helper";
            offsetHelperToolStripMenuItem.Click += OffsetHelperToolStripMenuItem_Click;
            // 
            // realtimePPToolStripMenuItem
            // 
            realtimePPToolStripMenuItem.Name = "realtimePPToolStripMenuItem";
            realtimePPToolStripMenuItem.Size = new Size(148, 22);
            realtimePPToolStripMenuItem.Text = "RealtimePP";
            realtimePPToolStripMenuItem.Click += RealtimePPToolStripMenuItem_Click;
            // 
            // osuModeToolStripMenuItem
            // 
            osuModeToolStripMenuItem.Name = "osuModeToolStripMenuItem";
            osuModeToolStripMenuItem.Size = new Size(199, 22);
            osuModeToolStripMenuItem.Text = "Enable InGameOverlay";
            osuModeToolStripMenuItem.Click += OsuModeToolStripMenuItem_Click;
            // 
            // calculationOptionToolStripMenuItem
            // 
            calculationOptionToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { pPLossModeToolStripMenuItem, calculateFirstToolStripMenuItem });
            calculationOptionToolStripMenuItem.Name = "calculationOptionToolStripMenuItem";
            calculationOptionToolStripMenuItem.Size = new Size(199, 22);
            calculationOptionToolStripMenuItem.Text = "Calculation Settings";
            // 
            // pPLossModeToolStripMenuItem
            // 
            pPLossModeToolStripMenuItem.Name = "pPLossModeToolStripMenuItem";
            pPLossModeToolStripMenuItem.Size = new Size(180, 22);
            pPLossModeToolStripMenuItem.Text = "PPLossMode";
            // 
            // calculateFirstToolStripMenuItem
            // 
            calculateFirstToolStripMenuItem.Name = "calculateFirstToolStripMenuItem";
            calculateFirstToolStripMenuItem.Size = new Size(180, 22);
            calculateFirstToolStripMenuItem.Text = "Calculate First";
            // 
            // inGameOverlayToolStripMenuItem
            // 
            inGameOverlayToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { sRToolStripMenuItem, sSPPToolStripMenuItem, currentPPToolStripMenuItem, currentACCToolStripMenuItem, hitsToolStripMenuItem, ifFCHitsToolStripMenuItem, uRToolStripMenuItem, offsetHelpToolStripMenuItem, expectedManiaScoreToolStripMenuItem, avgOffsetToolStripMenuItem, progressToolStripMenuItem, ifFCPPToolStripMenuItem, healthPercentageToolStripMenuItem, currentPositionToolStripMenuItem, higherScoreToolStripMenuItem, highestScoreToolStripMenuItem, userScoreToolStripMenuItem, currentBPMToolStripMenuItem, currentRankToolStripMenuItem, remainingNotesToolStripMenuItem });
            inGameOverlayToolStripMenuItem.Name = "inGameOverlayToolStripMenuItem";
            inGameOverlayToolStripMenuItem.Size = new Size(199, 22);
            inGameOverlayToolStripMenuItem.Text = "InGameOverlay";
            // 
            // sRToolStripMenuItem
            // 
            sRToolStripMenuItem.Name = "sRToolStripMenuItem";
            sRToolStripMenuItem.Size = new Size(187, 22);
            sRToolStripMenuItem.Text = "SR";
            // 
            // sSPPToolStripMenuItem
            // 
            sSPPToolStripMenuItem.Name = "sSPPToolStripMenuItem";
            sSPPToolStripMenuItem.Size = new Size(187, 22);
            sSPPToolStripMenuItem.Text = "SSPP";
            // 
            // currentPPToolStripMenuItem
            // 
            currentPPToolStripMenuItem.Name = "currentPPToolStripMenuItem";
            currentPPToolStripMenuItem.Size = new Size(187, 22);
            currentPPToolStripMenuItem.Text = "CurrentPP";
            // 
            // currentACCToolStripMenuItem
            // 
            currentACCToolStripMenuItem.Name = "currentACCToolStripMenuItem";
            currentACCToolStripMenuItem.Size = new Size(187, 22);
            currentACCToolStripMenuItem.Text = "CurrentACC";
            // 
            // hitsToolStripMenuItem
            // 
            hitsToolStripMenuItem.Name = "hitsToolStripMenuItem";
            hitsToolStripMenuItem.Size = new Size(187, 22);
            hitsToolStripMenuItem.Text = "Hits";
            // 
            // ifFCHitsToolStripMenuItem
            // 
            ifFCHitsToolStripMenuItem.Name = "ifFCHitsToolStripMenuItem";
            ifFCHitsToolStripMenuItem.Size = new Size(187, 22);
            ifFCHitsToolStripMenuItem.Text = "ifFCHits";
            // 
            // uRToolStripMenuItem
            // 
            uRToolStripMenuItem.Name = "uRToolStripMenuItem";
            uRToolStripMenuItem.Size = new Size(187, 22);
            uRToolStripMenuItem.Text = "UR";
            // 
            // offsetHelpToolStripMenuItem
            // 
            offsetHelpToolStripMenuItem.Name = "offsetHelpToolStripMenuItem";
            offsetHelpToolStripMenuItem.Size = new Size(187, 22);
            offsetHelpToolStripMenuItem.Text = "OffsetHelp";
            // 
            // expectedManiaScoreToolStripMenuItem
            // 
            expectedManiaScoreToolStripMenuItem.Name = "expectedManiaScoreToolStripMenuItem";
            expectedManiaScoreToolStripMenuItem.Size = new Size(187, 22);
            expectedManiaScoreToolStripMenuItem.Text = "ExpectedManiaScore";
            // 
            // avgOffsetToolStripMenuItem
            // 
            avgOffsetToolStripMenuItem.Name = "avgOffsetToolStripMenuItem";
            avgOffsetToolStripMenuItem.Size = new Size(187, 22);
            avgOffsetToolStripMenuItem.Text = "AvgOffset";
            // 
            // progressToolStripMenuItem
            // 
            progressToolStripMenuItem.Name = "progressToolStripMenuItem";
            progressToolStripMenuItem.Size = new Size(187, 22);
            progressToolStripMenuItem.Text = "Progress";
            // 
            // ifFCPPToolStripMenuItem
            // 
            ifFCPPToolStripMenuItem.Name = "ifFCPPToolStripMenuItem";
            ifFCPPToolStripMenuItem.Size = new Size(187, 22);
            ifFCPPToolStripMenuItem.Text = "IFFCPP | LossModePP";
            // 
            // healthPercentageToolStripMenuItem
            // 
            healthPercentageToolStripMenuItem.Name = "healthPercentageToolStripMenuItem";
            healthPercentageToolStripMenuItem.Size = new Size(187, 22);
            healthPercentageToolStripMenuItem.Text = "Health Percentage";
            // 
            // currentPositionToolStripMenuItem
            // 
            currentPositionToolStripMenuItem.Name = "currentPositionToolStripMenuItem";
            currentPositionToolStripMenuItem.Size = new Size(187, 22);
            currentPositionToolStripMenuItem.Text = "CurrentPosition";
            // 
            // higherScoreToolStripMenuItem
            // 
            higherScoreToolStripMenuItem.Name = "higherScoreToolStripMenuItem";
            higherScoreToolStripMenuItem.Size = new Size(187, 22);
            higherScoreToolStripMenuItem.Text = "HigherScoreDiff";
            // 
            // highestScoreToolStripMenuItem
            // 
            highestScoreToolStripMenuItem.Name = "highestScoreToolStripMenuItem";
            highestScoreToolStripMenuItem.Size = new Size(187, 22);
            highestScoreToolStripMenuItem.Text = "HighestScoreDiff";
            // 
            // userScoreToolStripMenuItem
            // 
            userScoreToolStripMenuItem.Name = "userScoreToolStripMenuItem";
            userScoreToolStripMenuItem.Size = new Size(187, 22);
            userScoreToolStripMenuItem.Text = "UserScore";
            // 
            // currentBPMToolStripMenuItem
            // 
            currentBPMToolStripMenuItem.Name = "currentBPMToolStripMenuItem";
            currentBPMToolStripMenuItem.Size = new Size(187, 22);
            currentBPMToolStripMenuItem.Text = "CurrentBPM";
            // 
            // currentRankToolStripMenuItem
            // 
            currentRankToolStripMenuItem.Name = "currentRankToolStripMenuItem";
            currentRankToolStripMenuItem.Size = new Size(187, 22);
            currentRankToolStripMenuItem.Text = "CurrentRank";
            // 
            // remainingNotesToolStripMenuItem
            // 
            remainingNotesToolStripMenuItem.Name = "remainingNotesToolStripMenuItem";
            remainingNotesToolStripMenuItem.Size = new Size(187, 22);
            remainingNotesToolStripMenuItem.Text = "RemainingNotes";
            // 
            // overlayOptionToolStripMenuItem
            // 
            overlayOptionToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { changePriorityToolStripMenuItem1, changeFontToolStripMenuItem1, loadFontToolStripMenuItem1, resetFontToolStripMenuItem1 });
            overlayOptionToolStripMenuItem.Name = "overlayOptionToolStripMenuItem";
            overlayOptionToolStripMenuItem.Size = new Size(199, 22);
            overlayOptionToolStripMenuItem.Text = "InGameOverlay Settings";
            // 
            // changePriorityToolStripMenuItem1
            // 
            changePriorityToolStripMenuItem1.Name = "changePriorityToolStripMenuItem1";
            changePriorityToolStripMenuItem1.Size = new Size(180, 22);
            changePriorityToolStripMenuItem1.Text = "Change Priority";
            changePriorityToolStripMenuItem1.Click += ChangePriorityToolStripMenuItem_Click;
            // 
            // changeFontToolStripMenuItem1
            // 
            changeFontToolStripMenuItem1.Name = "changeFontToolStripMenuItem1";
            changeFontToolStripMenuItem1.Size = new Size(180, 22);
            changeFontToolStripMenuItem1.Text = "Change Font";
            changeFontToolStripMenuItem1.Click += ChangeFontToolStripMenuItem_Click;
            // 
            // loadFontToolStripMenuItem1
            // 
            loadFontToolStripMenuItem1.Name = "loadFontToolStripMenuItem1";
            loadFontToolStripMenuItem1.Size = new Size(180, 22);
            loadFontToolStripMenuItem1.Text = "LoadFont";
            loadFontToolStripMenuItem1.Click += LoadFontToolStripMenuItem_Click;
            // 
            // resetFontToolStripMenuItem1
            // 
            resetFontToolStripMenuItem1.Name = "resetFontToolStripMenuItem1";
            resetFontToolStripMenuItem1.Size = new Size(180, 22);
            resetFontToolStripMenuItem1.Text = "ResetFont";
            resetFontToolStripMenuItem1.Click += ResetFontToolStripMenuItem_Click;
            // 
            // graphToolStripMenuItem
            // 
            graphToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { uRGraphToolStripMenuItem1, strainGraphToolStripMenuItem1 });
            graphToolStripMenuItem.Name = "graphToolStripMenuItem";
            graphToolStripMenuItem.Size = new Size(199, 22);
            graphToolStripMenuItem.Text = "Graph";
            // 
            // uRGraphToolStripMenuItem1
            // 
            uRGraphToolStripMenuItem1.Name = "uRGraphToolStripMenuItem1";
            uRGraphToolStripMenuItem1.Size = new Size(139, 22);
            uRGraphToolStripMenuItem1.Text = "UR Graph";
            uRGraphToolStripMenuItem1.Click += URGraphToolStripMenuItem_Click;
            // 
            // strainGraphToolStripMenuItem1
            // 
            strainGraphToolStripMenuItem1.Name = "strainGraphToolStripMenuItem1";
            strainGraphToolStripMenuItem1.Size = new Size(139, 22);
            strainGraphToolStripMenuItem1.Text = "Strain Graph";
            strainGraphToolStripMenuItem1.Click += StrainGraphToolStripMenuItem_Click;
            // 
            // discordRichPresenceToolStripMenuItem
            // 
            discordRichPresenceToolStripMenuItem.Name = "discordRichPresenceToolStripMenuItem";
            discordRichPresenceToolStripMenuItem.Size = new Size(199, 22);
            discordRichPresenceToolStripMenuItem.Text = "Discord Rich Presence";
            // 
            // saveConfigToolStripMenuItem
            // 
            saveConfigToolStripMenuItem.Name = "saveConfigToolStripMenuItem";
            saveConfigToolStripMenuItem.Size = new Size(199, 22);
            saveConfigToolStripMenuItem.Text = "Save Config";
            saveConfigToolStripMenuItem.Click += SaveConfigToolStripMenuItem_Click;
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.Size = new Size(199, 22);
            closeToolStripMenuItem.Text = "Close";
            closeToolStripMenuItem.Click += CloseToolStripMenuItem_Click;
            // 
            // inGameValue
            // 
            inGameValue.ForeColor = Color.White;
            inGameValue.Location = new Point(0, 0);
            inGameValue.Name = "inGameValue";
            inGameValue.Size = new Size(0, 15);
            inGameValue.TabIndex = 1;
            inGameValue.TabStop = false;
            inGameValue.Visible = false;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(199, 22);
            toolStripMenuItem2.Text = "----------------------";
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new Size(199, 22);
            toolStripMenuItem3.Text = "----------------------";
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new Size(199, 22);
            toolStripMenuItem4.Text = "----------------------";
            // 
            // RealtimePpur
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.PPUR;
            ClientSize = new Size(316, 130);
            ContextMenuStrip = contextMenuStrip1;
            Controls.Add(inGameValue);
            Controls.Add(currentPp);
            Controls.Add(sr);
            Controls.Add(iffc);
            Controls.Add(good);
            Controls.Add(ok);
            Controls.Add(miss);
            Controls.Add(avgoffset);
            Controls.Add(ur);
            Controls.Add(avgoffsethelp);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "RealtimePpur";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "RealtimePPUR";
            TransparencyKey = SystemColors.Control;
            Closed += RealtimePPUR_Closed;
            Shown += RealtimePpur_Shown;
            MouseDown += RealtimePPUR_MouseDown;
            MouseMove += RealtimePPUR_MouseMove;
            contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)inGameValue).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.ToolStripMenuItem healthPercentageToolStripMenuItem;

        #endregion

        private Label currentPp;
        private Label sr;
        private Label iffc;
        private Label good;
        private Label ok;
        private Label miss;
        private Label avgoffset;
        private Label ur;
        private Label avgoffsethelp;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem modeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem offsetHelperToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem realtimePPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem realtimePPURToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem osuModeToolStripMenuItem;
        private System.Windows.Forms.PictureBox inGameValue;
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
        private System.Windows.Forms.ToolStripMenuItem ifFCPPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ifFCHitsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expectedManiaScoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem currentPositionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem higherScoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem highestScoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem userScoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem discordRichPresenceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveConfigToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem currentBPMToolStripMenuItem;
        private ToolStripMenuItem currentRankToolStripMenuItem;
        private ToolStripMenuItem remainingNotesToolStripMenuItem;
        private ToolStripMenuItem calculationOptionToolStripMenuItem;
        private ToolStripMenuItem pPLossModeToolStripMenuItem;
        private ToolStripMenuItem calculateFirstToolStripMenuItem;
        private ToolStripMenuItem overlayOptionToolStripMenuItem;
        private ToolStripMenuItem changePriorityToolStripMenuItem1;
        private ToolStripMenuItem changeFontToolStripMenuItem1;
        private ToolStripMenuItem loadFontToolStripMenuItem1;
        private ToolStripMenuItem resetFontToolStripMenuItem1;
        private ToolStripMenuItem graphToolStripMenuItem;
        private ToolStripMenuItem uRGraphToolStripMenuItem1;
        private ToolStripMenuItem strainGraphToolStripMenuItem1;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripMenuItem toolStripMenuItem4;
    }
}

