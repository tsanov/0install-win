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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Common.Controls;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Management.WinForms.Properties;

namespace ZeroInstall.Store.Management.WinForms.Nodes
{
    /// <summary>
    /// Models information about an implementation in an <see cref="IStore"/> for display in a GUI.
    /// </summary>
    public abstract class ImplementationNode : StoreNode, IContextMenu
    {
        #region Variables
        private readonly IStore _store;
        private readonly ManifestDigest _digest;
        #endregion

        #region Properities
        /// <summary>
        /// The digest identifying the implementation in the store.
        /// </summary>
        [Description("The digest identifying the implementation in the store.")]
        public string Digest { get { return _digest.BestDigest; } }

        /// <summary>
        /// The total size of the implementation in bytes.
        /// </summary>
        [Description("The total size of the implementation in bytes.")]
        public long Size { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new implementation node.
        /// </summary>
        /// <param name="store">The <see cref="IStore"/> the implementation is located in.</param>
        /// <param name="digest">The digest identifying the implementation.</param>
        protected ImplementationNode(IStore store, ManifestDigest digest)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            _store = store;
            _digest = digest;

            // Determine the total size of an implementation via its manifest file
            string manifestPath = Path.Combine(store.GetPath(digest), ".manifest");
            if (File.Exists(manifestPath))
                Size = Manifest.Load(manifestPath, ManifestFormat.FromPrefix(digest.BestPrefix)).TotalSize;
        }
        #endregion

        #region Delete
        /// <summary>
        /// Deletes this implemenation from the <see cref="IStore"/> it is located in.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if no matching implementation could be found in the <see cref="IStore"/>.</exception>
        /// <exception cref="IOException">Thrown if the implementation could not be deleted because it was in use.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the store is not permitted.</exception>
        public override void Delete()
        {
            try { _store.Remove(_digest); }
            #region Error handling
            catch (ImplementationNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message, ex);
            }
            #endregion
        }
        #endregion
        
        #region Verify
        /// <inheritdoc/>
        public override void Verify(IIOHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            _store.Verify(_digest, handler);
        }
        #endregion

        #region Context menu
        /// <inheritdoc/>
        public ContextMenu GetContextMenu()
        {
            return new ContextMenu(new[]
            {
                new MenuItem(Resources.OpenInFileManager, delegate { Process.Start(_store.GetPath(_digest)); })
            });
        }
        #endregion
    }
}
