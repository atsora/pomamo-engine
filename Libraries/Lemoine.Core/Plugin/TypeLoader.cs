// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Lemoine.FileRepository;

namespace Lemoine.Core.Plugin
{
  /// <summary>
  /// Type loader using an <see cref="IAssemblyLoader"/>
  /// </summary>
  public class TypeLoader
  {
    readonly ILog log = LogManager.GetLogger (typeof (TypeLoader).FullName);
    readonly ILog successLog = LogManager.GetLogger ($"{typeof (TypeLoader).FullName}.Success");

    readonly IAssemblyLoader m_assemblyLoader;
    readonly Func<AssemblyName, string> m_downloadAssembly;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="assemblyLoader"></param>
    /// <param name="downloadAssembly"></param>
    public TypeLoader (IAssemblyLoader assemblyLoader, Func<AssemblyName, string> downloadAssembly = null)
    {
      m_assemblyLoader = assemblyLoader;
      m_downloadAssembly = downloadAssembly;
    }

    /// <summary>
    /// Constructor using the singleton <see cref="AssemblyLoaderProvider"/> 
    /// </summary>
    public TypeLoader ()
      : this (AssemblyLoaderProvider.AssemblyLoader)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="assemblyLoader"></param>
    /// <param name="fileRepoClientFactory">not null</param>
    /// <param name="distantDirectory">not null</param>
    public TypeLoader (IAssemblyLoader assemblyLoader, IFileRepoClientFactory fileRepoClientFactory, string distantDirectory)
    {
      m_assemblyLoader = assemblyLoader;

      if (null != fileRepoClientFactory) {
        if (string.IsNullOrEmpty (distantDirectory)) {
          log.Error ($"TypeLoader: distandDirectory is null or empty");
          throw new ArgumentNullException ("distantDirectory");
        }
        m_downloadAssembly = a => DownloadAssemblyWithFileRepoClientFactory (a, fileRepoClientFactory, distantDirectory);
      }
      else {
        log.Error ($"TypeLoader: file repo client factory is null");
        m_downloadAssembly = null;
      }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="assemblyLoader"></param>
    /// <param name="fileRepoClient"></param>
    /// <param name="distantDirectory">not null if fileRepoClient is not null</param>
    public TypeLoader (IAssemblyLoader assemblyLoader, IFileRepoClient fileRepoClient, string distantDirectory)
    {
      m_assemblyLoader = assemblyLoader;

      if (null != fileRepoClient) {
        if (string.IsNullOrEmpty (distantDirectory)) {
          log.Error ($"TypeLoader: distandDirectory is null or empty");
          throw new ArgumentNullException ("distantDirectory");
        }
        m_downloadAssembly = a => DownloadAssemblyWithFileRepoClient (a, fileRepoClient, distantDirectory);
      }
      else {
        log.Warn ($"TypeLoader: file repo client is null");
        m_downloadAssembly = null;
      }
    }
    #endregion // Constructors

    string DownloadAssemblyWithFileRepoClientFactory (AssemblyName assemblyName, IFileRepoClientFactory fileRepoClientFactory, string distantDirectory)
    {
      return DownloadAssemblyWithFileRepoClient (assemblyName, fileRepoClientFactory.GetFileRepositoryClient (), distantDirectory);
    }

    string DownloadAssemblyWithFileRepoClient (AssemblyName assemblyName, IFileRepoClient fileRepoClient, string distantDirectory)
    {
      var fileName = assemblyName.Name + ".dll";
      var localCncModulesSubDirectory = distantDirectory;
      var localFileDirectory = System.IO.Path.Combine (Lemoine.Info.PulseInfo.LocalConfigurationDirectory, localCncModulesSubDirectory);
      var programName = Lemoine.Info.ProgramInfo.Name;
      if (!string.IsNullOrEmpty (programName)) {
        localFileDirectory = System.IO.Path.Combine (localFileDirectory, programName);
      }
      var localPath = System.IO.Path.Combine (localFileDirectory, fileName);
      if (log.IsDebugEnabled) {
        log.Debug ($"DownloadAssemblyWithFileRepoClient: try to download {fileName} in {distantDirectory} into {localPath}");
      }
      try {
        var synchronizeResult = fileRepoClient.SynchronizeFile (distantDirectory, fileName, localPath);
        if (!synchronizeResult && log.IsInfoEnabled) {
          log.Info ($"DownloadAssemblyWithFileRepoClient: SynchronizeFile failed and the old file is used");
        }
      }
      catch (Exception ex) {
        if (log.IsErrorEnabled) {
          log.Error ($"DownloadAssemblyWithFileRepoClient: couldn't synchronize file {fileName} into {localPath}", ex);
        }
        throw;
      }
      return localPath;
    }

#if NETSTANDARD
    /// <summary>
    /// Load an object
    /// </summary>
    /// <returns></returns>
    public T Load<T> ()
      where T : class
    {
      return Load<T> (typeof (T));
    }

    /// <summary>
    /// Load an object
    /// </summary>
    /// <returns></returns>
    public T Load<T, U> ()
      where T : class
      where U : T
    {
      return Load<T> (typeof (U));
    }
#endif // NETSTANDARD

    /// <summary>
    /// Load an object given its type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public T Load<T> (Type type, params object[] args)
      where T : class
    {
      return Load<T> (type.AssemblyQualifiedName, args);
    }

    /// <summary>
    /// Load an object given its qualified name
    /// </summary>
    /// <param name="typeQualifiedName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public T Load<T> (string typeQualifiedName, params object[] args)
      where T : class
    {
      try {
        var t = LoadFromTypeQualifiedName<T> (typeQualifiedName, args);
        if (t is null) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Load: LoadFromTypeQualifiedName returned null for {typeQualifiedName}");
          }
#if !NETSTANDARD
          return t;
#endif // !NETSTANDARD
        }
        else {
          if (successLog.IsInfoEnabled) {
            log.Info ($"Load: {typeQualifiedName} with LoadFromTypeQualifiedName");
          }
          return t;
        }
      }
      catch (Exception ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Load: LoadFromTypeQualifiedName of {typeQualifiedName} exception", ex);
        }
#if NETSTANDARD
      }

      try {
        var t = LoadFromFile<T> (typeQualifiedName, args);
        if (successLog.IsWarnEnabled) {
          if (t is null) {
            successLog.Warn ($"Load: {typeQualifiedName} could not be loaded");
          }
          else if (successLog.IsInfoEnabled) {
            successLog.Info ($"Load: {typeQualifiedName} with LoadFromFile");
          }
        }
        return t;
      }
      catch (Exception ex) {
        if (log.IsErrorEnabled) {
          log.Error ($"Load: LoadFromFile of {typeQualifiedName} exception", ex);
        }
        if (successLog.IsErrorEnabled) {
          successLog.Error ($"Load: {typeQualifiedName} failure");
        }
#endif // NETSTANDARD
        throw;
      }
    }

    /// <summary>
    /// Get a type from its qualified name
    /// </summary>
    /// <param name="typeQualifiedName"></param>
    /// <returns></returns>
    public Type GetType (string typeQualifiedName)
    {
      try {
        return GetTypeFromTypeQualifiedName (typeQualifiedName);
      }
      catch (Exception ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetType: LoadFromTypeQualifiedName of {typeQualifiedName} exception", ex);
        }
#if NETSTANDARD
      }

      try {
        return GetTypeFromFile (typeQualifiedName);
      }
      catch (Exception ex) {
        if (log.IsErrorEnabled) {
          log.Error ($"GetType: LoadFromFile of {typeQualifiedName} exception", ex);
        }
#endif // NETSTANDARD
        throw;
      }
    }

    Type GetTypeFromTypeQualifiedName (string typeQualifiedName)
    {
      return Type.GetType (typeQualifiedName, true); // throws on error
    }

#if NETSTANDARD
    Type GetTypeFromFile (string typeQualifiedName)
    {
      var (assemblyName, typeFullName) = SplitTypeQualifiedName (typeQualifiedName);

      string[] directories = new string[] { };
      try {
        // TODO: LoadFromName with no search in sub-directories for some directories
        var programDirectory = Lemoine.Info.ProgramInfo.AbsoluteDirectory;
        var parentProgramDirectory = (null != programDirectory)
          ? System.IO.Directory.GetParent (programDirectory)?.FullName
          : null;
        var installationDirectory = Lemoine.Info.PulseInfo.InstallationDir;
        if (!string.IsNullOrEmpty (installationDirectory)) { // deprecated
          directories = new string[] { ".", Lemoine.Info.ProgramInfo.AbsoluteDirectory, "..", parentProgramDirectory, installationDirectory };
        }
        else {
          directories = new string[] { ".", Lemoine.Info.ProgramInfo.AbsoluteDirectory, "..", parentProgramDirectory };
        }
        var assembly = m_assemblyLoader.LoadFromName (assemblyName, true, directories);
        return GetTypeFromAssembly (assembly, typeFullName);
      }
      catch (Exception ex) {
        if (m_downloadAssembly is null) {
          log.Error ($"GetTypeFromFile: assembly {assemblyName} not found in {string.Join (",", directories)} (and no download assembly method)", ex);
          throw;
        }
        else { // null != m_downloadAssembly
          if (log.IsDebugEnabled) {
            log.Debug ($"GetTypeFromFile: assembly {assemblyName} not found in {string.Join (",", directories)} => try to download it", ex);
          }
          try {
            var path = m_downloadAssembly (assemblyName);
            return GetTypeFromFilePath (typeFullName, path);
          }
          catch (Exception ex1) {
            log.Error ($"GetTypeFromFile: assembly load after download failed", ex1);
            log.Error ($"GetTypeFromFile: initial exception when trying to load directly {assemblyName} in {string.Join (",", directories)} was {ex.Message}", ex);
            throw;
          }
        }
      }
    }
#endif // NETSTANDARD

    Type GetTypeFromAssembly (System.Reflection.Assembly assembly, string typeFullName)
    {
      return assembly.GetExportedTypes ().First (t => t.FullName.Equals (typeFullName));
    }

    Type GetTypeFromFilePath (string typeFullName, string assemblyFilePath)
    {
      var assembly = m_assemblyLoader.LoadFromPath (assemblyFilePath);
      return GetTypeFromAssembly (assembly, typeFullName);
    }

#if NETSTANDARD
    (AssemblyName, string) SplitTypeQualifiedName (string typeQualifiedName)
    {
      var typeQualifiedNameSplit = typeQualifiedName.Split (new char[] { ',' }, 2);
      if (2 != typeQualifiedNameSplit.Length) {
        if (log.IsErrorEnabled) {
          log.Error ($"SplitTypeQualifiedName: split of {typeQualifiedName} did not return two items");
        }
        throw new ArgumentException ("Invalid type qualified name", "typeQualifiedName");
      }
      var typeFullName = typeQualifiedNameSplit[0].Trim ();
      var assemblyName = new System.Reflection.AssemblyName (typeQualifiedNameSplit[1].Trim ());
      return (assemblyName, typeFullName);
    }
#endif // NETSTANDARD

    T LoadFromTypeQualifiedName<T> (string typeQualifiedName, params object[] args)
      where T : class
    {
      var type = GetTypeFromTypeQualifiedName (typeQualifiedName);
#if NETSTANDARD
      if (!typeof (T).IsAssignableFrom (type)
        && !typeof (T).GetTypeInfo ().IsAssignableFrom (type.GetTypeInfo ())) {
        log.Warn ($"LoadFromTypeQualifiedName: {typeof (T)} is not assignable from {typeQualifiedName}");
      }
#endif // NETSTANDARD
      return (T)Activator.CreateInstance (type, args);
    }

#if NETSTANDARD
    T LoadFromFile<T> (string typeQualifiedName, params object[] args)
      where T : class
    {
      var type = GetTypeFromFile (typeQualifiedName);
      if (!typeof (T).IsAssignableFrom (type)
        && !typeof (T).GetTypeInfo ().IsAssignableFrom (type.GetTypeInfo ())) {
        log.Warn ($"LoadFromFile: {typeof (T)} is not assignable from {typeQualifiedName}");
      }
      return (T)Activator.CreateInstance (type, args);
    }
#endif // NETSTANDARD

    T LoadFromFilePath<T> (string typeFullName, string assemblyFilePath, params object[] args)
      where T : class
    {
      var assembly = m_assemblyLoader.LoadFromPath (assemblyFilePath);
      return LoadFromAssembly<T> (assembly, typeFullName, args);
    }

    T LoadFromAssembly<T> (System.Reflection.Assembly assembly, string typeFullName, params object[] args)
      where T : class
    {
      var type = GetTypeFromAssembly (assembly, typeFullName);
      return Activator.CreateInstance (type, args) as T;
    }
  }
}
