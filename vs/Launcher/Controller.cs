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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using Common;
using ZeroInstall.Fetchers;
using ZeroInstall.Launcher.Properties;
using ZeroInstall.Model;
using ZeroInstall.Launcher.Solver;
using ZeroInstall.Store.Feed;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Launcher
{
    /// <summary>
    /// Takes an interface URI and provides operations like solving and launching.
    /// </summary>
    /// <remarks>This class acts as a mediator between <see cref="ISolver"/>, <see cref="Fetcher"/> and <see cref="Executor"/>.</remarks>
    public class Controller
    {
        #region Variables
        /// <summary>Cached <see cref="ISolver"/> results.</summary>
        private Selections _selections;
        #endregion

        #region Properties
        /// <summary>
        /// The URI or local path (is always absolute) to the interface to use.
        /// </summary>
        public string InterfaceID { get; private set; }

        /// <summary>
        /// The solver to use for solving dependencies.
        /// </summary>
        public ISolver Solver { get; private set; }

        /// <summary>
        /// The user settings controlling the solving process.
        /// </summary>
        public Policy Policy { get; private set; }

        /// <summary>
        /// The user settings controlling the solving process.
        /// </summary>
        public IHandler Handler { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new Launcher for a specific interface.
        /// </summary>
        /// <param name="interfaceID">The URI or local path (may be relative) to the feed to use.</param>
        /// <param name="solver">The solver to use for solving dependencies.</param>
        /// <param name="policy">The user settings controlling the solving process.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked any questions or informed about progress.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="interfaceID"/> is not a valid URI or an existing local file.</exception>
        public Controller(string interfaceID, ISolver solver, Policy policy, IHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (solver == null) throw new ArgumentNullException("solver");
            if (policy == null) throw new ArgumentNullException("policy");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (interfaceID.StartsWith("http:")) InterfaceID = interfaceID;
            else
            { // If the interface ID is no HTTP URI, try to get an absolute path instead
                InterfaceID = Path.GetFullPath(interfaceID);
                if (!File.Exists(InterfaceID)) throw new ArgumentException(string.Format(Resources.NotInterfaceID, interfaceID, InterfaceID));
            }
            Solver = solver;
            Policy = policy;
            Handler = handler;
        }
        #endregion
        
        //--------------------//

        #region Selections
        /// <summary>
        /// Uses an <see cref="ISolver"/> to solve the dependencies for the specified interface.
        /// </summary>
        /// <remarks>Feed files may be downloaded, signature validation is performed, implementations are not downloaded.</remarks>
        /// <exception cref="UserCancelException">Thrown if the user clicked the "Cancel" button.</exception>
        /// <exception cref="IOException">Thrown if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="SolverException">Thrown if the dependencies could not be solved.</exception>
        public void Solve()
        {
            // Run the solver algorithm
            _selections = Solver.Solve(InterfaceID, Policy, Handler);
        }

        /// <summary>
        /// Uses an external set of <see cref="Selections"/> instead of using a <see cref="ISolver"/>.
        /// </summary>
        /// <param name="selections">The external set of <see cref="Selections"/> to use.</param>
        public void SetSelections(Selections selections)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            #endregion

            _selections = selections.CloneSelections();
        }

        /// <summary>
        /// Returns the <see cref="Selections"/> made earlier.
        /// </summary>
        /// <returns>The <see cref="ImplementationSelection"/>s chosen for the interface.</returns>
        /// <exception cref="InvalidOperationException">Thrown if neither <see cref="Solve"/> nor <see cref="SetSelections"/> was not called first.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Returns a new clone on each call")]
        public Selections GetSelections()
        {
            #region Sanity checks
            if (_selections == null) throw new InvalidOperationException(Resources.NotSolved);
            #endregion

            return _selections.CloneSelections();
        }
        #endregion

        #region Implementation
        /// <summary>
        /// Returns a list of any selected <see cref="ImplementationBase"/>s that are missing from the <see cref="ZeroInstall.Launcher.Policy.SearchStore"/>.
        /// </summary>
        /// <returns>An object that allows the main <see cref="ImplementationBase"/> to be executed with all its <see cref="Dependency"/>s injected.</returns>
        /// <exception cref="InvalidOperationException">Thrown if neither <see cref="Solve"/> nor <see cref="SetSelections"/> was not called first.</exception>
        /// <remarks>Feed files may be downloaded, no implementations are downloaded.</remarks>
        public IEnumerable<Implementation> ListUncachedImplementations()
        {
            ICollection<Implementation> notCached = new LinkedList<Implementation>();

            foreach (var implementation in _selections.Implementations)
            {
                // Local paths are considered to be always available
                if (!string.IsNullOrEmpty(implementation.LocalPath)) continue;

                // Don't try to download PackageImplementations
                // ToDo: PackageKit integration
                if (!string.IsNullOrEmpty(implementation.Package)) continue;

                // Check if an implementation with a matching digest is available in the cache
                if (Policy.SearchStore.Contains(implementation.ManifestDigest)) continue;

                // If not, get download information for the implementation by checking the original feed
                string feedUrl = implementation.FromFeed ?? implementation.InterfaceID;
                Feed feed = File.Exists(feedUrl) ? Feed.Load(feedUrl) : Policy.FeedManager.Cache.GetFeed(new Uri(feedUrl));
                feed.Simplify();
                notCached.Add(feed.GetImplementation(implementation.ID));
            }

            return notCached;
        }

        /// <summary>
        /// Downloads any selected <see cref="ImplementationBase"/>s that are missing from the <see cref="ZeroInstall.Launcher.Policy.SearchStore"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if neither <see cref="Solve"/> nor <see cref="SetSelections"/> was not called first.</exception>
        /// <remarks>Implementation archives may be downloaded, digest validation is performed. Will do nothing, if <see cref="NetworkLevel"/> is <see cref="NetworkLevel.Offline"/>.</remarks>
        /// <exception cref="UserCancelException">Thrown if a download or IO task was cancelled from another thread.</exception>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="IOException">Thrown if a downloaded file could not be written to the disk or extracted.</exception>
        /// <exception cref="DigestMismatchException">Thrown an <see cref="Model.Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to an <see cref="IStore"/> is not permitted.</exception>
        public void DownloadUncachedImplementations()
        {
            #region Sanity checks
            if (_selections == null) throw new InvalidOperationException(Resources.NotSolved);
            #endregion

            if (Policy.FeedManager.NetworkLevel == NetworkLevel.Offline) return;

            Policy.Fetcher.RunSync(new FetchRequest(ListUncachedImplementations()), Handler);
        }
        #endregion

        #region Launcher
        /// <summary>
        /// Prepares to run an application.
        /// </summary>
        /// <returns>An object that allows the main <see cref="ImplementationBase"/> to be executed with all its <see cref="Dependency"/>s injected.</returns>
        /// <exception cref="InvalidOperationException">Thrown if neither <see cref="Solve"/> nor <see cref="SetSelections"/> was not called first.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Performs extensive disk and network IO")]
        public Executor GetExecutor()
        {
            #region Sanity checks
            if (_selections == null) throw new InvalidOperationException(Resources.NotSolved);
            #endregion

            // Read to run the application
            return new Executor(_selections, Policy.SearchStore);
        }
        #endregion
    }
}
