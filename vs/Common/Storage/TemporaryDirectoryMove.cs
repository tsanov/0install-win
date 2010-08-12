﻿using System;
using System.IO;

namespace Common.Storage
{
    /// <summary>
    /// Helper class to move an existing directory to a temporary directory only within a
    /// using statement block.
    /// </summary>
    public class TemporaryDirectoryMove : IDisposable
    {
        private readonly string _originalPath, _movedPath;

        public string OriginalPath
        {
            get { return _originalPath; }
        }

        public string BackupPath
        {
            get { return _movedPath; }
        }

        /// <summary>
        /// Renames an existing directory by moving it to a path generated by <see cref="Path.GetRandomFileName"/>
        /// If the path doesn't point to anything, it does nothing.
        /// </summary>
        /// <param name="path">file system path to move</param>
        public TemporaryDirectoryMove(string path)
        {
            if (String.IsNullOrEmpty(path)) throw new ArgumentNullException("path", @"null or empty string passed to TemporaryDirectoryMove");
            if (Directory.Exists(path))
            {
                string inexistantPath = Path.Combine(Path.Combine(path, ".."), Path.GetRandomFileName());
                Directory.Move(path, inexistantPath);
                _originalPath = path;
                _movedPath = inexistantPath;
            }
        }

        /// <summary>
        /// Deletes the directory currently existing at the original path and
        /// moves the previously renamed directory to it's original path.
        /// </summary>
        public void Dispose()
        {
            if (!String.IsNullOrEmpty(_originalPath))
            {
                if (Directory.Exists(OriginalPath))
                    Directory.Delete(OriginalPath, true);
                Directory.Move(BackupPath, OriginalPath);
            }
        }
    }
}
