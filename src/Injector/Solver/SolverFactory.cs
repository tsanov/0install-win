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

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Creates <see cref="ISolver"/> instances.
    /// </summary>
    public static class SolverFactory
    {
        /// <summary>
        /// Creates an <see cref="ISolver"/> instance.
        /// </summary>
        public static ISolver CreateDefault()
        {
            //return new FallbackSolver(new SimpleSolver(), new ExternalSolver());
            return new ExternalSolver();
        }
    }
}