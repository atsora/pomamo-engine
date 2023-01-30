// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_Configuration
{
  partial class MainForm
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    
    /// <summary>
    /// Disposes resources used by the form.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        if (components != null) {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }
    
    /// <summary>
    /// This method is required for Windows Forms designer support.
    /// Do not change the method contents inside the source code editor. The Forms designer might
    /// not be able to load this method if it was changed manually.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
      this.mainTabCtrl = new System.Windows.Forms.TabControl();
      this.generalTabPage = new System.Windows.Forms.TabPage();
      this.generalTabCtrl = new System.Windows.Forms.TabControl();
      this.availableOptionsTabPage = new System.Windows.Forms.TabPage();
      this.availableOptionsFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
      this.globalTabPage = new System.Windows.Forms.TabPage();
      this.globalOptionConfig1 = new Lemoine.ConfigControls.ConfigConfig();
      this.dataStructureTabPage = new System.Windows.Forms.TabPage();
      this.dataStructureOptionConfig1 = new Lemoine.ConfigControls.ConfigConfig();
      this.displayTabPage = new System.Windows.Forms.TabPage();
      this.dataStructureDisplayConfig1 = new Lemoine.ConfigControls.DataStructureDisplayConfig();
      this.analysisTabPage = new System.Windows.Forms.TabPage();
      this.analysisConfig1 = new Lemoine.ConfigControls.ConfigConfig();
      this.shiftTabPage = new System.Windows.Forms.TabPage();
      this.shiftConfig1 = new Lemoine.ConfigControls.ShiftConfig();
      this.computerTabPage = new System.Windows.Forms.TabPage();
      this.computerConfig = new Lemoine.ConfigControls.ComputerConfig();
      this.shiftTemplateTabPage = new System.Windows.Forms.TabPage();
      this.shiftTemplateConfig1 = new Lemoine.ConfigControls.ShiftTemplateConfig();
      this.dayTemplateTabPage = new System.Windows.Forms.TabPage();
      this.dayTemplateConfig1 = new Lemoine.ConfigControls.DayTemplateConfig();
      this.machineConfigsTabPage = new System.Windows.Forms.TabPage();
      this.machineTabControl = new System.Windows.Forms.TabControl();
      this.companyTabPage = new System.Windows.Forms.TabPage();
      this.companyConfig1 = new Lemoine.ConfigControls.CompanyConfig();
      this.departmentTabPage = new System.Windows.Forms.TabPage();
      this.departmentConfig1 = new Lemoine.ConfigControls.DepartmentConfig();
      this.cellTabPage = new System.Windows.Forms.TabPage();
      this.cellConfig1 = new Lemoine.ConfigControls.CellConfig();
      this.machineCategoryTabPage = new System.Windows.Forms.TabPage();
      this.machineCategoryConfig1 = new Lemoine.ConfigControls.MachineCategoryConfig();
      this.machineSubCategoryTabPage = new System.Windows.Forms.TabPage();
      this.machineSubCategoryConfig1 = new Lemoine.ConfigControls.MachineSubCategoryConfig();
      this.machineTabPage = new System.Windows.Forms.TabPage();
      this.machineConfig1 = new Lemoine.ConfigControls.MachineConfig();
      this.machineFilterTabPage = new System.Windows.Forms.TabPage();
      this.machineFilterConfig1 = new Lemoine.ConfigControls.MachineFilterConfig();
      this.acquisitionTabPage = new System.Windows.Forms.TabPage();
      this.acquisitionTabControl = new System.Windows.Forms.TabControl();
      this.monitoredMachineTabPage = new System.Windows.Forms.TabPage();
      this.monitoredMachineConfig1 = new Lemoine.ConfigControls.MonitoredMachineConfig();
      this.machineModuleTabPage = new System.Windows.Forms.TabPage();
      this.machineModuleConfig1 = new Lemoine.ConfigControls.MachineModuleConfig();
      this.cncAcquisitionTabPage = new System.Windows.Forms.TabPage();
      this.cncAcquisitionConfig1 = new Lemoine.ConfigControls.CncAcquisitionConfig();
      this.unitTabPage = new System.Windows.Forms.TabPage();
      this.unitConfig1 = new Lemoine.ConfigControls.UnitConfig();
      this.fieldTabPage = new System.Windows.Forms.TabPage();
      this.fieldConfig1 = new Lemoine.ConfigControls.FieldConfig();
      this.cncConfigTabPage = new System.Windows.Forms.TabPage();
      this.cncConfig = new Lemoine.ConfigControls.ConfigConfig();
      this.machineStatusTabPage = new System.Windows.Forms.TabPage();
      this.machineStatusTabControl = new System.Windows.Forms.TabControl();
      this.machineModeTabPage = new System.Windows.Forms.TabPage();
      this.machineModeConfig1 = new Lemoine.ConfigControls.MachineModeConfig();
      this.machineObservationStateTabPage = new System.Windows.Forms.TabPage();
      this.machineObservationStateConfig1 = new Lemoine.ConfigControls.MachineObservationStateConfig();
      this.reasonGroupTabPage = new System.Windows.Forms.TabPage();
      this.reasonGroupConfig1 = new Lemoine.ConfigControls.ReasonGroupConfig();
      this.reasonTabPage = new System.Windows.Forms.TabPage();
      this.reasonConfig1 = new Lemoine.ConfigControls.ReasonConfig();
      this.reasonSelectionTabPage = new System.Windows.Forms.TabPage();
      this.reasonSelectionConfig1 = new Lemoine.ConfigControls.ReasonSelectionConfig();
      this.defaultReasonTabPage = new System.Windows.Forms.TabPage();
      this.machineModeDefaultReasonConfig1 = new Lemoine.ConfigControls.MachineModeDefaultReasonConfig();
      this.machineStateTemplateTab = new System.Windows.Forms.TabPage();
      this.machineStateTemplateConfig1 = new Lemoine.ConfigControls.MachineStateTemplateConfig();
      this.machineStateTemplateRightTab = new System.Windows.Forms.TabPage();
      this.machineStateTemplateRightConfig1 = new Lemoine.ConfigControls.MachineStateTemplateRightConfig();
      this.autoMachineStateTemplateTabPage = new System.Windows.Forms.TabPage();
      this.autoMachineStateTemplateConfig1 = new Lemoine.ConfigControls.AutoMachineStateTemplateConfig();
      this.productionStateTabPage = new System.Windows.Forms.TabPage();
      this.productionStateConfig = new Lemoine.ConfigControls.ProductionStateConfig();
      this.interfacesTabPage = new System.Windows.Forms.TabPage();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.fieldLegendTabPage = new System.Windows.Forms.TabPage();
      this.fieldLegendConfig1 = new Lemoine.ConfigControls.FieldLegendConfig();
      this.guiConfigTabPage = new System.Windows.Forms.TabPage();
      this.guiConfigConfig = new Lemoine.ConfigControls.ConfigConfig();
      this.machineStateTemplateFlowTabPage = new System.Windows.Forms.TabPage();
      this.machineStateTemplateFlowConfig = new Lemoine.ConfigControls.MachineStateTemplateFlowConfig();
      this.evenTabPage = new System.Windows.Forms.TabPage();
      this.eventTabControl = new System.Windows.Forms.TabControl();
      this.eventTabPage = new System.Windows.Forms.TabPage();
      this.eventLevelConfig = new Lemoine.ConfigControls.EventLevelConfig();
      this.eventLongPeriodConfigTabPage = new System.Windows.Forms.TabPage();
      this.eventLongPeriodConfigConfig1 = new Lemoine.ConfigControls.EventLongPeriodConfigConfig();
      this.eventCnCValueConfigTabPage = new System.Windows.Forms.TabPage();
      this.eventCncValueConfigConfig = new Lemoine.ConfigControls.EventCncValueConfigConfig();
      this.eventToolLifeConfigTabPage = new System.Windows.Forms.TabPage();
      this.eventToolLifeConfigConfig = new Lemoine.ConfigControls.EventToolLifeConfigConfig();
      this.eventSMTPTabPage = new System.Windows.Forms.TabPage();
      this.smtpStatusStrip = new Lemoine.ConfigControls.SMTPStatusStrip();
      this.smtpConfig = new Lemoine.ConfigControls.ConfigConfig();
      this.eventEmailConfigTabPage = new System.Windows.Forms.TabPage();
      this.emailConfigConfig = new Lemoine.ConfigControls.EmailConfigConfig();
      this.qualityTabPage = new System.Windows.Forms.TabPage();
      this.qualityTabControl = new System.Windows.Forms.TabControl();
      this.nonConformanceReasonTabPage = new System.Windows.Forms.TabPage();
      this.nonConformanceReasonConfig = new Lemoine.ConfigControls.NonConformanceReasonConfig();
      this.mainUserTabPage = new System.Windows.Forms.TabPage();
      this.mainUserTabControl = new System.Windows.Forms.TabControl();
      this.userTabPage = new System.Windows.Forms.TabPage();
      this.userConfig = new Lemoine.ConfigControls.UserConfig();
      this.IconTabCont = new System.Windows.Forms.ImageList(this.components);
      this.stampingTabPage = new System.Windows.Forms.TabPage();
      this.stampingTabControl = new System.Windows.Forms.TabControl();
      this.stampingParametersTabPage = new System.Windows.Forms.TabPage();
      this.stampingParametersConfig = new Lemoine.ConfigControls.ConfigConfig ();
      this.mainTabCtrl.SuspendLayout();
      this.generalTabPage.SuspendLayout();
      this.generalTabCtrl.SuspendLayout();
      this.availableOptionsTabPage.SuspendLayout();
      this.globalTabPage.SuspendLayout();
      this.dataStructureTabPage.SuspendLayout();
      this.displayTabPage.SuspendLayout();
      this.analysisTabPage.SuspendLayout();
      this.shiftTabPage.SuspendLayout();
      this.computerTabPage.SuspendLayout();
      this.shiftTemplateTabPage.SuspendLayout();
      this.dayTemplateTabPage.SuspendLayout();
      this.machineConfigsTabPage.SuspendLayout();
      this.machineTabControl.SuspendLayout();
      this.companyTabPage.SuspendLayout();
      this.departmentTabPage.SuspendLayout();
      this.cellTabPage.SuspendLayout();
      this.machineCategoryTabPage.SuspendLayout();
      this.machineSubCategoryTabPage.SuspendLayout();
      this.machineTabPage.SuspendLayout();
      this.machineFilterTabPage.SuspendLayout();
      this.acquisitionTabPage.SuspendLayout();
      this.acquisitionTabControl.SuspendLayout();
      this.monitoredMachineTabPage.SuspendLayout();
      this.machineModuleTabPage.SuspendLayout();
      this.cncAcquisitionTabPage.SuspendLayout();
      this.unitTabPage.SuspendLayout();
      this.fieldTabPage.SuspendLayout();
      this.cncConfigTabPage.SuspendLayout();
      this.machineStatusTabPage.SuspendLayout();
      this.machineStatusTabControl.SuspendLayout();
      this.machineModeTabPage.SuspendLayout();
      this.machineObservationStateTabPage.SuspendLayout();
      this.reasonGroupTabPage.SuspendLayout();
      this.reasonTabPage.SuspendLayout();
      this.reasonSelectionTabPage.SuspendLayout();
      this.defaultReasonTabPage.SuspendLayout();
      this.machineStateTemplateTab.SuspendLayout();
      this.machineStateTemplateRightTab.SuspendLayout();
      this.autoMachineStateTemplateTabPage.SuspendLayout();
      this.productionStateTabPage.SuspendLayout();
      this.interfacesTabPage.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.fieldLegendTabPage.SuspendLayout();
      this.guiConfigTabPage.SuspendLayout();
      this.machineStateTemplateFlowTabPage.SuspendLayout();
      this.evenTabPage.SuspendLayout();
      this.eventTabControl.SuspendLayout();
      this.eventTabPage.SuspendLayout();
      this.eventLongPeriodConfigTabPage.SuspendLayout();
      this.eventCnCValueConfigTabPage.SuspendLayout();
      this.eventToolLifeConfigTabPage.SuspendLayout();
      this.eventSMTPTabPage.SuspendLayout();
      this.eventEmailConfigTabPage.SuspendLayout();
      this.qualityTabPage.SuspendLayout();
      this.qualityTabControl.SuspendLayout();
      this.nonConformanceReasonTabPage.SuspendLayout();
      this.mainUserTabPage.SuspendLayout();
      this.mainUserTabControl.SuspendLayout();
      this.userTabPage.SuspendLayout();
      this.stampingTabPage.SuspendLayout();
      this.stampingTabControl.SuspendLayout();
      this.stampingParametersConfig.SuspendLayout();
      this.SuspendLayout();
      // 
      // mainTabCtrl
      // 
      this.mainTabCtrl.Controls.Add(this.generalTabPage);
      this.mainTabCtrl.Controls.Add(this.machineConfigsTabPage);
      this.mainTabCtrl.Controls.Add(this.acquisitionTabPage);
      this.mainTabCtrl.Controls.Add(this.machineStatusTabPage);
      this.mainTabCtrl.Controls.Add(this.interfacesTabPage);
      this.mainTabCtrl.Controls.Add(this.evenTabPage);
      this.mainTabCtrl.Controls.Add(this.qualityTabPage);
      this.mainTabCtrl.Controls.Add(this.mainUserTabPage);
      this.mainTabCtrl.Controls.Add(this.stampingTabPage);
      this.mainTabCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.mainTabCtrl.ImageList = this.IconTabCont;
      this.mainTabCtrl.ItemSize = new System.Drawing.Size(110, 25);
      this.mainTabCtrl.Location = new System.Drawing.Point(0, 0);
      this.mainTabCtrl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.mainTabCtrl.Multiline = true;
      this.mainTabCtrl.Name = "mainTabCtrl";
      this.mainTabCtrl.Padding = new System.Drawing.Point(5, 5);
      this.mainTabCtrl.SelectedIndex = 0;
      this.mainTabCtrl.ShowToolTips = true;
      this.mainTabCtrl.Size = new System.Drawing.Size(1035, 517);
      this.mainTabCtrl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
      this.mainTabCtrl.TabIndex = 0;
      // 
      // generalTabPage
      // 
      this.generalTabPage.Controls.Add(this.generalTabCtrl);
      this.generalTabPage.ImageKey = "General.png";
      this.generalTabPage.Location = new System.Drawing.Point(4, 29);
      this.generalTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.generalTabPage.Name = "generalTabPage";
      this.generalTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.generalTabPage.Size = new System.Drawing.Size(1027, 484);
      this.generalTabPage.TabIndex = 0;
      this.generalTabPage.Text = "General";
      this.generalTabPage.ToolTipText = "General";
      this.generalTabPage.UseVisualStyleBackColor = true;
      // 
      // generalTabCtrl
      // 
      this.generalTabCtrl.Controls.Add(this.availableOptionsTabPage);
      this.generalTabCtrl.Controls.Add(this.globalTabPage);
      this.generalTabCtrl.Controls.Add(this.dataStructureTabPage);
      this.generalTabCtrl.Controls.Add(this.displayTabPage);
      this.generalTabCtrl.Controls.Add(this.analysisTabPage);
      this.generalTabCtrl.Controls.Add(this.shiftTabPage);
      this.generalTabCtrl.Controls.Add(this.computerTabPage);
      this.generalTabCtrl.Controls.Add(this.shiftTemplateTabPage);
      this.generalTabCtrl.Controls.Add(this.dayTemplateTabPage);
      this.generalTabCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.generalTabCtrl.ItemSize = new System.Drawing.Size(100, 25);
      this.generalTabCtrl.Location = new System.Drawing.Point(4, 3);
      this.generalTabCtrl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.generalTabCtrl.Name = "generalTabCtrl";
      this.generalTabCtrl.SelectedIndex = 0;
      this.generalTabCtrl.Size = new System.Drawing.Size(1019, 478);
      this.generalTabCtrl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
      this.generalTabCtrl.TabIndex = 0;
      // 
      // availableOptionsTabPage
      // 
      this.availableOptionsTabPage.Controls.Add(this.availableOptionsFlowLayoutPanel);
      this.availableOptionsTabPage.Location = new System.Drawing.Point(4, 29);
      this.availableOptionsTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.availableOptionsTabPage.Name = "availableOptionsTabPage";
      this.availableOptionsTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.availableOptionsTabPage.Size = new System.Drawing.Size(1011, 445);
      this.availableOptionsTabPage.TabIndex = 3;
      this.availableOptionsTabPage.Text = "AvailableOptions";
      this.availableOptionsTabPage.UseVisualStyleBackColor = true;
      // 
      // availableOptionsFlowLayoutPanel
      // 
      this.availableOptionsFlowLayoutPanel.AutoScroll = true;
      this.availableOptionsFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.availableOptionsFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
      this.availableOptionsFlowLayoutPanel.Location = new System.Drawing.Point(4, 3);
      this.availableOptionsFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.availableOptionsFlowLayoutPanel.Name = "availableOptionsFlowLayoutPanel";
      this.availableOptionsFlowLayoutPanel.Size = new System.Drawing.Size(1003, 439);
      this.availableOptionsFlowLayoutPanel.TabIndex = 0;
      // 
      // globalTabPage
      // 
      this.globalTabPage.Controls.Add(this.globalOptionConfig1);
      this.globalTabPage.Location = new System.Drawing.Point(4, 29);
      this.globalTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.globalTabPage.Name = "globalTabPage";
      this.globalTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.globalTabPage.Size = new System.Drawing.Size(1011, 445);
      this.globalTabPage.TabIndex = 0;
      this.globalTabPage.Text = "Global";
      this.globalTabPage.UseVisualStyleBackColor = true;
      // 
      // globalOptionConfig1
      // 
      this.globalOptionConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.globalOptionConfig1.Filter = "Global.%";
      this.globalOptionConfig1.Location = new System.Drawing.Point(4, 3);
      this.globalOptionConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.globalOptionConfig1.Name = "globalOptionConfig1";
      this.globalOptionConfig1.Size = new System.Drawing.Size(1003, 439);
      this.globalOptionConfig1.TabIndex = 0;
      // 
      // dataStructureTabPage
      // 
      this.dataStructureTabPage.Controls.Add(this.dataStructureOptionConfig1);
      this.dataStructureTabPage.Location = new System.Drawing.Point(4, 29);
      this.dataStructureTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.dataStructureTabPage.Name = "dataStructureTabPage";
      this.dataStructureTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.dataStructureTabPage.Size = new System.Drawing.Size(1011, 445);
      this.dataStructureTabPage.TabIndex = 0;
      this.dataStructureTabPage.Text = "DataStructure";
      this.dataStructureTabPage.UseVisualStyleBackColor = true;
      // 
      // dataStructureOptionConfig1
      // 
      this.dataStructureOptionConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataStructureOptionConfig1.Filter = "DataStructure.%";
      this.dataStructureOptionConfig1.Location = new System.Drawing.Point(4, 3);
      this.dataStructureOptionConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.dataStructureOptionConfig1.Name = "dataStructureOptionConfig1";
      this.dataStructureOptionConfig1.Size = new System.Drawing.Size(1003, 439);
      this.dataStructureOptionConfig1.TabIndex = 0;
      // 
      // displayTabPage
      // 
      this.displayTabPage.Controls.Add(this.dataStructureDisplayConfig1);
      this.displayTabPage.Location = new System.Drawing.Point(4, 29);
      this.displayTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.displayTabPage.Name = "displayTabPage";
      this.displayTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.displayTabPage.Size = new System.Drawing.Size(1011, 445);
      this.displayTabPage.TabIndex = 1;
      this.displayTabPage.Text = "Display";
      this.displayTabPage.UseVisualStyleBackColor = true;
      // 
      // dataStructureDisplayConfig1
      // 
      this.dataStructureDisplayConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataStructureDisplayConfig1.Location = new System.Drawing.Point(4, 3);
      this.dataStructureDisplayConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.dataStructureDisplayConfig1.Name = "dataStructureDisplayConfig1";
      this.dataStructureDisplayConfig1.Size = new System.Drawing.Size(1003, 439);
      this.dataStructureDisplayConfig1.TabIndex = 0;
      // 
      // analysisTabPage
      // 
      this.analysisTabPage.Controls.Add(this.analysisConfig1);
      this.analysisTabPage.Location = new System.Drawing.Point(4, 29);
      this.analysisTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.analysisTabPage.Name = "analysisTabPage";
      this.analysisTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.analysisTabPage.Size = new System.Drawing.Size(1011, 445);
      this.analysisTabPage.TabIndex = 2;
      this.analysisTabPage.Text = "Analysis";
      this.analysisTabPage.UseVisualStyleBackColor = true;
      // 
      // analysisConfig1
      // 
      this.analysisConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.analysisConfig1.Filter = "Analysis.%";
      this.analysisConfig1.Location = new System.Drawing.Point(4, 3);
      this.analysisConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.analysisConfig1.Name = "analysisConfig1";
      this.analysisConfig1.Size = new System.Drawing.Size(1003, 439);
      this.analysisConfig1.TabIndex = 0;
      // 
      // shiftTabPage
      // 
      this.shiftTabPage.Controls.Add(this.shiftConfig1);
      this.shiftTabPage.Location = new System.Drawing.Point(4, 29);
      this.shiftTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.shiftTabPage.Name = "shiftTabPage";
      this.shiftTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.shiftTabPage.Size = new System.Drawing.Size(1011, 445);
      this.shiftTabPage.TabIndex = 4;
      this.shiftTabPage.Text = "Shift";
      this.shiftTabPage.UseVisualStyleBackColor = true;
      // 
      // shiftConfig1
      // 
      this.shiftConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.shiftConfig1.Location = new System.Drawing.Point(4, 3);
      this.shiftConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.shiftConfig1.Name = "shiftConfig1";
      this.shiftConfig1.Size = new System.Drawing.Size(1003, 439);
      this.shiftConfig1.TabIndex = 0;
      // 
      // computerTabPage
      // 
      this.computerTabPage.Controls.Add(this.computerConfig);
      this.computerTabPage.Location = new System.Drawing.Point(4, 29);
      this.computerTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.computerTabPage.Name = "computerTabPage";
      this.computerTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.computerTabPage.Size = new System.Drawing.Size(1011, 445);
      this.computerTabPage.TabIndex = 5;
      this.computerTabPage.Text = "Computer";
      this.computerTabPage.UseVisualStyleBackColor = true;
      // 
      // computerConfig
      // 
      this.computerConfig.Dock = System.Windows.Forms.DockStyle.Fill;
      this.computerConfig.Location = new System.Drawing.Point(4, 3);
      this.computerConfig.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.computerConfig.Name = "computerConfig";
      this.computerConfig.Size = new System.Drawing.Size(1003, 439);
      this.computerConfig.TabIndex = 0;
      // 
      // shiftTemplateTabPage
      // 
      this.shiftTemplateTabPage.Controls.Add(this.shiftTemplateConfig1);
      this.shiftTemplateTabPage.Location = new System.Drawing.Point(4, 29);
      this.shiftTemplateTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.shiftTemplateTabPage.Name = "shiftTemplateTabPage";
      this.shiftTemplateTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.shiftTemplateTabPage.Size = new System.Drawing.Size(1011, 445);
      this.shiftTemplateTabPage.TabIndex = 6;
      this.shiftTemplateTabPage.Text = "Shift templates";
      this.shiftTemplateTabPage.UseVisualStyleBackColor = true;
      // 
      // shiftTemplateConfig1
      // 
      this.shiftTemplateConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.shiftTemplateConfig1.Location = new System.Drawing.Point(4, 3);
      this.shiftTemplateConfig1.Margin = new System.Windows.Forms.Padding(0);
      this.shiftTemplateConfig1.Name = "shiftTemplateConfig1";
      this.shiftTemplateConfig1.Size = new System.Drawing.Size(1003, 439);
      this.shiftTemplateConfig1.TabIndex = 0;
      // 
      // dayTemplateTabPage
      // 
      this.dayTemplateTabPage.Controls.Add(this.dayTemplateConfig1);
      this.dayTemplateTabPage.Location = new System.Drawing.Point(4, 29);
      this.dayTemplateTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.dayTemplateTabPage.Name = "dayTemplateTabPage";
      this.dayTemplateTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.dayTemplateTabPage.Size = new System.Drawing.Size(1011, 445);
      this.dayTemplateTabPage.TabIndex = 7;
      this.dayTemplateTabPage.Text = "Day templates";
      this.dayTemplateTabPage.UseVisualStyleBackColor = true;
      // 
      // dayTemplateConfig1
      // 
      this.dayTemplateConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dayTemplateConfig1.Location = new System.Drawing.Point(4, 3);
      this.dayTemplateConfig1.Margin = new System.Windows.Forms.Padding(0);
      this.dayTemplateConfig1.Name = "dayTemplateConfig1";
      this.dayTemplateConfig1.Size = new System.Drawing.Size(1003, 439);
      this.dayTemplateConfig1.TabIndex = 0;
      // 
      // machineConfigsTabPage
      // 
      this.machineConfigsTabPage.Controls.Add(this.machineTabControl);
      this.machineConfigsTabPage.Location = new System.Drawing.Point(4, 29);
      this.machineConfigsTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineConfigsTabPage.Name = "machineConfigsTabPage";
      this.machineConfigsTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineConfigsTabPage.Size = new System.Drawing.Size(1027, 484);
      this.machineConfigsTabPage.TabIndex = 2;
      this.machineConfigsTabPage.Text = "Machine";
      this.machineConfigsTabPage.UseVisualStyleBackColor = true;
      // 
      // machineTabControl
      // 
      this.machineTabControl.Controls.Add(this.companyTabPage);
      this.machineTabControl.Controls.Add(this.departmentTabPage);
      this.machineTabControl.Controls.Add(this.cellTabPage);
      this.machineTabControl.Controls.Add(this.machineCategoryTabPage);
      this.machineTabControl.Controls.Add(this.machineSubCategoryTabPage);
      this.machineTabControl.Controls.Add(this.machineTabPage);
      this.machineTabControl.Controls.Add(this.machineFilterTabPage);
      this.machineTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineTabControl.ItemSize = new System.Drawing.Size(100, 25);
      this.machineTabControl.Location = new System.Drawing.Point(4, 3);
      this.machineTabControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineTabControl.Name = "machineTabControl";
      this.machineTabControl.SelectedIndex = 0;
      this.machineTabControl.Size = new System.Drawing.Size(1019, 478);
      this.machineTabControl.TabIndex = 0;
      // 
      // companyTabPage
      // 
      this.companyTabPage.Controls.Add(this.companyConfig1);
      this.companyTabPage.Location = new System.Drawing.Point(4, 29);
      this.companyTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.companyTabPage.Name = "companyTabPage";
      this.companyTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.companyTabPage.Size = new System.Drawing.Size(1011, 445);
      this.companyTabPage.TabIndex = 0;
      this.companyTabPage.Text = "Company";
      this.companyTabPage.UseVisualStyleBackColor = true;
      // 
      // companyConfig1
      // 
      this.companyConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.companyConfig1.Location = new System.Drawing.Point(4, 3);
      this.companyConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.companyConfig1.Name = "companyConfig1";
      this.companyConfig1.Size = new System.Drawing.Size(1003, 439);
      this.companyConfig1.TabIndex = 0;
      // 
      // departmentTabPage
      // 
      this.departmentTabPage.Controls.Add(this.departmentConfig1);
      this.departmentTabPage.Location = new System.Drawing.Point(4, 29);
      this.departmentTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.departmentTabPage.Name = "departmentTabPage";
      this.departmentTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.departmentTabPage.Size = new System.Drawing.Size(1011, 445);
      this.departmentTabPage.TabIndex = 1;
      this.departmentTabPage.Text = "Department";
      this.departmentTabPage.UseVisualStyleBackColor = true;
      // 
      // departmentConfig1
      // 
      this.departmentConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.departmentConfig1.Location = new System.Drawing.Point(4, 3);
      this.departmentConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.departmentConfig1.Name = "departmentConfig1";
      this.departmentConfig1.Size = new System.Drawing.Size(1003, 439);
      this.departmentConfig1.TabIndex = 0;
      // 
      // cellTabPage
      // 
      this.cellTabPage.Controls.Add(this.cellConfig1);
      this.cellTabPage.Location = new System.Drawing.Point(4, 29);
      this.cellTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.cellTabPage.Name = "cellTabPage";
      this.cellTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.cellTabPage.Size = new System.Drawing.Size(1011, 445);
      this.cellTabPage.TabIndex = 6;
      this.cellTabPage.Text = "Cell";
      this.cellTabPage.UseVisualStyleBackColor = true;
      // 
      // cellConfig1
      // 
      this.cellConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.cellConfig1.Location = new System.Drawing.Point(4, 3);
      this.cellConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.cellConfig1.Name = "cellConfig1";
      this.cellConfig1.Size = new System.Drawing.Size(1003, 439);
      this.cellConfig1.TabIndex = 0;
      // 
      // machineCategoryTabPage
      // 
      this.machineCategoryTabPage.Controls.Add(this.machineCategoryConfig1);
      this.machineCategoryTabPage.Location = new System.Drawing.Point(4, 29);
      this.machineCategoryTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineCategoryTabPage.Name = "machineCategoryTabPage";
      this.machineCategoryTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineCategoryTabPage.Size = new System.Drawing.Size(1011, 445);
      this.machineCategoryTabPage.TabIndex = 2;
      this.machineCategoryTabPage.Text = "MachineCategory";
      this.machineCategoryTabPage.UseVisualStyleBackColor = true;
      // 
      // machineCategoryConfig1
      // 
      this.machineCategoryConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineCategoryConfig1.Location = new System.Drawing.Point(4, 3);
      this.machineCategoryConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.machineCategoryConfig1.Name = "machineCategoryConfig1";
      this.machineCategoryConfig1.Size = new System.Drawing.Size(1003, 439);
      this.machineCategoryConfig1.TabIndex = 0;
      // 
      // machineSubCategoryTabPage
      // 
      this.machineSubCategoryTabPage.Controls.Add(this.machineSubCategoryConfig1);
      this.machineSubCategoryTabPage.Location = new System.Drawing.Point(4, 29);
      this.machineSubCategoryTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineSubCategoryTabPage.Name = "machineSubCategoryTabPage";
      this.machineSubCategoryTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineSubCategoryTabPage.Size = new System.Drawing.Size(1011, 445);
      this.machineSubCategoryTabPage.TabIndex = 3;
      this.machineSubCategoryTabPage.Text = "MachineSubCategory";
      this.machineSubCategoryTabPage.UseVisualStyleBackColor = true;
      // 
      // machineSubCategoryConfig1
      // 
      this.machineSubCategoryConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineSubCategoryConfig1.Location = new System.Drawing.Point(4, 3);
      this.machineSubCategoryConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.machineSubCategoryConfig1.Name = "machineSubCategoryConfig1";
      this.machineSubCategoryConfig1.Size = new System.Drawing.Size(1003, 439);
      this.machineSubCategoryConfig1.TabIndex = 0;
      // 
      // machineTabPage
      // 
      this.machineTabPage.Controls.Add(this.machineConfig1);
      this.machineTabPage.Location = new System.Drawing.Point(4, 29);
      this.machineTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineTabPage.Name = "machineTabPage";
      this.machineTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineTabPage.Size = new System.Drawing.Size(1011, 445);
      this.machineTabPage.TabIndex = 4;
      this.machineTabPage.Text = "Machine";
      this.machineTabPage.UseVisualStyleBackColor = true;
      // 
      // machineConfig1
      // 
      this.machineConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineConfig1.Location = new System.Drawing.Point(4, 3);
      this.machineConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.machineConfig1.Name = "machineConfig1";
      this.machineConfig1.Size = new System.Drawing.Size(1003, 439);
      this.machineConfig1.TabIndex = 0;
      // 
      // machineFilterTabPage
      // 
      this.machineFilterTabPage.Controls.Add(this.machineFilterConfig1);
      this.machineFilterTabPage.Location = new System.Drawing.Point(4, 29);
      this.machineFilterTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineFilterTabPage.Name = "machineFilterTabPage";
      this.machineFilterTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineFilterTabPage.Size = new System.Drawing.Size(1011, 445);
      this.machineFilterTabPage.TabIndex = 5;
      this.machineFilterTabPage.Text = "MachineFilter";
      this.machineFilterTabPage.UseVisualStyleBackColor = true;
      // 
      // machineFilterConfig1
      // 
      this.machineFilterConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineFilterConfig1.Location = new System.Drawing.Point(4, 3);
      this.machineFilterConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.machineFilterConfig1.Name = "machineFilterConfig1";
      this.machineFilterConfig1.Size = new System.Drawing.Size(1003, 439);
      this.machineFilterConfig1.TabIndex = 0;
      // 
      // acquisitionTabPage
      // 
      this.acquisitionTabPage.Controls.Add(this.acquisitionTabControl);
      this.acquisitionTabPage.Location = new System.Drawing.Point(4, 29);
      this.acquisitionTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.acquisitionTabPage.Name = "acquisitionTabPage";
      this.acquisitionTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.acquisitionTabPage.Size = new System.Drawing.Size(1027, 484);
      this.acquisitionTabPage.TabIndex = 3;
      this.acquisitionTabPage.Text = "Acquisition";
      this.acquisitionTabPage.UseVisualStyleBackColor = true;
      // 
      // acquisitionTabControl
      // 
      this.acquisitionTabControl.Controls.Add(this.monitoredMachineTabPage);
      this.acquisitionTabControl.Controls.Add(this.machineModuleTabPage);
      this.acquisitionTabControl.Controls.Add(this.cncAcquisitionTabPage);
      this.acquisitionTabControl.Controls.Add(this.unitTabPage);
      this.acquisitionTabControl.Controls.Add(this.fieldTabPage);
      this.acquisitionTabControl.Controls.Add(this.cncConfigTabPage);
      this.acquisitionTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.acquisitionTabControl.ItemSize = new System.Drawing.Size(100, 25);
      this.acquisitionTabControl.Location = new System.Drawing.Point(4, 3);
      this.acquisitionTabControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.acquisitionTabControl.Name = "acquisitionTabControl";
      this.acquisitionTabControl.SelectedIndex = 0;
      this.acquisitionTabControl.Size = new System.Drawing.Size(1019, 478);
      this.acquisitionTabControl.TabIndex = 0;
      // 
      // monitoredMachineTabPage
      // 
      this.monitoredMachineTabPage.Controls.Add(this.monitoredMachineConfig1);
      this.monitoredMachineTabPage.Location = new System.Drawing.Point(4, 29);
      this.monitoredMachineTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.monitoredMachineTabPage.Name = "monitoredMachineTabPage";
      this.monitoredMachineTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.monitoredMachineTabPage.Size = new System.Drawing.Size(1011, 445);
      this.monitoredMachineTabPage.TabIndex = 0;
      this.monitoredMachineTabPage.Text = "MonitoredMachine";
      this.monitoredMachineTabPage.UseVisualStyleBackColor = true;
      // 
      // monitoredMachineConfig1
      // 
      this.monitoredMachineConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.monitoredMachineConfig1.Location = new System.Drawing.Point(4, 3);
      this.monitoredMachineConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.monitoredMachineConfig1.Name = "monitoredMachineConfig1";
      this.monitoredMachineConfig1.Size = new System.Drawing.Size(1003, 439);
      this.monitoredMachineConfig1.TabIndex = 0;
      // 
      // machineModuleTabPage
      // 
      this.machineModuleTabPage.Controls.Add(this.machineModuleConfig1);
      this.machineModuleTabPage.Location = new System.Drawing.Point(4, 29);
      this.machineModuleTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineModuleTabPage.Name = "machineModuleTabPage";
      this.machineModuleTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineModuleTabPage.Size = new System.Drawing.Size(1011, 445);
      this.machineModuleTabPage.TabIndex = 4;
      this.machineModuleTabPage.Text = "MachineModule";
      this.machineModuleTabPage.UseVisualStyleBackColor = true;
      // 
      // machineModuleConfig1
      // 
      this.machineModuleConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineModuleConfig1.Location = new System.Drawing.Point(4, 3);
      this.machineModuleConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.machineModuleConfig1.Name = "machineModuleConfig1";
      this.machineModuleConfig1.Size = new System.Drawing.Size(1003, 439);
      this.machineModuleConfig1.TabIndex = 0;
      // 
      // cncAcquisitionTabPage
      // 
      this.cncAcquisitionTabPage.Controls.Add(this.cncAcquisitionConfig1);
      this.cncAcquisitionTabPage.Location = new System.Drawing.Point(4, 29);
      this.cncAcquisitionTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.cncAcquisitionTabPage.Name = "cncAcquisitionTabPage";
      this.cncAcquisitionTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.cncAcquisitionTabPage.Size = new System.Drawing.Size(1011, 445);
      this.cncAcquisitionTabPage.TabIndex = 1;
      this.cncAcquisitionTabPage.Text = "CncAcquisition";
      this.cncAcquisitionTabPage.UseVisualStyleBackColor = true;
      // 
      // cncAcquisitionConfig1
      // 
      this.cncAcquisitionConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.cncAcquisitionConfig1.Location = new System.Drawing.Point(4, 3);
      this.cncAcquisitionConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.cncAcquisitionConfig1.Name = "cncAcquisitionConfig1";
      this.cncAcquisitionConfig1.Size = new System.Drawing.Size(1003, 439);
      this.cncAcquisitionConfig1.TabIndex = 0;
      // 
      // unitTabPage
      // 
      this.unitTabPage.Controls.Add(this.unitConfig1);
      this.unitTabPage.Location = new System.Drawing.Point(4, 29);
      this.unitTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.unitTabPage.Name = "unitTabPage";
      this.unitTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.unitTabPage.Size = new System.Drawing.Size(1011, 445);
      this.unitTabPage.TabIndex = 2;
      this.unitTabPage.Text = "Unit";
      this.unitTabPage.UseVisualStyleBackColor = true;
      // 
      // unitConfig1
      // 
      this.unitConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.unitConfig1.Location = new System.Drawing.Point(4, 3);
      this.unitConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.unitConfig1.Name = "unitConfig1";
      this.unitConfig1.Size = new System.Drawing.Size(1003, 439);
      this.unitConfig1.TabIndex = 0;
      // 
      // fieldTabPage
      // 
      this.fieldTabPage.Controls.Add(this.fieldConfig1);
      this.fieldTabPage.Location = new System.Drawing.Point(4, 29);
      this.fieldTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.fieldTabPage.Name = "fieldTabPage";
      this.fieldTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.fieldTabPage.Size = new System.Drawing.Size(1011, 445);
      this.fieldTabPage.TabIndex = 3;
      this.fieldTabPage.Text = "Field";
      this.fieldTabPage.UseVisualStyleBackColor = true;
      // 
      // fieldConfig1
      // 
      this.fieldConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.fieldConfig1.Location = new System.Drawing.Point(4, 3);
      this.fieldConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.fieldConfig1.Name = "fieldConfig1";
      this.fieldConfig1.Size = new System.Drawing.Size(1003, 439);
      this.fieldConfig1.TabIndex = 0;
      // 
      // cncConfigTabPage
      // 
      this.cncConfigTabPage.Controls.Add(this.cncConfig);
      this.cncConfigTabPage.Location = new System.Drawing.Point(4, 29);
      this.cncConfigTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.cncConfigTabPage.Name = "cncConfigTabPage";
      this.cncConfigTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.cncConfigTabPage.Size = new System.Drawing.Size(1011, 445);
      this.cncConfigTabPage.TabIndex = 5;
      this.cncConfigTabPage.Text = "Global settings";
      this.cncConfigTabPage.UseVisualStyleBackColor = true;
      // 
      // cncConfig
      // 
      this.cncConfig.Dock = System.Windows.Forms.DockStyle.Fill;
      this.cncConfig.Filter = "Cnc.%";
      this.cncConfig.Location = new System.Drawing.Point(4, 3);
      this.cncConfig.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.cncConfig.Name = "cncConfig";
      this.cncConfig.Size = new System.Drawing.Size(1003, 439);
      this.cncConfig.TabIndex = 0;
      // 
      // machineStatusTabPage
      // 
      this.machineStatusTabPage.Controls.Add(this.machineStatusTabControl);
      this.machineStatusTabPage.ImageKey = "MachineStatus_16x16.png";
      this.machineStatusTabPage.Location = new System.Drawing.Point(4, 29);
      this.machineStatusTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineStatusTabPage.Name = "machineStatusTabPage";
      this.machineStatusTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineStatusTabPage.Size = new System.Drawing.Size(1027, 484);
      this.machineStatusTabPage.TabIndex = 1;
      this.machineStatusTabPage.Text = "Machine status";
      this.machineStatusTabPage.UseVisualStyleBackColor = true;
      // 
      // machineStatusTabControl
      // 
      this.machineStatusTabControl.Controls.Add(this.machineModeTabPage);
      this.machineStatusTabControl.Controls.Add(this.machineObservationStateTabPage);
      this.machineStatusTabControl.Controls.Add(this.reasonGroupTabPage);
      this.machineStatusTabControl.Controls.Add(this.reasonTabPage);
      this.machineStatusTabControl.Controls.Add(this.reasonSelectionTabPage);
      this.machineStatusTabControl.Controls.Add(this.defaultReasonTabPage);
      this.machineStatusTabControl.Controls.Add(this.machineStateTemplateTab);
      this.machineStatusTabControl.Controls.Add(this.machineStateTemplateRightTab);
      this.machineStatusTabControl.Controls.Add(this.autoMachineStateTemplateTabPage);
      this.machineStatusTabControl.Controls.Add(this.productionStateTabPage);
      this.machineStatusTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineStatusTabControl.ItemSize = new System.Drawing.Size(100, 25);
      this.machineStatusTabControl.Location = new System.Drawing.Point(4, 3);
      this.machineStatusTabControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineStatusTabControl.Name = "machineStatusTabControl";
      this.machineStatusTabControl.SelectedIndex = 0;
      this.machineStatusTabControl.Size = new System.Drawing.Size(1019, 478);
      this.machineStatusTabControl.TabIndex = 0;
      // 
      // machineModeTabPage
      // 
      this.machineModeTabPage.Controls.Add(this.machineModeConfig1);
      this.machineModeTabPage.Location = new System.Drawing.Point(4, 29);
      this.machineModeTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineModeTabPage.Name = "machineModeTabPage";
      this.machineModeTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineModeTabPage.Size = new System.Drawing.Size(1011, 445);
      this.machineModeTabPage.TabIndex = 5;
      this.machineModeTabPage.Text = "MachineMode";
      this.machineModeTabPage.UseVisualStyleBackColor = true;
      // 
      // machineModeConfig1
      // 
      this.machineModeConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineModeConfig1.Location = new System.Drawing.Point(4, 3);
      this.machineModeConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.machineModeConfig1.Name = "machineModeConfig1";
      this.machineModeConfig1.Size = new System.Drawing.Size(1003, 439);
      this.machineModeConfig1.TabIndex = 0;
      // 
      // machineObservationStateTabPage
      // 
      this.machineObservationStateTabPage.Controls.Add(this.machineObservationStateConfig1);
      this.machineObservationStateTabPage.Location = new System.Drawing.Point(4, 29);
      this.machineObservationStateTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineObservationStateTabPage.Name = "machineObservationStateTabPage";
      this.machineObservationStateTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineObservationStateTabPage.Size = new System.Drawing.Size(1011, 445);
      this.machineObservationStateTabPage.TabIndex = 0;
      this.machineObservationStateTabPage.Text = "Machine Observation State";
      this.machineObservationStateTabPage.UseVisualStyleBackColor = true;
      // 
      // machineObservationStateConfig1
      // 
      this.machineObservationStateConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineObservationStateConfig1.Location = new System.Drawing.Point(4, 3);
      this.machineObservationStateConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.machineObservationStateConfig1.Name = "machineObservationStateConfig1";
      this.machineObservationStateConfig1.Size = new System.Drawing.Size(1003, 439);
      this.machineObservationStateConfig1.TabIndex = 0;
      // 
      // reasonGroupTabPage
      // 
      this.reasonGroupTabPage.Controls.Add(this.reasonGroupConfig1);
      this.reasonGroupTabPage.Location = new System.Drawing.Point(4, 29);
      this.reasonGroupTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.reasonGroupTabPage.Name = "reasonGroupTabPage";
      this.reasonGroupTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.reasonGroupTabPage.Size = new System.Drawing.Size(1011, 445);
      this.reasonGroupTabPage.TabIndex = 2;
      this.reasonGroupTabPage.Text = "ReasonGroup";
      this.reasonGroupTabPage.UseVisualStyleBackColor = true;
      // 
      // reasonGroupConfig1
      // 
      this.reasonGroupConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.reasonGroupConfig1.Location = new System.Drawing.Point(4, 3);
      this.reasonGroupConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.reasonGroupConfig1.Name = "reasonGroupConfig1";
      this.reasonGroupConfig1.Size = new System.Drawing.Size(1003, 439);
      this.reasonGroupConfig1.TabIndex = 0;
      // 
      // reasonTabPage
      // 
      this.reasonTabPage.Controls.Add(this.reasonConfig1);
      this.reasonTabPage.Location = new System.Drawing.Point(4, 29);
      this.reasonTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.reasonTabPage.Name = "reasonTabPage";
      this.reasonTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.reasonTabPage.Size = new System.Drawing.Size(1011, 445);
      this.reasonTabPage.TabIndex = 1;
      this.reasonTabPage.Text = "Reason";
      this.reasonTabPage.UseVisualStyleBackColor = true;
      // 
      // reasonConfig1
      // 
      this.reasonConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.reasonConfig1.Location = new System.Drawing.Point(4, 3);
      this.reasonConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.reasonConfig1.Name = "reasonConfig1";
      this.reasonConfig1.Size = new System.Drawing.Size(1003, 439);
      this.reasonConfig1.TabIndex = 0;
      // 
      // reasonSelectionTabPage
      // 
      this.reasonSelectionTabPage.Controls.Add(this.reasonSelectionConfig1);
      this.reasonSelectionTabPage.Location = new System.Drawing.Point(4, 29);
      this.reasonSelectionTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.reasonSelectionTabPage.Name = "reasonSelectionTabPage";
      this.reasonSelectionTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.reasonSelectionTabPage.Size = new System.Drawing.Size(1011, 445);
      this.reasonSelectionTabPage.TabIndex = 3;
      this.reasonSelectionTabPage.Text = "Reason selection";
      this.reasonSelectionTabPage.UseVisualStyleBackColor = true;
      // 
      // reasonSelectionConfig1
      // 
      this.reasonSelectionConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.reasonSelectionConfig1.Location = new System.Drawing.Point(4, 3);
      this.reasonSelectionConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.reasonSelectionConfig1.Name = "reasonSelectionConfig1";
      this.reasonSelectionConfig1.Size = new System.Drawing.Size(1003, 439);
      this.reasonSelectionConfig1.TabIndex = 0;
      // 
      // defaultReasonTabPage
      // 
      this.defaultReasonTabPage.Controls.Add(this.machineModeDefaultReasonConfig1);
      this.defaultReasonTabPage.Location = new System.Drawing.Point(4, 29);
      this.defaultReasonTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.defaultReasonTabPage.Name = "defaultReasonTabPage";
      this.defaultReasonTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.defaultReasonTabPage.Size = new System.Drawing.Size(1011, 445);
      this.defaultReasonTabPage.TabIndex = 4;
      this.defaultReasonTabPage.Text = "Default reason";
      this.defaultReasonTabPage.UseVisualStyleBackColor = true;
      // 
      // machineModeDefaultReasonConfig1
      // 
      this.machineModeDefaultReasonConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineModeDefaultReasonConfig1.Location = new System.Drawing.Point(4, 3);
      this.machineModeDefaultReasonConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.machineModeDefaultReasonConfig1.Name = "machineModeDefaultReasonConfig1";
      this.machineModeDefaultReasonConfig1.Size = new System.Drawing.Size(1003, 439);
      this.machineModeDefaultReasonConfig1.TabIndex = 0;
      // 
      // machineStateTemplateTab
      // 
      this.machineStateTemplateTab.Controls.Add(this.machineStateTemplateConfig1);
      this.machineStateTemplateTab.Location = new System.Drawing.Point(4, 29);
      this.machineStateTemplateTab.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineStateTemplateTab.Name = "machineStateTemplateTab";
      this.machineStateTemplateTab.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineStateTemplateTab.Size = new System.Drawing.Size(1011, 445);
      this.machineStateTemplateTab.TabIndex = 7;
      this.machineStateTemplateTab.Text = "MachineStateTemplateTab";
      this.machineStateTemplateTab.UseVisualStyleBackColor = true;
      // 
      // machineStateTemplateConfig1
      // 
      this.machineStateTemplateConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineStateTemplateConfig1.Location = new System.Drawing.Point(4, 3);
      this.machineStateTemplateConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.machineStateTemplateConfig1.Name = "machineStateTemplateConfig1";
      this.machineStateTemplateConfig1.Size = new System.Drawing.Size(1003, 439);
      this.machineStateTemplateConfig1.TabIndex = 0;
      // 
      // machineStateTemplateRightTab
      // 
      this.machineStateTemplateRightTab.Controls.Add(this.machineStateTemplateRightConfig1);
      this.machineStateTemplateRightTab.Location = new System.Drawing.Point(4, 29);
      this.machineStateTemplateRightTab.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineStateTemplateRightTab.Name = "machineStateTemplateRightTab";
      this.machineStateTemplateRightTab.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineStateTemplateRightTab.Size = new System.Drawing.Size(1011, 445);
      this.machineStateTemplateRightTab.TabIndex = 8;
      this.machineStateTemplateRightTab.Text = "MachineStateTemplate Right Tab";
      this.machineStateTemplateRightTab.UseVisualStyleBackColor = true;
      // 
      // machineStateTemplateRightConfig1
      // 
      this.machineStateTemplateRightConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineStateTemplateRightConfig1.Location = new System.Drawing.Point(4, 3);
      this.machineStateTemplateRightConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.machineStateTemplateRightConfig1.Name = "machineStateTemplateRightConfig1";
      this.machineStateTemplateRightConfig1.Size = new System.Drawing.Size(1003, 439);
      this.machineStateTemplateRightConfig1.TabIndex = 0;
      // 
      // autoMachineStateTemplateTabPage
      // 
      this.autoMachineStateTemplateTabPage.Controls.Add(this.autoMachineStateTemplateConfig1);
      this.autoMachineStateTemplateTabPage.Location = new System.Drawing.Point(4, 29);
      this.autoMachineStateTemplateTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.autoMachineStateTemplateTabPage.Name = "autoMachineStateTemplateTabPage";
      this.autoMachineStateTemplateTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.autoMachineStateTemplateTabPage.Size = new System.Drawing.Size(1011, 445);
      this.autoMachineStateTemplateTabPage.TabIndex = 9;
      this.autoMachineStateTemplateTabPage.Text = "tabPage1";
      this.autoMachineStateTemplateTabPage.UseVisualStyleBackColor = true;
      // 
      // autoMachineStateTemplateConfig1
      // 
      this.autoMachineStateTemplateConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.autoMachineStateTemplateConfig1.Location = new System.Drawing.Point(4, 3);
      this.autoMachineStateTemplateConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.autoMachineStateTemplateConfig1.Name = "autoMachineStateTemplateConfig1";
      this.autoMachineStateTemplateConfig1.Size = new System.Drawing.Size(1003, 439);
      this.autoMachineStateTemplateConfig1.TabIndex = 0;
      // 
      // productionStateTabPage
      // 
      this.productionStateTabPage.Controls.Add(this.productionStateConfig);
      this.productionStateTabPage.Location = new System.Drawing.Point(4, 29);
      this.productionStateTabPage.Name = "productionStateTabPage";
      this.productionStateTabPage.Padding = new System.Windows.Forms.Padding(3);
      this.productionStateTabPage.Size = new System.Drawing.Size(1011, 445);
      this.productionStateTabPage.TabIndex = 10;
      this.productionStateTabPage.Text = "Production state";
      this.productionStateTabPage.UseVisualStyleBackColor = true;
      // 
      // productionStateConfig
      // 
      this.productionStateConfig.Dock = System.Windows.Forms.DockStyle.Fill;
      this.productionStateConfig.Location = new System.Drawing.Point(3, 3);
      this.productionStateConfig.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.productionStateConfig.Name = "productionStateConfig";
      this.productionStateConfig.Size = new System.Drawing.Size(1005, 439);
      this.productionStateConfig.TabIndex = 0;
      // 
      // interfacesTabPage
      // 
      this.interfacesTabPage.Controls.Add(this.tabControl1);
      this.interfacesTabPage.Location = new System.Drawing.Point(4, 29);
      this.interfacesTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.interfacesTabPage.Name = "interfacesTabPage";
      this.interfacesTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.interfacesTabPage.Size = new System.Drawing.Size(1027, 484);
      this.interfacesTabPage.TabIndex = 4;
      this.interfacesTabPage.Text = "Interfaces";
      this.interfacesTabPage.UseVisualStyleBackColor = true;
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.fieldLegendTabPage);
      this.tabControl1.Controls.Add(this.guiConfigTabPage);
      this.tabControl1.Controls.Add(this.machineStateTemplateFlowTabPage);
      this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl1.ItemSize = new System.Drawing.Size(100, 25);
      this.tabControl1.Location = new System.Drawing.Point(4, 3);
      this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(1019, 478);
      this.tabControl1.TabIndex = 0;
      // 
      // fieldLegendTabPage
      // 
      this.fieldLegendTabPage.Controls.Add(this.fieldLegendConfig1);
      this.fieldLegendTabPage.Location = new System.Drawing.Point(4, 29);
      this.fieldLegendTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.fieldLegendTabPage.Name = "fieldLegendTabPage";
      this.fieldLegendTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.fieldLegendTabPage.Size = new System.Drawing.Size(1011, 445);
      this.fieldLegendTabPage.TabIndex = 0;
      this.fieldLegendTabPage.Text = "Field legend";
      this.fieldLegendTabPage.UseVisualStyleBackColor = true;
      // 
      // fieldLegendConfig1
      // 
      this.fieldLegendConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.fieldLegendConfig1.Location = new System.Drawing.Point(4, 3);
      this.fieldLegendConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.fieldLegendConfig1.Name = "fieldLegendConfig1";
      this.fieldLegendConfig1.Size = new System.Drawing.Size(1003, 439);
      this.fieldLegendConfig1.TabIndex = 1;
      // 
      // guiConfigTabPage
      // 
      this.guiConfigTabPage.Controls.Add(this.guiConfigConfig);
      this.guiConfigTabPage.Location = new System.Drawing.Point(4, 29);
      this.guiConfigTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.guiConfigTabPage.Name = "guiConfigTabPage";
      this.guiConfigTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.guiConfigTabPage.Size = new System.Drawing.Size(1011, 445);
      this.guiConfigTabPage.TabIndex = 1;
      this.guiConfigTabPage.Text = "guiConfigTabPage";
      this.guiConfigTabPage.UseVisualStyleBackColor = true;
      // 
      // guiConfigConfig
      // 
      this.guiConfigConfig.Dock = System.Windows.Forms.DockStyle.Fill;
      this.guiConfigConfig.Filter = "Gui.%";
      this.guiConfigConfig.Location = new System.Drawing.Point(4, 3);
      this.guiConfigConfig.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.guiConfigConfig.Name = "guiConfigConfig";
      this.guiConfigConfig.Size = new System.Drawing.Size(1003, 439);
      this.guiConfigConfig.TabIndex = 0;
      // 
      // machineStateTemplateFlowTabPage
      // 
      this.machineStateTemplateFlowTabPage.Controls.Add(this.machineStateTemplateFlowConfig);
      this.machineStateTemplateFlowTabPage.Location = new System.Drawing.Point(4, 29);
      this.machineStateTemplateFlowTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineStateTemplateFlowTabPage.Name = "machineStateTemplateFlowTabPage";
      this.machineStateTemplateFlowTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.machineStateTemplateFlowTabPage.Size = new System.Drawing.Size(1011, 445);
      this.machineStateTemplateFlowTabPage.TabIndex = 2;
      this.machineStateTemplateFlowTabPage.Text = "MachineStateTemplateFlow";
      this.machineStateTemplateFlowTabPage.UseVisualStyleBackColor = true;
      // 
      // machineStateTemplateFlowConfig
      // 
      this.machineStateTemplateFlowConfig.Dock = System.Windows.Forms.DockStyle.Fill;
      this.machineStateTemplateFlowConfig.Location = new System.Drawing.Point(4, 3);
      this.machineStateTemplateFlowConfig.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.machineStateTemplateFlowConfig.Name = "machineStateTemplateFlowConfig";
      this.machineStateTemplateFlowConfig.Size = new System.Drawing.Size(1003, 439);
      this.machineStateTemplateFlowConfig.TabIndex = 0;
      // 
      // evenTabPage
      // 
      this.evenTabPage.Controls.Add(this.eventTabControl);
      this.evenTabPage.Location = new System.Drawing.Point(4, 29);
      this.evenTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.evenTabPage.Name = "evenTabPage";
      this.evenTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.evenTabPage.Size = new System.Drawing.Size(1027, 484);
      this.evenTabPage.TabIndex = 5;
      this.evenTabPage.Text = "Event";
      this.evenTabPage.UseVisualStyleBackColor = true;
      // 
      // eventTabControl
      // 
      this.eventTabControl.Controls.Add(this.eventTabPage);
      this.eventTabControl.Controls.Add(this.eventLongPeriodConfigTabPage);
      this.eventTabControl.Controls.Add(this.eventCnCValueConfigTabPage);
      this.eventTabControl.Controls.Add(this.eventToolLifeConfigTabPage);
      this.eventTabControl.Controls.Add(this.eventSMTPTabPage);
      this.eventTabControl.Controls.Add(this.eventEmailConfigTabPage);
      this.eventTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.eventTabControl.ItemSize = new System.Drawing.Size(100, 25);
      this.eventTabControl.Location = new System.Drawing.Point(4, 3);
      this.eventTabControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.eventTabControl.Name = "eventTabControl";
      this.eventTabControl.SelectedIndex = 0;
      this.eventTabControl.Size = new System.Drawing.Size(1019, 478);
      this.eventTabControl.TabIndex = 0;
      // 
      // eventTabPage
      // 
      this.eventTabPage.Controls.Add(this.eventLevelConfig);
      this.eventTabPage.Location = new System.Drawing.Point(4, 29);
      this.eventTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.eventTabPage.Name = "eventTabPage";
      this.eventTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.eventTabPage.Size = new System.Drawing.Size(1011, 445);
      this.eventTabPage.TabIndex = 0;
      this.eventTabPage.Text = "Event";
      this.eventTabPage.UseVisualStyleBackColor = true;
      // 
      // eventLevelConfig
      // 
      this.eventLevelConfig.Dock = System.Windows.Forms.DockStyle.Fill;
      this.eventLevelConfig.Location = new System.Drawing.Point(4, 3);
      this.eventLevelConfig.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.eventLevelConfig.Name = "eventLevelConfig";
      this.eventLevelConfig.Size = new System.Drawing.Size(1003, 439);
      this.eventLevelConfig.TabIndex = 0;
      // 
      // eventLongPeriodConfigTabPage
      // 
      this.eventLongPeriodConfigTabPage.Controls.Add(this.eventLongPeriodConfigConfig1);
      this.eventLongPeriodConfigTabPage.Location = new System.Drawing.Point(4, 29);
      this.eventLongPeriodConfigTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.eventLongPeriodConfigTabPage.Name = "eventLongPeriodConfigTabPage";
      this.eventLongPeriodConfigTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.eventLongPeriodConfigTabPage.Size = new System.Drawing.Size(1011, 445);
      this.eventLongPeriodConfigTabPage.TabIndex = 2;
      this.eventLongPeriodConfigTabPage.Text = "LongPeriodConfig";
      this.eventLongPeriodConfigTabPage.UseVisualStyleBackColor = true;
      // 
      // eventLongPeriodConfigConfig1
      // 
      this.eventLongPeriodConfigConfig1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.eventLongPeriodConfigConfig1.Location = new System.Drawing.Point(4, 3);
      this.eventLongPeriodConfigConfig1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.eventLongPeriodConfigConfig1.Name = "eventLongPeriodConfigConfig1";
      this.eventLongPeriodConfigConfig1.Size = new System.Drawing.Size(1003, 439);
      this.eventLongPeriodConfigConfig1.TabIndex = 0;
      // 
      // eventCnCValueConfigTabPage
      // 
      this.eventCnCValueConfigTabPage.Controls.Add(this.eventCncValueConfigConfig);
      this.eventCnCValueConfigTabPage.Location = new System.Drawing.Point(4, 29);
      this.eventCnCValueConfigTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.eventCnCValueConfigTabPage.Name = "eventCnCValueConfigTabPage";
      this.eventCnCValueConfigTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.eventCnCValueConfigTabPage.Size = new System.Drawing.Size(1011, 445);
      this.eventCnCValueConfigTabPage.TabIndex = 4;
      this.eventCnCValueConfigTabPage.Text = "CnCValueConfig";
      this.eventCnCValueConfigTabPage.UseVisualStyleBackColor = true;
      // 
      // eventCncValueConfigConfig
      // 
      this.eventCncValueConfigConfig.Dock = System.Windows.Forms.DockStyle.Fill;
      this.eventCncValueConfigConfig.Location = new System.Drawing.Point(4, 3);
      this.eventCncValueConfigConfig.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.eventCncValueConfigConfig.Name = "eventCncValueConfigConfig";
      this.eventCncValueConfigConfig.Size = new System.Drawing.Size(1003, 439);
      this.eventCncValueConfigConfig.TabIndex = 0;
      // 
      // eventToolLifeConfigTabPage
      // 
      this.eventToolLifeConfigTabPage.Controls.Add(this.eventToolLifeConfigConfig);
      this.eventToolLifeConfigTabPage.Location = new System.Drawing.Point(4, 29);
      this.eventToolLifeConfigTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.eventToolLifeConfigTabPage.Name = "eventToolLifeConfigTabPage";
      this.eventToolLifeConfigTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.eventToolLifeConfigTabPage.Size = new System.Drawing.Size(1011, 445);
      this.eventToolLifeConfigTabPage.TabIndex = 4;
      this.eventToolLifeConfigTabPage.Text = "ToolLifeConfig";
      this.eventToolLifeConfigTabPage.UseVisualStyleBackColor = true;
      // 
      // eventToolLifeConfigConfig
      // 
      this.eventToolLifeConfigConfig.Dock = System.Windows.Forms.DockStyle.Fill;
      this.eventToolLifeConfigConfig.Location = new System.Drawing.Point(4, 3);
      this.eventToolLifeConfigConfig.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.eventToolLifeConfigConfig.Name = "eventToolLifeConfigConfig";
      this.eventToolLifeConfigConfig.Size = new System.Drawing.Size(1003, 439);
      this.eventToolLifeConfigConfig.TabIndex = 0;
      // 
      // eventSMTPTabPage
      // 
      this.eventSMTPTabPage.Controls.Add(this.smtpStatusStrip);
      this.eventSMTPTabPage.Controls.Add(this.smtpConfig);
      this.eventSMTPTabPage.Location = new System.Drawing.Point(4, 29);
      this.eventSMTPTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.eventSMTPTabPage.Name = "eventSMTPTabPage";
      this.eventSMTPTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.eventSMTPTabPage.Size = new System.Drawing.Size(1011, 445);
      this.eventSMTPTabPage.TabIndex = 5;
      this.eventSMTPTabPage.Text = "SMTPConfig";
      this.eventSMTPTabPage.UseVisualStyleBackColor = true;
      // 
      // smtpStatusStrip
      // 
      this.smtpStatusStrip.CausesValidation = false;
      this.smtpStatusStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.smtpStatusStrip.Location = new System.Drawing.Point(4, 417);
      this.smtpStatusStrip.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.smtpStatusStrip.Name = "smtpStatusStrip";
      this.smtpStatusStrip.Size = new System.Drawing.Size(1003, 25);
      this.smtpStatusStrip.TabIndex = 1;
      // 
      // smtpConfig
      // 
      this.smtpConfig.Dock = System.Windows.Forms.DockStyle.Fill;
      this.smtpConfig.Filter = "net.mail.%";
      this.smtpConfig.Location = new System.Drawing.Point(4, 3);
      this.smtpConfig.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.smtpConfig.Name = "smtpConfig";
      this.smtpConfig.Size = new System.Drawing.Size(1003, 439);
      this.smtpConfig.TabIndex = 0;
      // 
      // eventEmailConfigTabPage
      // 
      this.eventEmailConfigTabPage.Controls.Add(this.emailConfigConfig);
      this.eventEmailConfigTabPage.Location = new System.Drawing.Point(4, 29);
      this.eventEmailConfigTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.eventEmailConfigTabPage.Name = "eventEmailConfigTabPage";
      this.eventEmailConfigTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.eventEmailConfigTabPage.Size = new System.Drawing.Size(1011, 445);
      this.eventEmailConfigTabPage.TabIndex = 6;
      this.eventEmailConfigTabPage.Text = "EmailConfig";
      this.eventEmailConfigTabPage.UseVisualStyleBackColor = true;
      // 
      // emailConfigConfig
      // 
      this.emailConfigConfig.Dock = System.Windows.Forms.DockStyle.Fill;
      this.emailConfigConfig.Location = new System.Drawing.Point(4, 3);
      this.emailConfigConfig.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.emailConfigConfig.Name = "emailConfigConfig";
      this.emailConfigConfig.Size = new System.Drawing.Size(1003, 439);
      this.emailConfigConfig.TabIndex = 0;
      // 
      // qualityTabPage
      // 
      this.qualityTabPage.Controls.Add(this.qualityTabControl);
      this.qualityTabPage.Location = new System.Drawing.Point(4, 29);
      this.qualityTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.qualityTabPage.Name = "qualityTabPage";
      this.qualityTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.qualityTabPage.Size = new System.Drawing.Size(1027, 484);
      this.qualityTabPage.TabIndex = 6;
      this.qualityTabPage.Text = "Quality";
      this.qualityTabPage.UseVisualStyleBackColor = true;
      // 
      // qualityTabControl
      // 
      this.qualityTabControl.Controls.Add(this.nonConformanceReasonTabPage);
      this.qualityTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.qualityTabControl.Location = new System.Drawing.Point(4, 3);
      this.qualityTabControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.qualityTabControl.Name = "qualityTabControl";
      this.qualityTabControl.SelectedIndex = 0;
      this.qualityTabControl.Size = new System.Drawing.Size(1019, 478);
      this.qualityTabControl.TabIndex = 0;
      // 
      // nonConformanceReasonTabPage
      // 
      this.nonConformanceReasonTabPage.Controls.Add(this.nonConformanceReasonConfig);
      this.nonConformanceReasonTabPage.Location = new System.Drawing.Point(4, 24);
      this.nonConformanceReasonTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.nonConformanceReasonTabPage.Name = "nonConformanceReasonTabPage";
      this.nonConformanceReasonTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.nonConformanceReasonTabPage.Size = new System.Drawing.Size(1011, 450);
      this.nonConformanceReasonTabPage.TabIndex = 0;
      this.nonConformanceReasonTabPage.Text = "NonConformanceReason";
      this.nonConformanceReasonTabPage.UseVisualStyleBackColor = true;
      // 
      // nonConformanceReasonConfig
      // 
      this.nonConformanceReasonConfig.Dock = System.Windows.Forms.DockStyle.Fill;
      this.nonConformanceReasonConfig.Location = new System.Drawing.Point(4, 3);
      this.nonConformanceReasonConfig.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.nonConformanceReasonConfig.Name = "nonConformanceReasonConfig";
      this.nonConformanceReasonConfig.Size = new System.Drawing.Size(1003, 444);
      this.nonConformanceReasonConfig.TabIndex = 0;
      // 
      // mainUserTabPage
      // 
      this.mainUserTabPage.Controls.Add(this.mainUserTabControl);
      this.mainUserTabPage.Location = new System.Drawing.Point(4, 29);
      this.mainUserTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.mainUserTabPage.Name = "mainUserTabPage";
      this.mainUserTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.mainUserTabPage.Size = new System.Drawing.Size(1027, 484);
      this.mainUserTabPage.TabIndex = 7;
      this.mainUserTabPage.Text = "User";
      this.mainUserTabPage.UseVisualStyleBackColor = true;
      // 
      // mainUserTabControl
      // 
      this.mainUserTabControl.Controls.Add(this.userTabPage);
      this.mainUserTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.mainUserTabControl.Location = new System.Drawing.Point(4, 3);
      this.mainUserTabControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.mainUserTabControl.Name = "mainUserTabControl";
      this.mainUserTabControl.SelectedIndex = 0;
      this.mainUserTabControl.Size = new System.Drawing.Size(1019, 478);
      this.mainUserTabControl.TabIndex = 0;
      // 
      // userTabPage
      // 
      this.userTabPage.Controls.Add(this.userConfig);
      this.userTabPage.Location = new System.Drawing.Point(4, 24);
      this.userTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.userTabPage.Name = "userTabPage";
      this.userTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.userTabPage.Size = new System.Drawing.Size(1011, 450);
      this.userTabPage.TabIndex = 0;
      this.userTabPage.Text = "User";
      this.userTabPage.UseVisualStyleBackColor = true;
      // 
      // userConfig
      // 
      this.userConfig.CompanyId = 0;
      this.userConfig.Dock = System.Windows.Forms.DockStyle.Fill;
      this.userConfig.Location = new System.Drawing.Point(4, 3);
      this.userConfig.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.userConfig.Name = "userConfig";
      this.userConfig.Size = new System.Drawing.Size(1003, 444);
      this.userConfig.TabIndex = 0;
      // 
      // IconTabCont
      // 
      this.IconTabCont.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
      this.IconTabCont.ImageSize = new System.Drawing.Size(16, 16);
      this.IconTabCont.TransparentColor = System.Drawing.Color.Transparent;
      // 
      // stampingTabPage
      // 
      this.stampingTabPage.Controls.Add(this.stampingTabControl);
      this.stampingTabPage.Location = new System.Drawing.Point(4, 29);
      this.stampingTabPage.Name = "stampingTabPage";
      this.stampingTabPage.Size = new System.Drawing.Size(1027, 484);
      this.stampingTabPage.TabIndex = 8;
      this.stampingTabPage.Text = "Stamping";
      this.stampingTabPage.UseVisualStyleBackColor = true;
      // 
      // stampingTabControl
      // 
      this.stampingTabControl.Controls.Add(this.stampingParametersTabPage);
      this.stampingTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.stampingTabControl.Location = new System.Drawing.Point(0, 0);
      this.stampingTabControl.Name = "stampingTabControl";
      this.stampingTabControl.SelectedIndex = 0;
      this.stampingTabControl.Size = new System.Drawing.Size(1027, 484);
      this.stampingTabControl.TabIndex = 0;
      // 
      // stampingParametersTabPage
      // 
      this.stampingParametersTabPage.Controls.Add (this.stampingParametersConfig);
      this.stampingParametersTabPage.Location = new System.Drawing.Point(4, 24);
      this.stampingParametersTabPage.Name = "stampingParametersTabPage";
      this.stampingParametersTabPage.Padding = new System.Windows.Forms.Padding(3);
      this.stampingParametersTabPage.Size = new System.Drawing.Size(1019, 456);
      this.stampingParametersTabPage.TabIndex = 0;
      this.stampingParametersTabPage.Text = "Parameters";
      this.stampingParametersTabPage.UseVisualStyleBackColor = true;
      // 
      // stampingParametersConfig
      // 
      this.stampingParametersConfig.Dock = System.Windows.Forms.DockStyle.Fill;
      this.stampingParametersConfig.Filter = "Stamping.%";
      this.stampingParametersConfig.Location = new System.Drawing.Point (4, 3);
      this.stampingParametersConfig.Margin = new System.Windows.Forms.Padding (5, 3, 5, 3);
      this.stampingParametersConfig.Name = "stampingParametersConfig";
      this.stampingParametersConfig.Size = new System.Drawing.Size (1003, 439);
      this.stampingParametersConfig.TabIndex = 0;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1035, 517);
      this.Controls.Add(this.mainTabCtrl);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.Name = "MainForm";
      this.Text = "Lem_Configuration";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
      this.Load += new System.EventHandler(this.MainFormLoad);
      this.mainTabCtrl.ResumeLayout(false);
      this.generalTabPage.ResumeLayout(false);
      this.generalTabCtrl.ResumeLayout(false);
      this.availableOptionsTabPage.ResumeLayout(false);
      this.globalTabPage.ResumeLayout(false);
      this.dataStructureTabPage.ResumeLayout(false);
      this.displayTabPage.ResumeLayout(false);
      this.analysisTabPage.ResumeLayout(false);
      this.shiftTabPage.ResumeLayout(false);
      this.computerTabPage.ResumeLayout(false);
      this.shiftTemplateTabPage.ResumeLayout(false);
      this.dayTemplateTabPage.ResumeLayout(false);
      this.machineConfigsTabPage.ResumeLayout(false);
      this.machineTabControl.ResumeLayout(false);
      this.companyTabPage.ResumeLayout(false);
      this.departmentTabPage.ResumeLayout(false);
      this.cellTabPage.ResumeLayout(false);
      this.machineCategoryTabPage.ResumeLayout(false);
      this.machineSubCategoryTabPage.ResumeLayout(false);
      this.machineTabPage.ResumeLayout(false);
      this.machineFilterTabPage.ResumeLayout(false);
      this.acquisitionTabPage.ResumeLayout(false);
      this.acquisitionTabControl.ResumeLayout(false);
      this.monitoredMachineTabPage.ResumeLayout(false);
      this.machineModuleTabPage.ResumeLayout(false);
      this.cncAcquisitionTabPage.ResumeLayout(false);
      this.unitTabPage.ResumeLayout(false);
      this.fieldTabPage.ResumeLayout(false);
      this.cncConfigTabPage.ResumeLayout(false);
      this.machineStatusTabPage.ResumeLayout(false);
      this.machineStatusTabControl.ResumeLayout(false);
      this.machineModeTabPage.ResumeLayout(false);
      this.machineObservationStateTabPage.ResumeLayout(false);
      this.reasonGroupTabPage.ResumeLayout(false);
      this.reasonTabPage.ResumeLayout(false);
      this.reasonSelectionTabPage.ResumeLayout(false);
      this.defaultReasonTabPage.ResumeLayout(false);
      this.machineStateTemplateTab.ResumeLayout(false);
      this.machineStateTemplateRightTab.ResumeLayout(false);
      this.autoMachineStateTemplateTabPage.ResumeLayout(false);
      this.productionStateTabPage.ResumeLayout(false);
      this.interfacesTabPage.ResumeLayout(false);
      this.tabControl1.ResumeLayout(false);
      this.fieldLegendTabPage.ResumeLayout(false);
      this.guiConfigTabPage.ResumeLayout(false);
      this.machineStateTemplateFlowTabPage.ResumeLayout(false);
      this.evenTabPage.ResumeLayout(false);
      this.eventTabControl.ResumeLayout(false);
      this.eventTabPage.ResumeLayout(false);
      this.eventLongPeriodConfigTabPage.ResumeLayout(false);
      this.eventCnCValueConfigTabPage.ResumeLayout(false);
      this.eventToolLifeConfigTabPage.ResumeLayout(false);
      this.eventSMTPTabPage.ResumeLayout(false);
      this.eventEmailConfigTabPage.ResumeLayout(false);
      this.qualityTabPage.ResumeLayout(false);
      this.qualityTabControl.ResumeLayout(false);
      this.nonConformanceReasonTabPage.ResumeLayout(false);
      this.mainUserTabPage.ResumeLayout(false);
      this.mainUserTabControl.ResumeLayout(false);
      this.userTabPage.ResumeLayout(false);
      this.stampingTabPage.ResumeLayout(false);
      this.stampingTabControl.ResumeLayout(false);
      this.stampingParametersConfig.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private Lemoine.ConfigControls.ProductionStateConfig productionStateConfig;
    private System.Windows.Forms.TabPage productionStateTabPage;
    private Lemoine.ConfigControls.DayTemplateConfig dayTemplateConfig1;
    private Lemoine.ConfigControls.ShiftTemplateConfig shiftTemplateConfig1;
    private System.Windows.Forms.TabPage dayTemplateTabPage;
    private System.Windows.Forms.TabPage shiftTemplateTabPage;
    private Lemoine.ConfigControls.AutoMachineStateTemplateConfig autoMachineStateTemplateConfig1;
    private System.Windows.Forms.TabPage autoMachineStateTemplateTabPage;
    private Lemoine.ConfigControls.MachineStateTemplateFlowConfig machineStateTemplateFlowConfig;
    private System.Windows.Forms.TabPage machineStateTemplateFlowTabPage;
    private Lemoine.ConfigControls.SMTPStatusStrip smtpStatusStrip;
    private Lemoine.ConfigControls.UserConfig userConfig;
    private System.Windows.Forms.TabPage userTabPage;
    private System.Windows.Forms.TabControl mainUserTabControl;
    private System.Windows.Forms.TabPage mainUserTabPage;
    private Lemoine.ConfigControls.NonConformanceReasonConfig nonConformanceReasonConfig;
    private Lemoine.ConfigControls.ComputerConfig computerConfig;
    private System.Windows.Forms.TabPage nonConformanceReasonTabPage;
    private System.Windows.Forms.TabControl qualityTabControl;
    private System.Windows.Forms.TabPage qualityTabPage;
    private System.Windows.Forms.TabPage computerTabPage;
    private Lemoine.ConfigControls.ConfigConfig guiConfigConfig;
    private System.Windows.Forms.TabPage guiConfigTabPage;
    private Lemoine.ConfigControls.EmailConfigConfig emailConfigConfig;
    private System.Windows.Forms.TabPage eventEmailConfigTabPage;
    private Lemoine.ConfigControls.ConfigConfig smtpConfig;
    private System.Windows.Forms.TabPage eventSMTPTabPage;
    private Lemoine.ConfigControls.EventCncValueConfigConfig eventCncValueConfigConfig;
    private System.Windows.Forms.TabPage eventCnCValueConfigTabPage;
    private Lemoine.ConfigControls.EventToolLifeConfigConfig eventToolLifeConfigConfig;
    private System.Windows.Forms.TabPage eventToolLifeConfigTabPage;
    private Lemoine.ConfigControls.EventLongPeriodConfigConfig eventLongPeriodConfigConfig1;
    private System.Windows.Forms.TabPage eventLongPeriodConfigTabPage;
    private Lemoine.ConfigControls.EventLevelConfig eventLevelConfig;
    private System.Windows.Forms.TabPage eventTabPage;
    private System.Windows.Forms.TabControl eventTabControl;
    private System.Windows.Forms.TabPage evenTabPage;
    private Lemoine.ConfigControls.MachineStateTemplateRightConfig machineStateTemplateRightConfig1;
    private System.Windows.Forms.TabPage machineStateTemplateRightTab;
    private Lemoine.ConfigControls.MachineStateTemplateConfig machineStateTemplateConfig1;
    private System.Windows.Forms.TabPage machineStateTemplateTab;
    private Lemoine.ConfigControls.CellConfig cellConfig1;
    private System.Windows.Forms.TabPage cellTabPage;
    private Lemoine.ConfigControls.ConfigConfig cncConfig;
    private System.Windows.Forms.TabPage cncConfigTabPage;
    private Lemoine.ConfigControls.MachineModuleConfig machineModuleConfig1;
    private System.Windows.Forms.TabPage machineModuleTabPage;
    private Lemoine.ConfigControls.ShiftConfig shiftConfig1;
    private System.Windows.Forms.TabPage shiftTabPage;
    private Lemoine.ConfigControls.MachineModeConfig machineModeConfig1;
    private System.Windows.Forms.TabPage machineModeTabPage;
    private Lemoine.ConfigControls.FieldLegendConfig fieldLegendConfig1;
    private System.Windows.Forms.TabPage fieldLegendTabPage;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage interfacesTabPage;
    private Lemoine.ConfigControls.FieldConfig fieldConfig1;
    private System.Windows.Forms.TabPage fieldTabPage;
    private Lemoine.ConfigControls.UnitConfig unitConfig1;
    private System.Windows.Forms.TabPage unitTabPage;
    private Lemoine.ConfigControls.CncAcquisitionConfig cncAcquisitionConfig1;
    private Lemoine.ConfigControls.MonitoredMachineConfig monitoredMachineConfig1;
    private System.Windows.Forms.TabPage cncAcquisitionTabPage;
    private System.Windows.Forms.TabPage monitoredMachineTabPage;
    private System.Windows.Forms.TabControl acquisitionTabControl;
    private System.Windows.Forms.TabPage acquisitionTabPage;
    private System.Windows.Forms.TabPage reasonGroupTabPage;
    private System.Windows.Forms.TabPage reasonTabPage;
    private Lemoine.ConfigControls.MachineFilterConfig machineFilterConfig1;
    private System.Windows.Forms.TabPage machineFilterTabPage;
    private Lemoine.ConfigControls.MachineConfig machineConfig1;
    private System.Windows.Forms.TabPage machineTabPage;
    private Lemoine.ConfigControls.MachineSubCategoryConfig machineSubCategoryConfig1;
    private Lemoine.ConfigControls.MachineCategoryConfig machineCategoryConfig1;
    private System.Windows.Forms.TabPage machineSubCategoryTabPage;
    private System.Windows.Forms.TabPage machineCategoryTabPage;
    private Lemoine.ConfigControls.CompanyConfig companyConfig1;
    private Lemoine.ConfigControls.DepartmentConfig departmentConfig1;
    private System.Windows.Forms.TabPage departmentTabPage;
    private System.Windows.Forms.TabPage companyTabPage;
    private System.Windows.Forms.TabControl machineTabControl;
    private System.Windows.Forms.TabPage machineConfigsTabPage;
    private System.Windows.Forms.FlowLayoutPanel availableOptionsFlowLayoutPanel;
    private System.Windows.Forms.TabPage availableOptionsTabPage;
    private Lemoine.ConfigControls.MachineModeDefaultReasonConfig machineModeDefaultReasonConfig1;
    private System.Windows.Forms.TabPage defaultReasonTabPage;
    private Lemoine.ConfigControls.ReasonSelectionConfig reasonSelectionConfig1;
    private System.Windows.Forms.TabPage reasonSelectionTabPage;
    private Lemoine.ConfigControls.ReasonConfig reasonConfig1;
    private Lemoine.ConfigControls.ReasonGroupConfig reasonGroupConfig1;
    private Lemoine.ConfigControls.MachineObservationStateConfig machineObservationStateConfig1;
    private System.Windows.Forms.TabPage machineObservationStateTabPage;
    private System.Windows.Forms.TabControl machineStatusTabControl;
    private System.Windows.Forms.TabPage machineStatusTabPage;
    private Lemoine.ConfigControls.ConfigConfig analysisConfig1;
    private System.Windows.Forms.TabControl generalTabCtrl;
    private System.Windows.Forms.TabPage analysisTabPage;
    private Lemoine.ConfigControls.DataStructureDisplayConfig dataStructureDisplayConfig1;
    private Lemoine.ConfigControls.ConfigConfig dataStructureOptionConfig1;
    private Lemoine.ConfigControls.ConfigConfig globalOptionConfig1;
    private System.Windows.Forms.TabPage displayTabPage;
    private System.Windows.Forms.TabPage dataStructureTabPage;
    private System.Windows.Forms.TabPage globalTabPage;
    private System.Windows.Forms.TabControl mainTabCtrl;
    private System.Windows.Forms.TabPage generalTabPage;
    private System.Windows.Forms.ImageList IconTabCont;
    private System.Windows.Forms.TabPage stampingTabPage;
    private System.Windows.Forms.TabControl stampingTabControl;
    private System.Windows.Forms.TabPage stampingParametersTabPage;
    private Lemoine.ConfigControls.ConfigConfig stampingParametersConfig;
  }
}
