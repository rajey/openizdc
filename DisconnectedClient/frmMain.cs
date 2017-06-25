﻿using OpenIZ.Mobile.Core;
using OpenIZ.Mobile.Core.Xamarin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp.WinForms;
using CefSharp;
using DisconnectedClient.JNI;

namespace DisconnectedClient
{
    public partial class frmDisconnectedClient : Form
    {

        // Browser
        public ChromiumWebBrowser m_browser = null;
        EventHandler<ApplicationProgressEventArgs> m_progressHandler;

        // Disconnected client ctor
        public frmDisconnectedClient(String url)
        {

            InitializeComponent();

            this.InitializeChromium(url);
        }

        /// <summary>
        /// Initialize chromium
        /// </summary>
        private void InitializeChromium(string url)
        {
         
            Action<ApplicationProgressEventArgs> updateUi = (e) =>
            {
                if (String.IsNullOrEmpty(e.ProgressText))
                    return;
                else
                {
                    lblStatus.Text = String.Format("{0} ({1:#0}%)", e.ProgressText, e.Progress * 100);
                    if (e.Progress >= 0 && e.Progress <= 1)
                    {
                        pgMain.Style = ProgressBarStyle.Continuous;
                        pgMain.Value = (int)(e.Progress * 100);
                    }
                    else
                    {
                        pgMain.Style = ProgressBarStyle.Marquee;

                    }
                }
                Application.DoEvents();
            };

            this.m_progressHandler = (o, ev) =>
            {
                this.Invoke(updateUi, ev);
            };
            XamarinApplicationContext.ProgressChanged += this.m_progressHandler;

            this.m_browser = new ChromiumWebBrowser(url);

#if !DEBUG
            mnsTools.Visible = Program.Parameters.Debug;
#endif

            this.m_browser.RegisterJsObject("OpenIZApplicationService", new AppletFunctionBridge(this));
            this.pnlMain.Controls.Add(this.m_browser);
            this.m_browser.Dock = DockStyle.Fill;

        }

        private void frmDisconnectedClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cef.Shutdown();
            XamarinApplicationContext.Current.Stop();
            XamarinApplicationContext.ProgressChanged -= this.m_progressHandler;

        }

        // Go dback
        public void Back()
        {
            this.m_browser.Back();
        }

        private void showDebugToolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.m_browser.ShowDevTools();
        }

        private void btnZoomWidth_Click(object sender, EventArgs e)
        {
            
            double zoomFactor = Double.Parse((sender as ToolStripMenuItem).Tag.ToString());
            this.m_browser.SetZoomLevel(zoomFactor);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.m_browser.Back();
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            this.m_browser.Forward();
        }

    }
}
