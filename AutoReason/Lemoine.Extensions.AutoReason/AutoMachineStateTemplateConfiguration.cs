// Copyright (C) 2026 Atsora Solutions

using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.ModelDAO;
using System.Collections.Generic;
using System.Linq;

namespace Lemoine.Extensions.AutoReason
{
  /// <summary>
  /// Abstract class for an auto machine state template configuration
  /// </summary>
  public abstract class AutoMachineStateTemplateConfiguration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , Pulse.Extensions.Configuration.IConfigurationWithMachineFilter
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoMachineStateTemplateConfiguration).FullName);

    /// <summary>
    /// Machine state template that is applied automatically
    /// </summary>
    [PluginConf ("MachineStateTemplate", "Machine state template", Description = "the machine state template to apply automatically", Multiple = false, Optional = false)]
    public int MachineStateTemplateId { get; set; }

    /// <summary>
    /// Optionally, the machine state template to apply once the dynamic end is reached
    /// 
    /// If not set, the next machine state template of the machine state template above is considered
    /// </summary>
    [PluginConf ("MachineStateTemplate", "Next machine state template", Description = "optionally the machine state template to apply once the dynamic end is reached", Multiple = false, Optional = true)]
    public int NextMachineStateTemplateId { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    protected AutoMachineStateTemplateConfiguration ()
    {
    }

    /// <summary>
    /// By default, the machine filter parameter is not required
    /// </summary>
    /// <returns></returns>
    protected override bool IsMachineFilterRequired () => false;

    /// <summary>
    /// <see cref="Lemoine.Extensions.Configuration.IConfiguration"/>
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public override bool IsValid (out IEnumerable<string> errors)
    {
      var result = base.IsValid (out var baseErrors);

      var errorList = new List<string> ();

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Plugin.AutoMachineStateTemplate.IsValidConfiguration")) {
          if (this.MachineStateTemplateId <= 0) {
            var message = $"invalid machine state template id {this.MachineStateTemplateId}: not strictly positive";
            log.Error ($"IsValid: {message}");
            errorList.Add (message);
          }
          else if (null == ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.FindById (this.MachineStateTemplateId)) {
            var message = $"invalid machine state template id {this.MachineStateTemplateId}: unknown machine state template";
            log.Error ($"IsValid: {message}");
            errorList.Add (message);
          }

          if ((0 != this.NextMachineStateTemplateId)
            && (null == ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.FindById (this.NextMachineStateTemplateId))) {
            var message = $"invalid next machine state template id {this.NextMachineStateTemplateId}: unknown machine state template";
            log.Error ($"IsValid: {message}");
            errorList.Add (message);
          }
        }
      }
      result &= (0 == errorList.Count);

      errors = errorList.Concat (baseErrors);
      return result;
    }
  }
}
