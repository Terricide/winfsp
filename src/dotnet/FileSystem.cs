/**
 * @file dotnet/FileSystem.cs
 *
 * @copyright 2015-2017 Bill Zissimopoulos
 */
/*
 * This file is part of WinFsp.
 *
 * You can redistribute it and/or modify it under the terms of the GNU
 * General Public License version 3 as published by the Free Software
 * Foundation.
 *
 * Licensees holding a valid commercial license may use this file in
 * accordance with the commercial license agreement provided with the
 * software.
 */

using System;
using System.Security.AccessControl;

using Fsp.Interop;

namespace Fsp
{

    public partial class FileSystem : IDisposable
    {
        /* ctor/dtor */
        public FileSystem()
        {
            _VolumeParams.Flags = VolumeParams.UmFileContextIsFullContext;
        }
        ~FileSystem()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            lock (this)
                Dispose(true);
            GC.SuppressFinalize(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (IntPtr.Zero != _FileSystem)
            {
                Api.FspFileSystemStopDispatcher(_FileSystem);
                Api.FspFileSystemSetUserContext(_FileSystem, null);
                Api.FspFileSystemDelete(_FileSystem);
                _FileSystem = IntPtr.Zero;
            }
        }

        /* properties */
        public void SetSectorSize(UInt16 SectorSize)
        {
            _VolumeParams.SectorSize = SectorSize;
        }
        public void SetSectorsPerAllocationUnit(UInt16 SectorsPerAllocationUnit)
        {
            _VolumeParams.SectorsPerAllocationUnit = SectorsPerAllocationUnit;
        }
        public void SetMaxComponentLength(UInt16 MaxComponentLength)
        {
            _VolumeParams.MaxComponentLength = MaxComponentLength;
        }
        public void SetVolumeCreationTime(UInt64 VolumeCreationTime)
        {
            _VolumeParams.VolumeCreationTime = VolumeCreationTime;
        }
        public void SetVolumeSerialNumber(UInt32 VolumeSerialNumber)
        {
            _VolumeParams.VolumeSerialNumber = VolumeSerialNumber;
        }
        public void SetFileInfoTimeout(UInt32 FileInfoTimeout)
        {
            _VolumeParams.FileInfoTimeout = FileInfoTimeout;
        }
        public void SetCaseSensitiveSearch(Boolean CaseSensitiveSearch)
        {
            _VolumeParams.Flags = CaseSensitiveSearch ? VolumeParams.CaseSensitiveSearch : 0;
        }
        public void SetCasePreservedNames(Boolean CasePreservedNames)
        {
            _VolumeParams.Flags = CasePreservedNames ? VolumeParams.CasePreservedNames : 0;
        }
        public void SetUnicodeOnDisk(Boolean UnicodeOnDisk)
        {
            _VolumeParams.Flags = UnicodeOnDisk ? VolumeParams.UnicodeOnDisk : 0;
        }
        public void SetPersistentAcls(Boolean PersistentAcls)
        {
            _VolumeParams.Flags = PersistentAcls ? VolumeParams.PersistentAcls : 0;
        }
        public void SetReparsePoints(Boolean ReparsePoints)
        {
            _VolumeParams.Flags = ReparsePoints ? VolumeParams.ReparsePoints : 0;
        }
        public void SetReparsePointsAccessCheck(Boolean ReparsePointsAccessCheck)
        {
            _VolumeParams.Flags = ReparsePointsAccessCheck ? VolumeParams.ReparsePointsAccessCheck : 0;
        }
        public void SetNamedStreams(Boolean NamedStreams)
        {
            _VolumeParams.Flags = NamedStreams ? VolumeParams.NamedStreams : 0;
        }
        public void SetPostCleanupWhenModifiedOnly(Boolean PostCleanupWhenModifiedOnly)
        {
            _VolumeParams.Flags = PostCleanupWhenModifiedOnly ? VolumeParams.PostCleanupWhenModifiedOnly : 0;
        }
        public void SetPassQueryDirectoryPattern(Boolean PassQueryDirectoryPattern)
        {
            _VolumeParams.Flags = PassQueryDirectoryPattern ? VolumeParams.PassQueryDirectoryPattern : 0;
        }
        public void SetPrefix(String Prefix)
        {
            _VolumeParams.SetPrefix(Prefix);
        }
        public void SetFileSystemName(String FileSystemName)
        {
            _VolumeParams.SetFileSystemName(FileSystemName);
        }

        /* control */
        Int32 Preflight(String MountPoint)
        {
            return Api.FspFileSystemPreflight(
                _VolumeParams.IsPrefixEmpty() ? "WinFsp.Disk" : "WinFsp.Net",
                MountPoint);
        }
        Int32 Mount(String MountPoint,
            GenericSecurityDescriptor SecurityDescriptor = null,
            Boolean Synchronized = false,
            UInt32 DebugLog = 0)
        {
            Int32 Result;
            Result = Api.FspFileSystemCreate(
                _VolumeParams.IsPrefixEmpty() ? "WinFsp.Disk" : "WinFsp.Net",
                ref _VolumeParams, ref _FileSystemInterface, out _FileSystem);
            if (0 <= Result)
            {
                Api.FspFileSystemSetUserContext(_FileSystem, this);
#if false
                FspFileSystemSetOperationGuardStrategy(_FileSystem, Synchronized ?
                    FSP_FILE_SYSTEM_OPERATION_GUARD_STRATEGY_COARSE :
                    FSP_FILE_SYSTEM_OPERATION_GUARD_STRATEGY_FINE);
                FspFileSystemSetDebugLog(_FileSystem, DebugLog);
#endif
                Result = Api.FspFileSystemSetMountPointEx(_FileSystem, MountPoint,
                    SecurityDescriptor);
                if (0 <= Result)
                    Result = Api.FspFileSystemStartDispatcher(_FileSystem, 0);
            }
            if (0 > Result && IntPtr.Zero != _FileSystem)
            {
                Api.FspFileSystemSetUserContext(_FileSystem, null);
                Api.FspFileSystemDelete(_FileSystem);
                _FileSystem = IntPtr.Zero;
            }
            return Result;
        }
        public void Unmount()
        {
            Dispose();
        }
#if false
        PWSTR MountPoint()
        {
            return 0 != _FileSystem ? FspFileSystemMountPoint(_FileSystem) : 0;
        }
#endif
        IntPtr FileSystemHandle()
        {
            return _FileSystem;
        }

        /* FSP_FILE_SYSTEM_INTERFACE */
        private static Int32 GetVolumeInfo(
            IntPtr FileSystem,
            out VolumeInfo VolumeInfo)
        {
            VolumeInfo = default(VolumeInfo);
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 SetVolumeLabel(
            IntPtr FileSystem,
            String VolumeLabel,
            out VolumeInfo VolumeInfo)
        {
            VolumeInfo = default(VolumeInfo);
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 GetSecurityByName(
            IntPtr FileSystem,
            String FileName,
            out UInt32 PFileAttributes/* or ReparsePointIndex */,
            IntPtr SecurityDescriptor,
            out UIntPtr PSecurityDescriptorSize)
        {
            PFileAttributes = default(UInt32);
            PSecurityDescriptorSize = default(UIntPtr);
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 Create(
            IntPtr FileSystem,
            String FileName,
            UInt32 CreateOptions,
            UInt32 GrantedAccess,
            UInt32 FileAttributes,
            IntPtr SecurityDescriptor,
            UInt64 AllocationSize,
            IntPtr PFileContext,
            out FileInfo FileInfo)
        {
            FileInfo = default(FileInfo);
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 Open(
            IntPtr FileSystem,
            String FileName,
            UInt32 CreateOptions,
            UInt32 GrantedAccess,
            IntPtr PFileContext,
            out FileInfo FileInfo)
        {
            FileInfo = default(FileInfo);
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 Overwrite(
            IntPtr FileSystem,
            IntPtr FileContext,
            UInt32 FileAttributes,
            Boolean ReplaceFileAttributes,
            UInt64 AllocationSize,
            out FileInfo FileInfo)
        {
            FileInfo = default(FileInfo);
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static void Cleanup(
            IntPtr FileSystem,
            IntPtr FileContext,
            String FileName,
            UInt32 Flags)
        {
        }
        private static void Close(
            IntPtr FileSystem,
            IntPtr FileContext)
        {
        }
        private static Int32 Read(
            IntPtr FileSystem,
            IntPtr FileContext,
            IntPtr Buffer,
            UInt64 Offset,
            UInt32 Length,
            out UInt32 PBytesTransferred)
        {
            PBytesTransferred = default(UInt32);
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 Write(
            IntPtr FileSystem,
            IntPtr FileContext,
            IntPtr Buffer,
            UInt64 Offset,
            UInt32 Length,
            Boolean WriteToEndOfFile,
            Boolean ConstrainedIo,
            out UInt32 PBytesTransferred,
            out FileInfo FileInfo)
        {
            PBytesTransferred = default(UInt32);
            FileInfo = default(FileInfo);
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 Flush(
            IntPtr FileSystem,
            IntPtr FileContext,
            out FileInfo FileInfo)
        {
            FileInfo = default(FileInfo);
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 GetFileInfo(
            IntPtr FileSystem,
            IntPtr FileContext,
            out FileInfo FileInfo)
        {
            FileInfo = default(FileInfo);
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 SetBasicInfo(
            IntPtr FileSystem,
            IntPtr FileContext,
            UInt32 FileAttributes,
            UInt64 CreationTime,
            UInt64 LastAccessTime,
            UInt64 LastWriteTime,
            UInt64 ChangeTime,
            out FileInfo FileInfo)
        {
            FileInfo = default(FileInfo);
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 SetFileSize(
            IntPtr FileSystem,
            IntPtr FileContext,
            UInt64 NewSize,
            Boolean SetAllocationSize,
            out FileInfo FileInfo)
        {
            FileInfo = default(FileInfo);
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 CanDelete(
            IntPtr FileSystem,
            IntPtr FileContext,
            String FileName)
        {
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 Rename(
            IntPtr FileSystem,
            IntPtr FileContext,
            String FileName,
            String NewFileName,
            Boolean ReplaceIfExists)
        {
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 GetSecurity(
            IntPtr FileSystem,
            IntPtr FileContext,
            IntPtr SecurityDescriptor,
            out UIntPtr PSecurityDescriptorSize)
        {
            PSecurityDescriptorSize = default(UIntPtr);
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 SetSecurity(
            IntPtr FileSystem,
            IntPtr FileContext,
            UInt32 SecurityInformation,
            IntPtr ModificationDescriptor)
        {
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 ReadDirectory(
            IntPtr FileSystem,
            IntPtr FileContext,
            String Pattern,
            String Marker,
            IntPtr Buffer,
            UInt32 Length,
            out UInt32 PBytesTransferred)
        {
            PBytesTransferred = default(UInt32);
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 ResolveReparsePoints(
            IntPtr FileSystem,
            String FileName,
            UInt32 ReparsePointIndex,
            Boolean ResolveLastPathComponent,
            out IoStatusBlock PIoStatus,
            IntPtr Buffer,
            out UIntPtr PSize)
        {
            PIoStatus = default(IoStatusBlock);
            PSize = default(UIntPtr);
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 GetReparsePoint(
            IntPtr FileSystem,
            IntPtr FileContext,
            String FileName,
            IntPtr Buffer,
            out UIntPtr PSize)
        {
            PSize = default(UIntPtr);
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 SetReparsePoint(
            IntPtr FileSystem,
            IntPtr FileContext,
            String FileName,
            IntPtr Buffer,
            UIntPtr Size)
        {
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 DeleteReparsePoint(
            IntPtr FileSystem,
            IntPtr FileContext,
            String FileName,
            IntPtr Buffer,
            UIntPtr Size)
        {
            return STATUS_INVALID_DEVICE_REQUEST;
        }
        private static Int32 GetStreamInfo(
            IntPtr FileSystem,
            IntPtr FileContext,
            IntPtr Buffer,
            UInt32 Length,
            out UInt32 PBytesTransferred)
        {
            PBytesTransferred = default(UInt32);
            return STATUS_INVALID_DEVICE_REQUEST;
        }

        static FileSystem()
        {
            _FileSystemInterface.GetVolumeInfo = GetVolumeInfo;
            _FileSystemInterface.SetVolumeLabel = SetVolumeLabel;
            _FileSystemInterface.GetSecurityByName = GetSecurityByName;
            _FileSystemInterface.Create = Create;
            _FileSystemInterface.Open = Open;
            _FileSystemInterface.Overwrite = Overwrite;
            _FileSystemInterface.Cleanup = Cleanup;
            _FileSystemInterface.Close = Close;
            _FileSystemInterface.Read = Read;
            _FileSystemInterface.Write = Write;
            _FileSystemInterface.Flush = Flush;
            _FileSystemInterface.GetFileInfo = GetFileInfo;
            _FileSystemInterface.SetBasicInfo = SetBasicInfo;
            _FileSystemInterface.SetFileSize = SetFileSize;
            _FileSystemInterface.CanDelete = CanDelete;
            _FileSystemInterface.Rename = Rename;
            _FileSystemInterface.GetSecurity = GetSecurity;
            _FileSystemInterface.SetSecurity = SetSecurity;
            _FileSystemInterface.ReadDirectory = ReadDirectory;
            _FileSystemInterface.ResolveReparsePoints = ResolveReparsePoints;
            _FileSystemInterface.GetReparsePoint = GetReparsePoint;
            _FileSystemInterface.SetReparsePoint = SetReparsePoint;
            _FileSystemInterface.DeleteReparsePoint = DeleteReparsePoint;
        }

        private static FileSystemInterface _FileSystemInterface;
        private VolumeParams _VolumeParams;
        private IntPtr _FileSystem;
    }

}
