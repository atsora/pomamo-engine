// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Runtime.Remoting.Channels;

using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Services;
using Lemoine.Core.Log;
using omg.org.CosNaming;

namespace Lemoine.CorbaHelper
{
  /// <summary>
  /// Description of CorbaConnection.
  /// </summary>
  public sealed class CorbaClientConnection
  {
    /// <summary>
    /// Default timeout: 5s = 5000ms
    /// </summary>
    static readonly int DEFAULT_TIMEOUT = 5000;
    
    #region Members
    IiopClientChannel m_channel = null;
    NamingContext m_nameService;
    int m_timeout = DEFAULT_TIMEOUT;
    bool m_initialized = false;
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (CorbaClientConnection).FullName);

    #region Getters / Setters
    /// <summary>
    /// Name service
    /// </summary>
    public static NamingContext NameService {
      get { CheckInitialization (); return Instance.m_nameService; }
    }
    
    /// <summary>
    /// IIOP Channel
    /// </summary>
    public static IiopClientChannel Channel {
      get { CheckInitialization (); return Instance.m_channel; }
    }
    
    /// <summary>
    /// Timeout in ms
    /// 
    /// It must be set before any other method call
    /// </summary>
    public static int Timeout {
      get { return Instance.m_timeout; }
      set { Instance.m_timeout = value; }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private CorbaClientConnection()
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Check the singleton class has been initialized. If not, initialize it.
    /// </summary>
    static void CheckInitialization ()
    {
      if (false == Instance.m_initialized) {
        // 1. Get the host and port of the name service
        string nameServiceIOR =
          System.Environment.GetEnvironmentVariable ("NAMESERVICEIOR");
        string [] nameServiceIORElements = (nameServiceIOR != null ? 
          nameServiceIOR.Split ('/') [0].Split (':') : new string[] { });
        if (nameServiceIORElements.Length != 4) {
          log.ErrorFormat ("CheckInitialization: " +
                           "name service IOR {0} is invalid",
                           nameServiceIOR);
          throw new Exception ("Invalid name service IOR");
        }
        System.Diagnostics.Debug.Assert (nameServiceIORElements.Length >= 4);
        System.Diagnostics.Debug.Assert (nameServiceIORElements [0] == "corbaloc");
        System.Diagnostics.Debug.Assert (nameServiceIORElements [1] == "iiop");
        string nameServiceHostName = nameServiceIORElements [2];
        int nameServicePort;
        try {
          nameServicePort = Int32.Parse (nameServiceIORElements [3]);
        }
        catch (Exception ex) {
          log.ErrorFormat ("CheckInitialization: " +
                           "could not get the port in name service IOR {0} " +
                           "exception {1}",
                           nameServiceIOR, ex);
          throw;
        }
        log.DebugFormat ("CheckInitialization: " +
                         "name service host={0} port={1}",
                         nameServiceHostName, nameServicePort);
        
        // 2. Do the CORBA stuff
        
        // 2.a) Create and register the channel
        RegisterChannel ();
        
        // 2.b) Access COS naming service
        try {
          CorbaInit init = CorbaInit.GetInit ();
          Instance.m_nameService = init.GetNameService (nameServiceHostName,
                                                        nameServicePort);
        }
        catch (Exception ex) {
          log.ErrorFormat ("CheckInitialization: " +
                           "CORBA exception {0}",
                           ex);
          throw;
        }
        
        Instance.m_initialized = true;
      }
      
      if (null == Instance.m_channel) {
        RegisterChannel ();
      }
    }
    

    /// <summary>
    /// Create and register the IIOP Client channel with the set Timeout
    /// </summary>
    public static void RegisterChannel ()
    {
      try {
        if (null == Instance.m_channel) {
          IDictionary properties = new Hashtable ();
          properties[IiopClientChannel.CLIENT_RECEIVE_TIMEOUT_KEY] = Instance.m_timeout;
          properties[IiopClientChannel.CLIENT_SEND_TIMEOUT_KEY] = Instance.m_timeout;
          properties[IiopClientChannel.CLIENT_REQUEST_TIMEOUT_KEY] = Instance.m_timeout;
          Instance.m_channel = new IiopClientChannel (properties);
        }
        ChannelServices.RegisterChannel (Instance.m_channel, false);
      }
      catch (System.Runtime.Remoting.RemotingException alreadyRegisteredException) {
        log.InfoFormat ("RegisterChannel: " +
                        "the channel is already registered " +
                        "=> consider this is ok " +
                        "{0}",
                        alreadyRegisteredException);
      }
      catch (Exception ex) {
        log.ErrorFormat ("RegisterChannel: " +
                         "RegisterChannel failed with {0}",
                         ex);
        Instance.m_channel = null;
        throw;
      }
    }
    
    /// <summary>
    /// Reset the IIOP Client channel
    /// </summary>
    public static void ResetChannel ()
    {
      if (null != Instance.m_channel) {
        try {
          ChannelServices.UnregisterChannel (Instance.m_channel);
        }
        catch (Exception ex) {
          log.InfoFormat ("ResetChannel: " +
                          "UnregisterChannel failed with exception ex",
                          ex);
        }
        finally {
          Instance.m_channel = null;
        }
      }
      
      log.Debug ("ResetChannel: " +
                 "Register channel again");
      try {
        RegisterChannel ();
      }
      catch (Exception ex) {
        log.ErrorFormat ("ResetChannel: " +
                         "RegisterChannel failed with {0} " +
                         "=> fully disconnect",
                         ex);
        Instance.m_initialized = false;
      }
    }
    
    /// <summary>
    /// Get a reference to a CORBA object
    /// </summary>
    /// <param name="context">Naming context</param>
    /// <param name="objectReference">Object reference</param>
    /// <returns></returns>
    public static MarshalByRefObject GetObject (NameComponent context, NameComponent objectReference)
    {
      CheckInitialization ();
      
      // - Context
      NameComponent [] moduleName =
        new NameComponent [] { context };
      NamingContext nameSpace;
      try {
        nameSpace = (NamingContext)NameService.resolve (moduleName);
      }
      catch (Exception ex) {
        log.ErrorFormat ("GetObject: " +
                         "resolving the context failed, " +
                         "{0}",
                         ex);
        throw;
      }
      
      // - Interface
      NameComponent [] interfaceName =
        new NameComponent [] { objectReference };
      try {
        return nameSpace.resolve (interfaceName);
      }
      catch (Exception ex) {
        log.ErrorFormat ("GetObject: " +
                         "resolving the interface failed, " +
                         "{0}",
                         ex);
        throw;
      }
    }
    
    /// <summary>
    /// Get a reference to a CORBA object
    /// </summary>
    /// <param name="context">Naming context</param>
    /// <param name="objectReference">Object reference</param>
    /// <returns></returns>
    public static MarshalByRefObject GetObject (string context, string objectReference)
    {
      return GetObject (new NameComponent (context, context),
                        new NameComponent (objectReference, objectReference));
    }
    #endregion
    
    #region Instance
    static CorbaClientConnection Instance
    {
      get { return Nested.instance; }
    }
    
    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested()
      {
      }

      internal static readonly CorbaClientConnection instance = new CorbaClientConnection ();
    }
    #endregion
  }
}
