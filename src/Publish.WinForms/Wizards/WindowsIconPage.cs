﻿/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Common;
using Common.Controls;
using ZeroInstall.Publish.Properties;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    internal partial class WindowsIconPage : UserControl
    {
        private System.Drawing.Icon _icon;

        /// <summary>
        /// Raised with the selected <see cref="Model.Icon"/>s.
        /// </summary>
        public event Action<IEnumerable<Model.Icon>> IconsSelected;

        public WindowsIconPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Injects the selected icon.
        /// </summary>
        public void SetIcon(System.Drawing.Icon icon)
        {
            _icon = icon;
            pictureBoxIcon.Image = icon.ToBitmap();
        }

        private void textBoxHref_TextChanged(object sender, EventArgs e)
        {
            buttonNext.Enabled = IsValid(textBoxHrefIco) && IsValid(textBoxHrefPng);
        }

        private static bool IsValid(UriTextBox uriTextBox)
        {
            return !string.IsNullOrEmpty(uriTextBox.Text) && uriTextBox.IsValid;
        }

        private void buttonSaveIco_Click(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog {Filter = "Windows Icon files|*.ico|All files|*.*"})
            {
                if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    using (var stream = File.Create(saveFileDialog.FileName))
                        _icon.Save(stream);
                }
            }
        }

        private void buttonSavePng_Click(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog {Filter = "PNG image files|*.png|All files|*.*"})
            {
                if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                    _icon.ToBitmap().Save(saveFileDialog.FileName, ImageFormat.Png);
            }
        }

        private void buttonSkip_Click(object sender, EventArgs e)
        {
            if (Msg.YesNo(this, Resources.AskSkipIcon, MsgSeverity.Info))
                IconsSelected(new Model.Icon[0]);
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            IconsSelected(new[]
            {
                new Model.Icon {Href = textBoxHrefIco.Uri, MimeType = Model.Icon.MimeTypeIco},
                new Model.Icon {Href = textBoxHrefPng.Uri, MimeType = Model.Icon.MimeTypePng}
            });
        }
    }
}
