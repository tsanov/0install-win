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
using System.IO;
using Common;
using ZeroInstall.Injector.Feeds;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Chooses a set of <see cref="Model.Implementation"/>s to satisfy the requirements of a program and its user. 
    /// </summary>
    /// <remarks>This is an application of the strategy pattern. Implementations of this interface should be immutable.</remarks>
    public interface ISolver
    {
        /// <summary>
        /// Solves the dependencies for a specific feed.
        /// </summary>
        /// <param name="requirements">A set of requirements/restrictions imposed by the user on the implementation selection process.</param>
        /// <param name="policy">Combines UI access, preferences and resources used to solve dependencies and download implementations.</param>
        /// <param name="staleFeeds">Indicates that one or more of the <see cref="Model.Feed"/>s used by the solver should be updated.</param>
        /// <returns>The <see cref="ImplementationSelection"/>s chosen for the feed.</returns>
        /// <remarks>Feed files may be downloaded, signature validation is performed, implementations are not downloaded.</remarks>
        /// <exception cref="UserCancelException">Thrown if the user clicked the "Cancel" button.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="requirements"/> is incomplete.</exception>
        /// <exception cref="IOException">Thrown if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="SolverException">Thrown if the dependencies could not be solved.</exception>
        Selections Solve(Requirements requirements, Policy policy, out bool staleFeeds);
    }
}
