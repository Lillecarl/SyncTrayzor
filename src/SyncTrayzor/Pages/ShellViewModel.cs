﻿using Stylet;
using SyncTrayzor.NotifyIcon;
using SyncTrayzor.Services;
using SyncTrayzor.SyncThing;
using SyncTrayzor.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SyncTrayzor.Pages
{
    public class ShellViewModel : Screen, INotifyIconDelegate
    {
        private readonly IWindowManager windowManager;
        private readonly ISyncThingManager syncThingManager;
        private readonly IApplicationState application;
        private readonly Func<SettingsViewModel> settingsViewModelFactory;
        private readonly Func<AboutViewModel> aboutViewModelFactory;

        public string ExecutablePath { get; private set; }
        public ConsoleViewModel Console { get; private set; }
        public ViewerViewModel Viewer { get; private set; }

        public SyncThingState SyncThingState { get; private set; }

        public ShellViewModel(
            IWindowManager windowManager,
            ISyncThingManager syncThingManager,
            IApplicationState application,
            ConsoleViewModel console,
            ViewerViewModel viewer,
            Func<SettingsViewModel> settingsViewModelFactory,
            Func<AboutViewModel> aboutViewModelFactory)
        {
            this.DisplayName = "SyncTrayzor";

            this.windowManager = windowManager;
            this.syncThingManager = syncThingManager;
            this.application = application;
            this.Console = console;
            this.Viewer = viewer;
            this.settingsViewModelFactory = settingsViewModelFactory;
            this.aboutViewModelFactory = aboutViewModelFactory;

            this.Console.ConductWith(this);
            this.Viewer.ConductWith(this);

            this.syncThingManager.StateChanged += (o, e) => this.SyncThingState = e.NewState;
        }

        public bool CanStart
        {
            get { return this.SyncThingState == SyncThingState.Stopped; }
        }
        public void Start()
        {
            try
            {
                this.syncThingManager.Start();
            }
            catch (Exception e)
            {
                this.windowManager.ShowMessageBox(String.Format("Error starting SyncThing: {0}", e.Message), "Error starting SyncThing", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool CanStop
        {
            get { return this.SyncThingState == SyncThingState.Running; }
        }
        public void Stop()
        {
            this.syncThingManager.StopAsync();
        }

        public bool CanKill
        {
            get { return this.SyncThingState != SyncThingState.Stopped; }
        }
        public void Kill()
        {
            this.syncThingManager.Kill();
        }

        public bool CanRefreshBrowser
        {
            get { return this.SyncThingState == SyncThingState.Running; }
        }
        public void RefreshBrowser()
        {
            this.Viewer.RefreshBrowser();
        }

        public bool CanOpenBrowser
        {
            get { return this.SyncThingState == SyncThingState.Running; }
        }
        public void OpenBrowser()
        {
            Process.Start(this.syncThingManager.Address.NormalizeZeroHost().ToString());
        }

        public void KillAllSyncthingProcesses()
        {
            if (this.windowManager.ShowMessageBox("Are you sure you want to kill all Syncthing processes, even those not managed by SyncTrayzor?", "Really?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                this.syncThingManager.KillAllSyncthingProcesses();
        }

        public void ShowSettings()
        {
            var vm = this.settingsViewModelFactory();
            this.windowManager.ShowDialog(vm);
        }

        public void ShowAbout()
        {
            var vm = this.aboutViewModelFactory();
            this.windowManager.ShowDialog(vm);
        }

        public void CloseToTray()
        {
            this.RequestClose();
        }

        public void RestoreFromTray()
        {
            this.windowManager.ShowWindow(this);
        }

        public void Shutdown()
        {
            this.application.Shutdown();
        }
    }
}