// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lemoine.ModelDAO;
using Lemoine.Settings;

namespace WizardCreateStampingConfig
{
  public partial class Page1 : UserControl, IWizardPage
  {
    public Page1 ()
    {
      InitializeComponent ();
    }

    public string Title => "Stamping config creation wizard";

    public string Help => "Properties to create a new stamping configuration (for a specific post-processor for example)";

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

    public IList<string> GetErrorsToGoNext (ItemData data)
    {
      IList<string> errors = new List<string> ();

      var name = data.Get<string> (Item.CONFIG_NAME);
      if (string.IsNullOrEmpty (name)) {
        errors.Add ("Missing name");
      }

      var configId = data.Get<int?> (Item.CONFIG_ID);
      if (!configId.HasValue) {
        var template = data.Get<string> (Item.TEMPLATE_PATH);
        if (string.IsNullOrEmpty (template)) {
          errors.Add ("Missing template");
        }
      }

      return errors;
    }

    public string GetNextPageName (ItemData data) => "Page2";

    public IList<string> GetSummary (ItemData data)
    {
      var summary = new List<string> {
          $"Name={data.Get<string> (Item.CONFIG_NAME)}"
        };
      var templatePath = data.Get<string> (Item.TEMPLATE_PATH);
      if (!string.IsNullOrEmpty (templatePath)) {
        summary.Add ($"Template={templatePath}");
      }
      var configId = data.Get<int?> (Item.CONFIG_ID);
      if (configId.HasValue) {
        summary.Add ($"ConfigId={configId.Value}");
      }
      return summary;
    }

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
      data.Store (Item.CONFIG_NAME, nameTextBox.Text);
      data.Store (Item.TEMPLATE_PATH, templatePathTextBox.Text);
      if (null != stampingConfigSelection.SelectedStampingConfig) {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var stampingConfig = stampingConfigSelection.SelectedStampingConfig;
          data.Store (Item.CONFIG_ID, (int?)stampingConfig.Id);
          if (string.IsNullOrEmpty (templatePathTextBox.Text)) {
            var json = System.Text.Json.JsonSerializer.Serialize (stampingConfig.Config);
            data.Store (Item.CONFIG_JSON, json);
          }
        }
      }
      else {
        data.Store (Item.CONFIG_ID, (int?)null);
        data.Store (Item.CONFIG_JSON, "");
      }
    }

    private void templateOpenButton_Click (object sender, EventArgs e)
    {
      var result = templateOpenFileDialog.ShowDialog ();
      if (result.Equals (DialogResult.OK)) {
        templatePathTextBox.Text = Path.Combine (templateOpenFileDialog.InitialDirectory, templateOpenFileDialog.FileName);
      }
    }

    private void nameLabel_Click (object sender, EventArgs e)
    {

    }

    private void nameTextBox_TextChanged (object sender, EventArgs e)
    {

    }

    private void templatePathLabel_Click (object sender, EventArgs e)
    {

    }

    private void stampingConfigSelection_AfterSelect (object sender, EventArgs e)
    {
      if (stampingConfigSelection.SelectedStampingConfig is null) {
        nameTextBox.Text = "";
      }
      else {
        nameTextBox.Text = stampingConfigSelection.SelectedStampingConfig.Name;
      }
    }
  }
}
