// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;
using Lemoine.Settings;

namespace WizardInitialConfiguration
{
  /// <summary>
  /// Description of Page1.
  /// </summary>
  internal partial class Page1 : GenericWizardPage, IWizardPage
  {
    #region Members
    Image m_infoImage = null;
    Image m_warnImage = null;
    IList<ToolTip> m_toolTips = new List<ToolTip>();
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Global options"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Here are options that are set according to the configuration of the client company. " +
          "Each box checked will increase the degrees of freedom but will also higher the complexity to set up the system.\n\n" +
          "Checking checkboxes is always safe, but please take care when unchecking them. This may result in a corrupted database.\n\n" +
          "For your convenience, predefined sets of options can be loaded."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page1()
    {
      var rm = new ResourceManager("WizardInitialConfiguration.Item", GetType().Assembly);
      m_infoImage = (Image)rm.GetObject("help");
      m_warnImage = (Image)rm.GetObject("warning");
      
      InitializeComponent();
      
      // Fill table
      PrepareImages();
      
      // Part count
      comboPartCount.AddItem("none", 0);
      comboPartCount.AddItem("per user shift", 3);
      comboPartCount.AddItem("per machine shift", 4);
      
      // Predefined configurations
      comboConf.AddItem("Mold shops", 0);
      comboConf.AddItem("Production", 1);
      comboConf.SelectedIndex = 0;
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize(ItemContext context) {}
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      // Operations
      if (data.Get<bool>(Item.STRUCTURE_UNIQUE_COMP_FROM_OP) == data.Get<bool>(Item.STRUCTURE_COMP_FROM_OP)) {
        pictureOp1.Image = m_infoImage;
        checkOp1.Enabled = true;
        checkOp1.Checked = !data.Get<bool>(Item.STRUCTURE_UNIQUE_COMP_FROM_OP);
        m_toolTips[0].SetToolTip(pictureOp1, "This option allows the use of an operation for the production " +
                                 "or different kind of part / component.");
      } else {
        pictureOp1.Image = m_warnImage;
        checkOp1.Enabled = false;
        checkOp1.Checked = false;
        m_toolTips[0].SetToolTip(pictureOp1, "This option allows the use of an operation for the production " +
                                 "or different kind of part / component. Please ask the development team to change it.");
      }
      checkOp2.Checked = !data.Get<bool>(Item.STRUCTURE_IWP_IS_OP);
      checkOp3.Checked = !data.Get<bool>(Item.STRUCTURE_SINGLE_PATH);
      
      // Work orders
      checkWo3.Enabled = checkWo2.Enabled = checkWo1.Checked = !data.Get<bool>(Item.STRUCTURE_WO_IS_JOB);
      checkWo2.Checked = !data.Get<bool>(Item.STRUCTURE_UNIQUE_PART_FROM_WO);
      if (data.Get<bool>(Item.STRUCTURE_UNIQUE_WO) == data.Get<bool>(Item.STRUCTURE_WO_FROM_COMP)) {
        pictureWo3.Image = m_infoImage;
        checkWo3.Checked = !data.Get<bool>(Item.STRUCTURE_UNIQUE_WO);
        m_toolTips[0].SetToolTip(pictureWo3, "This option allows a part / component to be produced by several work orders.");
      } else {
        pictureWo3.Image = m_warnImage;
        checkWo3.Enabled = false;
        checkWo3.Checked = false;
        m_toolTips[0].SetToolTip(pictureWo3, "This option allows a part / component to be produced by several work orders. " +
                                 "Please ask the development team to change it");
      }
      
      // Miscellaneous
      checkMisc1.Checked = !data.Get<bool>(Item.STRUCTURE_P_C_IS_PART);
      checkMisc2.Checked = !data.Get<bool>(Item.STRUCTURE_UNIQUE_COMP_FROM_LINE);
      checkMisc3.Checked = data.Get<bool> (Item.OPERATION_EXPLORER_PART_AT_THE_TOP);
      comboPartCount.SelectedValue = data.Get<int>(Item.OPERATION_SLOT_SPLIT_OPTION);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      // Operations
      if (data.Get<bool>(Item.STRUCTURE_UNIQUE_COMP_FROM_OP) == data.Get<bool>(Item.STRUCTURE_COMP_FROM_OP)) {
        data.Store(Item.STRUCTURE_UNIQUE_COMP_FROM_OP, !checkOp1.Checked);
        data.Store(Item.STRUCTURE_COMP_FROM_OP, !checkOp1.Checked);
      }
      data.Store(Item.STRUCTURE_IWP_IS_OP, !checkOp2.Checked);
      data.Store(Item.STRUCTURE_SINGLE_PATH, !checkOp3.Checked);
      
      // Work orders
      data.Store(Item.STRUCTURE_WO_IS_JOB, !checkWo1.Checked);
      data.Store(Item.STRUCTURE_UNIQUE_PART_FROM_WO, !checkWo2.Checked);
      if (data.Get<bool>(Item.STRUCTURE_UNIQUE_WO) == data.Get<bool>(Item.STRUCTURE_WO_FROM_COMP)) {
        data.Store(Item.STRUCTURE_UNIQUE_WO, !checkWo3.Checked);
        data.Store(Item.STRUCTURE_WO_FROM_COMP, !checkWo3.Checked);
      }
      
      // Miscellaneous
      data.Store(Item.STRUCTURE_P_C_IS_PART, !checkMisc1.Checked);
      data.Store(Item.STRUCTURE_UNIQUE_COMP_FROM_LINE, !checkMisc2.Checked);
      data.Store (Item.OPERATION_EXPLORER_PART_AT_THE_TOP, checkMisc3.Checked);
      data.Store(Item.OPERATION_SLOT_SPLIT_OPTION, comboPartCount.SelectedValue);
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data"></param>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      if (comboPartCount.SelectedValue == null) {
        errors.Add("a part count must be selected");
      }

      return errors;
    }
    
    /// <summary>
    /// If no errors are found to go to the next page, non-blocking warnings are checked.
    /// Data may be slightly modified here before continuing to the next page.
    /// Full sentences are desired here.
    /// </summary>
    /// <returns>List of warnings, can be null</returns>
    public override IList<string> GetWarnings(ItemData data)
    {
      IList<string> warnings = new List<string>();
      
      if (data.Get<bool>(Item.STRUCTURE_WO_IS_JOB) &&
          data.Get<bool>(Item.STRUCTURE_P_C_IS_PART)) {
        data.Store(Item.STRUCTURE_P_C_IS_PART, false);
        warnings.Add("A production unrelated to a workorder must take place within a project.");
      }
      
      return warnings;
    }
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName(ItemData data)
    {
      return "Page2";
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      // Operations
      string opStr = "Operations";
      if (data.Get<bool>(Item.STRUCTURE_UNIQUE_COMP_FROM_OP) == data.Get<bool>(Item.STRUCTURE_COMP_FROM_OP)) {
        if (data.Get<bool>(Item.STRUCTURE_UNIQUE_COMP_FROM_OP)) {
          opStr += "\nare dedicated to a part / component";
        }
        else {
          opStr += "\ncan be shared by different parts / components";
        }
      }
      if (data.Get<bool>(Item.STRUCTURE_IWP_IS_OP)) {
        opStr += "\ncan produce only one intermediate work piece or part / component";
      }
      else {
        opStr += "\ncan produce different types of intermediate work pieces";
      }

      if (data.Get<bool>(Item.STRUCTURE_SINGLE_PATH)) {
        opStr += "\ncannot be made of sequences running in parallel";
      }
      else {
        opStr += "\nmay be made of sequences running in parallel";
      }

      summary.Add(opStr);
      
      // Work orders
      string woStr = "Work orders";
      if (!data.Get<bool>(Item.STRUCTURE_WO_IS_JOB)) {
        if (data.Get<bool>(Item.STRUCTURE_UNIQUE_PART_FROM_WO)) {
          woStr += "\nare made of only one type of part / component";
        }
        else {
          woStr += "\nmay comprise different types of part / component";
        }

        if (data.Get<bool>(Item.STRUCTURE_UNIQUE_WO) == data.Get<bool>(Item.STRUCTURE_WO_FROM_COMP)) {
          if (data.Get<bool>(Item.STRUCTURE_UNIQUE_WO)) {
            opStr += "\ncannot comprise a part / component already present in another work order";
          }
          else {
            opStr += "\nmay comprise a part / component already present in another work order";
          }
        }
      }
      else {
        woStr += " are not allowed";
      }

      summary.Add(woStr);
      
      // Miscellaneous
      string miscStr = "Miscellaneous";
      if (data.Get<bool>(Item.STRUCTURE_P_C_IS_PART)) {
        miscStr += "\nprojects disabled";
      }
      else {
        miscStr += "\ndifferent parts / components may be grouped by project";
      }

      if (data.Get<bool>(Item.STRUCTURE_UNIQUE_COMP_FROM_LINE)) {
        miscStr += "\nproduction lines produce only one type of part / component";
      }
      else {
        miscStr += "\nproduction lines can produce different types of part / component";
      }

      if (data.Get<bool> (Item.OPERATION_EXPLORER_PART_AT_THE_TOP)) {
        miscStr += "\noperation classified by part in the operation explorer";
      }
      else {
        miscStr += "\noperation classified by work order in the operation explorer";
      }

      switch (data.Get<int>(Item.OPERATION_SLOT_SPLIT_OPTION)) {
        case 0:
          miscStr += "\nno part count";
          break;
        case 3:
          miscStr += "\npart count per user shift";
          break;
        case 4:
          miscStr += "\npart count per machine shift";
          break;
      }
      summary.Add(miscStr);
      
      return summary;
    }
    #endregion // Page methods
    
    #region Private methods
    void PrepareImages()
    {
      pictureOp2.Image = m_infoImage;
      pictureOp3.Image = m_infoImage;
      pictureWo1.Image = m_infoImage;
      pictureWo2.Image = m_infoImage;
      pictureMisc1.Image = m_infoImage;
      pictureMisc2.Image = m_infoImage;
      pictureMisc3.Image = m_infoImage;

      SetToolTip (pictureOp1, "Work orders may be used to drive the production.");
      SetToolTip(pictureOp2, "When an operation is performed, different types of intermediate work " +
                 "piece may be created.\nThis is not related to the fact that an operation can " +
                 "produce several parts / components of the same type within one cycle.");
      SetToolTip(pictureOp3, "An operation is made of sequences of instructions that can be run in parallel.");
      SetToolTip(pictureWo1, "Enable the use of workorders, in which a production takes place.");
      SetToolTip(pictureWo2, "This option allows a work order to comprise different types of part / component.");
      SetToolTip(pictureWo3, "");
      SetToolTip(pictureMisc1, "The final part / component may be an assembly of several parts / components being separately produced.\n" +
                 "This option enables the notion of project, being made of a coherent set of parts / components.");
      SetToolTip(pictureMisc2, "This option allows a production line to produce several types of part / component.");
      SetToolTip (pictureMisc3, "The operation in the operation explorer can be classified by part, otherwise by work order.");
    }
    
    void SetToolTip(Control control, string text)
    {
      var toolTip = new ToolTip();
      toolTip.SetToolTip(control, text);
      toolTip.AutoPopDelay = 32000;
      m_toolTips.Add(toolTip);
    }
    
    void LoadConfMoldShops()
    {
      if (checkOp1.Enabled) {
        checkOp1.Checked = false;
      }

      checkOp2.Checked = false;
      checkOp3.Checked = false;
      checkWo1.Checked = false;
      checkWo2.Checked = false;
      if (checkWo3.Enabled) {
        checkWo3.Checked = false;
      }

      checkMisc1.Checked = true;
      checkMisc2.Checked = false;
      checkMisc3.Checked = false;
      comboPartCount.SelectedValue = 0;
    }
    
    void LoadConfProduction()
    {
      if (checkOp1.Enabled) {
        checkOp1.Checked = false;
      }

      checkOp2.Checked = false;
      checkOp3.Checked = false;
      checkWo1.Checked = true;
      checkWo2.Checked = false;
      if (checkWo3.Enabled) {
        checkWo3.Checked = true;
      }

      checkMisc1.Checked = false;
      checkMisc2.Checked = false;
      checkMisc3.Checked = true;
      comboPartCount.SelectedValue = 3;
    }
    #endregion // Private methods
    
    #region Event reactions
    void ButtonLoadClick(object sender, System.EventArgs e)
    {
      switch ((int)comboConf.SelectedValue) {
        case 0:
          LoadConfMoldShops();
          break;
        case 1:
          LoadConfProduction();
          break;
        default:
          break;
      }
    }
    
    void CheckWo1CheckedChanged(object sender, EventArgs e)
    {
      if (checkWo1.Checked) {
        checkWo2.Enabled = checkWo3.Enabled = true;
      }
      else {
        checkWo2.Enabled = checkWo3.Enabled = checkWo2.Checked = checkWo3.Checked = false;
      }
    }
    #endregion // Event reactions
  }
}
