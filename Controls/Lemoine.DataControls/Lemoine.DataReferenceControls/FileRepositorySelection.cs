// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Lemoine.I18N;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Control to select a machine mode
  /// </summary>
  public partial class FileRepositorySelection : UserControl
  {
    #region Members
    string m_nspace = "";
    string m_path = "";
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (FileRepositorySelection).FullName);

    #region Getters / Setters
    /// <summary>
    /// FileRepository namespace
    /// </summary>
    [Category("Configuration"), Browsable(true), Description("FileRepository namespace")]
    public string NSpace {
      get { return m_nspace; }
      set { m_nspace = value; }
    }
    
    /// <summary>
    /// FileRepository path
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(""), Description("FileRepository path")]
    public string Path {
      get { return m_path; }
      set { m_path = value; }
    }
    
    /// <summary>
    /// Can several rows be selected ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Allow multi-selection")]
    public bool MultiSelect {
      get { return (listBox.SelectionMode != SelectionMode.One); }
      set
      {
        listBox.SelectionMode =
          value ? SelectionMode.MultiExtended : SelectionMode.One;
      }
    }
    
    /// <summary>
    /// Is a null String a valid value ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Is a null value valid ?")]
    public bool Nullable {
      get { return nullCheckBox.Visible; }
      set
      {
        if (value) {
          tableLayoutPanel1.RowStyles [1].Height = 30;
          nullCheckBox.Visible = true;
        }
        else {
          tableLayoutPanel1.RowStyles [1].Height = 0;
          nullCheckBox.Visible = false;
        }
      }
    }

    /// <summary>
    /// Selected files
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    public IList<string> SelectedFiles {
      get
      {
        IList<string> list =
          new List<string> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            string file =
              item as string;
            list.Add (file);
          }
        }
        return list;
      }
    }
    #endregion // Getters / Setters
    
    #region Events
    /// <summary>
    /// Selection changed
    /// </summary>
    [Category("Behavior"), Description("Raised after a selection")]
    public event EventHandler AfterSelect;
    #endregion // Events

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public FileRepositorySelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("FileRepositoryNull");
      
      this.Nullable = false;
    }
    #endregion // Constructors

    #region Methods
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
    #endregion // Methods
    
    void FileRepositorySelectionLoad(object sender, EventArgs e)
    {
      ICollection<string> files;
      try {
        files = FileRepository.FileRepoClient.ListFilesInDirectory (System.IO.Path.Combine(m_nspace, m_path));
      } catch (Exception) {
        // to allow use in designer
        return;
      }
      listBox.Items.Clear ();
      foreach (string file in files) {
        listBox.Items.Add (file);
      }
    }
    
    void NullCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if (nullCheckBox.Checked) {
        listBox.Enabled = false;
      }
      else {
        listBox.Enabled = true;
      }
      
      OnAfterSelect (new EventArgs ());
    }
    
    void ListBoxSelectedIndexChanged(object sender, EventArgs e)
    {
      OnAfterSelect (new EventArgs ());
    }
  }
}
