// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Xml;
using System.Xml.XPath;

using Lemoine.Threading;
using Lemoine.Core.Log;

namespace Lemoine.DataRepository
{
  /// <summary>
  /// Type of the XML source
  /// </summary>
  public enum XmlSourceType
  {
    /// <summary>
    /// Raw string
    /// </summary>
    STRING,
    /// <summary>
    /// URI: Local file or URL
    /// </summary>
    URI
  }

  /// <summary>
  /// A Repository synchronizes configuration values
  ///
  /// A Repository keeps some configuration values
  /// and has some tools to synchronize the data
  /// between a main repository and a copy repository (file, database, ...).
  ///
  /// To work some modules must be attached to the repository:
  /// - a mainFactory: to get the data from the main repository
  /// - a copyBuilder: to store the data in the copy repository
  /// - a copyFactory: to get the data from the copy repository
  ///   in case the main repository is not available
  ///
  /// With a Repository, it is either possible to:
  /// - keep an up-to-date copy of main repository
  /// - to access some configuration values that are read
  ///   in the main repository or
  ///   in the copy repository if the first one is not available
  /// </summary>
  public class Repository
  {
    #region Exceptions
    /// <summary>
    /// The main factory is missing
    /// </summary>
    public class MissingMainFactory : RepositoryException
    {
      /// <summary>
      /// <see cref="Object.ToString" />
      /// </summary>
      /// <returns></returns>
      public override string ToString ()
      {
        return "The main factory is missing ; " + base.ToString ();
      }
    }

    /// <summary>
    /// The copy builder is missing
    /// </summary>
    public class MissingCopyBuilder : RepositoryException
    {
      /// <summary>
      /// <see cref="Object.ToString" />
      /// </summary>
      /// <returns></returns>
      public override string ToString ()
      {
        return "The copy builder is missing ; " + base.ToString ();
      }
    }

    /// <summary>
    /// No data is accessible
    /// </summary>
    public class NoData : RepositoryException
    {
      /// <summary>
      /// <see cref="Object.ToString" />
      /// </summary>
      /// <returns></returns>
      public override string ToString ()
      {
        return "No data is accessible ; " + base.ToString ();
      }
    }
    #endregion

    #region Types
    /// <summary>
    /// Different types of origin of the stored data
    /// </summary>
    public enum SourceOfData
    {
      /// <summary>
      /// The data comes from the main repository
      /// </summary>
      MAIN = 0,
      /// <summary>
      /// The data comes from the copy repository
      /// </summary>
      COPY
    };
    #endregion

    #region Members
    IFactory m_mainFactory = null;
    IFactory m_copyFactory = null;
    IBuilder m_copyBuilder = null;
    bool m_mainSynchronizationOkAction = false;
    XmlDocument m_document = null;
    SourceOfData m_source;
    bool m_copyUpToDate = false;
    readonly ReaderWriterLock m_rwLock = new ReaderWriterLock ();
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (Repository).FullName);

    #region Getters / Setters
    /// <summary>
    /// Main factory
    /// </summary>
    public IFactory MainFactory
    {
      get
      {
        using (ReadLockHolder holder = new ReadLockHolder (m_rwLock)) {
          return m_mainFactory;
        }
      }
      set
      {
        using (WriteLockHolder holder = new WriteLockHolder (m_rwLock)) {
          m_mainFactory = value;
          if (null != m_mainFactory) {
            this.m_mainSynchronizationOkAction = m_mainFactory.CheckSynchronizationOkAction ();
          }
        }
      }
    }

    /// <summary>
    /// Copy factory
    /// </summary>
    public IFactory CopyFactory
    {
      get
      {
        using (ReadLockHolder holder = new ReadLockHolder (m_rwLock)) {
          return m_copyFactory;
        }
      }
      set
      {
        using (WriteLockHolder holder = new WriteLockHolder (m_rwLock)) {
          m_copyFactory = value;
        }
      }
    }

    /// <summary>
    /// Copy builder
    /// </summary>
    public IBuilder CopyBuilder
    {
      get
      {
        using (ReadLockHolder holder = new ReadLockHolder (m_rwLock)) {
          return m_copyBuilder;
        }
      }
      set
      {
        using (WriteLockHolder holder = new WriteLockHolder (m_rwLock)) {
          m_copyUpToDate = false;
          m_copyBuilder = value;
        }
      }
    }

    /// <summary>
    /// Gets the origin of the stored data (main or copy)
    /// </summary>
    public Repository.SourceOfData Source
    {
      get { return m_source; }
    }

    /// <summary>
    /// Is the copy up to date ?
    /// </summary>
    public bool CopyUpToDate
    {
      get
      {
        using (ReadLockHolder holder = new ReadLockHolder (m_rwLock)) {
          return m_copyUpToDate;
        }
      }
    }

    /// <summary>
    /// XML document
    /// </summary>
    public XmlDocument Document
    {
      get { return m_document; }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Repository ()
    {
    }

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Repository (IFactory mainFactory)
    {
      this.m_mainFactory = mainFactory;
      this.m_copyUpToDate = true; // No copy builder => no copy to update

      if (null != m_mainFactory) {
        this.m_mainSynchronizationOkAction = m_mainFactory.CheckSynchronizationOkAction ();
      }
    }

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Repository (IFactory mainFactory,
                       IBuilder copyBuilder)
    {
      this.m_mainFactory = mainFactory;
      this.m_copyBuilder = copyBuilder;
      this.m_copyUpToDate = false;

      if (null != m_mainFactory) {
        this.m_mainSynchronizationOkAction = m_mainFactory.CheckSynchronizationOkAction ();
      }
    }

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Repository (IFactory mainFactory,
                       IBuilder copyBuilder,
                       IFactory copyFactory)
    {
      this.m_mainFactory = mainFactory;
      this.m_copyBuilder = copyBuilder;
      this.m_copyFactory = copyFactory;
      this.m_copyUpToDate = false;

      if (null != m_mainFactory) {
        this.m_mainSynchronizationOkAction = m_mainFactory.CheckSynchronizationOkAction ();
      }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Read in main. In case of failure, read in copy
    ///
    /// Try also to update the copy if the data is read in main.
    ///
    /// Pre-condition: A main factory was given
    /// </summary>
    public void ReadData (CancellationToken cancellationToken)
    {
      if (log.IsDebugEnabled) {
        log.Debug ("ReadData /B");
      }

      try {
        UpdateAndSynchronize (false, cancellationToken);
        if (log.IsDebugEnabled) {
          log.Debug ("ReadData: UpdateAndSynchronize successful (read in main)");
        }
      }
      catch (Exception ex) {
        log.Info ($"ReadData: Repository failed to read in main with error, try to read in copy", ex);

        using (WriteLockHolder holder = new WriteLockHolder (m_rwLock)) {
          if (null == m_copyFactory) {
            log.Warn ("ReadData: The copy factory is missing => raise the original exception", ex);
            throw;
          }

          m_document = m_copyFactory.GetData (cancellationToken);
          System.Diagnostics.Debug.Assert (null != m_document);
          m_source = SourceOfData.COPY;

          log.Info ("ReadData: Data was succesfully taken from the copy repository");
        }
      }
    }

    /// <summary>
    /// Force ReadData. Loop until it is successful in either the main or the copy factory
    /// </summary>
    /// <param name="sleepTime">sleep time between two attempts</param>
    /// <param name="cancellationToken"></param>
    /// <param name="checkedThread">checked thread (nullable)</param>
    public void ForceReadData (TimeSpan sleepTime, CancellationToken cancellationToken, IChecked checkedThread = null)
    {
      int attempt = 1;
      while (!cancellationToken.IsCancellationRequested) {
        checkedThread?.SetActive ();
        try {
          ReadData (cancellationToken);
          if (log.IsDebugEnabled) {
            log.Debug ("ForceReadData: ReadData successful");
          }
          return;
        }
        catch (RepositoryException ex) {
          log.Error ("ForceReadData: ReadData failed with a RepositoryException, try again later", ex);
        }
        catch (Exception ex) {
          log.Fatal ("ForceReadData: ReadData failed with an unexpected exception", ex);
          throw;
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"ForceReadData: try again in {sleepTime}, attempt={attempt}");
        }
        attempt++;
        checkedThread.Sleep (sleepTime, cancellationToken);
      }
      log.Error ($"ForceReadData: canceled");
    }

    /// <summary>
    /// Read in main only and update the repository
    ///
    /// Pre-condition: A main factory was given
    /// 
    /// <exception cref="MissingMainFactory">When the main factory does not exist</exception>
    /// </summary>
    public void UpdateData (CancellationToken cancellationToken)
    {
      log.Debug ("UpdateData /B");
      using (WriteLockHolder holder = new WriteLockHolder (m_rwLock)) {
        if (null == m_mainFactory) {
          log.Error ("UpdateData: " +
                     "The main factory is missing");
          throw new MissingMainFactory ();
        }

        XmlDocument oldDoc = m_document;
        m_document = m_mainFactory.GetData (cancellationToken);
        if (log.IsDebugEnabled) {
          log.Debug ("UpdateData: GetData successful");
        }
        System.Diagnostics.Debug.Assert (null != m_document);
        m_source = SourceOfData.MAIN;

        cancellationToken.ThrowIfCancellationRequested ();

        // Compare oldDoc and document
        // to tell is copyUpToDate must be changed.
        // There is no need to do it if there is no copyBuilder
        // because copyUpToDate is always true.
        if (null != m_copyBuilder) {
          if ((null == oldDoc)
              || (false == oldDoc.Equals (m_document))) { // TODO: isEqualNode
            log.Debug ("UpdateData: " +
                       "!oldDoc.Equals (newDoc) => copy is not up to date");
            m_copyUpToDate = false;
          }
          else {
            log.DebugFormat ("UpdateData: " +
                             "oldDoc.Equals (newDoc) copyUpToDate={0} remains unchanged",
                             m_copyUpToDate);
          }
        }
        else {
          log.DebugFormat ("UpdateData: " +
                           "No copy => copyUpToDate={0} ramins unchanged (true)",
                           m_copyUpToDate);
          System.Diagnostics.Debug.Assert (m_copyUpToDate);
        }

        log.Info ("UpdateData: " +
                  "Data was successfully taken from the main repository");
      }
    }

    /// <summary>
    /// Update the copy from the stored document
    /// 
    /// Pre-condition: a main factory and a copy builder were given
    /// </summary>
    public void SynchronizeCopy (CancellationToken cancellationToken)
    {
      SynchronizeCopy (false, cancellationToken);
    }

    /// <summary>
    /// Update the copy from the stored document
    /// 
    /// Pre-condition: a main factory a a copy builder were given
    /// </summary>
    /// <param name="compareFirst">Compare the copy with the main before updating the copy</param>
    /// <param name="cancellationToken"></param>
    public void SynchronizeCopy (bool compareFirst, CancellationToken cancellationToken)
    {
      if (log.IsDebugEnabled) {
        log.DebugFormat ("SynchronizeCopy compareFirst={0} /B",
                         compareFirst);
      }

      using (ReadLockHolder holder = new ReadLockHolder (m_rwLock)) {
        // Check if the copy needs to be updated
        if (m_copyUpToDate) {
          log.Debug ("SynchronizeCopy: As the copy is up to date, there is no need to synchronize");
          return;
        }

        // If document is null, there is nothing to synchronize
        if (null == m_document) {
          log.Info ("SynchronizeCopy: as the document is empty, there is nothing to synchronize");
          return;
        }

        // If the source of document is the copy,
        // there is no need to synchronize the copy
        if (m_source == SourceOfData.COPY) {
          log.Debug ("SynchronizeCopy: as the source of document is the copy, there is no need to synchronize");
          return;
        }

        if (compareFirst) {
          // Compare first both data
          // TODO: copyFactory...
        }

        if (null == m_copyBuilder) {
          log.Warn ("SynchronizeCopy: there is no copy builder => disable the synchronization");
          using (UpgradeLockHolder upgrade = new UpgradeLockHolder (m_rwLock)) {
            m_copyUpToDate = true;
          }
        }

        cancellationToken.ThrowIfCancellationRequested ();

        System.Diagnostics.Debug.Assert (null != m_document);
        using (UpgradeLockHolder upgrade = new UpgradeLockHolder (m_rwLock)) {
          try {
            if (!m_mainSynchronizationOkAction) {
              m_copyBuilder.SetAsynchronousCommit ();
            }
            m_copyBuilder.Build (m_document, cancellationToken);
            m_copyUpToDate = true;
            m_mainFactory.FlagSynchronizationAsSuccess (m_document);
          }
          catch (ArgumentException ex) {
            log.Error ($"SynchronizeCopy: synchronization failed with an argument exception on parameter {ex.ParamName}", ex);
            try {
              m_mainFactory.FlagSynchronizationAsFailure (m_document);
            }
            catch (Exception ex2) {
              log.Error ("SynchronizeCopy: FlagSynchronizationAsFailure failed", ex2);
            }
            throw;
          }
          catch (Exception ex) {
            log.Error ("SynchronizeCopy: synchronization failed", ex);
            try {
              m_mainFactory.FlagSynchronizationAsFailure (m_document);
            }
            catch (Exception ex2) {
              log.Error ("SynchronizeCopy: FlagSynchronizationAsFailure failed with error", ex2);
            }
            throw;
          }
        }
      }
    }

    /// <summary>
    /// Read in main first and then update the copy
    /// TODO: asynchronously
    /// 
    /// Pre-condition: a main factory and a copy builder were given
    /// </summary>
    public void UpdateAndSynchronize (CancellationToken cancellationToken)
    {
      UpdateAndSynchronize (false, cancellationToken);
    }

    /// <summary>
    /// Read in main first and then update the copy
    /// TODO: asynchronously
    /// 
    /// Pre-condition: a main factory and a copy builder were given
    /// </summary>
    /// <param name="compareFirst">Compare the copy with the main before</param>
    /// <param name="cancellationToken"></param>
    public void UpdateAndSynchronize (bool compareFirst, CancellationToken cancellationToken)
    {
      if (log.IsDebugEnabled) {
        log.DebugFormat ("UpdateAndSynchronize compareFirst={0} /B",
                         compareFirst);
      }
      try {
        UpdateData (cancellationToken);
        if (log.IsDebugEnabled) {
          log.Debug ("UpdateAndSynchronize: UpdateData succesful");
        }
      }
      catch (RepositoryException ex) {
        log.Error ("UpdateAndSynchronize: UpdateData failed", ex);
        log.Error ("UpdateAndSynchronize: inner exception", ex.InnerException);

        // if the copy is synchron with the main or if the document is empty, throw
        // else try to synchronize first the copy
        bool isSynchronOrEmpty;
        using (ReadLockHolder holder = new ReadLockHolder (m_rwLock)) {
          isSynchronOrEmpty = m_copyUpToDate || (null == m_document);
        }
        if (isSynchronOrEmpty) {
          log.Error ("UpdateAndSynchronize: failed because of UpdateData");
          throw;
        }
        else {
          log.Info ("UpdateAndSynchronize: " +
                    "try however to synchronize the copy (and then throw)");
          SynchronizeCopy (compareFirst, cancellationToken);
          log.Error ("UpdateAndSynchronize: failed because of UpdateData");
          throw;
        }
      }
      catch (Exception ex) {
        log.FatalFormat ("UpdateAndSynchronize: " +
                         "unexpected error {0} in UpdateData",
                         ex);
        log.FatalFormat ("UpdateAndSynchronize: the exception should be a RepositoryException in case it is a temporary error");
        throw;
      }

      cancellationToken.ThrowIfCancellationRequested ();

      // TODO: if document != null ?
      // TODO: asynchron ???
      if (log.IsDebugEnabled) {
        log.Debug ("UpdateAndSynchronize: about to run SynchronizeCopy");
      }
      SynchronizeCopy (compareFirst, cancellationToken);
      if (log.IsDebugEnabled) {
        log.Debug ("UpdateAndSynchronize: SynchronizeCopy completed");
      }
    }

    /// <summary>
    /// Get the first data that matches a given XPath
    ///
    /// Get the first data in the internal DOMElement structure
    /// that matches the given XPath
    /// 
    /// Try to synchronize the copy with the main if the copy is not up to date
    /// </summary>
    /// <param name="xpath">XPath expression</param>
    /// <param name="cancellationToken"></param>
    /// <returns>null if no data was found, else the first data found</returns>
    public string GetData (string xpath, CancellationToken cancellationToken)
    {
      XmlNamespaceManager xmlnsManager = new XmlNamespaceManager (this.Document.NameTable);
      return GetData (xpath, xmlnsManager, cancellationToken);
    }

    /// <summary>
    /// Get the first data that matches a given XPath
    ///
    /// Get the first data in the internal DOMElement structure
    /// that matches the given XPath
    /// 
    /// Try to synchronize the copy with the main if the copy is not up to date
    /// </summary>
    /// <param name="xpath">XPath expression</param>
    /// <param name="xmlnsResolver">Namespace resolver</param>
    /// <param name="cancellationToken"></param>
    /// <returns>null if no data was found, else the first data found</returns>
    public string GetData (string xpath, IXmlNamespaceResolver xmlnsResolver, CancellationToken cancellationToken)
    {
      return GetData (xpath, xmlnsResolver, true, cancellationToken);
    }

    /// <summary>
    /// Get the first data that matches a given XPath
    ///
    /// Get the first data in the internal DOMElement structure
    /// that matches the given XPath
    /// </summary>
    /// <param name="xpath">XPath expression</param>
    /// <param name="xmlnsResolver">Namespace resolver</param>
    /// <param name="synchronize">Try to synchronize if the copy is not up to date. Set to false if you have just read the data</param>
    /// <param name="cancellationToken"></param>
    /// <returns>null if no data was found, else the first data found</returns>
    public string GetData (string xpath, IXmlNamespaceResolver xmlnsResolver, bool synchronize, CancellationToken cancellationToken)
    {
      // 1. If the copy is not synchronized with the main
      //    try first to synchronize both sources of data
      if (synchronize && (false == this.CopyUpToDate)) { // Thread safe
        log.Debug ("GetData: " +
                   "Copy and Main are not synchronized, " +
                   "try to refresh the data");
        try {
          this.ReadData (cancellationToken);
        }
        catch (Exception ex) {
          log.Warn ("GetData: error occured in ReadData while reading the data (in both main and copy)", ex);
          using (ReadLockHolder holder = new ReadLockHolder (m_rwLock)) {
            if (null == m_document) {
              log.Error ("GetData: " +
                         "error while reading both sources of data " +
                         "and there is no previous document in memory");
              throw new NoData ();
            }
            else {
              log.Warn ("GetData: " +
                        "error while reading sources of data: " +
                        "previous internal document is used instead");
            }
          }
        }
      }

      // 2. Get the data
      using (ReadLockHolder holder = new ReadLockHolder (m_rwLock)) {
        System.Diagnostics.Debug.Assert (null != m_document);
        XPathNavigator pathNavigator = m_document.CreateNavigator ();
        XPathNavigator node = pathNavigator.SelectSingleNode (xpath, xmlnsResolver);
        if (null == node) {
          log.InfoFormat ("GetData: " +
                          "no node was found for xpath {0} " +
                          "=> return null",
                          xpath);
          return null;
        }
        else {
          return node.Value;
        }
      }
    }

    /// <summary>
    /// Check if the repository document is empty.
    /// 
    /// It is considered empty if the document element has
    /// no child element
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public bool IsEmpty (CancellationToken cancellationToken)
    {
      // 1. If the copy is not synchronized with the main
      //    try first to synchronize both sources of data
      if (false == this.CopyUpToDate) { // Thread safe
        log.Debug ("IsEmpty: " +
                   "Copy and Main are not synchronized, " +
                   "try to refresh the data");
        try {
          this.ReadData (cancellationToken);
        }
        catch (Exception ex) {
          log.Warn ("IsEmpty: error occured in ReadData while reading the data (in both main and copy)", ex);
          using (ReadLockHolder holder = new ReadLockHolder (m_rwLock)) {
            if (null == m_document) {
              log.Error ("IsEmpty: " +
                         "error while reading both sources of data " +
                         "and there is no previous document in memory");
              throw new NoData ();
            }
            else {
              log.Warn ("IsEmpty: " +
                        "error while reading sources of data: " +
                        "previous internal document is used instead");
            }
          }
        }
      }

      // 2. Get the data
      using (ReadLockHolder holder = new ReadLockHolder (m_rwLock)) {
        System.Diagnostics.Debug.Assert (null != m_document);
        if (null == m_document.DocumentElement) {
          log.Info ("IsEmpty: DocumentElement is null => return true");
          return true;
        }
        else {
          foreach (XmlNode node in m_document.DocumentElement.ChildNodes) {
            if (node is XmlElement) {
              log.Debug ("IsEmpty: element found, return false");
              return false;
            }
          }
          log.Info ("IsEmpty: no child element found, return true");
          return true;
        }
      }
    }
    #endregion
  }
}
