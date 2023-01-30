// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.Settings;

namespace WizardCreateStampingConfig
{
  public partial class Page3 : UserControl, IWizardPage
  {
    public Page3 ()
    {
      InitializeComponent ();
    }

    public string Title => "Associated machines";

    public string Help => "Select the associated machines";

    public IList<Type> EditableTypes => null;

    public LemSettingsGlobal.PageFlag Flags => LemSettingsGlobal.PageFlag.NONE;

    /// <summary>
    /// Change the title of the page
    /// In most cases you can take the initial title of the page
    /// and append precisions between brackets
    /// A null value will restore the original title
    /// </summary>
    public event Action<string> SetTitle;

    /// <summary>
    /// Change the title of the page
    /// In most cases you can take the initial title of the page
    /// and append precisions between brackets
    /// A null value will restore the original title
    /// </summary>
    /// <param name="text">text to append to the title</param>
    protected void EmitSetTitle (string text)
    {
      SetTitle (text);
    }

    /// <summary>
    /// Specify a header with a text and a color:
    /// * LemSettingsGlobal.COLOR_OK
    /// * LemSettingsGlobal.COLOR_WARNING
    /// * LemSettingsGlobal.COLOR_ERROR
    /// An empty text remove the header.
    /// </summary>
    public event Action<System.Drawing.Color, string> SpecifyHeader;

    /// <summary>
    /// Specify a header with a text and a color
    /// </summary>
    /// <param name="color">LemSettingsGlobal.COLOR_OK, .COLOR_WARNING or .COLOR_ERROR</param>
    /// <param name="text">If empty, remove the header</param>
    protected void EmitSpecifyHeader (System.Drawing.Color color, string text)
    {
      SpecifyHeader (color, text);
    }

    public void DoSomethingBeforePrevious (ItemData data)
    {
    }

    public IList<string> GetErrorsToGoNext (ItemData data) => null;

    public string GetNextPageName (ItemData data) => null;

    public IList<string> GetSummary (ItemData data)
    {
      var summaries = new List<string> ();
      var monitoredMachines = data.Get<IList<IMonitoredMachine>> (Item.MONITORED_MACHINES);
      foreach (var monitoredMachine in monitoredMachines) {
        summaries.Add (monitoredMachine.Name);
      }
      return summaries;
    }

    public IList<string> GetWarnings (ItemData data) => null;

    public void Initialize (ItemContext context)
    {
    }

    public void LoadPageFromData (ItemData data)
    {
      var monitoredMachines = data.Get<IList<IMonitoredMachine>> (Item.MONITORED_MACHINES);
      if (null != monitoredMachines) {
        monitoredMachineSelection.SelectedMonitoredMachines = monitoredMachines;
      }
    }

    public void OnTimeOut ()
    {
    }

    public void SavePageInData (ItemData data)
    {
      var monitoredMachines = monitoredMachineSelection.SelectedMonitoredMachines;
      data.Store (Item.MONITORED_MACHINES, monitoredMachines);
    }

  }
}
