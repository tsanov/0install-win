﻿/*
 * Copyright 2010 Bastian Eicher, Simon E. Silva Lauinger
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ZeroInstall.Store.Feed
{
    #region Enumerations
    /// <seealso cref="GnuPGSecretKey.Algorithm"/>
    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    public enum GnuPGAlgorithm
    {
        ///<summary>The algorithm used is unknown.</summary>
        Unknown = 0,
        /// <summary>RSA crypto system</summary>
        Rsa = 1,
        /// <summary>Elgamal crypto system</summary>
        Elgamal = 16,
        /// <summary>DAS crypto system</summary>
        Dsa = 17
    }
    #endregion

    /// <summary>
    /// Represents a secret key stored in the local <see cref="GnuPG"/> profile.
    /// </summary>
    public struct GnuPGSecretKey
    {
        #region Variables
        /// <summary>
        /// Length of key in bits.
        /// </summary>
        public readonly int BitLength;

        /// <summary>
        /// Encryption algorithm used.
        /// </summary>
        public readonly GnuPGAlgorithm Algorithm;

        /// <summary>
        /// A unique identifier string for the key.
        /// </summary>
        public readonly string KeyID;

        /// <summary>
        /// The point in time when the key was created in UTC.
        /// </summary>
        public readonly DateTime CreationDate;

        /// <summary>
        /// The user's name, e-mail address, etc.
        /// </summary>
        public readonly string UserID;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="GnuPG"/> secret key representation.
        /// </summary>
        /// <param name="bitLength">Length of key in bits.</param>
        /// <param name="keyType">Encryption algorithm used.</param>
        /// <param name="keyID"> A unique identifier string for the key.</param>
        /// <param name="creationDate">The point in time when the key was created in UTC.</param>
        /// <param name="userID">The user's name, e-mail address, etc.</param>
        public GnuPGSecretKey(int bitLength, GnuPGAlgorithm keyType, string keyID, DateTime creationDate, string userID)
        {
            BitLength = bitLength;
            Algorithm = keyType;
            KeyID = keyID;
            CreationDate = creationDate;
            UserID = userID;
        }
        #endregion

        #region Factory methods
        /// <summary>
        /// Creates a new <see cref="GnuPG"/> secret key representation from a console line.
        /// </summary>
        /// <param name="line">A console line generated by "gpgp --list-secret-keys --with-colons".</param>
        /// <returns>The parsed see cref="GnuPG"/> secret key representation.</returns>
        /// <exception cref="FormatException">Thrown <paramref name="line"/> cannot be properly parsed.</exception>
        public static GnuPGSecretKey Parse(string line)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(line)) throw new ArgumentNullException("line");
            #endregion

            string[] componenets = line.Split(':');
            if (componenets.Length != 13) throw new FormatException("The line must contain 13 colon-separated blocks.");

            return new GnuPGSecretKey(
                int.Parse(componenets[2]),
                (GnuPGAlgorithm)int.Parse(componenets[3]),
                componenets[4],
                DateTime.Parse(componenets[5], CultureInfo.InvariantCulture),
                componenets[9]);
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <inheritdoc/>
        public override string ToString()
        {
            return UserID;
        }
        #endregion
    }
}
