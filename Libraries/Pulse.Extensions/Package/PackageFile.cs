// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.IO;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Lemoine.Extensions.Package
{
  /// <summary>
  /// Utility class to read a package file and install a package
  /// </summary>
  public class PackageFile
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PackageFile).FullName);

    static readonly JsonSerializerSettings s_jsonSettings = new JsonSerializerSettings {
      DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
      NullValueHandling = NullValueHandling.Ignore,
      Converters = new List<JsonConverter>
      {
        new Lemoine.Conversion.JavaScript.TimeSpanConverter ()
      }
    };

    IPackage m_package = null;
    bool m_packageSet = false;

    /// <summary>
    /// Path of the package file
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Package description
    /// </summary>
    public PackageDescription PackageDescription { get; }

    /// <summary>
    /// Associated package if installed (active or not)
    /// 
    /// null if not installed
    /// </summary>
    public IPackage Package
    {
      get {
        if (!m_packageSet) {
          using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            m_package = ModelDAOHelper.DAOFactory.PackageDAO.FindByIdentifyingName (this.PackageDescription.Identifier);
          }
          m_packageSet = true;
        }
        return m_package;
      }
    }

    /// <summary>
    /// Is the package installed (active or inactive)
    /// </summary>
    public bool IsInstalled => (null != this.Package);

    /// <summary>
    /// Is the package installed and active?
    /// </summary>
    public bool IsActive => this.Package?.Activated ?? false;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public PackageFile (string path)
    {
      this.Path = path;
      var text = File.ReadAllText (path);
      this.PackageDescription = JsonConvert.DeserializeObject<PackageDescription> (text, s_jsonSettings);
    }
    #endregion // Constructors

    /// <summary>
    /// Install or upgrade a packageFile
    /// </summary>
    /// <param name="overwriteParameters"></param>
    /// <param name="active"></param>
    /// <returns></returns>
    public IPackage InstallOrUpgrade (bool overwriteParameters = false, bool active = true)
    {
      IPackage package;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("PackageFile.InstallOrUpgrade")) {
          package = ModelDAOHelper.DAOFactory.PackageDAO.FindByIdentifyingName (this.PackageDescription.Identifier);
          if (null == package) {
            package = ModelDAOHelper.ModelFactory.CreatePackage (this.PackageDescription.Identifier);
          }
          package.Name = this.PackageDescription.Name;
          package.Description = this.PackageDescription.Description;
          if (null != this.PackageDescription.Tags) {
            package.SetTags (this.PackageDescription.Tags);
          }
          package.NumVersion = this.PackageDescription.Version;
          package.Activated = active;
          ModelDAOHelper.DAOFactory.PackageDAO.MakePersistent (package);

          var packageAssociations = ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO
            .FindByPackage (package);

          foreach (var pluginDescription in this.PackageDescription.Plugins) {
            IPlugin plugin = ModelDAOHelper.DAOFactory.PluginDAO.FindByName (pluginDescription.Name);
            if (null == plugin) {
              plugin = ModelDAOHelper.ModelFactory.CreatePlugin (pluginDescription.Name);
              plugin.NumVersion = 0;
              ModelDAOHelper.DAOFactory.PluginDAO.MakePersistent (plugin);
            }
            var associations = packageAssociations
              .Where (a => a.Plugin.Id == plugin.Id);
            foreach (var pluginInstance in pluginDescription.Instances) {
              IPackagePluginAssociation association = associations
                .FirstOrDefault (a => IsSameInstance (a, pluginInstance));
              if (null == association) {
                association = ModelDAOHelper.ModelFactory
                  .CreatePackagePluginAssociation (package, plugin, pluginInstance.Name);
                association.Parameters = JsonConvert.SerializeObject (pluginInstance.Parameters, s_jsonSettings);
                ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO.MakePersistent (association);
              }
              else if (!association.Custom || overwriteParameters) {
                log.Warn ($"InstallJson: {package}/{plugin}/{pluginInstance.Name} has already a configuration => overwrite it, custom={association.Custom}");
                association.Parameters = JsonConvert.SerializeObject (pluginInstance.Parameters, s_jsonSettings);
                ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO.MakePersistent (association);
              }
              else { // !ovewriteParameters
                log.Info ($"InstallJson: {package}/{plugin}/{pluginInstance.Name} has already a configuration => do nothing");
              }
            }

            // Remove any deprecated configuration
            RemoveDeprecatedInstances (associations, pluginDescription.Instances);
          }

          RemoveDeprecatedPlugins (packageAssociations, this.PackageDescription.Plugins);

          transaction.Commit ();
        }

        m_package = package;
        m_packageSet = true;
      }

      NotifyConfigUpdate ();

      return package;
    }

    #region Methods
    /// <summary>
    /// Install a package from its file description
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="overwriteParameters"></param>
    /// <returns>package</returns>
    public static IPackage InstallOrUpgrade (string filePath, bool overwriteParameters)
    {
      return InstallOrUpgrade (filePath, overwriteParameters, true);
    }

    /// <summary>
    /// Install a package from its file description
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="overwriteParameters"></param>
    /// <param name="active"></param>
    /// <returns>package</returns>
    public static IPackage InstallOrUpgrade (string filePath, bool overwriteParameters, bool active)
    {
      return InstallOrUpgradeJson (filePath, overwriteParameters, active);
    }

    /// <summary>
    /// Install or upgrade a package which is described in JSON
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="overwriteParameters"></param>
    /// <param name="active">is the package active by default ?</param>
    /// <returns>package</returns>
    public static IPackage InstallOrUpgradeJson (string filePath, bool overwriteParameters, bool active)
    {
      var text = File.ReadAllText (filePath);
      return InstallOrUpgradeJsonString (text, overwriteParameters, active);
    }

    /// <summary>
    /// Install or upgrade a package which is described in JSON
    /// </summary>
    /// <param name="text"></param>
    /// <param name="overwriteParameters"></param>
    /// <param name="active">is the package active by default ?</param>
    /// <returns>package</returns>
    public static IPackage InstallOrUpgradeJsonString (string text, bool overwriteParameters, bool active)
    {
      var packageDescription = JsonConvert.DeserializeObject<PackageDescription> (text, s_jsonSettings);

      IPackage package;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("InstallOrUpgradePackage")) {
          package = ModelDAOHelper.DAOFactory.PackageDAO.FindByIdentifyingName (packageDescription.Identifier);
          if (null == package) {
            package = ModelDAOHelper.ModelFactory.CreatePackage (packageDescription.Identifier);
          }
          package.Name = packageDescription.Name;
          package.Description = packageDescription.Description;
          if (null != packageDescription.Tags) {
            package.SetTags (packageDescription.Tags);
          }
          package.NumVersion = packageDescription.Version;
          package.Activated = active;
          ModelDAOHelper.DAOFactory.PackageDAO.MakePersistent (package);

          var packageAssociations = ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO
            .FindByPackage (package);

          foreach (var pluginDescription in packageDescription.Plugins) {
            IPlugin plugin = ModelDAOHelper.DAOFactory.PluginDAO.FindByName (pluginDescription.Name);
            if (null == plugin) {
              plugin = ModelDAOHelper.ModelFactory.CreatePlugin (pluginDescription.Name);
              plugin.NumVersion = 0;
              ModelDAOHelper.DAOFactory.PluginDAO.MakePersistent (plugin);
            }
            var associations = packageAssociations
              .Where (a => a.Plugin.Id == plugin.Id);
            foreach (var pluginInstance in pluginDescription.Instances) {
              IPackagePluginAssociation association = associations
                .FirstOrDefault (a => IsSameInstance (a, pluginInstance));
              if (null == association) {
                association = ModelDAOHelper.ModelFactory
                  .CreatePackagePluginAssociation (package, plugin, pluginInstance.Name);
                association.Parameters = JsonConvert.SerializeObject (pluginInstance.Parameters, s_jsonSettings);
                ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO.MakePersistent (association);
              }
              else if (!association.Custom || overwriteParameters) {
                log.WarnFormat ("InstallJson: " +
                                "{0}/{1}/{2} has already a configuration " +
                                "=> overwrite it, custom={3}",
                                package, plugin, pluginInstance.Name, association.Custom);
                association.Parameters = JsonConvert.SerializeObject (pluginInstance.Parameters, s_jsonSettings);
                ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO.MakePersistent (association);
              }
              else { // !ovewriteParameters
                log.InfoFormat ("InstallJson: " +
                                "{0}/{1}/{2} has already a configuration " +
                                "=> do nothing",
                                package, plugin, pluginInstance.Name);
              }
            }

            // Remove any deprecated configuration
            RemoveDeprecatedInstances (associations, pluginDescription.Instances);
          }

          RemoveDeprecatedPlugins (packageAssociations, packageDescription.Plugins);

          transaction.Commit ();
        }
      }

      NotifyConfigUpdate ();

      return package;
    }

    static void NotifyConfigUpdate ()
    {
      if (!Lemoine.WebClient.Request.NotifyConfigUpdate ()) {
        log.Warn ($"NotifyConfigUpdate: NotifyConfigUpdate failed => try a direct update in database");
        try {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          using (IDAOTransaction transaction = session.BeginTransaction ("Package.PackageFile.configupdate")) {
            ModelDAOHelper.DAOFactory.ApplicationStateDAO.Update (ApplicationStateKey.ConfigUpdate.ToKey (), DateTime.UtcNow);
            transaction.Commit ();
          }
        }
        catch (Exception ex) {
          log.Error ($"NotifyConfigUpdate: configupdate in database failed", ex);
        }
      }
    }

    static bool IsSameInstance (IPackagePluginAssociation association, PluginInstance pluginInstance)
    {
      if (string.IsNullOrEmpty (association.Name) && string.IsNullOrEmpty (pluginInstance.Name)) {
        return true;
      }

      return string.Equals (association.Name, pluginInstance.Name, StringComparison.InvariantCultureIgnoreCase);
    }

    static void RemoveDeprecatedInstances (IEnumerable<IPackagePluginAssociation> associations, IEnumerable<PluginInstance> pluginInstances)
    {
      // Only the ones that are not custom
      var deprecatedAssociations = associations
        .Where (a => !a.Custom)
        .Where (a => !pluginInstances.Any (i => IsSameInstance (a, i)));
      RemoveAssociations (deprecatedAssociations);
    }

    static void RemoveDeprecatedPlugins (IEnumerable<IPackagePluginAssociation> associations, IList<PluginDescription> pluginDescriptions)
    {
      var deprecatedAssociations = associations
        .Where (a => !pluginDescriptions.Any (p => string.Equals (p.Name, a.Plugin.IdentifyingName)));
      var anyCustom = deprecatedAssociations.Any (a => a.Custom);
      if (anyCustom) { // => de-activate them, to keep the configurations
        DeactivateAssociations (deprecatedAssociations);
      }
      else {
        RemoveAssociations (deprecatedAssociations);
      }
    }

    static void RemoveAssociations (IEnumerable<IPackagePluginAssociation> associations)
    {
      foreach (var association in associations) {
        if (log.IsWarnEnabled) {
          log.WarnFormat ("RemoveAssociations: {0} of package {1} plugin {2} with parameters {3}",
            association.Name, association.Package.IdentifyingName, association.Plugin.IdentifyingName, association.Parameters);
        }
        ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO
          .MakeTransient (association);
      }
    }

    static void DeactivateAssociations (IEnumerable<IPackagePluginAssociation> associations)
    {
      foreach (var association in associations) {
        if (log.IsWarnEnabled) {
          log.WarnFormat ("DeactivateAssociations: {0} of package {1} plugin {2} with parameters {3}",
            association.Name, association.Package.IdentifyingName, association.Plugin.IdentifyingName, association.Parameters);
        }
        association.Active = false;
        ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO.MakePersistent (association);
      }
    }
    #endregion // Methods
  }
}
