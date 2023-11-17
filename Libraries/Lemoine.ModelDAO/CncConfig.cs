// Copyright (c) 2023 Atsora Solutions

using Lemoine.Collections;
using Lemoine.Core.Log;
using Lemoine.I18N;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace Lemoine.Model
{
  /// <summary>
  /// Supported machine in a <see cref="CncConfig"/>
  /// </summary>
  public class CncConfigMachine
  {
    /// <summary>
    /// Associated text
    /// </summary>
    public string Text { get; private set; }

    /// <summary>
    /// Model of the machine (nullable)
    /// </summary>
    public string Model { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="text"></param>
    public CncConfigMachine (string text) { this.Text = text; }
  }

  /// <summary>
  /// Supported control in a <see cref="CncConfig"/>
  /// </summary>
  public class CncConfigControl
  {
    /// <summary>
    /// Associated text
    /// </summary>
    public string Text { get; private set; }

    /// <summary>
    /// Version (nullable)
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="text"></param>
    public CncConfigControl (string text) { this.Text = text; }
  }

  /// <summary>
  /// Supported protocol in a <see cref="CncConfig"/>
  /// </summary>
  public class CncConfigProtocol
  {
    /// <summary>
    /// Associated text
    /// </summary>
    public string Text { get; private set; }

    /// <summary>
    /// Version (nullable)
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="text"></param>
    public CncConfigProtocol (string text) { this.Text = text; }
  }

  /// <summary>
  /// Cnc config parameter
  /// </summary>
  public class CncConfigParam
  {
    /// <summary>
    /// Name of the parameter. Currently ParamX
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Optional label
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Type: ip / url / path / host / string / int / integer / double / list / bool / boolean / file / null
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Optional
    /// </summary>
    public bool Optional { get; set; } = false;

    /// <summary>
    /// If Type=int or integer
    /// </summary>
    public int? Min { get; set; } = null;

    /// <summary>
    /// If Type=int or integer
    /// </summary>
    public int? Max { get; set; } = null;

    /// <summary>
    /// Default value
    /// </summary>
    public string Default { get; set; }

    /// <summary>
    /// Advanced parameter
    /// </summary>
    public bool Advanced { get; set; } = false;

    /// <summary>
    /// If Type=list or file
    /// </summary>
    public IList<string> Values { get; set; } = null;

    /// <summary>
    /// If Type=double
    /// </summary>
    public int? Decimal { get; set; } = null;

    /// <summary>
    /// Regex
    /// </summary>
    public string Regex { get; set; }

    /// <summary>
    /// Is this parameter hidden?
    /// </summary>
    public bool Hidden { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    public CncConfigParam (string name)
    {
      this.Name = name;
    }
  }

  /// <summary>
  /// Cnc config
  /// </summary>
  public class CncConfig
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncConfig).FullName);

    readonly string XML_DOC_ERROR_DELAY_KEY = "CncConfig.GetXmlDocErrorDelay";
    readonly TimeSpan XML_DOC_ERROR_DELAY_DEFAULT = TimeSpan.FromMinutes (1);

    bool m_loaded = false;
    DateTime? m_getXmlDocumentError = null;
    bool m_parseError = false;
    string m_description;
    IList<CncConfigMachine> m_supportedMachines = new List<CncConfigMachine> ();
    IList<CncConfigControl> m_supportedControls = new List<CncConfigControl> ();
    IList<CncConfigProtocol> m_supportedProtocols = new List<CncConfigProtocol> ();
    string m_unit;
    IList<CncConfigParam> m_parameters = new List<CncConfigParam> ();
    bool m_deprecated;

    /// <summary>
    /// File name
    /// </summary>
    public string FileName { get; private set; }

    /// <summary>
    /// Name (file name without the .xml extension)
    /// </summary>
    public string Name
    {
      get => System.IO.Path.GetFileNameWithoutExtension (this.FileName);
    }

    /// <summary>
    /// Description
    /// </summary>
    public string Description
    {
      get {
        Load ();
        return m_description;
      }
    }

    /// <summary>
    /// Supported machines
    /// </summary>
    public IList<CncConfigMachine> SupportedMachines
    {
      get {
        Load ();
        return m_supportedMachines;
      }
    }

    /// <summary>
    /// Supported controls
    /// </summary>
    public IList<CncConfigControl> SupportedControls
    {
      get {
        Load ();
        return m_supportedControls;
      }
    }

    /// <summary>
    /// Supported protocols
    /// </summary>
    public IList<CncConfigProtocol> SupportedProtocols
    {
      get {
        Load ();
        return m_supportedProtocols;
      }
    }

    /// <summary>
    /// Unit
    /// </summary>
    public string Unit
    {
      get {
        Load ();
        return m_unit;
      }
    }


    /// <summary>
    /// Parameters
    /// </summary>
    public IList<CncConfigParam> Parameters
    {
      get {
        Load ();
        return m_parameters;
      }
    }

    /// <summary>
    /// Is the configuration deprecated?
    /// </summary>
    public bool Deprecated
    {
      get {
        Load ();
        return m_deprecated;
      }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fileName"></param>
    public CncConfig (string fileName)
    {
      this.FileName = fileName;
    }

    void Load ()
    {
      if (!m_loaded) {
        if (m_parseError) {
          log.Debug ($"Load: parse error for {this.FileName}");
          throw new Exception ($"Xml parse error for a cnc config");
        }

        if (m_getXmlDocumentError.HasValue) {
          var delay = Lemoine.Info.ConfigSet
            .LoadAndGet (XML_DOC_ERROR_DELAY_KEY, XML_DOC_ERROR_DELAY_DEFAULT);
          if (DateTime.UtcNow < m_getXmlDocumentError.Value.Add (delay)) {
            log.Debug ($"Load: {delay} not reached yet for {this.FileName}");
            throw new Exception ("Delay after error not reached yet");
          }
        }

        XmlDocument xmlDocument;
        try {
          xmlDocument = GetXmlDocument ();
        }
        catch (Exception ex) {
          log.Error ($"Load: exception while loading the Xml document", ex);
          m_getXmlDocumentError = DateTime.UtcNow;
          throw;
        }

        try {
          Parse (xmlDocument);
        }
        catch (Exception ex) {
          log.Error ($"Load: exception while parsing the xml documnet", ex);
          m_parseError = true;
          throw;
        }

        m_loaded = true;
      }
    }

    XmlDocument GetXmlDocument ()
    {
      try {
        // Crossed-reference
        var factory = new Lemoine.DataRepository.FileRepoFactory ("cncconfigs", this.FileName);
        return factory.GetData (cancellationToken: System.Threading.CancellationToken.None);
      }
      catch (Exception ex) {
        log.Error ($"GetXmlDocument: exception for fileName={this.FileName}", ex);
        throw;
      }
    }

    void Parse (XmlDocument xmlDocument)
    {
      m_supportedMachines.Clear ();
      m_supportedControls.Clear ();
      m_supportedProtocols.Clear ();
      m_parameters.Clear ();

      foreach (XmlElement descriptionElement in xmlDocument.GetElementsByTagName ("description")) {
        m_description = descriptionElement.InnerText;
      }
      foreach (XmlElement supportedMachineElement in xmlDocument.GetElementsByTagName ("supported-machine")) {
        var cncConfigMachine = new CncConfigMachine (supportedMachineElement.InnerText);
        cncConfigMachine.Model = supportedMachineElement.GetAttribute ("model");
        m_supportedMachines.Add (cncConfigMachine);
      }
      foreach (XmlElement supportedControlElement in xmlDocument.GetElementsByTagName ("supported-control")) {
        var cncConfigControl = new CncConfigControl (supportedControlElement.InnerText);
        cncConfigControl.Version = supportedControlElement.GetAttribute ("version");
        m_supportedControls.Add (cncConfigControl);
      }
      foreach (XmlElement supportedProtocolElement in xmlDocument.GetElementsByTagName ("supported-protocol")) {
        var cncConfigProtocol = new CncConfigProtocol (supportedProtocolElement.InnerText);
        cncConfigProtocol.Version = supportedProtocolElement.GetAttribute ("version");
        m_supportedProtocols.Add (cncConfigProtocol);
      }
      foreach (XmlElement unitElement in xmlDocument.GetElementsByTagName ("unit")) {
        m_unit = unitElement.InnerText.Trim ();
      }
      foreach (XmlElement parameterElement in xmlDocument.GetElementsByTagName ("parameter")) {
        var cncConfigParam = new CncConfigParam (parameterElement.GetAttribute ("name"));
        cncConfigParam.Label = parameterElement.GetAttribute ("label");
        cncConfigParam.Description = parameterElement.InnerText;
        cncConfigParam.Type = parameterElement.GetAttribute ("type");
        var min = parameterElement.GetAttribute ("min");
        if (!string.IsNullOrEmpty (min)) {
          try {
            cncConfigParam.Min = int.Parse (min);
          }
          catch (Exception ex) {
            log.Error ($"Parse: invalid min attribute for param {cncConfigParam.Name}", ex);
            throw;
          }
        }
        var max = parameterElement.GetAttribute ("max");
        if (!string.IsNullOrEmpty (max)) {
          try {
            cncConfigParam.Max = int.Parse (max);
          }
          catch (Exception ex) {
            log.Error ($"Parse: invalid max attribute for param {cncConfigParam.Name}", ex);
            throw;
          }
        }
        cncConfigParam.Default = parameterElement.GetAttribute ("default");
        cncConfigParam.Optional = parameterElement.GetBoolAttribute ("optional");
        cncConfigParam.Advanced = parameterElement.GetBoolAttribute ("advanced");
        cncConfigParam.Values = parameterElement.GetAttribute ("values").Split (new char[] { '|' });
        var decimalValue = parameterElement.GetAttribute ("decimal");
        if (!string.IsNullOrEmpty (decimalValue)) {
          try {
            cncConfigParam.Decimal = int.Parse (decimalValue);
          }
          catch (Exception ex) {
            log.Error ($"Parse: invalid decimal attribute for param {cncConfigParam.Name}", ex);
            throw;
          }
        }
        cncConfigParam.Regex = parameterElement.GetAttribute ("regex");
        cncConfigParam.Hidden = parameterElement.GetBoolAttribute ("hidden");
        if (!cncConfigParam.Type.Equals ("null", StringComparison.InvariantCultureIgnoreCase)) {
          m_parameters.Add (cncConfigParam);
        }
        else if (log.IsDebugEnabled) {
          log.Debug ($"Parse: omit param {cncConfigParam.Name} since its type is {cncConfigParam.Type}");
        }
      }
      foreach (XmlElement deprecatedElement in xmlDocument.GetElementsByTagName ("deprecated")) {
        m_deprecated = true;
      }
    }
  }

  /// <summary>
  /// Cnc config parameter value
  /// </summary>
  public class CncConfigParamValue
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncConfigParamValue));

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Value
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public CncConfigParamValue (string name, string value)
    {
      this.Name = name;
      this.Value = value;
    }

    public static string GetParametersString (IList<CncConfigParamValue> parameters)
    {
      var a = new string[10];
      int maxParamNumber = 0;
      foreach (var paramValue in parameters) {
        if (paramValue.Name.StartsWith ("Param", StringComparison.CurrentCultureIgnoreCase)) {
          if (int.TryParse (paramValue.Name.Substring ("Param".Length), out var paramNumber)) {
            if (a.Length < paramNumber) {
              log.Error ($"GetParametersString: param number {paramNumber} for name {paramValue.Name} is not supported yet");
            }
            else {
              a[paramNumber - 1] = paramValue.Value;
              if (maxParamNumber < paramNumber) {
                maxParamNumber = paramNumber;
              }
            }
          }
          else {
            log.Error ($"GetParametersString: parameter name {paramValue.Name} does not contain the param number");
          }
        }
        else {
          log.Error ($"GetParametersString: skip the parameter of name {paramValue.Name}");
        }
      }
      if (0 < maxParamNumber) {
        return a.Take (maxParamNumber).ToListString ();
      }
      else {
        log.Warn ($"GetParametersString: no parameter found => return an empty string");
        return "";
      }
    }
  }

  /// <summary>
  /// Extensions to <see cref="XmlElement"/>
  /// </summary>
  public static class XmlElementExtensions
  {
    static ILog log = LogManager.GetLogger (typeof (XmlElementExtensions).FullName);

    /// <summary>
    /// Read an attribute as a boolean
    /// </summary>
    /// <param name="xmlElement"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool GetBoolAttribute (this XmlElement xmlElement, string name)
    {
      var v = xmlElement.GetAttribute (name);
      return new string[] { "1", "true" }.Contains (v.Trim ().ToLowerInvariant ());
    }

    /// <summary>
    /// Read an attribute as an integer
    /// </summary>
    /// <param name="xmlElement"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static int GetIntAttribute (this XmlElement xmlElement, string name)
    {
      var v = xmlElement.GetAttribute (name);
      try {
        return int.Parse (v.Trim ());
      }
      catch (Exception ex) {
        log.Error ("GetIntAttribute: exception", ex);
        throw;
      }
    }
  }
}
