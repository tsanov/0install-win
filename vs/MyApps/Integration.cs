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
using System.Xml.Serialization;
using ZeroInstall.Model;

namespace ZeroInstall.MyApps
{
    /// <summary>
    /// Integrations describe how an application is made available to user in the operating system's environment.
    /// </summary>
    [XmlType("integration", Namespace = "http://0install.de/schema/my-apps/app-list")]
    public abstract class Integration : ICloneable
    {
        #region Properties
        /// <summary>
        /// An alternative executable to to run from the main <see cref="Implementation"/> instead of <see cref="Element.Main"/>.
        /// </summary>
        public string Main { get; set; }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Integration"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Integration"/>.</returns>
        public abstract Integration CloneIntegration();

        public object Clone()
        {
            return CloneIntegration();
        }
        #endregion
    }
}
