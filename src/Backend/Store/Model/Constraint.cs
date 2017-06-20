﻿/*
 * Copyright 2010-2016 Bastian Eicher
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
using JetBrains.Annotations;
using NanoByte.Common;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Restricts the set of versions from which the injector may choose an <see cref="Implementation"/>.
    /// </summary>
    [Description("Restricts the set of versions from which the injector may choose an implementation.")]
    [Serializable, XmlRoot("constraint", Namespace = Feed.XmlNamespace), XmlType("constraint", Namespace = Feed.XmlNamespace)]
    public class Constraint : FeedElement, ICloneable<Constraint>, IEquatable<Constraint>
    {
        /// <summary>
        /// This is the lowest-numbered version that can be chosen.
        /// </summary>
        [Description("This is the lowest-numbered version that can be chosen.")]
        [XmlIgnore, CanBeNull]
        public ImplementationVersion NotBefore { get; set; }

        /// <summary>
        /// This version and all later versions are unsuitable.
        /// </summary>
        [Description("This version and all later versions are unsuitable.")]
        [XmlIgnore, CanBeNull]
        public ImplementationVersion Before { get; set; }

        #region XML serialization
        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="NotBefore"/>
        [XmlAttribute("not-before"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string NotBeforeString { get => (NotBefore == null ? null : NotBefore.ToString()); set => NotBefore = string.IsNullOrEmpty(value) ? null : new ImplementationVersion(value); }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Before"/>
        [XmlAttribute("before"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string BeforeString { get => Before?.ToString(); set => Before = string.IsNullOrEmpty(value) ? null : new ImplementationVersion(value); }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the constraint in the form "NotBefore =&lt; Ver %lt; Before". Not safe for parsing!
        /// </summary>
        public override string ToString() => $"{NotBefore} =< Ver < {Before}";
        #endregion

        #region Clone
        /// <summary>
        /// Creates a copy of this <see cref="Constraint"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Constraint"/>.</returns>
        public Constraint Clone() => new Constraint {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, IfZeroInstallVersion = IfZeroInstallVersion, NotBefore = NotBefore, Before = Before};
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Constraint other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.NotBefore == NotBefore && other.Before == Before;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj.GetType() == typeof(Constraint) && Equals((Constraint)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ NotBefore?.GetHashCode() ?? 0;
                result = (result * 397) ^ Before?.GetHashCode() ?? 0;
                return result;
            }
        }
        #endregion
    }
}
