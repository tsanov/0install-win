﻿/*
 * Copyright 2010-2015 Bastian Eicher
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

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// An object containing a key ID for an <see cref="IOpenPgp"/> public or private key.
    /// </summary>
    public interface IKeyIDContainer
    {
        /// <summary>
        /// An OpenPGP key ID. A short identifier for a key. The lower 64 bits of <see cref="IFingerprintContainer.GetFingerprint"/>.
        /// </summary>
        long KeyID { get; }
    }
}