// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using Ionic.Zip;
using Lemoine.Core.Log;
using Microsoft.Win32;

namespace Lem_Translator
{
  /// <summary>
  /// Description of MainForm.
  /// </summary>
  [SupportedOSPlatform ("windows7.0")]
  public partial class MainForm : Form
  {
    #region Members
    CultureSelectionForm cultureSelectionForm;
    CultureInfo cultureInfo;
    PulseTranslationFile pulseSourceFile;
    PulseTranslationFile pulseLocaleFile;
    DotNetTranslationFile dotNetSourceFile;
    DotNetTranslationFile dotNetLocaleFile;
    BirtTranslationFile birtSourceFile;
    BirtTranslationFile birtLocaleFile;
    Hashtable mnrSource;
    Hashtable mnrLocale;
    Hashtable msgSource;
    Hashtable msgLocale;
    Hashtable reportSource;
    Hashtable reportLocale;
    Hashtable dotNetSource;
    Hashtable dotNetLocale;
    Hashtable birtSource;
    Hashtable birtLocale;
    String installationPath;
    bool standaloneMode; // Standalone option
    bool standalone; // use it as a standalone application
    #endregion

    private static readonly ILog log = LogManager.GetLogger (typeof (MainForm).FullName);

    #region Constructors
    public MainForm ()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();

      //
      // Add constructor code after the InitializeComponent() call.
      //
      cultureSelectionForm = new CultureSelectionForm ();
      mnrSource = null;
      mnrLocale = null;
      msgSource = null;
      msgLocale = null;
      reportSource = null;
      reportLocale = null;
      dotNetSource = null;
      dotNetLocale = null;
      birtSource = null;
      birtLocale = null;
      standaloneMode = false;
    }
    #endregion

    #region Getters / Setters
    public bool HideRegionSpecificLocales
    {
      set { cultureSelectionForm.HideRegionSpecificLocales = value; }
    }

    public bool StandaloneMode
    {
      set { standaloneMode = value; }
    }
    #endregion

    #region Methods
    void LoadLocale ()
    {
      cultureSelectionForm.ShowDialog ();
      cultureInfo = cultureSelectionForm.SelectedCulture;
      LoadLocale (cultureInfo);
    }

    private void LoadLocale (CultureInfo cultureInfo)
    {
      if (pulseSourceFile == null) {
        // pulse_default.txt
        string pulseTxtPath = installationPath;
        if (!File.Exists (Path.Combine (pulseTxtPath, "pulse_default.txt"))) {
          pulseTxtPath = Path.Combine (pulseTxtPath, "share");
          if (!File.Exists (Path.Combine (pulseTxtPath, "pulse_default.txt"))) {
            pulseTxtPath = null;
          }
        }
        if (pulseTxtPath == null) {
          pulseSourceFile = null;
          mnrSource = null;
          msgSource = null;
          reportSource = null;
        }
        else {
          pulseSourceFile = new PulseTranslationFile (Path.Combine (pulseTxtPath,
                                                                    "pulse_default.txt"));
          mnrSource = pulseSourceFile.ReadSection ("MMS_MNR");
          msgSource = pulseSourceFile.ReadSection ("MMS_MSG");
          reportSource = pulseSourceFile.ReadSection ("REPORT");
        }
      }
      if (pulseSourceFile == null) {
        MessageBox.Show ("The original translation file pulse_default.txt could not be found.\n" +
                         "The MMS_MNR, MMS_MSG and REPORT tabs could not be loaded.",
                         "pulse_default.txt load error",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Error);
        pulseLocaleFile = null;
        mnrLocale = null;
        msgLocale = null;
        reportLocale = null;
        MMSMNRTabPage.Enabled = false;
        MMSMSGTabPage.Enabled = false;
        REPORTTabPage.Enabled = false;
        listViewMMSMNR.Items.Clear ();
        listViewMMSMSG.Items.Clear ();
        listViewREPORT.Items.Clear ();
      }
      else {
        MMSMNRTabPage.Enabled = true;
        MMSMSGTabPage.Enabled = true;
        REPORTTabPage.Enabled = true;
        // pulse_xx-XX.txt / pulse_xx.txt / pulse_xxx.txt
        string pulseTxtLocalePath =
          Path.Combine (Path.GetDirectoryName (pulseSourceFile.FilePath),
                        "pulse_"
                        + cultureInfo.Name
                        + ".txt");
        if (!File.Exists (pulseTxtLocalePath)) {
          string threeLetterTxtLocalePath =
            Path.Combine (Path.GetDirectoryName (pulseSourceFile.FilePath),
                          "pulse_"
                          + cultureInfo.ThreeLetterISOLanguageName
                          + ".txt");
          if (File.Exists (threeLetterTxtLocalePath)) {
            File.Move (threeLetterTxtLocalePath, pulseTxtLocalePath);
          }
          else {
            File.CreateText (pulseTxtLocalePath).Close ();
          }
        }
        log.Info ("translation file is "
                  + pulseTxtLocalePath);
        pulseLocaleFile = new PulseTranslationFile (pulseTxtLocalePath,
                                                    cultureInfo);
        mnrLocale = pulseLocaleFile.ReadSection ("MMS_MNR");
        msgLocale = pulseLocaleFile.ReadSection ("MMS_MSG");
        reportLocale = pulseLocaleFile.ReadSection ("REPORT");
        FillListView (listViewMMSMNR, mnrSource, mnrLocale, pulseLocaleFile.KeyComparer);
        FillListView (listViewMMSMSG, msgSource, msgLocale, pulseLocaleFile.KeyComparer);
        FillListView (listViewREPORT, reportSource, reportLocale, pulseLocaleFile.KeyComparer);
      }

      // PULSEi18n.txt (DotNet)
      string dotNetSourcePath = Lemoine.Info.PulseInfo.CommonConfigurationDirectory;
      dotNetSourcePath = Path.Combine (dotNetSourcePath,
                                       "PULSEi18n.txt");
      if (File.Exists (dotNetSourcePath)) {
        DotNetTabPage.Enabled = true;
        dotNetSourceFile = new DotNetTranslationFile (dotNetSourcePath);
        dotNetSource = dotNetSourceFile.Read ();

        // PULSEi18n.xx-XX.txt
        string dotNetLocalePath =
          dotNetSourcePath.Replace ("PULSEi18n.txt",
                                    "PULSEi18n."
                                    + cultureInfo.Name
                                    + ".txt");
        if (!File.Exists (dotNetLocalePath)) {
          File.CreateText (dotNetLocalePath).Close ();
        }
        dotNetLocaleFile = new DotNetTranslationFile (dotNetLocalePath, cultureInfo);
        dotNetLocale = dotNetLocaleFile.Read ();
        FillListView (listViewDotNet, dotNetSource, dotNetLocale, dotNetLocaleFile.KeyComparer);
      }

      // pulsereportsi18n.properties
      string birtSourcePath = installationPath;
      if (false == standalone) {
        birtSourcePath = Path.Combine (birtSourcePath,
                                       "l_ctr");
        birtSourcePath = Path.Combine (birtSourcePath, "pfrdata");
        birtSourcePath = Path.Combine (birtSourcePath, "report_templates");
      }
      birtSourcePath = Path.Combine (birtSourcePath, "pulsereportsi18n.properties");
      if (!File.Exists (birtSourcePath)) {
        MessageBox.Show ("The original BIRT translation file" + birtSourcePath +
                         " could not be found in directory.\n" +
                         "The BIRT reports tab could not be loaded.",
                         "pulsereportsi18n.properties load error",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Error);
        birtSourceFile = null;
        birtSource = null;
        birtLocale = null;
        BirtTabPage.Enabled = false;
        listViewBirt.Items.Clear ();
      }
      else {
        BirtTabPage.Enabled = true;
        birtSourceFile = new BirtTranslationFile (birtSourcePath);
        birtSource = birtSourceFile.Read ();

        // pulsereportsi18n_xx_XX.properties
        string birtLocalePath =
          birtSourcePath.Replace ("pulsereportsi18n.properties",
                                  "pulsereportsi18n_"
                                  + cultureInfo.Name.Replace ('-', '_')
                                  + ".properties");
        if (!File.Exists (birtLocalePath)) {
          File.CreateText (birtLocalePath).Close ();
        }
        birtLocaleFile = new BirtTranslationFile (birtLocalePath,
                                                  cultureInfo);
        birtLocale = birtLocaleFile.Read ();
        FillListView (listViewBirt, birtSource, birtLocale, birtLocaleFile.KeyComparer);
      }
    }

    void OpenButtonClick (object sender, EventArgs e)
    {
      LoadLocale ();
    }

    void MainFormLoad (object sender, EventArgs e)
    {
      // Get the installation path in registry
      try {
        if (standaloneMode) {
          throw new Exception ();
        }
        installationPath = Registry.LocalMachine.OpenSubKey ("SOFTWARE\\Lemoine\\PULSE").GetValue ("InstallDir").ToString ();
        standalone = false;
        Debug.Assert (null != installationPath, "Installation path is null");
        log.InfoFormat ("Use the full installation. Installation path is {0}",
                        installationPath);
      }
      catch (Exception) {
        // The software is not installed
        // Try to see if the standalone translator is installed
        try {
          installationPath = Registry.LocalMachine.OpenSubKey ("SOFTWARE\\Lemoine\\PulseTranslator").GetValue ("InstallDir").ToString ();
          standalone = true;
          Debug.Assert (null != installationPath, "Installation path is null");
          log.InfoFormat ("Use the standalone translator application. " +
                          "Installation path is {0}",
                          installationPath);
        }
        catch (Exception) {
          MessageBox.Show ("Something was not installed correctly.\n" +
                           "Please install it again.",
                           "Installation error",
                           MessageBoxButtons.OK,
                           MessageBoxIcon.Error);
          Application.Exit ();
        }
      }

      // pulse_default.txt
      string pulseTxtPath = installationPath;
      if (!File.Exists (Path.Combine (pulseTxtPath, "pulse_default.txt"))) {
        pulseTxtPath = Path.Combine (pulseTxtPath, "share");
        if (!File.Exists (Path.Combine (pulseTxtPath, "pulse_default.txt"))) {
          pulseTxtPath = null;
        }
      }
      if (pulseTxtPath == null) {
        pulseSourceFile = null;
        mnrSource = null;
        msgSource = null;
        reportSource = null;
      }
      else {
        pulseSourceFile = new PulseTranslationFile (Path.Combine (pulseTxtPath,
                                                                  "pulse_default.txt"));
        mnrSource = pulseSourceFile.ReadSection ("MMS_MNR");
        msgSource = pulseSourceFile.ReadSection ("MMS_MSG");
        reportSource = pulseSourceFile.ReadSection ("REPORT");
      }

      LoadLocale ();
    }

    void SaveButtonClick (object sender, EventArgs e)
    {
      if (pulseLocaleFile != null) {
        StreamWriter pulseWriter = pulseLocaleFile.GetWriter ();
        pulseLocaleFile.SaveSection (pulseWriter, "MMS_MNR", mnrLocale);
        pulseLocaleFile.SaveSection (pulseWriter, "MMS_MSG", msgLocale);
        pulseLocaleFile.SaveSection (pulseWriter, "REPORT", reportLocale);
        pulseWriter.Close ();
      }
      if (dotNetLocaleFile != null) {
        dotNetLocaleFile.Save (dotNetLocale);
      }
      if (birtLocaleFile != null) {
        birtLocaleFile.Save (birtLocale);
      }
    }

    void ListViewMMSMNRVisibleChanged (object sender, EventArgs e)
    {
      // TODO: charger au fur et `a mesure
      translationTextBox.Text = "";
      ListViewMMSMNRSelectedIndexChanged (sender, e);
    }

    void ListViewMMSMNRSelectedIndexChanged (object sender, EventArgs e)
    {
      if (listViewMMSMNR.SelectedItems.Count != 0) {
        translationTextBox.Text = listViewMMSMNR.SelectedItems[0].SubItems[2].Text;
      }
    }

    void ListViewMMSMSGVisibleChanged (object sender, EventArgs e)
    {
      // TODO: charger au fur et `a mesure
      translationTextBox.Text = "";
      ListViewMMSMSGSelectedIndexChanged (sender, e);
    }

    void ListViewMMSMSGSelectedIndexChanged (object sender, EventArgs e)
    {
      if (listViewMMSMSG.SelectedItems.Count != 0) {
        translationTextBox.Text = listViewMMSMSG.SelectedItems[0].SubItems[2].Text;
      }
    }

    void ListViewREPORTVisibleChanged (object sender, EventArgs e)
    {
      // TODO: charger au fur et `a mesure
      translationTextBox.Text = "";
      ListViewREPORTSelectedIndexChanged (sender, e);
    }

    void ListViewREPORTSelectedIndexChanged (object sender, EventArgs e)
    {
      if (listViewREPORT.SelectedItems.Count != 0) {
        translationTextBox.Text = listViewREPORT.SelectedItems[0].SubItems[2].Text;
      }
    }

    void ListViewDotNetVisibleChanged (object sender, EventArgs e)
    {
      // TODO: charger au fur et `a mesure
      translationTextBox.Text = "";
      ListViewDotNetSelectedIndexChanged (sender, e);
    }

    void ListViewDotNetSelectedIndexChanged (object sender, EventArgs e)
    {
      if (listViewDotNet.SelectedItems.Count != 0) {
        translationTextBox.Text = listViewDotNet.SelectedItems[0].SubItems[2].Text;
      }
    }

    void ListViewBirtVisibleChanged (object sender, EventArgs e)
    {
      // TODO: charger au fur et `a mesure
      translationTextBox.Text = "";
      ListViewBirtSelectedIndexChanged (sender, e);
    }

    void ListViewBirtSelectedIndexChanged (object sender, System.EventArgs e)
    {
      if (listViewBirt.SelectedItems.Count != 0) {
        translationTextBox.Text = listViewBirt.SelectedItems[0].SubItems[2].Text;
      }
    }

    void TranslationTextBoxCommitChange (object sender, EventArgs e)
    {
      ListView listView;
      Hashtable hashTable;
      bool keyIsInteger = false;
      if (listViewMMSMNR.Visible == true) {
        listView = listViewMMSMNR;
        hashTable = mnrLocale;
        keyIsInteger = true;
      }
      else if (listViewMMSMSG.Visible == true) {
        listView = listViewMMSMSG;
        hashTable = msgLocale;
        keyIsInteger = true;
      }
      else if (listViewREPORT.Visible == true) {
        listView = listViewREPORT;
        hashTable = reportLocale;
        keyIsInteger = true;
      }
      else if (listViewDotNet.Visible == true) {
        listView = listViewDotNet;
        hashTable = dotNetLocale;
        keyIsInteger = false;
      }
      else if (listViewBirt.Visible == true) {
        listView = listViewBirt;
        hashTable = birtLocale;
        keyIsInteger = false;
      }
      else {
        listView = null;
        hashTable = null;
      }
      if (listView.SelectedItems.Count > 0) {
        listView.SelectedItems[0].SubItems[2].Text = translationTextBox.Text;
        if ((listView.SelectedItems[0].SubItems[1].Text.Trim ().Length != 0)
            && (translationTextBox.Text.Trim ().Length == 0)) {
          listView.SelectedItems[0].BackColor = Color.Yellow;
        }
        else {
          listView.SelectedItems[0].BackColor = Color.Empty;
        }
        object key;
        if (keyIsInteger) {
          try {
            key = Int32.Parse (listView.SelectedItems[0].Text);
          }
          catch (FormatException) {
            key = listView.SelectedItems[0].Text;
          }
        }
        else {
          key = listView.SelectedItems[0].Text;
        }
        if (translationTextBox.Text.Trim ().Length == 0) {
          hashTable.Remove (key);
        }
        else {
          hashTable[key]
            = translationTextBox.Text;
        }
      }
    }

    void TranslationTextBoxKeyDown (object sender, KeyEventArgs e)
    {
      switch (e.KeyCode) {
      case Keys.Enter:
        TranslationTextBoxCommitChange (sender, e);
        break;
      case Keys.Up:
        TranslationTextBoxCommitChange (sender, e);
        UpAction ();
        e.Handled = true;
        translationTextBox.SelectAll ();
        break;
      case Keys.Down:
        TranslationTextBoxCommitChange (sender, e);
        DownAction ();
        e.Handled = true;
        translationTextBox.SelectAll ();
        break;
      case Keys.Escape:
        ResetTranslationBox ();
        e.Handled = true;
        translationTextBox.SelectAll ();
        break;
      default:
        break;
      }
    }

    void ListViewMMSMNREnter (object sender, EventArgs e)
    {
      translationTextBox.Focus ();
    }

    void ListViewMMSMSGEnter (object sender, EventArgs e)
    {
      translationTextBox.Focus ();
    }

    void ListViewREPORTEnter (object sender, EventArgs e)
    {
      translationTextBox.Focus ();
    }

    void ListViewDotNetEnter (object sender, EventArgs e)
    {
      translationTextBox.Focus ();
    }

    void ListViewBirtEnter (object sender, EventArgs e)
    {
      translationTextBox.Focus ();
    }

    private void UpAction ()
    {
      ListView listView = GetCurrentListView ();
      Debug.Assert (null != listView);
      if ((listView.SelectedItems.Count > 0)
          && (listView.SelectedIndices[0] > 0)) {
        listView.Items[listView.SelectedIndices[0] - 1].Selected = true;
      }
      listView.EnsureVisible (listView.SelectedIndices[0]);
    }

    private void DownAction ()
    {
      ListView listView = GetCurrentListView ();
      Debug.Assert (null != listView);
      if ((listView.SelectedItems.Count > 0)
          && (listView.SelectedIndices[0] < listView.Items.Count - 1)) {
        listView.Items[listView.SelectedIndices[0] + 1].Selected = true;
      }
      listView.EnsureVisible (listView.SelectedIndices[0]);
    }

    private void ResetTranslationBox ()
    {
      ListView listView = GetCurrentListView ();
      Debug.Assert (null != listView);
      if (listView.SelectedItems.Count != 0) {
        translationTextBox.Text = listView.SelectedItems[0].SubItems[2].Text;
      }
      else {
        translationTextBox.Text = "";
      }
    }

    private ListView GetCurrentListView ()
    {
      if (listViewMMSMNR.Visible == true) {
        return listViewMMSMNR;
      }
      else if (listViewMMSMSG.Visible == true) {
        return listViewMMSMSG;
      }
      else if (listViewREPORT.Visible == true) {
        return listViewREPORT;
      }
      else if (listViewDotNet.Visible == true) {
        return listViewDotNet;
      }
      else if (listViewBirt.Visible == true) {
        return listViewBirt;
      }
      else {
        Debug.Assert (false);
        return null;
      }
    }

    private void FillListView (ListView listView,
                               Hashtable source,
                               Hashtable locale,
                               IComparer keyComparer)
    {
      Debug.Assert (source != null);
      listView.Items.Clear ();
      object[] keys = new object[source.Count];
      source.Keys.CopyTo (keys, 0);
      Array.Sort (keys, keyComparer);
      foreach (object key in keys) {
        Debug.Assert (source[key] != null);
        string localeString = "";
        if (locale[key] != null) {
          localeString = locale[key].ToString ();
        }
        ListViewItem lvItem = new ListViewItem (new string[] {
                                                  key.ToString (),
                                                  source [key].ToString (),
                                                  localeString });
        if ((source[key].ToString ().Trim ().Length != 0)
            && (localeString.Trim ().Length == 0)) {
          lvItem.BackColor = Color.Yellow;
        }
        listView.Items.Add (lvItem);
      }
    }

    void NextToolStripButtonClick (object sender, EventArgs e)
    {
      ListView listView = GetCurrentListView ();
      Debug.Assert (null != listView);
      int i = 0; // Initial position
      if (listView.SelectedItems.Count > 0) {
        i = listView.SelectedIndices[0] + 1;
      }
      // Search the next not translated item
      for (;
           i < listView.Items.Count;
           ++i) {
        if ((listView.Items[i].SubItems[1].Text.Length != 0)
            && (listView.Items[i].SubItems[2].Text.Length == 0)) {
          TranslationTextBoxCommitChange (sender, e);
          listView.Items[i].Selected = true;
          listView.EnsureVisible (listView.SelectedIndices[0]);
          translationTextBox.Focus ();
          break;
        }
      }
    }

    void PrevToolStripButtonClick (object sender, EventArgs e)
    {
      ListView listView = GetCurrentListView ();
      Debug.Assert (null != listView);
      int i = listView.Items.Count - 1; // Initial position
      if (listView.SelectedItems.Count > 0) {
        i = listView.SelectedIndices[0] - 1;
      }
      // Search the next not translated item
      for (;
           i >= 0;
           --i) {
        if ((listView.Items[i].SubItems[1].Text.Length != 0)
            && (listView.Items[i].SubItems[2].Text.Length == 0)) {
          TranslationTextBoxCommitChange (sender, e);
          listView.Items[i].Selected = true;
          listView.EnsureVisible (listView.SelectedIndices[0]);
          translationTextBox.Focus ();
          break;
        }
      }
    }

    void MMSMNRTabPagePaint (object sender, PaintEventArgs e)
    {
      if (pulseLocaleFile == null) {
        translationFileLabel.Text = "Translation file was not loaded";
      }
      else {
        translationFileLabel.Text = "Translation file: "
          + pulseLocaleFile.FilePath;
      }
    }

    void MMSMSGTabPagePaint (object sender, PaintEventArgs e)
    {
      if (pulseLocaleFile == null) {
        translationFileLabel.Text = "Translation file was not loaded";
      }
      else {
        translationFileLabel.Text = "Translation file: "
          + pulseLocaleFile.FilePath;
      }
    }

    void REPORTTabPagePaint (object sender, PaintEventArgs e)
    {
      if (pulseLocaleFile == null) {
        translationFileLabel.Text = "Translation file was not loaded";
      }
      else {
        translationFileLabel.Text = "Translation file: "
          + pulseLocaleFile.FilePath;
      }
    }

    void DotNetTabPagePaint (object sender, PaintEventArgs e)
    {
      if (dotNetLocaleFile == null) {
        translationFileLabel.Text = ".NET translation was not loaded";
      }
      else {
        translationFileLabel.Text = ".Net translation loaded";
      }
    }

    void BirtTabPagePaint (object sender, PaintEventArgs e)
    {
      if (birtLocaleFile == null) {
        translationFileLabel.Text = "Birt translation file was not loaded";
      }
      else {
        translationFileLabel.Text = "Translation file: "
          + birtLocaleFile.FilePath;
      }
    }

    void ExportButtonClick (object sender, EventArgs e)
    {
      string currentDirectory = Directory.GetCurrentDirectory ();
      SaveButtonClick (sender, e);
      exportFileDialog.FileName = "PulseTranslations." + cultureInfo.Name + ".zip";
      exportFileDialog.ShowDialog ();
      Directory.SetCurrentDirectory (currentDirectory);
    }

    void ExportFileDialogFileOk (object sender, System.ComponentModel.CancelEventArgs e)
    {
      try {
        using (ZipFile zip = new ZipFile ()) {
          string lemInfosPath = installationPath;
          if (!File.Exists (Path.Combine (lemInfosPath, "Lem_Infos.exe"))) {
            lemInfosPath = Path.Combine (lemInfosPath, "share");
          }
          lemInfosPath = Path.Combine (lemInfosPath, "Lem_Infos.exe");

          string batScript =
            $@"@echo off
for /f ""tokens=* delims="" %%i in ('Lem_Infos.exe -i') do set installDir=%%i

REM pulse_xx-XX.txt
copy /Y ""{Path.GetFileName (pulseLocaleFile.FilePath)}"" ""%installDir%\\share""

REM pulsereportsi18n_xx_XX.properties
copy /Y ""{Path.GetFileName (birtLocaleFile.FilePath)}"" ""%installDir%\\l_ctr\\pfrdata\\report_templates""

REM PULSEi19n_xx-XX.txt
copy /Y ""{Path.GetFileName (birtLocaleFile.FilePath)}"" ""{Lemoine.Info.PulseInfo.CommonConfigurationDirectory}""

echo The installation of translation files was done successfully
pause";

          zip.AddFile (pulseLocaleFile.FilePath, "");
          zip.AddFile (birtLocaleFile.FilePath, "");
          zip.AddFile (dotNetLocaleFile.FilePath, "");
          zip.AddFile (lemInfosPath, "");
          // TODO: Install.bat in the .zip file
          /*
          zip.AddFileFromString ("Install.bat", "", batScript);
          */
          zip.Save (exportFileDialog.FileName);
        }
      }
      catch (Exception exc) {
        log.ErrorFormat ("Following error occured while trying to export to {0}: {1}",
                         exportFileDialog.FileName,
                         exc.ToString ());
        MessageBox.Show ("The translations files could not be exported",
                         "Translation files export error",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Error);
      }
    }
    #endregion

    void ImportButtonClick (object sender, EventArgs e)
    {
      string currentDirectory = Directory.GetCurrentDirectory ();
      if (DialogResult.Yes ==
          MessageBox.Show ("Do you want to save first your translations ?",
                           "Save your translations ?",
                           MessageBoxButtons.YesNo,
                           MessageBoxIcon.Question)) {
        SaveButtonClick (sender, e);
      }

      importFileDialog.ShowDialog ();
      Directory.SetCurrentDirectory (currentDirectory);
    }

    void ImportFileDialogFileOk (object sender, System.ComponentModel.CancelEventArgs e)
    {
      try {
        using (ZipFile zip = ZipFile.Read (importFileDialog.FileName)) {
          // - Import the zip file
          Regex pulseFileRegex =
            new Regex ("pulse_([a-z][a-z](-[A-Z][A-Z])?).txt");
          string localeString = null;
          foreach (var zipEntry in zip) {
            var filename = zipEntry.FileName;
            Match m = pulseFileRegex.Match (filename);
            if (m.Success) {
              localeString = m.Groups[1].Value;
            }
            if (filename.StartsWith ("pulse_")) {
              string extractPath = Path.GetDirectoryName (pulseLocaleFile.FilePath);
              log.InfoFormat ("Extract filename {0} into directory {1}",
                              filename, extractPath);
              zipEntry.Extract (extractPath, ExtractExistingFileAction.OverwriteSilently);
            }
            else if (filename.StartsWith ("pulsereportsi18n_")) {
              string extractPath = Path.GetDirectoryName (birtLocaleFile.FilePath);
              log.InfoFormat ("Extract filename {0} into directory {1}",
                              filename, extractPath);
              zipEntry.Extract (extractPath, ExtractExistingFileAction.OverwriteSilently);
            }
            {
              string extractPath =
                Path.GetDirectoryName (pulseLocaleFile.FilePath);
            }
          }

          // - Open the imported locale
          Debug.Assert (null != localeString);
          LoadLocale (new CultureInfo (localeString));
        }
      }
      catch (Exception exc) {
        log.Error (exc);
      }
    }

    void NextSearchButtonClick (object sender, EventArgs e)
    {
      ListView listView = GetCurrentListView ();
      Debug.Assert (null != listView);
      int i = 0; // Initial position
      if (listView.SelectedItems.Count > 0) {
        i = listView.SelectedIndices[0] + 1;
      }
      // Search the next not translated item
      for (;
           i < listView.Items.Count;
           ++i) {
        if ((listView.Items[i].SubItems[0].Text.ToUpper ().Contains (searchTextBox.Text.ToUpper ()))
            || (listView.Items[i].SubItems[1].Text.ToUpper ().Contains (searchTextBox.Text.ToUpper ()))
            || (listView.Items[i].SubItems[2].Text.ToUpper ().Contains (searchTextBox.Text.ToUpper ()))) {
          TranslationTextBoxCommitChange (sender, e);
          listView.Items[i].Selected = true;
          listView.EnsureVisible (listView.SelectedIndices[0]);
          translationTextBox.Focus ();
          break;
        }
      }
    }

    void PreviousSearchButtonClick (object sender, EventArgs e)
    {
      ListView listView = GetCurrentListView ();
      Debug.Assert (null != listView);
      int i = listView.Items.Count - 1; // Initial position
      if (listView.SelectedItems.Count > 0) {
        i = listView.SelectedIndices[0] - 1;
      }
      // Search the next not translated item
      for (;
           i >= 0;
           --i) {
        if ((listView.Items[i].SubItems[0].Text.ToUpper ().Contains (searchTextBox.Text.ToUpper ()))
            || (listView.Items[i].SubItems[1].Text.ToUpper ().Contains (searchTextBox.Text.ToUpper ()))
            || (listView.Items[i].SubItems[2].Text.ToUpper ().Contains (searchTextBox.Text.ToUpper ()))) {
          TranslationTextBoxCommitChange (sender, e);
          listView.Items[i].Selected = true;
          listView.EnsureVisible (listView.SelectedIndices[0]);
          translationTextBox.Focus ();
          break;
        }
      }
    }

    void SearchTextBoxKeyDown (object sender, KeyEventArgs e)
    {
      if ((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Return)) {
        NextSearchButtonClick (sender, e);
      }
    }
  }
}
