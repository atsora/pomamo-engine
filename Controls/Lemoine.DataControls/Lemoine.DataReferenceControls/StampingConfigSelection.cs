// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.DataReferenceControls
{
  public partial class StampingConfigSelection : UserControl
  {
    string m_displayedProperty = "Name";
    IList<IStampingConfigByName> m_stampingConfigs = null;
    IStampingConfigByName m_stampingConfig = null;

    /// <summary>
    /// Can several rows be selected ?
    /// </summary>
    [Category ("Configuration"), Browsable (true), DefaultValue (false), Description ("Allow multi-selection")]
    public bool MultiSelect
    {
      get { return (listBox.SelectionMode != SelectionMode.One); }
      set {
        listBox.SelectionMode =
          value ? SelectionMode.MultiExtended : SelectionMode.One;
      }
    }

    /// <summary>
    /// Is a null StampingConfig a valid value ?
    /// </summary>
    [Category ("Configuration"), Browsable (true), DefaultValue (false), Description ("Is a null StampingConfig valid ?")]
    public bool Nullable
    {
      get { return nullCheckBox.Visible; }
      set {
        if (value) {
          tableLayoutPanel1.RowStyles[1].Height = 30;
          nullCheckBox.Visible = true;
        }
        else {
          tableLayoutPanel1.RowStyles[1].Height = 0;
          nullCheckBox.Visible = false;
        }
      }
    }

    /// <summary>
    /// Property that is displayed
    /// </summary>
    [Category ("Configuration"), Browsable (true), DefaultValue ("Name"), Description ("Property to display")]
    public string DisplayedProperty
    {
      get { return m_displayedProperty; }
      set { m_displayedProperty = value; }
    }

    /// <summary>
    /// Selected StampingConfigs
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    public IList<IStampingConfigByName> SelectedStampingConfigs
    {
      get {
        var list = new List<IStampingConfigByName> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            var stampingConfig = item as IStampingConfigByName;
            list.Add (stampingConfig);
          }
        }
        return list;
      }
      set {
        if (value != null && value.Count >= 1) {
          this.m_stampingConfigs = value;
          if (this.Nullable) {
            this.nullCheckBox.Checked = true;
          }
          for (int i = 0; i < listBox.Items.Count; i++) {
            listBox.SetSelected (i, false);
          }
        }
        else {
          this.nullCheckBox.Checked = false;
          for (int i = 0; i < listBox.Items.Count; i++) {
            var item = listBox.Items[i];
            var c = item as IStampingConfigByName;
            listBox.SetSelected (i, value.Select (x => x.Id).Contains (c.Id));
          }
        }
      }
    }

    /// <summary>
    /// Selected StampingConfigByName
    /// Return the first selected (if multiselection) or null 
    /// </summary>
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    public IStampingConfigByName SelectedStampingConfig
    {
      get {
        var stampingConfigs = this.SelectedStampingConfigs;
        if (stampingConfigs.Count >= 1 && !this.nullCheckBox.Checked) {
          return stampingConfigs[0] as IStampingConfigByName;
        }
        else {
          return null;
        }
      }
      set {
        this.m_stampingConfig = value;
        if (value is null) {
          if (this.Nullable) {
            this.nullCheckBox.Checked = true;
          }
          for (int i = 0; i < listBox.Items.Count; i++) {
            listBox.SetSelected (i, false);
          }
        }
        else {
          this.nullCheckBox.Checked = false;
          foreach (var item in listBox.Items) {
            if (item is IStampingConfigByName c) {
              if (c.Id == value.Id) {
                listBox.SelectedItem = item;
                break;
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Selection changed
    /// </summary>
    [Category ("Behavior"), Description ("Raised after a selection")]
    public event EventHandler AfterSelect;

    public StampingConfigSelection ()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();

      nullCheckBox.Text = PulseCatalog.GetString ("StampingConfigNull");

      this.Nullable = false;
    }

    /// <summary>
    /// Raise the AfterSelect event
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnAfterSelect (EventArgs e)
    {
      if (null != AfterSelect) {
        AfterSelect (this, e);
      }
    }

    private void StampingConfigSelection_Load (object sender, EventArgs e)
    {
      IList<IStampingConfigByName> stampingConfigs;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;

      if (daoFactory == null) {
        // to allow use in designer
        return;
      }

      using (var session = daoFactory.OpenSession ())
      using (var transaction = session.BeginReadOnlyTransaction ("StampingConfigSelection.Load")) {
        stampingConfigs = daoFactory.StampingConfigByNameDAO
          .FindAllForConfig ()
          .OrderBy (x => x.Name)
          .ToList ();
      }

      listBox.Items.Clear ();
      foreach (var stampingConfig in stampingConfigs) {
        listBox.Items.Add (stampingConfig);
      }
      listBox.ValueMember = DisplayedProperty;

      this.SetSelectedStampingConfig ();
      this.SetSelectedStampingConfigs ();
    }

    private void nullCheckBox_CheckedChanged (object sender, EventArgs e)
    {
      if (nullCheckBox.Checked) {
        listBox.Enabled = false;
      }
      else {
        listBox.Enabled = true;
      }

      OnAfterSelect (new EventArgs ());
    }

    private void listBox_SelectedIndexChanged (object sender, EventArgs e)
    {
      OnAfterSelect (new EventArgs ());
    }

    /// <summary>
    /// Set Selected StampingConfig in listbox
    /// </summary>
    void SetSelectedStampingConfig ()
    {
      if (this.m_stampingConfig != null) {
        int index = this.listBox.Items.IndexOf (this.m_stampingConfig);
        if (index >= 0) {
          this.listBox.SetSelected (index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected StampingConfigs in listbox
    /// </summary>
    void SetSelectedStampingConfigs ()
    {
      if (this.m_stampingConfigs != null) {
        int index = -1;
        foreach (var stampingConfig in this.m_stampingConfigs) {
          index = this.listBox.Items.IndexOf (stampingConfig);
          if (index >= 0) {
            this.listBox.SetSelected (index, true);
          }
        }
      }
    }
  }
}
