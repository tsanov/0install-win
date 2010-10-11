﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics;

namespace Common.Utils
{
    public abstract class HierarchyEntry
    {
        public delegate void EntryHandler(HierarchyEntry entry);

        private EntryContainer _parent;

        public string Name { get; private set; }
        public DateTime LastWriteTime { get; private set; }
        public string RelativePath
        {
            get
            {
                if (IsRoot()) return Name;
                else return Path.Combine(_parent.RelativePath, Name);
            }
        }

        protected HierarchyEntry(string name, EntryContainer parent, DateTime lastWriteTime)
        {
            #region Preconditions
            Debug.Assert(name != null);
            #endregion

            if (name != null && name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1) throw new ArgumentException("Invalid file name.");

            _parent = parent;
            Name = name;
            LastWriteTime = lastWriteTime;
        }

        public bool IsRoot()
        {
            return _parent == null;
        }

        public abstract void AcceptVisitor(HierarchyVisitor visitor);
    }

    public class FileEntry : HierarchyEntry
    {
        private MemoryStream _content;
        private readonly bool _executable;
        public bool IsExecutable { get { return _executable; } }

        public byte[] Content
        {
            get { return _content.ToArray(); }
        }

        internal FileEntry(string name, byte[] content, EntryContainer parent, bool executable, DateTime lastWrite)
            : base(name, parent, lastWrite)
        {
            #region Preconditions
            Debug.Assert(!string.IsNullOrEmpty(name));
            Debug.Assert(content != null);
            Debug.Assert(parent != null);
            #endregion

            _content = new MemoryStream(content.Length);
            _content.Write(content, 0, content.Length);
            _content.Seek(0, SeekOrigin.Begin);
            _executable = executable;
        }
        
        public override void AcceptVisitor(HierarchyVisitor visitor)
        { visitor.VisitFile(this); }
    }

    public abstract class EntryContainer : HierarchyEntry
    {
        protected List<HierarchyEntry> entries = new List<HierarchyEntry>();

        protected EntryContainer(string name, EntryContainer parent, DateTime lastWrite)
            : base(name, parent, lastWrite)
        { }

        public void Add(HierarchyEntry newEntry)
        {
            entries.Add(newEntry);
            entries.Sort((HierarchyEntry left, HierarchyEntry right) => StringComparer.InvariantCulture.Compare(left.Name, right.Name));
        }

        public IEnumerable<HierarchyEntry> Children
        {
            get { return entries; }
        }
    }

    public class FolderEntry : EntryContainer
    {
        internal FolderEntry(string name, EntryContainer parent, DateTime lastWrite)
            : base(name, parent, lastWrite)
        {
            #region Preconditions
            Debug.Assert(!string.IsNullOrEmpty(name));
            Debug.Assert(parent != null);
            #endregion
        }

        public override void AcceptVisitor(HierarchyVisitor visitor)
        { visitor.VisitFolder(this); }
    }

    public class RootEntry : EntryContainer
    {
        internal RootEntry(DateTime lastWrite)
            : base("", null, lastWrite)
        { }

        public override void AcceptVisitor(HierarchyVisitor visitor)
        { visitor.VisitRoot(this); }
    }

    public abstract class HierarchyVisitor
    {
        public virtual void VisitFile(FileEntry entry) { }

        public virtual void VisitFolder(FolderEntry entry)
        { visitChildren(entry); }

        public virtual void VisitRoot(RootEntry entry)
        { visitChildren(entry); }

        protected void visitChildren(EntryContainer entry)
        {
            foreach (var child in entry.Children)
                child.AcceptVisitor(this);
        }
    }

    class HierarchyToZip : HierarchyVisitor
    {
        ZipOutputStream zip;

        internal HierarchyToZip(ZipOutputStream zipOut)
        {
            Debug.Assert(zipOut != null);
            zip = zipOut;
        }

        public override void VisitFile(FileEntry entry)
        {
            WriteFileEntryToZip(entry);
        }

        private void WriteFileEntryToZip(FileEntry entry)
        {
            zip.PutNextEntry(CreateZipEntry(entry));
            var writer = new BinaryWriter(zip);
            writer.Write(entry.Content);
            writer.Flush();
        }

        private static ZipEntry CreateZipEntry(HierarchyEntry entry)
        {
            var zipEntry = new ZipEntry(entry.RelativePath);
            zipEntry.DateTime = entry.LastWriteTime;
            return zipEntry;
        }

        public override void VisitFolder(FolderEntry entry)
        {
            WriteFolderEntryToZip(zip, entry);
            visitChildren(entry);
        }

        private static void WriteFolderEntryToZip(ZipOutputStream zip, FolderEntry entry)
        {
            var zipEntry = new ZipEntry(entry.RelativePath + "/");
            zipEntry.DateTime = entry.LastWriteTime;
            zip.PutNextEntry(zipEntry);
        }

        public override void VisitRoot(RootEntry entry)
        {
            visitChildren(entry);
        }
    }

    class HierarchyToFolder : HierarchyVisitor
    {
        readonly string folder;

        public HierarchyToFolder(string targetFolder)
        {
            if (targetFolder.IndexOfAny(Path.GetInvalidPathChars()) != -1) throw new ArgumentException("Invalid path supplied.");
            if (!Directory.Exists(targetFolder)) throw new InvalidOperationException("Folder " + Path.GetFullPath(targetFolder) + " does not exist.");
            folder = targetFolder;
        }

        public override void VisitFile(FileEntry entry)
        {
            string combinedPath = Path.Combine(folder, entry.RelativePath);
            CheckWritePath(combinedPath);
            WriteFileEntryTo(entry, combinedPath);
        }

        private static void CheckWritePath(string combinedPath)
        {
            if (File.Exists(combinedPath)) throw new InvalidOperationException("Can't overwrite existing file.");
            if (Directory.Exists(combinedPath)) throw new InvalidOperationException("Can't overwrite existing folder.");
        }

        private static void WriteFileEntryTo(FileEntry entry, string combinedPath)
        {
            File.WriteAllBytes(combinedPath, entry.Content);
            SetDestinationDate(combinedPath, entry);
        }

        public override void VisitFolder(FolderEntry entry)
        {
            string combinedPath = Path.Combine(folder, entry.RelativePath);
            CheckAndPrepareWritePathForFolder(combinedPath);
            SetDestinationDate(combinedPath, entry);
            visitChildren(entry);
        }

        protected static void CheckAndPrepareWritePathForFolder(string thisFoldersPath)
        {
            if (Directory.Exists(thisFoldersPath)) throw new InvalidOperationException("Can't overwrite existing folder.");
            if (File.Exists(thisFoldersPath)) throw new InvalidOperationException("Can't overwrite existing file.");
            Directory.CreateDirectory(thisFoldersPath);
        }

        protected static void SetDestinationDate(string combinedPath, HierarchyEntry entry)
        {
            Directory.SetLastWriteTimeUtc(combinedPath, entry.LastWriteTime);
        }

        public override void VisitRoot(RootEntry entry)
        {
            RejectNonEmptyFolder(folder);
            SetDestinationDate(folder, entry);
            visitChildren(entry);
        }

        protected static void RejectNonEmptyFolder(string path)
        {
            if (Directory.GetFileSystemEntries(path).Length > 0) throw new InvalidOperationException("Can't write into non-empty folder.");
        }
    }

    public class PackageBuilder
    {
        public static readonly DateTime DefaultDate = new DateTime(2000, 1, 1);

        private readonly EntryContainer _packageHierarchy;

        public EntryContainer Hierarchy
        {
            get { return _packageHierarchy; }
        }

        public PackageBuilder()
        {
            _packageHierarchy = new RootEntry(DefaultDate);
        }

        internal PackageBuilder(EntryContainer folder)
        {
            _packageHierarchy = folder;
        }

        public PackageBuilder AddFolder(string name)
        { return AddFolder(name, DefaultDate); }

        public PackageBuilder AddFolder(string name, DateTime lastWrite)
        {
            var item = new FolderEntry(name, _packageHierarchy, lastWrite);
            _packageHierarchy.Add(item);
            return new PackageBuilder(item);
        }

        public PackageBuilder AddFile(string name, byte[] content)
        { return AddFile(name, content, DefaultDate); }

        public PackageBuilder AddFile(string name, string content)
        {
            byte[] contentData = Encoding.UTF8.GetBytes(content);
            return AddFile(name, contentData);
        }

        public PackageBuilder AddFile(string name, byte[] content, DateTime lastWrite)
        {
            _packageHierarchy.Add(new FileEntry(name, content, _packageHierarchy, false, lastWrite));
            return this;
        }

        public PackageBuilder AddFile(string name, string content, DateTime lastWrite)
        {
            byte[] contentData = Encoding.UTF8.GetBytes(content);
            return AddFile(name, contentData, lastWrite);
        }

        public PackageBuilder AddExecutable(string name, byte[] content)
        { return AddExecutable(name, content, DefaultDate); }

        public PackageBuilder AddExecutable(string name, byte[] content, DateTime lastWrite)
        {
            _packageHierarchy.Add(new FileEntry(name, content, _packageHierarchy, true, lastWrite));
            return this;
        }

        public void WritePackageInto(string packageDirectory)
        {
            var hierarchyExpander = new HierarchyToFolder(packageDirectory);
            _packageHierarchy.AcceptVisitor(hierarchyExpander);
        }

        public void GeneratePackageArchive(string destination)
        {
            using (var output = File.Create(destination))
            using (var zip = new ZipOutputStream(output) { IsStreamOwner = false })
            {
                zip.SetLevel(9);
                var hierarchyToZip = new HierarchyToZip(zip);
                _packageHierarchy.AcceptVisitor(hierarchyToZip);
            }
        }

        public void GeneratePackageArchive(Stream output)
        {
            using (var zip = new ZipOutputStream(output) { IsStreamOwner = false })
            {
                zip.SetLevel(9);
                var hierarchyToZip = new HierarchyToZip(zip);
                _packageHierarchy.AcceptVisitor(hierarchyToZip);
            }
        }
    }
}