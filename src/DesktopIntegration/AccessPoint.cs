﻿/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.ComponentModel;
using System.Xml.Serialization;
using ZeroInstall.Model;
using ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Access points build upon <see cref="Capability"/>s and represent more invasive changes to the desktop environment's UI.
    /// </summary>
    [XmlType("access-point", Namespace = XmlNamespace)]
    public abstract class AccessPoint : XmlUnknown, ICloneable
    {
        #region Constants
        /// <summary>
        /// The XML namespace used for storing desktop integration data.
        /// </summary>
        public const string XmlNamespace = "http://0install.de/schema/injector/desktop-integration";
        #endregion

        #region Properties
        /// <summary>
        /// The name of the command in the <see cref="Feed"/> to use when launching via this access point.
        /// </summary>
        [Description("The name of the command in the feed to use when launching via this access point.")]
        [XmlAttribute("command")]
        public string Command { get; set; }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="AccessPoint"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AccessPoint"/>.</returns>
        public abstract AccessPoint CloneAccessPoint();

        /// <summary>
        /// Creates a deep copy of this <see cref="AccessPoint"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AccessPoint"/>.</returns>
        public object Clone()
        {
            return CloneAccessPoint();
        }
        #endregion
    }
}
