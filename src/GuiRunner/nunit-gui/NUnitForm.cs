#region Copyright (c) 2002, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Philip A. Craig
/************************************************************************************
'
' Copyright � 2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov
' Copyright � 2000-2002 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright � 2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov 
' or Copyright � 2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using Microsoft.Win32;

namespace NUnit.Gui
{
	using NUnit.Core;
	using NUnit.Util;
	using NUnit.UiKit;

	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class NUnitForm : System.Windows.Forms.Form
	{
		#region Static and instance variables

		/// <summary>
		/// Object maintaining the list of recently used assemblies
		/// </summary>
		private static RecentAssemblySettings recentAssemblies;

		/// <summary>
		/// The currently loaded assembly file name
		/// </summary>
		private string currentAssemblyFileName;

		/// <summary>
		/// Object that coordinates loading and running of tests
		/// </summary>
		private UIActions actions;

		public System.Windows.Forms.Splitter splitter1;
		public System.Windows.Forms.Panel panel1;
		public System.Windows.Forms.TabPage testsNotRun;
		public System.Windows.Forms.MenuItem openMenuItem;
		public System.Windows.Forms.MenuItem exitMenuItem;
		public System.Windows.Forms.MenuItem helpMenuItem;
		public System.Windows.Forms.MenuItem aboutMenuItem;
		public System.Windows.Forms.MainMenu mainMenu;
		public System.Windows.Forms.ListBox detailList;
		public System.Windows.Forms.Splitter splitter3;
		public System.Windows.Forms.TextBox stackTrace;
		public NUnit.UiKit.StatusBar statusBar;
		public System.Windows.Forms.OpenFileDialog openFileDialog;
		public NUnit.UiKit.TestSuiteTreeView testSuiteTreeView;
		public System.Windows.Forms.TabControl resultTabs;
		public System.Windows.Forms.TabPage errorPage;
		public System.Windows.Forms.TabPage stderr;
		public System.Windows.Forms.TabPage stdout;
		public System.Windows.Forms.TextBox stdErrTab;
		public System.Windows.Forms.TextBox stdOutTab;
		public System.Windows.Forms.MenuItem recentAssembliesMenu;
		public System.Windows.Forms.TreeView notRunTree;
		private System.ComponentModel.IContainer components;
		public TextWriter stdOutWriter;
		public System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.MenuItem closeMenuItem;
		public System.Windows.Forms.MenuItem fileMenu;
		private System.Windows.Forms.MenuItem fileMenuSeparator1;
		public System.Windows.Forms.MenuItem fileMenuSeparator2;
		public System.Windows.Forms.MenuItem helpItem;
		public System.Windows.Forms.MenuItem helpMenuSeparator1;
		private System.Windows.Forms.MenuItem reloadMenuItem;
		public System.Windows.Forms.GroupBox groupBox1;
		public System.Windows.Forms.Button runButton;
		public System.Windows.Forms.Label suiteName;
		public NUnit.UiKit.ProgressBar progressBar;
		private System.Windows.Forms.Button stopButton;
		private System.Windows.Forms.MenuItem viewMenu;
		private System.Windows.Forms.MenuItem toolsMenu;
		private System.Windows.Forms.MenuItem optionsMenuItem;
		private System.Windows.Forms.MenuItem expandMenuItem;
		private System.Windows.Forms.MenuItem collapseMenuItem;
		private System.Windows.Forms.MenuItem viewMenuSeparatorItem1;
		private System.Windows.Forms.MenuItem expandAllMenuItem;
		private System.Windows.Forms.MenuItem collapseAllMenuItem;
		private System.Windows.Forms.MenuItem viewMenuSeparatorItem2;
		private System.Windows.Forms.MenuItem expandFixturesMenuItem;
		private System.Windows.Forms.MenuItem collapseFixturesMenuItem;
		private System.Windows.Forms.MenuItem viewMenuSeparatorItem3;
		private System.Windows.Forms.MenuItem propertiesMenuItem;
		public TextWriter stdErrWriter;

		#endregion
		
		#region Properties

		/// <summary>
		/// True if an assembly is currently loaded
		/// </summary>
		private bool IsAssemblyLoaded
		{
			get { return LoadedAssembly != null; }
		}

		/// <summary>
		/// Full path to the currently loaded assembly file
		/// </summary>
		private string LoadedAssembly
		{
			get { return currentAssemblyFileName; }
			set { currentAssemblyFileName = value; }
		}

		#endregion

		#region Construction and Disposal

		/// <summary>
		/// Static constructor creates our recent assemblies list
		/// </summary>
		static NUnitForm()	
		{
			recentAssemblies = UserSettings.RecentAssemblies;
		}
		
		/// <summary>
		/// Construct our form, optionally providing the
		/// full path of to an assembly file to be loaded.
		/// </summary>
		/// <param name="assemblyFileName">Assembly to be loaded</param>
		public NUnitForm(string assemblyFileName)
		{
			InitializeComponent();

			stdErrTab.Enabled = true;
			stdOutTab.Enabled = true;

			SetDefault(runButton);
			runButton.Enabled = false;
			stopButton.Enabled = false;

			stdOutWriter = new TextBoxWriter(stdOutTab);
			Console.SetOut(stdOutWriter);
			stdErrWriter = new TextBoxWriter(stdErrTab);
			Console.SetError(stdErrWriter);

			actions = new UIActions(stdOutWriter, stdErrWriter);

			// Set up events handled by the form
			actions.RunStartingEvent += new TestEventHandler( OnRunStarting );
			actions.RunFinishedEvent += new TestEventHandler( OnRunFinished );

			actions.LoadCompleteEvent	+= new TestLoadEventHandler( OnTestLoaded );
			actions.UnloadCompleteEvent += new TestLoadEventHandler( OnTestUnloaded );
			actions.ReloadCompleteEvent += new TestLoadEventHandler( OnTestChanged );
			actions.LoadFailedEvent		+= new TestLoadEventHandler( OnAssemblyLoadFailure );
			actions.ReloadFailedEvent	+= new TestLoadEventHandler( OnAssemblyLoadFailure );

			// ToDo: Migrate more ui elements to handle events on their own.
			this.testSuiteTreeView.InitializeEvents( actions );
			this.progressBar.InitializeEvents( actions );
			this.statusBar.InitializeEvents( actions );

			if (assemblyFileName == null && UserSettings.Options.LoadLastAssembly )
				assemblyFileName = recentAssemblies.RecentAssembly;

			if(assemblyFileName != null)
				LoadAssembly(assemblyFileName);

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#endregion
		
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(NUnitForm));
			this.statusBar = new NUnit.UiKit.StatusBar();
			this.mainMenu = new System.Windows.Forms.MainMenu();
			this.fileMenu = new System.Windows.Forms.MenuItem();
			this.openMenuItem = new System.Windows.Forms.MenuItem();
			this.closeMenuItem = new System.Windows.Forms.MenuItem();
			this.reloadMenuItem = new System.Windows.Forms.MenuItem();
			this.fileMenuSeparator1 = new System.Windows.Forms.MenuItem();
			this.recentAssembliesMenu = new System.Windows.Forms.MenuItem();
			this.fileMenuSeparator2 = new System.Windows.Forms.MenuItem();
			this.exitMenuItem = new System.Windows.Forms.MenuItem();
			this.viewMenu = new System.Windows.Forms.MenuItem();
			this.expandMenuItem = new System.Windows.Forms.MenuItem();
			this.collapseMenuItem = new System.Windows.Forms.MenuItem();
			this.viewMenuSeparatorItem1 = new System.Windows.Forms.MenuItem();
			this.expandAllMenuItem = new System.Windows.Forms.MenuItem();
			this.collapseAllMenuItem = new System.Windows.Forms.MenuItem();
			this.viewMenuSeparatorItem2 = new System.Windows.Forms.MenuItem();
			this.expandFixturesMenuItem = new System.Windows.Forms.MenuItem();
			this.collapseFixturesMenuItem = new System.Windows.Forms.MenuItem();
			this.viewMenuSeparatorItem3 = new System.Windows.Forms.MenuItem();
			this.propertiesMenuItem = new System.Windows.Forms.MenuItem();
			this.toolsMenu = new System.Windows.Forms.MenuItem();
			this.optionsMenuItem = new System.Windows.Forms.MenuItem();
			this.helpItem = new System.Windows.Forms.MenuItem();
			this.helpMenuItem = new System.Windows.Forms.MenuItem();
			this.helpMenuSeparator1 = new System.Windows.Forms.MenuItem();
			this.aboutMenuItem = new System.Windows.Forms.MenuItem();
			this.testSuiteTreeView = new NUnit.UiKit.TestSuiteTreeView();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.panel1 = new System.Windows.Forms.Panel();
			this.resultTabs = new System.Windows.Forms.TabControl();
			this.errorPage = new System.Windows.Forms.TabPage();
			this.stackTrace = new System.Windows.Forms.TextBox();
			this.splitter3 = new System.Windows.Forms.Splitter();
			this.detailList = new System.Windows.Forms.ListBox();
			this.testsNotRun = new System.Windows.Forms.TabPage();
			this.notRunTree = new System.Windows.Forms.TreeView();
			this.stderr = new System.Windows.Forms.TabPage();
			this.stdErrTab = new System.Windows.Forms.TextBox();
			this.stdout = new System.Windows.Forms.TabPage();
			this.stdOutTab = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.stopButton = new System.Windows.Forms.Button();
			this.runButton = new System.Windows.Forms.Button();
			this.suiteName = new System.Windows.Forms.Label();
			this.progressBar = new NUnit.UiKit.ProgressBar();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.panel1.SuspendLayout();
			this.resultTabs.SuspendLayout();
			this.errorPage.SuspendLayout();
			this.testsNotRun.SuspendLayout();
			this.stderr.SuspendLayout();
			this.stdout.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusBar
			// 
			this.statusBar.DisplayTestProgress = true;
			this.statusBar.Location = new System.Drawing.Point(0, 511);
			this.statusBar.Name = "statusBar";
			this.statusBar.ShowPanels = true;
			this.statusBar.Size = new System.Drawing.Size(931, 31);
			this.statusBar.TabIndex = 0;
			this.statusBar.Text = "Status";
			// 
			// mainMenu
			// 
			this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.fileMenu,
																					 this.viewMenu,
																					 this.toolsMenu,
																					 this.helpItem});
			// 
			// fileMenu
			// 
			this.fileMenu.Index = 0;
			this.fileMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.openMenuItem,
																					 this.closeMenuItem,
																					 this.reloadMenuItem,
																					 this.fileMenuSeparator1,
																					 this.recentAssembliesMenu,
																					 this.fileMenuSeparator2,
																					 this.exitMenuItem});
			this.fileMenu.Text = "&File";
			this.fileMenu.Popup += new System.EventHandler(this.fileMenu_Popup);
			// 
			// openMenuItem
			// 
			this.openMenuItem.Index = 0;
			this.openMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
			this.openMenuItem.Text = "&Open...";
			this.openMenuItem.Click += new System.EventHandler(this.openMenuItem_Click);
			// 
			// closeMenuItem
			// 
			this.closeMenuItem.Index = 1;
			this.closeMenuItem.Text = "&Close";
			this.closeMenuItem.Click += new System.EventHandler(this.closeMenuItem_Click);
			// 
			// reloadMenuItem
			// 
			this.reloadMenuItem.Index = 2;
			this.reloadMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlR;
			this.reloadMenuItem.Text = "&Reload";
			this.reloadMenuItem.Click += new System.EventHandler(this.reloadMenuItem_Click);
			// 
			// fileMenuSeparator1
			// 
			this.fileMenuSeparator1.Index = 3;
			this.fileMenuSeparator1.Text = "-";
			// 
			// recentAssembliesMenu
			// 
			this.recentAssembliesMenu.Index = 4;
			this.recentAssembliesMenu.Text = "Recent Assemblies";
			// 
			// fileMenuSeparator2
			// 
			this.fileMenuSeparator2.Index = 5;
			this.fileMenuSeparator2.Text = "-";
			// 
			// exitMenuItem
			// 
			this.exitMenuItem.Index = 6;
			this.exitMenuItem.Text = "E&xit";
			this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
			// 
			// viewMenu
			// 
			this.viewMenu.Index = 1;
			this.viewMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.expandMenuItem,
																					 this.collapseMenuItem,
																					 this.viewMenuSeparatorItem1,
																					 this.expandAllMenuItem,
																					 this.collapseAllMenuItem,
																					 this.viewMenuSeparatorItem2,
																					 this.expandFixturesMenuItem,
																					 this.collapseFixturesMenuItem,
																					 this.viewMenuSeparatorItem3,
																					 this.propertiesMenuItem});
			this.viewMenu.Text = "&View";
			this.viewMenu.Popup += new System.EventHandler(this.viewMenu_Popup);
			// 
			// expandMenuItem
			// 
			this.expandMenuItem.Index = 0;
			this.expandMenuItem.Text = "&Expand";
			this.expandMenuItem.Click += new System.EventHandler(this.expandMenuItem_Click);
			// 
			// collapseMenuItem
			// 
			this.collapseMenuItem.Index = 1;
			this.collapseMenuItem.Text = "&Collapse";
			this.collapseMenuItem.Click += new System.EventHandler(this.collapseMenuItem_Click);
			// 
			// viewMenuSeparatorItem1
			// 
			this.viewMenuSeparatorItem1.Index = 2;
			this.viewMenuSeparatorItem1.Text = "-";
			// 
			// expandAllMenuItem
			// 
			this.expandAllMenuItem.Index = 3;
			this.expandAllMenuItem.Text = "Expand All";
			this.expandAllMenuItem.Click += new System.EventHandler(this.expandAllMenuItem_Click);
			// 
			// collapseAllMenuItem
			// 
			this.collapseAllMenuItem.Index = 4;
			this.collapseAllMenuItem.Text = "Collapse All";
			this.collapseAllMenuItem.Click += new System.EventHandler(this.collapseAllMenuItem_Click);
			// 
			// viewMenuSeparatorItem2
			// 
			this.viewMenuSeparatorItem2.Index = 5;
			this.viewMenuSeparatorItem2.Text = "-";
			// 
			// expandFixturesMenuItem
			// 
			this.expandFixturesMenuItem.Index = 6;
			this.expandFixturesMenuItem.Text = "Expand Fixtures";
			this.expandFixturesMenuItem.Click += new System.EventHandler(this.expandFixturesMenuItem_Click);
			// 
			// collapseFixturesMenuItem
			// 
			this.collapseFixturesMenuItem.Index = 7;
			this.collapseFixturesMenuItem.Text = "Collapse Fixtures";
			this.collapseFixturesMenuItem.Click += new System.EventHandler(this.collapseFixturesMenuItem_Click);
			// 
			// viewMenuSeparatorItem3
			// 
			this.viewMenuSeparatorItem3.Index = 8;
			this.viewMenuSeparatorItem3.Text = "-";
			// 
			// propertiesMenuItem
			// 
			this.propertiesMenuItem.Index = 9;
			this.propertiesMenuItem.Text = "&Properties";
			this.propertiesMenuItem.Click += new System.EventHandler(this.propertiesMenuItem_Click);
			// 
			// toolsMenu
			// 
			this.toolsMenu.Index = 2;
			this.toolsMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.optionsMenuItem});
			this.toolsMenu.Text = "&Tools";
			// 
			// optionsMenuItem
			// 
			this.optionsMenuItem.Index = 0;
			this.optionsMenuItem.Text = "&Options";
			this.optionsMenuItem.Click += new System.EventHandler(this.optionsMenuItem_Click);
			// 
			// helpItem
			// 
			this.helpItem.Index = 3;
			this.helpItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.helpMenuItem,
																					 this.helpMenuSeparator1,
																					 this.aboutMenuItem});
			this.helpItem.Text = "&Help";
			// 
			// helpMenuItem
			// 
			this.helpMenuItem.Enabled = false;
			this.helpMenuItem.Index = 0;
			this.helpMenuItem.Text = "Help";
			// 
			// helpMenuSeparator1
			// 
			this.helpMenuSeparator1.Index = 1;
			this.helpMenuSeparator1.Text = "-";
			// 
			// aboutMenuItem
			// 
			this.aboutMenuItem.Index = 2;
			this.aboutMenuItem.Text = "&About NUnit...";
			this.aboutMenuItem.Click += new System.EventHandler(this.aboutMenuItem_Click);
			// 
			// testSuiteTreeView
			// 
			this.testSuiteTreeView.AllowDrop = true;
			this.testSuiteTreeView.DisplayTestProgress = false;
			this.testSuiteTreeView.Dock = System.Windows.Forms.DockStyle.Left;
			this.testSuiteTreeView.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.testSuiteTreeView.HideSelection = false;
			this.testSuiteTreeView.Name = "testSuiteTreeView";
			this.testSuiteTreeView.RunCommandSupported = true;
			this.testSuiteTreeView.Size = new System.Drawing.Size(358, 511);
			this.testSuiteTreeView.Sorted = true;
			this.testSuiteTreeView.TabIndex = 1;
			this.testSuiteTreeView.SelectedTestChanged += new NUnit.UiKit.SelectedTestChangedHandler(this.OnSelectedTestChanged);
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(358, 0);
			this.splitter1.MinSize = 240;
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(6, 511);
			this.splitter1.TabIndex = 2;
			this.splitter1.TabStop = false;
			// 
			// panel1
			// 
			this.panel1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.resultTabs,
																				 this.groupBox1});
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(364, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(567, 511);
			this.panel1.TabIndex = 3;
			// 
			// resultTabs
			// 
			this.resultTabs.Controls.AddRange(new System.Windows.Forms.Control[] {
																					 this.errorPage,
																					 this.testsNotRun,
																					 this.stderr,
																					 this.stdout});
			this.resultTabs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.resultTabs.Location = new System.Drawing.Point(0, 88);
			this.resultTabs.Name = "resultTabs";
			this.resultTabs.SelectedIndex = 0;
			this.resultTabs.Size = new System.Drawing.Size(567, 423);
			this.resultTabs.TabIndex = 2;
			// 
			// errorPage
			// 
			this.errorPage.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.stackTrace,
																					this.splitter3,
																					this.detailList});
			this.errorPage.Location = new System.Drawing.Point(4, 25);
			this.errorPage.Name = "errorPage";
			this.errorPage.Size = new System.Drawing.Size(559, 394);
			this.errorPage.TabIndex = 0;
			this.errorPage.Text = "Errors and Failures";
			// 
			// stackTrace
			// 
			this.stackTrace.Dock = System.Windows.Forms.DockStyle.Fill;
			this.stackTrace.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.stackTrace.Location = new System.Drawing.Point(0, 127);
			this.stackTrace.Multiline = true;
			this.stackTrace.Name = "stackTrace";
			this.stackTrace.ReadOnly = true;
			this.stackTrace.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.stackTrace.Size = new System.Drawing.Size(559, 267);
			this.stackTrace.TabIndex = 2;
			this.stackTrace.Text = "";
			this.stackTrace.WordWrap = false;
			// 
			// splitter3
			// 
			this.splitter3.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter3.Location = new System.Drawing.Point(0, 124);
			this.splitter3.MinSize = 100;
			this.splitter3.Name = "splitter3";
			this.splitter3.Size = new System.Drawing.Size(559, 3);
			this.splitter3.TabIndex = 1;
			this.splitter3.TabStop = false;
			// 
			// detailList
			// 
			this.detailList.Dock = System.Windows.Forms.DockStyle.Top;
			this.detailList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.detailList.HorizontalExtent = 2000;
			this.detailList.HorizontalScrollbar = true;
			this.detailList.ItemHeight = 20;
			this.detailList.Name = "detailList";
			this.detailList.ScrollAlwaysVisible = true;
			this.detailList.Size = new System.Drawing.Size(559, 124);
			this.detailList.TabIndex = 0;
			this.detailList.SelectedIndexChanged += new System.EventHandler(this.detailList_SelectedIndexChanged);
			// 
			// testsNotRun
			// 
			this.testsNotRun.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.notRunTree});
			this.testsNotRun.Location = new System.Drawing.Point(4, 25);
			this.testsNotRun.Name = "testsNotRun";
			this.testsNotRun.Size = new System.Drawing.Size(559, 394);
			this.testsNotRun.TabIndex = 1;
			this.testsNotRun.Text = "Tests Not Run";
			// 
			// notRunTree
			// 
			this.notRunTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.notRunTree.ImageIndex = -1;
			this.notRunTree.Name = "notRunTree";
			this.notRunTree.SelectedImageIndex = -1;
			this.notRunTree.Size = new System.Drawing.Size(559, 394);
			this.notRunTree.TabIndex = 0;
			// 
			// stderr
			// 
			this.stderr.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.stdErrTab});
			this.stderr.Location = new System.Drawing.Point(4, 25);
			this.stderr.Name = "stderr";
			this.stderr.Size = new System.Drawing.Size(559, 394);
			this.stderr.TabIndex = 2;
			this.stderr.Text = "Standard Error";
			// 
			// stdErrTab
			// 
			this.stdErrTab.Dock = System.Windows.Forms.DockStyle.Fill;
			this.stdErrTab.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.stdErrTab.Multiline = true;
			this.stdErrTab.Name = "stdErrTab";
			this.stdErrTab.ReadOnly = true;
			this.stdErrTab.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.stdErrTab.Size = new System.Drawing.Size(559, 394);
			this.stdErrTab.TabIndex = 0;
			this.stdErrTab.Text = "";
			this.stdErrTab.WordWrap = false;
			// 
			// stdout
			// 
			this.stdout.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.stdOutTab});
			this.stdout.Location = new System.Drawing.Point(4, 25);
			this.stdout.Name = "stdout";
			this.stdout.Size = new System.Drawing.Size(559, 394);
			this.stdout.TabIndex = 3;
			this.stdout.Text = "Standard Out";
			// 
			// stdOutTab
			// 
			this.stdOutTab.Dock = System.Windows.Forms.DockStyle.Fill;
			this.stdOutTab.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.stdOutTab.Multiline = true;
			this.stdOutTab.Name = "stdOutTab";
			this.stdOutTab.ReadOnly = true;
			this.stdOutTab.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.stdOutTab.Size = new System.Drawing.Size(559, 394);
			this.stdOutTab.TabIndex = 0;
			this.stdOutTab.Text = "";
			this.stdOutTab.WordWrap = false;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.stopButton,
																					this.runButton,
																					this.suiteName,
																					this.progressBar});
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(567, 88);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			// 
			// stopButton
			// 
			this.stopButton.Location = new System.Drawing.Point(96, 16);
			this.stopButton.Name = "stopButton";
			this.stopButton.Size = new System.Drawing.Size(75, 30);
			this.stopButton.TabIndex = 4;
			this.stopButton.Text = "&Stop";
			this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
			// 
			// runButton
			// 
			this.runButton.Location = new System.Drawing.Point(8, 16);
			this.runButton.Name = "runButton";
			this.runButton.Size = new System.Drawing.Size(75, 30);
			this.runButton.TabIndex = 3;
			this.runButton.Text = "&Run";
			this.runButton.Click += new System.EventHandler(this.runButton_Click);
			// 
			// suiteName
			// 
			this.suiteName.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.suiteName.Location = new System.Drawing.Point(184, 24);
			this.suiteName.Name = "suiteName";
			this.suiteName.Size = new System.Drawing.Size(359, 24);
			this.suiteName.TabIndex = 2;
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.progressBar.CausesValidation = false;
			this.progressBar.Enabled = false;
			this.progressBar.ForeColor = System.Drawing.SystemColors.Highlight;
			this.progressBar.Location = new System.Drawing.Point(8, 56);
			this.progressBar.Maximum = 100;
			this.progressBar.Minimum = 0;
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(551, 24);
			this.progressBar.Step = 1;
			this.progressBar.TabIndex = 0;
			this.progressBar.Value = 0;
			// 
			// openFileDialog
			// 
			this.openFileDialog.Filter = "Assemblies (*.dll)|*.dll|Executables (*.exe)|*.exe|All Files (*.*)|*.*";
			// 
			// NUnitForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(931, 542);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.panel1,
																		  this.splitter1,
																		  this.testSuiteTreeView,
																		  this.statusBar});
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.mainMenu;
			this.Name = "NUnitForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "NUnit";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.NUnitForm_Closing);
			this.Load += new System.EventHandler(this.NUnitForm_Load);
			this.panel1.ResumeLayout(false);
			this.resultTabs.ResumeLayout(false);
			this.errorPage.ResumeLayout(false);
			this.testsNotRun.ResumeLayout(false);
			this.stderr.ResumeLayout(false);
			this.stdout.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Application Entry Point

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args) 
		{
			string assemblyName = null;

			GuiOptions parser = new GuiOptions(args);
			if(parser.Validate() && !parser.help) 
			{
				if(!parser.NoArgs)
					assemblyName = (string)parser.Assembly;

				if(assemblyName != null)
				{
					FileInfo fileInfo = new FileInfo(assemblyName);
					if(!fileInfo.Exists)
					{
						string message = String.Format("{0} does not exist", fileInfo.FullName);
						MessageBox.Show(null,message,"assembly does not exist",
							MessageBoxButtons.OK,MessageBoxIcon.Stop);
						return 1;
					}
					else
					{
						assemblyName = fileInfo.FullName;
					}
				}

				NUnitForm form = new NUnitForm(assemblyName);
				Application.Run(form);
			}
			else
			{
				string message = parser.GetHelpText();
				MessageBox.Show(null,message,"Help Syntax",
					MessageBoxButtons.OK,MessageBoxIcon.Stop);
				return 2;
			}	
				
			return 0;
		}

		#endregion

		#region Handlers for UI Events

		#region Main Menu Handlers

		#region File Menu

		/// <summary>
		/// When File menu is about to display, enable/disable Close
		/// </summary>
		private void fileMenu_Popup(object sender, System.EventArgs e)
		{
			openMenuItem.Enabled = !actions.IsTestRunning;
			closeMenuItem.Enabled = IsAssemblyLoaded && !actions.IsTestRunning;
			reloadMenuItem.Enabled = IsAssemblyLoaded && !actions.IsTestRunning;
			recentAssembliesMenu.Enabled = !actions.IsTestRunning;
		}

		/// <summary>
		/// When File+Open is selected, display FileOpenDialog and
		/// open whatever assembly the user selects.
		/// </summary>
		private void openMenuItem_Click(object sender, System.EventArgs e)
		{
			if (openFileDialog.ShowDialog(this) == DialogResult.OK) 
			{
				LoadAssembly( openFileDialog.FileName );
			}
		}

		/// <summary>
		/// When File+Close is selected, unload the assembly
		/// </summary>
		private void closeMenuItem_Click(object sender, System.EventArgs e)
		{
			UnloadAssembly();
		}

		/// <summary>
		/// Open recently used assembly when it's menu item is selected.
		/// </summary>
		private void recentFile_clicked(object sender, System.EventArgs args) 
		{
			MenuItem item = (MenuItem) sender;
			string assemblyFileName = item.Text.Substring( 2 );

			LoadAssembly( assemblyFileName );
		}

		/// <summary>
		/// reload the current AppDoamin which will flush the cache and reload it
		/// </summary>
		private void reloadMenuItem_Click(object sender, System.EventArgs e)
		{
			using ( new WaitCursor() )
			{
				actions.ReloadAssembly();
			}
		}

		/// <summary>
		/// Exit application when menu item is selected.
		/// </summary>
		private void exitMenuItem_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		#endregion

		#region View Menu

		private void viewMenu_Popup(object sender, System.EventArgs e)
		{
			TreeNode selectedNode = testSuiteTreeView.SelectedNode;
			if ( selectedNode != null && selectedNode.Nodes.Count > 0 )
			{
				bool isExpanded = testSuiteTreeView.SelectedNode.IsExpanded;
				collapseMenuItem.Enabled = isExpanded;
				expandMenuItem.Enabled = !isExpanded;		
			}
			else
			{
				collapseMenuItem.Enabled = expandMenuItem.Enabled = false;
			}
		}

		private void collapseMenuItem_Click(object sender, System.EventArgs e)
		{
			testSuiteTreeView.SelectedNode.Collapse();
		}

		private void expandMenuItem_Click(object sender, System.EventArgs e)
		{
			testSuiteTreeView.SelectedNode.Expand();
		}

		private void collapseAllMenuItem_Click(object sender, System.EventArgs e)
		{
			testSuiteTreeView.CollapseAll();
		}

		private void expandAllMenuItem_Click(object sender, System.EventArgs e)
		{
			testSuiteTreeView.ExpandAll();
		}

		private void collapseFixturesMenuItem_Click(object sender, System.EventArgs e)
		{
			testSuiteTreeView.CollapseFixtures();		
		}

		private void expandFixturesMenuItem_Click(object sender, System.EventArgs e)
		{
			testSuiteTreeView.ExpandFixtures();		
		}

		private void propertiesMenuItem_Click(object sender, System.EventArgs e)
		{
			testSuiteTreeView.ShowTestProperties( testSuiteTreeView.SelectedTest );
		}

		#endregion

		#region Tools Menu

		private void optionsMenuItem_Click(object sender, System.EventArgs e)
		{
			ShowOptionsDialog();
		}

		#endregion

		#region Help Menu

		/// <summary>
		/// Display the about box when menu item is selected
		/// </summary>
		private void aboutMenuItem_Click(object sender, System.EventArgs e)
		{
			AboutBox aboutBox = new AboutBox();
			aboutBox.Show();
		}

		#endregion

		#endregion

		/// <summary>
		/// When the Run Button is clicked, run the selected test.
		/// </summary>
		private void runButton_Click(object sender, System.EventArgs e)
		{
			runButton.Enabled = false;
			if ( actions.IsReloadPending )
				actions.ReloadAssembly();

			actions.RunTestSuite( testSuiteTreeView.SelectedTest );
		}

		/// <summary>
		/// When the Stop Button is clicked, cancel running test
		/// </summary>
		private void stopButton_Click(object sender, System.EventArgs e)
		{
			stopButton.Enabled = false;

			if ( actions.IsTestRunning )
			{
				DialogResult dialogResult = MessageBox.Show( 
					"Do you want to cancel the running test?", "NUnit", MessageBoxButtons.YesNo );

				if ( dialogResult == DialogResult.No )
					stopButton.Enabled = true;
				else
					actions.CancelTestRun();
			}
		}

		/// <summary>
		/// When a tree item is selected, display info pertaining 
		/// to that test unless a test is running.
		/// </summary>
		private void OnSelectedTestChanged( UITestNode test )
		{
			if ( !actions.IsTestRunning )
			{
				suiteName.Text = test.ShortName;
				statusBar.Initialize( test.CountTestCases );
			}
		}

		/// <summary>
		/// When one of the detail failure items is selected, display
		/// the stack trace and set up the tool tip for that item.
		/// </summary>
		private void detailList_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			TestResultItem resultItem = (TestResultItem)detailList.SelectedItem;
			//string stackTrace = resultItem.StackTrace;
			stackTrace.Text = resultItem.StackTrace;
			toolTip.SetToolTip(detailList,resultItem.GetMessage());
		}

		/// <summary>
		/// Exit application when space key is tapped
		/// </summary>
		protected override bool ProcessKeyPreview(ref 
			System.Windows.Forms.Message m) 
		{ 
			const int SPACE_BAR=32; 
			const int WM_CHAR = 258;

			if (m.Msg == WM_CHAR && m.WParam.ToInt32() == SPACE_BAR )
			{ 
				int altKeyBit = (int)m.LParam & 0x10000000;
				if ( altKeyBit == 0 )
				{
					this.Close();
					return true;
				}
			}

			return base.ProcessKeyEventArgs( ref m ); 
		} 

		/// <summary>
		///	Form is about to close, first see if we 
		///	have a test run going on and if so whether
		///	we should cancel it. Then unload the 
		///	assembly and save the latest form position.
		/// </summary>
		private void NUnitForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if ( actions.IsTestRunning )
			{
				DialogResult dialogResult = MessageBox.Show( 
					"A test is running, do you want to stop the test and exit?", "NUnit", MessageBoxButtons.YesNo );

				if ( dialogResult == DialogResult.No )
				{
					e.Cancel = true;
					return;
				}
				
				actions.CancelTestRun();
			}

			UnloadAssembly();

			if ( this.WindowState == FormWindowState.Normal )
			{
				UserSettings.Form.Location = this.Location;
				UserSettings.Form.Size = this.Size;
			}
		}

		/// <summary>
		/// Get last position when form loads
		/// </summary>
		private void NUnitForm_Load(object sender, System.EventArgs e)
		{
			this.Location = UserSettings.Form.Location;
			this.Size = UserSettings.Form.Size;
		}

		#endregion

		#region Handlers for events related to loading and running tests

		/// <summary>
		/// A test run is starting, so prepare the UI
		/// </summary>
		/// <param name="test">Top level Test for this run</param>
		private void OnRunStarting( object sender, TestEventArgs e )
		{
			suiteName.Text = e.Test.ShortName;
			runButton.Enabled = false;
			stopButton.Enabled = true;

			ClearTabs();
		}

		/// <summary>
		/// A test run has finished, so display the results
		/// and re-enable the run button.
		/// </summary>
		/// <param name="result">Result of the run</param>
		private void OnRunFinished( object sender, TestEventArgs e )
		{
			stopButton.Enabled = false;
			runButton.Enabled = false;

			if ( e.Result != null )
				DisplayResults(e.Result);

			if ( e.Exception != null )
			{
				if ( ! ( e.Exception is System.Threading.ThreadAbortException ) )
					FailureMessage( e.Exception, "NUnit Test Run Failed" );
			}

			runButton.Enabled = true;
		}

		/// <summary>
		/// A test suite assembly has been loaded, so update 
		/// recent assemblies and display the tests in the UI
		/// </summary>
		/// <param name="test">Top level test for the assembly</param>
		/// <param name="assemblyFileName">The full path of the assembly file</param>
		/// Note: second argument could be omitted, but tests may not
		/// always have the FullName set to the assembly name in future.
		private void OnTestLoaded( object sender, TestLoadEventArgs e )
		{
			LoadedAssembly = e.AssemblyName;
			UpdateRecentAssemblies( e.AssemblyName );

			runButton.Enabled = true;
			ClearTabs();
		}

		/// <summary>
		/// A test suite has been unloaded, so clear the UI
		/// and remove any references to the suite.
		/// </summary>
		private void OnTestUnloaded( object sender, TestLoadEventArgs e )
		{
			LoadedAssembly = null;
			suiteName.Text = null;
			runButton.Enabled = false;

			ClearTabs();
		}

		/// <summary>
		/// The current test suite has changed in some way,
		/// so update the info in the UI and clear the
		/// test results, since they are no longer valid.
		/// </summary>
		/// <param name="test">Top level Test for the current assembly</param>
		private void OnTestChanged( object sender, TestLoadEventArgs e )
		{
			if ( UserSettings.Options.ClearResults )
				ClearTabs();
		}

		/// <summary>
		/// Event handler for assembly load faiulres. We do this via
		/// an event since some errors may occur asynchronously.
		/// </summary>
		/// <param name="assemblyFileName">Name of the assembly file</param>
		/// <param name="exception">Exception that occurred.</param>
		private void OnAssemblyLoadFailure( object sender, TestLoadEventArgs e )
		{
			FailureMessage( e.Exception, "Assembly Load Failure" );
			RemoveRecentAssembly( e.AssemblyName );

			if ( e.AssemblyName == LoadedAssembly )
				OnTestUnloaded( sender, e );
			else
				runButton.Enabled = true;
		}

		#endregion

		#region Helper methods for loading and running tests

		/// <summary>
		/// Make an assembly the new most recent assembly, and
		/// update the menu accordingly. The assembly may already
		/// be in the list of recent assemblies, or may not be.
		/// </summary>
		/// <param name="assemblyFileName">Full path of the assembly to make most recent</param>
		private void UpdateRecentAssemblies(string assemblyFileName)
		{
			recentAssemblies.RecentAssembly = assemblyFileName;
			LoadRecentAssemblyMenu();
		}

		/// <summary>
		/// Remove an assembly from the recent assembly list and
		/// update the menu accordingly.
		/// </summary>
		/// <param name="assemblyFileName">Full path of the assembly to remove</param>
		private void RemoveRecentAssembly(string assemblyFileName)
		{
			recentAssemblies.Remove( assemblyFileName );
			LoadRecentAssemblyMenu();
		}

		/// <summary>
		/// Display failure message box
		/// </summary>
		/// <param name="exception">Exception that caused the failure</param>
		/// <param name="caption">Caption for the message box</param>
		private void FailureMessage( Exception exception, string caption )
		{
			string message = exception.Message;
			if(exception.InnerException != null)
				message = exception.InnerException.Message;

			FailureMessage( message, caption );
		}

		/// <summary>
		/// Display failure message box
		/// </summary>
		/// <param name="message">Message to display</param>
		/// <param name="caption">Caption for the message box</param>
		private void FailureMessage( string message, string caption )
		{
			MessageBox.Show( this, message, caption,
				MessageBoxButtons.OK,MessageBoxIcon.Stop);
		}

		/// <summary>
		/// Load an assembly into the UI
		/// </summary>
		/// <param name="assemblyFileName">Full path of the assembly to load</param>
		private void LoadAssembly(string assemblyFileName)
		{
			runButton.Enabled = false;
			actions.LoadAssembly( assemblyFileName );
		}

		/// <summary>
		/// Unload the current assembly
		/// </summary>
		private void UnloadAssembly()
		{
			runButton.Enabled = false;
			actions.UnloadAssembly();
		}

		#endregion

		#region Helper methods for modifying the UI display

		/// <summary>
		/// Set a button as the default for the form.
		/// </summary>
		/// <param name="myDefaultBtn">The button to be set as the default</param>
		private void SetDefault(Button myDefaultBtn)
		{
			this.AcceptButton = myDefaultBtn;
		}

		/// <summary>
		/// Load our menu with recently used assemblies.
		/// </summary>
		private void LoadRecentAssemblyMenu() 
		{
			IList assemblies = recentAssemblies.GetAssemblies();
			if (assemblies.Count == 0)
				recentAssembliesMenu.Enabled = false;
			else 
			{
				recentAssembliesMenu.Enabled = true;
				recentAssembliesMenu.MenuItems.Clear();
				int index = 1;
				foreach (string name in assemblies) 
				{
					MenuItem item = new MenuItem(String.Format("{0} {1}", index++, name));
					item.Click += new System.EventHandler(recentFile_clicked);
					this.recentAssembliesMenu.MenuItems.Add(item);
				}
			}
		}

		/// <summary>
		/// Clear info out of our four tabs and stack trace
		/// </summary>
		private void ClearTabs()
		{
			detailList.Items.Clear();
			notRunTree.Nodes.Clear();

			stdErrTab.Clear();
			stdOutTab.Clear();
			
			stackTrace.Text = "";
		}
	
		/// <summary>
		/// Display test results in the UI
		/// </summary>
		/// <param name="results">Test results to be displayed</param>
		private void DisplayResults(TestResult results)
		{
			DetailResults detailResults = new DetailResults(detailList, notRunTree);
			detailResults.DisplayResults( results );
		}

		private void ShowOptionsDialog( )
		{
			OptionsDialog dialog = new OptionsDialog();
			DialogResult result = dialog.ShowDialog();
		}

		#endregion	
	
	}
}

