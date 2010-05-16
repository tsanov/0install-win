﻿/*
 * Copyright 2010 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ZeroInstall.Central
{
    partial class MainForm : Form
    {
        #region Constructor
        public MainForm()
        {
            InitializeComponent();

            browserNewApps.CanGoBackChanged += delegate { toolStripButtonBack.Enabled = browserNewApps.CanGoBack; };
        }
        #endregion

        #region Startup
        private void MainForm_Load(object sender, EventArgs e)
        {
            // ToDo: Check if the user has any MyApps entries, before showing the "new apps" page
            tabControlApps.SelectedTab = tabPageNewApps;
        }
        #endregion

        //--------------------//

        #region New apps browser
        private void toolStripButtonBack_Click(object sender, EventArgs e)
        {
            browserNewApps.GoBack();
        }

        private const string UrlPostfixFeed = "#0install-feed";
        private const string UrlPostfixBrowser = "#external-browser";

        private void browserNewApps_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            switch (e.Url.Fragment)
            {
                case UrlPostfixFeed:
                    // ToDo: Display details about the feed
                    MessageBox.Show(e.Url.OriginalString.Replace(UrlPostfixFeed, ""));
                    e.Cancel = true;
                    break;

                case UrlPostfixBrowser:
                    // Use the system's default web browser to open the URL
                    Process.Start(e.Url.OriginalString.Replace(UrlPostfixBrowser, ""));
                    e.Cancel = true;
                    break;
            }
        }

        private void browserNewApps_NewWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Prevent any popups
            e.Cancel = true;
        }
        #endregion

        #region Tools
        private void buttonAddFeed_Click(object sender, EventArgs e)
        {
            using (var feedUrlForm = new FeedUriForm())
                feedUrlForm.ShowDialog(this);
        }

        private void buttonManageCache_Click(object sender, EventArgs e)
        {
            Program.LaunchHelperApp(this, "0storew.exe");
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            // ToDo
        }
        #endregion
    }
}
