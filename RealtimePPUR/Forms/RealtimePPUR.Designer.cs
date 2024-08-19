using System.Drawing;
using System.Windows.Forms;
using static RealtimePPUR.Classes.Helper;

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
            currentPp = new System.Windows.Forms.Label();
            sr = new System.Windows.Forms.Label();
            iffc = new System.Windows.Forms.Label();
            good = new System.Windows.Forms.Label();
            ok = new System.Windows.Forms.Label();
            miss = new System.Windows.Forms.Label();
            avgoffset = new System.Windows.Forms.Label();
            ur = new System.Windows.Forms.Label();
            avgoffsethelp = new System.Windows.Forms.Label();
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
            pPLossModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            changePriorityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            changeFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            resetFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            discordRichPresenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            inGameValue = new System.Windows.Forms.Label();
            currentBPMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            contextMenuStrip1.SuspendLayout();
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
            ur.RightToLeft = System.Windows.Forms.RightToLeft.No;
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
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { modeToolStripMenuItem, osuModeToolStripMenuItem, inGameOverlayToolStripMenuItem, pPLossModeToolStripMenuItem, changePriorityToolStripMenuItem, changeFontToolStripMenuItem, loadFontToolStripMenuItem, resetFontToolStripMenuItem, discordRichPresenceToolStripMenuItem, saveConfigToolStripMenuItem, closeToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(191, 268);
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
            osuModeToolStripMenuItem.Size = new Size(190, 22);
            osuModeToolStripMenuItem.Text = "osu! mode";
            osuModeToolStripMenuItem.Click += OsuModeToolStripMenuItem_Click;
            // 
            // inGameOverlayToolStripMenuItem
            // 
            inGameOverlayToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { sRToolStripMenuItem, sSPPToolStripMenuItem, currentPPToolStripMenuItem, currentACCToolStripMenuItem, hitsToolStripMenuItem, ifFCHitsToolStripMenuItem, uRToolStripMenuItem, offsetHelpToolStripMenuItem, expectedManiaScoreToolStripMenuItem, avgOffsetToolStripMenuItem, progressToolStripMenuItem, ifFCPPToolStripMenuItem, healthPercentageToolStripMenuItem, currentPositionToolStripMenuItem, higherScoreToolStripMenuItem, highestScoreToolStripMenuItem, userScoreToolStripMenuItem, currentBPMToolStripMenuItem });
            inGameOverlayToolStripMenuItem.Name = "inGameOverlayToolStripMenuItem";
            inGameOverlayToolStripMenuItem.Size = new Size(190, 22);
            inGameOverlayToolStripMenuItem.Text = "InGameOverlay";
            // 
            // sRToolStripMenuItem
            // 
            sRToolStripMenuItem.Name = "sRToolStripMenuItem";
            sRToolStripMenuItem.Size = new Size(184, 22);
            sRToolStripMenuItem.Text = "SR";
            sRToolStripMenuItem.Click += ToggleChecked;
            // 
            // sSPPToolStripMenuItem
            // 
            sSPPToolStripMenuItem.Name = "sSPPToolStripMenuItem";
            sSPPToolStripMenuItem.Size = new Size(184, 22);
            sSPPToolStripMenuItem.Text = "SSPP";
            sSPPToolStripMenuItem.Click += ToggleChecked;
            // 
            // currentPPToolStripMenuItem
            // 
            currentPPToolStripMenuItem.Name = "currentPPToolStripMenuItem";
            currentPPToolStripMenuItem.Size = new Size(184, 22);
            currentPPToolStripMenuItem.Text = "CurrentPP";
            currentPPToolStripMenuItem.Click += ToggleChecked;
            // 
            // currentACCToolStripMenuItem
            // 
            currentACCToolStripMenuItem.Name = "currentACCToolStripMenuItem";
            currentACCToolStripMenuItem.Size = new Size(184, 22);
            currentACCToolStripMenuItem.Text = "CurrentACC";
            currentACCToolStripMenuItem.Click += ToggleChecked;
            // 
            // hitsToolStripMenuItem
            // 
            hitsToolStripMenuItem.Name = "hitsToolStripMenuItem";
            hitsToolStripMenuItem.Size = new Size(184, 22);
            hitsToolStripMenuItem.Text = "Hits";
            hitsToolStripMenuItem.Click += ToggleChecked;
            // 
            // ifFCHitsToolStripMenuItem
            // 
            ifFCHitsToolStripMenuItem.Name = "ifFCHitsToolStripMenuItem";
            ifFCHitsToolStripMenuItem.Size = new Size(184, 22);
            ifFCHitsToolStripMenuItem.Text = "ifFCHits";
            ifFCHitsToolStripMenuItem.Click += ToggleChecked;
            // 
            // uRToolStripMenuItem
            // 
            uRToolStripMenuItem.Name = "uRToolStripMenuItem";
            uRToolStripMenuItem.Size = new Size(184, 22);
            uRToolStripMenuItem.Text = "UR";
            uRToolStripMenuItem.Click += ToggleChecked;
            // 
            // offsetHelpToolStripMenuItem
            // 
            offsetHelpToolStripMenuItem.Name = "offsetHelpToolStripMenuItem";
            offsetHelpToolStripMenuItem.Size = new Size(184, 22);
            offsetHelpToolStripMenuItem.Text = "OffsetHelp";
            offsetHelpToolStripMenuItem.Click += ToggleChecked;
            // 
            // expectedManiaScoreToolStripMenuItem
            // 
            expectedManiaScoreToolStripMenuItem.Name = "expectedManiaScoreToolStripMenuItem";
            expectedManiaScoreToolStripMenuItem.Size = new Size(184, 22);
            expectedManiaScoreToolStripMenuItem.Text = "ExpectedManiaScore";
            expectedManiaScoreToolStripMenuItem.Click += ToggleChecked;
            // 
            // avgOffsetToolStripMenuItem
            // 
            avgOffsetToolStripMenuItem.Name = "avgOffsetToolStripMenuItem";
            avgOffsetToolStripMenuItem.Size = new Size(184, 22);
            avgOffsetToolStripMenuItem.Text = "AvgOffset";
            avgOffsetToolStripMenuItem.Click += ToggleChecked;
            // 
            // progressToolStripMenuItem
            // 
            progressToolStripMenuItem.Name = "progressToolStripMenuItem";
            progressToolStripMenuItem.Size = new Size(184, 22);
            progressToolStripMenuItem.Text = "Progress";
            progressToolStripMenuItem.Click += ToggleChecked;
            // 
            // ifFCPPToolStripMenuItem
            // 
            ifFCPPToolStripMenuItem.Name = "ifFCPPToolStripMenuItem";
            ifFCPPToolStripMenuItem.Size = new Size(184, 22);
            ifFCPPToolStripMenuItem.Text = "ifFCPP";
            ifFCPPToolStripMenuItem.Click += ToggleChecked;
            // 
            // healthPercentageToolStripMenuItem
            // 
            healthPercentageToolStripMenuItem.Name = "healthPercentageToolStripMenuItem";
            healthPercentageToolStripMenuItem.Size = new Size(184, 22);
            healthPercentageToolStripMenuItem.Text = "Health Percentage";
            healthPercentageToolStripMenuItem.Click += ToggleChecked;
            // 
            // currentPositionToolStripMenuItem
            // 
            currentPositionToolStripMenuItem.Name = "currentPositionToolStripMenuItem";
            currentPositionToolStripMenuItem.Size = new Size(184, 22);
            currentPositionToolStripMenuItem.Text = "CurrentPosition";
            currentPositionToolStripMenuItem.Click += ToggleChecked;
            // 
            // higherScoreToolStripMenuItem
            // 
            higherScoreToolStripMenuItem.Name = "higherScoreToolStripMenuItem";
            higherScoreToolStripMenuItem.Size = new Size(184, 22);
            higherScoreToolStripMenuItem.Text = "HigherScoreDiff";
            higherScoreToolStripMenuItem.Click += ToggleChecked;
            // 
            // highestScoreToolStripMenuItem
            // 
            highestScoreToolStripMenuItem.Name = "highestScoreToolStripMenuItem";
            highestScoreToolStripMenuItem.Size = new Size(184, 22);
            highestScoreToolStripMenuItem.Text = "HighestScoreDiff";
            highestScoreToolStripMenuItem.Click += ToggleChecked;
            // 
            // userScoreToolStripMenuItem
            // 
            userScoreToolStripMenuItem.Name = "userScoreToolStripMenuItem";
            userScoreToolStripMenuItem.Size = new Size(184, 22);
            userScoreToolStripMenuItem.Text = "UserScore";
            userScoreToolStripMenuItem.Click += ToggleChecked;
            // 
            // pPLossModeToolStripMenuItem
            // 
            pPLossModeToolStripMenuItem.Name = "pPLossModeToolStripMenuItem";
            pPLossModeToolStripMenuItem.Size = new Size(190, 22);
            pPLossModeToolStripMenuItem.Text = "PPLossMode";
            pPLossModeToolStripMenuItem.Click += ToggleChecked;
            // 
            // changePriorityToolStripMenuItem
            // 
            changePriorityToolStripMenuItem.Name = "changePriorityToolStripMenuItem";
            changePriorityToolStripMenuItem.Size = new Size(190, 22);
            changePriorityToolStripMenuItem.Text = "Change Priority";
            changePriorityToolStripMenuItem.Click += ChangePriorityToolStripMenuItem_Click;
            // 
            // changeFontToolStripMenuItem
            // 
            changeFontToolStripMenuItem.Name = "changeFontToolStripMenuItem";
            changeFontToolStripMenuItem.Size = new Size(190, 22);
            changeFontToolStripMenuItem.Text = "Change Font";
            changeFontToolStripMenuItem.Click += ChangeFontToolStripMenuItem_Click;
            // 
            // loadFontToolStripMenuItem
            // 
            loadFontToolStripMenuItem.Name = "loadFontToolStripMenuItem";
            loadFontToolStripMenuItem.Size = new Size(190, 22);
            loadFontToolStripMenuItem.Text = "Load Font";
            loadFontToolStripMenuItem.Click += LoadFontToolStripMenuItem_Click;
            // 
            // resetFontToolStripMenuItem
            // 
            resetFontToolStripMenuItem.Name = "resetFontToolStripMenuItem";
            resetFontToolStripMenuItem.Size = new Size(190, 22);
            resetFontToolStripMenuItem.Text = "Reset Font";
            resetFontToolStripMenuItem.Click += ResetFontToolStripMenuItem_Click;
            // 
            // discordRichPresenceToolStripMenuItem
            // 
            discordRichPresenceToolStripMenuItem.Name = "discordRichPresenceToolStripMenuItem";
            discordRichPresenceToolStripMenuItem.Size = new Size(190, 22);
            discordRichPresenceToolStripMenuItem.Text = "Discord Rich Presence";
            discordRichPresenceToolStripMenuItem.Click +=  ToggleChecked;
            // 
            // saveConfigToolStripMenuItem
            // 
            saveConfigToolStripMenuItem.Name = "saveConfigToolStripMenuItem";
            saveConfigToolStripMenuItem.Size = new Size(190, 22);
            saveConfigToolStripMenuItem.Text = "Save Config";
            saveConfigToolStripMenuItem.Click += SaveConfigToolStripMenuItem_Click;
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.Size = new Size(190, 22);
            closeToolStripMenuItem.Text = "Close";
            closeToolStripMenuItem.Click += CloseToolStripMenuItem_Click;
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
            // currentBPMToolStripMenuItem
            // 
            currentBPMToolStripMenuItem.Name = "currentBPMToolStripMenuItem";
            currentBPMToolStripMenuItem.Size = new Size(184, 22);
            currentBPMToolStripMenuItem.Text = "CurrentBPM";
            currentBPMToolStripMenuItem.Click += ToggleChecked;
            // 
            // RealtimePpur
            // 
            this.avgoffsethelp.Font = new System.Drawing.Font(GuiFont, 20F, System.Drawing.FontStyle.Bold);
            this.ur.Font = new System.Drawing.Font(GuiFont, 25F, System.Drawing.FontStyle.Bold);
            this.avgoffset.Font = new System.Drawing.Font(GuiFont, 13F, System.Drawing.FontStyle.Bold);
            this.miss.Font = new System.Drawing.Font(GuiFont, 15F, System.Drawing.FontStyle.Bold);
            this.ok.Font = new System.Drawing.Font(GuiFont, 15F, System.Drawing.FontStyle.Bold);
            this.good.Font = new System.Drawing.Font(GuiFont, 15F, System.Drawing.FontStyle.Bold);
            this.iffc.Font = new System.Drawing.Font(GuiFont, 13F, System.Drawing.FontStyle.Bold);
            this.sr.Font = new System.Drawing.Font(GuiFont, 13F, System.Drawing.FontStyle.Bold);
            this.currentPp.Font = new System.Drawing.Font(GuiFont, 20F, System.Drawing.FontStyle.Bold);
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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
        private System.Windows.Forms.ToolStripMenuItem pPLossModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveConfigToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem currentBPMToolStripMenuItem;
    }
}

