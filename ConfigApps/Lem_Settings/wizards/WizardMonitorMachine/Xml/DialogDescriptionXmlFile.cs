// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of DialogDescriptionXmlFile.
  /// </summary>
  public partial class DialogDescriptionXmlFile : Form
  {
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DialogDescriptionXmlFile(string fileTitle, CncDocument cncDoc)
    {
      InitializeComponent();
      
      this.Text = fileTitle;
      if (cncDoc.IsValid) {
        richTextDescription.Text = cncDoc.Description;
        labelUnit.Text = cncDoc.Unit ?? "None";
        
        // Supported machines
        foreach (var attribute in cncDoc.SupportedMachines) {
          listMachines.AddItem(Format(attribute));
        }

        // Supported controls
        foreach (var attribute in cncDoc.SupportedControls) {
          listControls.AddItem(Format(attribute));
        }

        // Supported controls
        foreach (var attribute in cncDoc.SupportedProtocols) {
          listProtocols.AddItem(Format(attribute));
        }

        // Modules
        label6.Text = cncDoc.Modules.Count + " module";
        if (cncDoc.Modules.Count > 1) {
          label6.Text += "s";
        }

        string modules = "";
        bool first = true;
        foreach (var module in cncDoc.Modules) {
          if (first) {
            first = false;
          }
          else {
            modules += ", ";
          }

          modules += module.m_identifier == "" ? "Main" : module.m_identifier;
        }
        labelModules.Text = modules;
      }
    }
    
    string Format(CncDocument.SupportedAttributes attribute)
    {
      string txt = attribute.m_mainAttribute;
      
      if (attribute.m_additionalAttributes.Count > 0) {
        txt += " (";
        bool first = true;
        foreach (var attributeKey in attribute.m_additionalAttributes.Keys) {
          if (first) {
            first = false;
          }
          else {
            txt += ", ";
          }

          txt += attributeKey + ": " + attribute.m_additionalAttributes[attributeKey];
        }
        txt += ")";
      }
      
      return txt;
    }
    #endregion // Constructors
  }
}
