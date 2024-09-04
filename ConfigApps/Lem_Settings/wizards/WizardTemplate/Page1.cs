// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Settings;

namespace WizardTemplate
{
  public partial class Page1 : UserControl, IWizardPage
  {
    public Page1 ()
    {
      InitializeComponent ();
    }

    public string Title => "Title of the page";

    public string Help => "Short help";

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

    public string GetNextPageName (ItemData data) => "Page2";

    public IList<string> GetSummary (ItemData data) => null;

    public IList<string> GetWarnings (ItemData data) => null;

    public void Initialize (ItemContext context)
    {
    }

    public void LoadPageFromData (ItemData data)
    {
    }

    public void OnTimeOut ()
    {
    }

    public void SavePageInData (ItemData data)
    {
    }

  }
}
