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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;
using NanoByte.Common;

namespace ZeroInstall.Store.Model.Selection
{
    /// <summary>
    /// An executable implementation of a <see cref="Feed"/> as a part of a <see cref="Selections"/>.
    /// </summary>
    /// <remarks>This class does not contain information on how to download the implementation in case it is not in cache. That must be obtained from a <see cref="Implementation"/> instance.</remarks>
    /// <seealso cref="Selections.Implementations"/>
    [XmlType("selection", Namespace = Feed.XmlNamespace)]
    [SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Justification = "IComparable is only used for deterministic ordering")]
    public sealed class ImplementationSelection : ImplementationBase, IInterfaceUriBindingContainer, ICloneable<ImplementationSelection>, IEquatable<ImplementationSelection>, IComparable<ImplementationSelection>
    {
        /// <inheritdoc/>
        internal override IEnumerable<Implementation> Implementations => Enumerable.Empty<Implementation>();

        /// <summary>
        /// The URI or local path of the interface this implementation is for.
        /// </summary>
        [Description("The URI or local path of the interface this implementation is for.")]
        [XmlIgnore]
        public FeedUri InterfaceUri { get; set; }

        /// <summary>
        /// The URL or local path of the feed that contains this implementation.
        /// <see cref="FeedUri.FromDistributionPrefix"/> is prepended if data is pulled from a native package manager.
        /// If <c>null</c> or <see cref="string.Empty"/> use <see cref="InterfaceUri"/> instead.
        /// </summary>
        [Description("The URL or local path of the feed that contains this implementation. \"distribution:\" is prepended if data is pulled from a native package manager. If null or empty use InterfaceUri instead.")]
        [XmlIgnore, CanBeNull]
        public FeedUri FromFeed { get; set; }

        #region XML serialization
        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="InterfaceUri"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [XmlAttribute("interface"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string InterfaceUriString { get => InterfaceUri?.ToStringRfc(); set => InterfaceUri = (value == null) ? null : new FeedUri(value); }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="FromFeed"/>
        [XmlAttribute("from-feed"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string FromFeedString { get => FromFeed?.ToStringRfc(); set => FromFeed = (value == null) ? null : new FeedUri(value); }

        /// <summary>
        /// Used for XML serialization.
        /// </summary>
        public ImplementationSelection()
        {}
        #endregion

        /// <summary>
        /// A file which, if present, indicates that the selection is still valid. This is sometimes used with distribution-provided selections. If not present and the ID starts with "package:", you'll need to query the distribution's package manager to check that this version is still installed.
        /// </summary>
        [Description("A file which, if present, indicates that the selection is still valid. This is sometimes used with distribution-provided selections. If not present and the ID starts with \"package:\", you'll need to query the distribution's package manager to check that this version is still installed.")]
        [XmlAttribute("quick-test-file"), CanBeNull]
        public string QuickTestFile { get; set; }

        /// <summary>
        /// All <see cref="Implementation"/>s that were considered by the solver when this one was chosen. May be <c>null</c> when generated by an external solver.
        /// </summary>
        [Browsable(false)]
        [XmlIgnore, CanBeNull]
        public IEnumerable<SelectionCandidate> Candidates { get; }

        /// <summary>
        /// Creates a new implemenetation selection.
        /// </summary>
        /// <param name="candidates">All candidates that were considered for selection (including the selected one). These are used to present the user with possible alternatives.</param>
        public ImplementationSelection(IEnumerable<SelectionCandidate> candidates)
        {
            Candidates = candidates.ToList();
        }

        /// <inheritdoc/>
        public override void Normalize(FeedUri feedUri)
        {
            base.Normalize(feedUri);

            EnsureNotNull(InterfaceUri, "interface", "selection");
        }

        #region Conversion
        /// <inheritdoc/>
        public override string ToString() => base.ToString() + " (" + InterfaceUri + ")";
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="ImplementationSelection"/>
        /// </summary>
        /// <returns>The cloned <see cref="ImplementationSelection"/>.</returns>
        ImplementationSelection ICloneable<ImplementationSelection>.Clone()
        {
            var implementation = new ImplementationSelection {InterfaceUri = InterfaceUri, FromFeed = FromFeed, QuickTestFile = QuickTestFile};
            CloneFromTo(this, implementation);
            return implementation;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="ImplementationSelection"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="ImplementationSelection"/>.</returns>
        public override Element Clone() => ((ICloneable<ImplementationSelection>)this).Clone();
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ImplementationSelection other)
        {
            if (other == null) return false;
            return base.Equals(other) && InterfaceUri == other.InterfaceUri && FromFeed == other.FromFeed && QuickTestFile == other.QuickTestFile;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj is ImplementationSelection && Equals((ImplementationSelection)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ InterfaceUri?.GetHashCode() ?? 0;
                result = (result * 397) ^ FromFeed?.GetHashCode() ?? 0;
                result = (result * 397) ^ QuickTestFile?.GetHashCode() ?? 0;
                return result;
            }
        }
        #endregion

        #region Comparison
        /// <inheritdoc/>
        public int CompareTo(ImplementationSelection other)
        {
            #region Sanity checks
            if (other == null) throw new ArgumentNullException(nameof(other));
            #endregion

            return StringComparer.Ordinal.Compare(InterfaceUri.ToStringRfc(), other.InterfaceUri.ToStringRfc());
        }
        #endregion
    }
}
