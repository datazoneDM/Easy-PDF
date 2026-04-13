
namespace H2PControl
{
    partial class H2PControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(H2PControl));
            this.lsth2p = new DevExpress.XtraEditors.ListBoxControl();
            this.bnt종료 = new DevExpress.XtraEditors.SimpleButton();
            this.lbxClients = new DevExpress.XtraEditors.ListBoxControl();
            this.BtnStart = new DevExpress.XtraEditors.SimpleButton();
            this.BtnStop = new DevExpress.XtraEditors.SimpleButton();
            this.TabControl = new DevExpress.XtraTab.XtraTabControl();
            this.Tabserver = new DevExpress.XtraTab.XtraTabPage();
            this.lsth2p_5 = new DevExpress.XtraEditors.ListBoxControl();
            this.lsth2p_4 = new DevExpress.XtraEditors.ListBoxControl();
            this.lsth2p_3 = new DevExpress.XtraEditors.ListBoxControl();
            this.lsth2p_2 = new DevExpress.XtraEditors.ListBoxControl();
            this.lsth2p_1 = new DevExpress.XtraEditors.ListBoxControl();
            this.lab작업중 = new DevExpress.XtraEditors.LabelControl();
            this.lbJobs = new DevExpress.XtraEditors.ListBoxControl();
            this.lab접속중 = new DevExpress.XtraEditors.LabelControl();
            this.Tabclient = new DevExpress.XtraTab.XtraTabPage();
            this.lbxMsg = new DevExpress.XtraEditors.ListBoxControl();
            this.lblProgress = new DevExpress.XtraEditors.TextEdit();
            this.progressBar = new DevExpress.XtraEditors.ProgressBarControl();
            this.btnSendFile = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.lsth2p)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lbxClients)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TabControl)).BeginInit();
            this.TabControl.SuspendLayout();
            this.Tabserver.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lsth2p_5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lsth2p_4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lsth2p_3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lsth2p_2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lsth2p_1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lbJobs)).BeginInit();
            this.Tabclient.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lbxMsg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lblProgress.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBar.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // lsth2p
            // 
            this.lsth2p.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lsth2p.Appearance.BackColor = System.Drawing.Color.White;
            this.lsth2p.Appearance.Font = new System.Drawing.Font("나눔고딕코딩", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lsth2p.Appearance.Options.UseBackColor = true;
            this.lsth2p.Appearance.Options.UseFont = true;
            this.lsth2p.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.lsth2p.Location = new System.Drawing.Point(107, 0);
            this.lsth2p.Name = "lsth2p";
            this.lsth2p.Size = new System.Drawing.Size(400, 332);
            this.lsth2p.TabIndex = 0;
            // 
            // bnt종료
            // 
            this.bnt종료.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bnt종료.Appearance.Font = new System.Drawing.Font("카이겐고딕 KR Regular", 9.749999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bnt종료.Appearance.Options.UseFont = true;
            this.bnt종료.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("bnt종료.ImageOptions.Image")));
            this.bnt종료.Location = new System.Drawing.Point(0, 398);
            this.bnt종료.Name = "bnt종료";
            this.bnt종료.Size = new System.Drawing.Size(274, 40);
            this.bnt종료.TabIndex = 1;
            this.bnt종료.Text = "F1 ~ F8 프로그램 강제종료 &&  this.종료";
            this.bnt종료.Click += new System.EventHandler(this.bnt종료_Click);
            // 
            // lbxClients
            // 
            this.lbxClients.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbxClients.Appearance.Font = new System.Drawing.Font("카이겐고딕 KR Regular", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbxClients.Appearance.Options.UseFont = true;
            this.lbxClients.Appearance.Options.UseTextOptions = true;
            this.lbxClients.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lbxClients.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.lbxClients.Location = new System.Drawing.Point(30, 0);
            this.lbxClients.Name = "lbxClients";
            this.lbxClients.Size = new System.Drawing.Size(75, 165);
            this.lbxClients.TabIndex = 2;
            // 
            // BtnStart
            // 
            this.BtnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BtnStart.Appearance.Font = new System.Drawing.Font("카이겐고딕 KR Regular", 9.749999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnStart.Appearance.Options.UseFont = true;
            this.BtnStart.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("BtnStart.ImageOptions.Image")));
            this.BtnStart.Location = new System.Drawing.Point(280, 398);
            this.BtnStart.Name = "BtnStart";
            this.BtnStart.Size = new System.Drawing.Size(67, 40);
            this.BtnStart.TabIndex = 3;
            this.BtnStart.Text = "시작";
            this.BtnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // BtnStop
            // 
            this.BtnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BtnStop.Appearance.Font = new System.Drawing.Font("카이겐고딕 KR Regular", 9.749999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnStop.Appearance.Options.UseFont = true;
            this.BtnStop.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("BtnStop.ImageOptions.Image")));
            this.BtnStop.Location = new System.Drawing.Point(353, 398);
            this.BtnStop.Name = "BtnStop";
            this.BtnStop.Size = new System.Drawing.Size(67, 40);
            this.BtnStop.TabIndex = 4;
            this.BtnStop.Text = "종료";
            this.BtnStop.Click += new System.EventHandler(this.BtnStop_Click);
            // 
            // TabControl
            // 
            this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TabControl.Location = new System.Drawing.Point(0, 2);
            this.TabControl.Name = "TabControl";
            this.TabControl.SelectedTabPage = this.Tabserver;
            this.TabControl.Size = new System.Drawing.Size(1247, 363);
            this.TabControl.TabIndex = 5;
            this.TabControl.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.Tabserver,
            this.Tabclient});
            // 
            // Tabserver
            // 
            this.Tabserver.Controls.Add(this.lsth2p_5);
            this.Tabserver.Controls.Add(this.lsth2p_4);
            this.Tabserver.Controls.Add(this.lsth2p_3);
            this.Tabserver.Controls.Add(this.lsth2p_2);
            this.Tabserver.Controls.Add(this.lsth2p_1);
            this.Tabserver.Controls.Add(this.lab작업중);
            this.Tabserver.Controls.Add(this.lbJobs);
            this.Tabserver.Controls.Add(this.lab접속중);
            this.Tabserver.Controls.Add(this.lsth2p);
            this.Tabserver.Controls.Add(this.lbxClients);
            this.Tabserver.Name = "Tabserver";
            this.Tabserver.Size = new System.Drawing.Size(1239, 332);
            this.Tabserver.Text = "SERVER";
            // 
            // lsth2p_5
            // 
            this.lsth2p_5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lsth2p_5.Appearance.Font = new System.Drawing.Font("나눔고딕코딩", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lsth2p_5.Appearance.Options.UseFont = true;
            this.lsth2p_5.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.lsth2p_5.Location = new System.Drawing.Point(2072, 0);
            this.lsth2p_5.Name = "lsth2p_5";
            this.lsth2p_5.Size = new System.Drawing.Size(390, 332);
            this.lsth2p_5.TabIndex = 12;
            // 
            // lsth2p_4
            // 
            this.lsth2p_4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lsth2p_4.Appearance.Font = new System.Drawing.Font("나눔고딕코딩", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lsth2p_4.Appearance.Options.UseFont = true;
            this.lsth2p_4.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.lsth2p_4.Location = new System.Drawing.Point(1681, 0);
            this.lsth2p_4.Name = "lsth2p_4";
            this.lsth2p_4.Size = new System.Drawing.Size(390, 332);
            this.lsth2p_4.TabIndex = 11;
            // 
            // lsth2p_3
            // 
            this.lsth2p_3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lsth2p_3.Appearance.Font = new System.Drawing.Font("나눔고딕코딩", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lsth2p_3.Appearance.Options.UseFont = true;
            this.lsth2p_3.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.lsth2p_3.Location = new System.Drawing.Point(1290, 0);
            this.lsth2p_3.Name = "lsth2p_3";
            this.lsth2p_3.Size = new System.Drawing.Size(390, 332);
            this.lsth2p_3.TabIndex = 10;
            // 
            // lsth2p_2
            // 
            this.lsth2p_2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lsth2p_2.Appearance.Font = new System.Drawing.Font("나눔고딕코딩", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lsth2p_2.Appearance.Options.UseFont = true;
            this.lsth2p_2.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.lsth2p_2.Location = new System.Drawing.Point(899, 0);
            this.lsth2p_2.Name = "lsth2p_2";
            this.lsth2p_2.Size = new System.Drawing.Size(390, 332);
            this.lsth2p_2.TabIndex = 9;
            // 
            // lsth2p_1
            // 
            this.lsth2p_1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lsth2p_1.Appearance.BackColor = System.Drawing.Color.White;
            this.lsth2p_1.Appearance.Font = new System.Drawing.Font("나눔고딕코딩", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lsth2p_1.Appearance.Options.UseBackColor = true;
            this.lsth2p_1.Appearance.Options.UseFont = true;
            this.lsth2p_1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.lsth2p_1.Location = new System.Drawing.Point(508, 0);
            this.lsth2p_1.Name = "lsth2p_1";
            this.lsth2p_1.Size = new System.Drawing.Size(390, 332);
            this.lsth2p_1.TabIndex = 8;
            // 
            // lab작업중
            // 
            this.lab작업중.Appearance.Font = new System.Drawing.Font("카이겐고딕 KR Regular", 9.749999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lab작업중.Appearance.Options.UseFont = true;
            this.lab작업중.Appearance.Options.UseTextOptions = true;
            this.lab작업중.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lab작업중.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lab작업중.Location = new System.Drawing.Point(3, 170);
            this.lab작업중.Name = "lab작업중";
            this.lab작업중.Size = new System.Drawing.Size(24, 162);
            this.lab작업중.TabIndex = 7;
            this.lab작업중.Text = "작\r\n\r\n\r\n업\r\n\r\n\r\n중";
            // 
            // lbJobs
            // 
            this.lbJobs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbJobs.Appearance.Font = new System.Drawing.Font("카이겐고딕 KR Regular", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbJobs.Appearance.Options.UseFont = true;
            this.lbJobs.Appearance.Options.UseTextOptions = true;
            this.lbJobs.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lbJobs.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.lbJobs.Location = new System.Drawing.Point(30, 167);
            this.lbJobs.Name = "lbJobs";
            this.lbJobs.Size = new System.Drawing.Size(75, 165);
            this.lbJobs.TabIndex = 6;
            // 
            // lab접속중
            // 
            this.lab접속중.Appearance.Font = new System.Drawing.Font("카이겐고딕 KR Regular", 9.749999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lab접속중.Appearance.Options.UseFont = true;
            this.lab접속중.Appearance.Options.UseTextOptions = true;
            this.lab접속중.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lab접속중.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lab접속중.Location = new System.Drawing.Point(3, 3);
            this.lab접속중.Name = "lab접속중";
            this.lab접속중.Size = new System.Drawing.Size(24, 161);
            this.lab접속중.TabIndex = 5;
            this.lab접속중.Text = "접\r\n\r\n\r\n속\r\n\r\n\r\n중";
            // 
            // Tabclient
            // 
            this.Tabclient.Controls.Add(this.lbxMsg);
            this.Tabclient.Name = "Tabclient";
            this.Tabclient.Size = new System.Drawing.Size(1239, 332);
            this.Tabclient.Text = "CLIENT";
            // 
            // lbxMsg
            // 
            this.lbxMsg.Appearance.Font = new System.Drawing.Font("나눔고딕코딩", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbxMsg.Appearance.Options.UseFont = true;
            this.lbxMsg.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.lbxMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbxMsg.Location = new System.Drawing.Point(0, 0);
            this.lbxMsg.Name = "lbxMsg";
            this.lbxMsg.Size = new System.Drawing.Size(1239, 332);
            this.lbxMsg.TabIndex = 3;
            // 
            // lblProgress
            // 
            this.lblProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblProgress.Location = new System.Drawing.Point(134, 371);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Properties.AutoHeight = false;
            this.lblProgress.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.lblProgress.Size = new System.Drawing.Size(359, 23);
            this.lblProgress.TabIndex = 5;
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progressBar.Location = new System.Drawing.Point(3, 371);
            this.progressBar.Name = "progressBar";
            this.progressBar.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.progressBar.Properties.ShowTitle = true;
            this.progressBar.Size = new System.Drawing.Size(129, 23);
            this.progressBar.TabIndex = 4;
            // 
            // btnSendFile
            // 
            this.btnSendFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSendFile.Appearance.Font = new System.Drawing.Font("카이겐고딕 KR Regular", 9.749999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSendFile.Appearance.Options.UseFont = true;
            this.btnSendFile.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnSendFile.ImageOptions.Image")));
            this.btnSendFile.Location = new System.Drawing.Point(426, 398);
            this.btnSendFile.Name = "btnSendFile";
            this.btnSendFile.Size = new System.Drawing.Size(67, 40);
            this.btnSendFile.TabIndex = 6;
            this.btnSendFile.Text = "전송";
            this.btnSendFile.Click += new System.EventHandler(this.btnSendFile_Click);
            // 
            // H2PControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1248, 438);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.btnSendFile);
            this.Controls.Add(this.TabControl);
            this.Controls.Add(this.BtnStop);
            this.Controls.Add(this.BtnStart);
            this.Controls.Add(this.bnt종료);
            this.IconOptions.Icon = ((System.Drawing.Icon)(resources.GetObject("H2PControl.IconOptions.Icon")));
            this.LookAndFeel.SkinName = "McSkin";
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.Name = "H2PControl";
            this.Opacity = 0.8D;
            this.Text = "H2PControl Ver2.0(2026.03.25)";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.H2PControl_FormClosed);
            this.Load += new System.EventHandler(this.H2PControl_Load);
            this.LocationChanged += new System.EventHandler(this.H2PControl_LocationChanged);
            ((System.ComponentModel.ISupportInitialize)(this.lsth2p)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lbxClients)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TabControl)).EndInit();
            this.TabControl.ResumeLayout(false);
            this.Tabserver.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lsth2p_5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lsth2p_4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lsth2p_3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lsth2p_2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lsth2p_1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lbJobs)).EndInit();
            this.Tabclient.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lbxMsg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lblProgress.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBar.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.ListBoxControl lsth2p;
        private DevExpress.XtraEditors.SimpleButton bnt종료;
        private DevExpress.XtraEditors.ListBoxControl lbxClients;
        private DevExpress.XtraEditors.SimpleButton BtnStart;
        private DevExpress.XtraEditors.SimpleButton BtnStop;
        private DevExpress.XtraTab.XtraTabControl TabControl;
        private DevExpress.XtraTab.XtraTabPage Tabserver;
        private DevExpress.XtraTab.XtraTabPage Tabclient;
        private DevExpress.XtraEditors.ListBoxControl lbxMsg;
        private DevExpress.XtraEditors.ProgressBarControl progressBar;
        private DevExpress.XtraEditors.TextEdit lblProgress;
        private DevExpress.XtraEditors.SimpleButton btnSendFile;
        private DevExpress.XtraEditors.LabelControl lab접속중;
        private DevExpress.XtraEditors.LabelControl lab작업중;
        private DevExpress.XtraEditors.ListBoxControl lbJobs;
        private DevExpress.XtraEditors.ListBoxControl lsth2p_5;
        private DevExpress.XtraEditors.ListBoxControl lsth2p_4;
        private DevExpress.XtraEditors.ListBoxControl lsth2p_3;
        private DevExpress.XtraEditors.ListBoxControl lsth2p_2;
        private DevExpress.XtraEditors.ListBoxControl lsth2p_1;
    }
}

