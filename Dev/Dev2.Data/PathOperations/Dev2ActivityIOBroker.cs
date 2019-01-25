/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.PathOperations.Extension;
using Dev2.Data.Util;
using Ionic.Zip;
using Warewolf.Resource.Errors;

namespace Dev2.PathOperations
{
    class Dev2ActivityIOBroker : IActivityOperationsBroker
    {
        readonly IFile _fileWrapper;
        readonly ICommon _common;
        static readonly ReaderWriterLockSlim FileLock = new ReaderWriterLockSlim();
        readonly List<string> _filesToDelete;
        readonly IActivityIOBrokerDriver _implementation;
        readonly IActivityIOBrokerValidatorDriver _validator;

        public Dev2ActivityIOBroker()
            : this(new FileWrapper(), new Data.Util.CommonDataUtils(), new ActivityIOBrokerDriver(), new ActivityIOBrokerValidatorDriver())
        {
            _filesToDelete = new List<string>();
        }

        public Dev2ActivityIOBroker(IFile fileWrapper, ICommon common)
            :this(fileWrapper, common, new ActivityIOBrokerDriver(), new ActivityIOBrokerValidatorDriver())
        {
        }

        public Dev2ActivityIOBroker(IFile fileWrapper, ICommon common, IActivityIOBrokerDriver implementation, IActivityIOBrokerValidatorDriver validator)
        {
            _implementation = implementation;
            _validator = validator;

            _fileWrapper = fileWrapper;
            _common = common;
            _filesToDelete = new List<string>();
        }

        public string Get(IActivityIOOperationsEndPoint path) => Get(path, false);
        public string Get(IActivityIOOperationsEndPoint path, bool deferredRead)
        {
            try
            {

                byte[] bytes;
                using (var s = path.Get(path.IOPath, _filesToDelete))
                {
                    bytes = new byte[s.Length];
                    s.Position = 0;
                    s.Read(bytes, 0, (int)s.Length);
                }

                return Encoding.UTF8.GetString(bytes);

            }
            finally
            {
                _filesToDelete.ForEach(_implementation.RemoveTmpFile);
            }
        }

        public string PutRaw(IActivityIOOperationsEndPoint dst, IDev2PutRawOperationTO args)
        {
            var result = ActivityIOBrokerDriverBase.ResultOk;
            try
            {
                FileLock.EnterWriteLock();
                if (dst.RequiresLocalTmpStorage())
                {
                    var tmp = _implementation.CreateTmpFile();
                    _implementation.WriteToLocalTempStorage(dst, args, tmp);
                    result = _implementation.MoveTmpFileToDestination(dst, tmp, result);
                }
                else
                {
                    if (dst.PathExist(dst.IOPath))
                    {
                        var tmp = _implementation.CreateTmpFile();
                        result = _implementation.WriteToRemoteTempStorage(dst, args, result, tmp);
                        _implementation.RemoveTmpFile(tmp);
                    }
                    else
                    {
                        var newArgs = new Dev2CRUDOperationTO(true);
                        _implementation.CreateEndPoint(dst, newArgs, true);
                        _implementation.WriteDataToFile(args, dst);
                    }
                }
            }
            finally
            {
                FileLock.ExitWriteLock();
                for (var index = _filesToDelete.Count - 1; index > 0; index--)
                {
                    var name = _filesToDelete[index];
                    _implementation.RemoveTmpFile(name);
                }
            }
            return result;
        }

        public string Delete(IActivityIOOperationsEndPoint src)
        {
            try
            {
                if (!src.Delete(src.IOPath))
                {
                    return ActivityIOBrokerDriverBase.ResultBad;
                }
            }
            catch
            {
                return ActivityIOBrokerDriverBase.ResultBad;
            }
            finally
            {
                _filesToDelete.ForEach(_implementation.RemoveTmpFile);
            }
            return ActivityIOBrokerDriverBase.ResultOk;
        }

        public IList<IActivityIOPath> ListDirectory(IActivityIOOperationsEndPoint src, ReadTypes readTypes)
        {
            return _implementation.ListDirectory(src, readTypes);
        }

        public string Create(IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args, bool createToFile)
        {
            try
            {
                _common.ValidateEndPoint(dst, args);
                return _implementation.CreateEndPoint(dst, args, createToFile);
            }
            finally
            {
                _filesToDelete.ForEach(_implementation.RemoveTmpFile);
            }
        }

        public string Rename(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args)
        {
            string performRename()
            {
                if (src.PathIs(src.IOPath) != dst.PathIs(dst.IOPath))
                {
                    throw new Exception(ErrorResource.SourceAndDestinationNOTFilesOrDirectory);
                }
                if (dst.PathExist(dst.IOPath))
                {
                    if (!args.Overwrite)
                    {
                        throw new Exception(ErrorResource.DestinationDirectoryExist);
                    }
                    dst.Delete(dst.IOPath);
                }

                return Move(src, dst, args);
            }

            try
            {
                return performRename();
            }
            finally
            {
                _filesToDelete.ForEach(_implementation.RemoveTmpFile);
            }
        }

        public string Copy(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args)
        {
            string status;
            try
            {
                status = _validator.ValidateCopySourceDestinationFileOperation(src, dst, args, () =>
                {
                    var result = -1;
                    if (src.RequiresLocalTmpStorage())
                    {
                        if (dst.PathIs(dst.IOPath) == enPathType.Directory)
                        {
                            dst.IOPath.Path = dst.Combine(_implementation.GetFileNameFromEndPoint(src));
                        }

                        using (var s = src.Get(src.IOPath, _filesToDelete))
                        {
                            result = dst.Put(s, dst.IOPath, args, Path.IsPathRooted(src.IOPath.Path) ? Path.GetDirectoryName(src.IOPath.Path) : null, _filesToDelete);
                            s.Close();
                        }
                    }
                    else
                    {
                        var sourceFile = new FileInfo(src.IOPath.Path);
                        if (dst.PathIs(dst.IOPath) == enPathType.Directory)
                        {
                            dst.IOPath.Path = dst.Combine(sourceFile.Name);
                        }

                        using (var s = src.Get(src.IOPath, _filesToDelete))
                        {
                            if (sourceFile.Directory != null)
                            {
                                result = dst.Put(s, dst.IOPath, args, sourceFile.Directory.ToString(), _filesToDelete);
                            }
                        }
                    }
                    return result == -1 ? ActivityIOBrokerDriverBase.ResultBad : ActivityIOBrokerDriverBase.ResultOk;
                });
            }
            finally
            {
                _filesToDelete.ForEach(_implementation.RemoveTmpFile);
            }
            return status;
        }

        public string Move(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args)
        {
            string result;
            try
            {
                result = Copy(src, dst, args);
                if (result.Equals(ActivityIOBrokerDriverBase.ResultOk))
                {
                    src.Delete(src.IOPath);
                }
            }
            finally
            {
                _filesToDelete.ForEach(_implementation.RemoveTmpFile);
            }

            return result;
        }

        public string UnZip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2UnZipOperationTO args)
        {
            string status;

            try
            {
                status = _validator.ValidateUnzipSourceDestinationFileOperation(src, dst, args, () =>
                {
                    ZipFile zip;
                    var tempFile = string.Empty;

                    if (src.RequiresLocalTmpStorage())
                    {
                        var tmpZip = _implementation.CreateTmpFile();
                        using (var s = src.Get(src.IOPath, _filesToDelete))
                        {
                            _fileWrapper.WriteAllBytes(tmpZip, s.ToByteArray());
                        }

                        tempFile = tmpZip;
                        zip = ZipFile.Read(tempFile);
                    }
                    else
                    {
                        zip = ZipFile.Read(src.Get(src.IOPath, _filesToDelete));
                    }

                    if (dst.RequiresLocalTmpStorage())
                    {
                        var tempPath = _common.CreateTmpDirectory();
                        _common.ExtractFile(args, new IonicZipFileWrapper(zip), tempPath);
                        var endPointPath = ActivityIOFactory.CreatePathFromString(tempPath, string.Empty, string.Empty);
                        var endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(endPointPath);
                        Move(endPoint, dst, new Dev2CRUDOperationTO(args.Overwrite));
                    }
                    else
                    {
                        _common.ExtractFile(args, new IonicZipFileWrapper(zip), dst.IOPath.Path);
                    }

                    if (src.RequiresLocalTmpStorage())
                    {
                        _fileWrapper.Delete(tempFile);
                    }

                    return ActivityIOBrokerDriverBase.ResultOk;
                });
            }
            finally
            {
                _filesToDelete.ForEach(_implementation.RemoveTmpFile);
            }

            return status;
        }

        public string Zip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2ZipOperationTO args)
        {
            string status;

            try
            {
                status = _validator.ValidateZipSourceDestinationFileOperation(src, dst, args, () =>
                {
                    string tempFileName;

                    tempFileName = src.PathIs(src.IOPath) == enPathType.Directory || Dev2ActivityIOPathUtils.IsStarWildCard(src.IOPath.Path) ? _implementation.ZipDirectoryToALocalTempFile(src, args) : _implementation.ZipFileToALocalTempFile(src, args);

                    return _implementation.TransferTempZipFileToDestination(src, dst, args, tempFileName);
                });
            }
            finally
            {
                _filesToDelete.ForEach(_implementation.RemoveTmpFile);
            }
            return status;
        }
    }
}