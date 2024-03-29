﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using Squirrel;

namespace vobla
{
    public partial class App : Application, IDisposable
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon = null;
        private SettingsWindow _settingsWindow = null;
        private AreaSelector _asForm = null;

        #region Init
        private void DoStartup()
        {
            this.AddNotifyIcon();
            Task.Run(async () => await App.CheckForUpdates());
            if (UserModel.IsLoggedIn())
            {
                ApiRequests.Instance.SetToken(UserModel.Token);
                var res = Task.Run(async () => await ApiRequests.Instance.SyncGet()).Result;
                if (!res)
                {
                    UserModel.Clear();
                }
            }
            this.InitSettingsWindow();
            this.updateContextMenu();
            this.AddKeyHook();
            this.Exit += App_Exit;
        }

        private static async Task<int> CheckForUpdates()
        {
            using (var mgr = new UpdateManager(new Uri(new Uri(vobla.Properties.Settings.Default.URL), "releases/win").ToString()))
            {
                var updateInfo = await mgr.CheckForUpdate(progress: x => Console.WriteLine(x / 3));
                var remoteVersion = new Version(updateInfo.FutureReleaseEntry.Version.ToString());
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                var localVersion = new Version(fvi.FileVersion);
                if (remoteVersion.CompareTo(localVersion) > 0)
                {
                    await mgr.UpdateApp();
                    UpdateManager.RestartApp();
                }
            }
            return 0;
        }

        private void InitSettingsWindow()
        {
            if (this._settingsWindow == null)
            {
                this._settingsWindow = new SettingsWindow();
                this._settingsWindow.Hided += this.SettingsWindow_Hided;
            }
            if (!UserModel.IsLoggedIn())
            {
                this._settingsWindow.Show();
            }
        }

        public void ShowSettingsWindow()
        {
            this._settingsWindow.Show();
            this.updateContextMenu();
        }

        #region NotifyIcon
        private void AddNotifyIcon()
        {
            // setup icon
            this._notifyIcon = new System.Windows.Forms.NotifyIcon();
            this._notifyIcon.Text = vobla.Properties.Resources.NotifyText;
            this._notifyIcon.Icon = vobla.Properties.Resources.Icon32;
            
            this._notifyIcon.MouseDoubleClick += (object sender, System.Windows.Forms.MouseEventArgs e) => {
                ShowSettingsWindow();
            };
            // show icon
            this._notifyIcon.Visible = true;
        }

        private void updateContextMenu()
        {
            this._notifyIcon.ContextMenu = this.CreateNotifyIconContextMenu();
        }

        private System.Windows.Forms.ContextMenu CreateNotifyIconContextMenu()
        {
            System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem itemExit = new System.Windows.Forms.MenuItem()
            {
                Text = vobla.Properties.Resources.NotifyExit
            };
            itemExit.Click += TrayExit_Click;
            var appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToString();
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            System.Windows.Forms.MenuItem voblaInfo = new System.Windows.Forms.MenuItem()
            {
                Checked = true,
                Index = 0,
                Enabled = false,
                Text = $"{appName} {version.Major}.{version.Minor}.{version.Build}"
            };
            contextMenu.MenuItems.Add(voblaInfo);
            System.Windows.Forms.MenuItem openSite = new System.Windows.Forms.MenuItem()
            {
                Text = vobla.Properties.Resources.NotifyOpenDashboard
            };
            openSite.Click += TrayOpenSite_Click;
            contextMenu.MenuItems.Add(openSite);
            if (!this._settingsWindow.IsVisible)
            {
                System.Windows.Forms.MenuItem itemSettings = new System.Windows.Forms.MenuItem()
                {
                    Index = 1,
                    Text = vobla.Properties.Resources.NotifySettings
                };
                itemSettings.Click += (object sender, EventArgs e) => {
                    ShowSettingsWindow();
                };
                contextMenu.MenuItems.Add(itemSettings);
            }
            contextMenu.MenuItems.Add(itemExit);
            return contextMenu;
        }
        #endregion
        #endregion

        #region Hotkeys
        private void AddKeyHook()
        {
            HotkeyManager.HotKeyPressedEvent += HotKeyHelper_HotKeyPressed;
            this.AddKeyHook(
                vobla.Properties.Settings.Default.CaptureAreaVKCode,
                vobla.Properties.Settings.Default.CaptureAreaVKModifier
            );

            this.AddKeyHook(
                vobla.Properties.Settings.Default.CaptureScreenVKCode,
                vobla.Properties.Settings.Default.CaptureScreenVKModifier
            );
        }

        private void AddKeyHook(uint vkCode, uint modKeys)
        {
            HotkeyManager.AddGlobalKeyHook(this._settingsWindow, modKeys, vkCode);
        }

        private void RemoveKeyHook()
        {
            HotkeyManager.RemoveGlobalKeyHook(this._settingsWindow);
        }

        private void HotKeyHelper_HotKeyPressed(object sender, EventArgs e)
        {
            var keyEvent = (System.Windows.Forms.KeyEventArgs)e;
            if (keyEvent.KeyValue == vobla.Properties.Settings.Default.CaptureAreaVKCode)
            {
                this.ScreenshotArea();
            }
            else if (keyEvent.KeyValue == vobla.Properties.Settings.Default.CaptureScreenVKCode)
            {
                this.ScreenshotScreen();
            }
        }
        #endregion

        #region Screenshots
        private async void ScreenshotScreen()
        {
            if (UserModel.IsLoggedIn())
            {
                Image img = CaptureScreen.CaptureFullscreen();
                var result = await ApiRequests.Instance.ImagePost(img);
                FileUploaded(result);
            }
        }

        private void ScreenshotArea()
        {
            if (this._asForm == null && UserModel.IsLoggedIn())
            {
                this._asForm = new AreaSelector();
                this._asForm.Show();
                this._asForm.AreaSelectedEvent += AreaSelectedHandler;
            }
        }

        private async void AreaSelectedHandler(Rectangle rect)
        {
            this._asForm = null;
            if (!rect.IsEmpty)
            {
                Image img = CaptureScreen.CaptureRectangle(rect);
                var result = await ApiRequests.Instance.ImagePost(img);
                FileUploaded(result);
            }
            
        }

        private void FileUploaded(string fileSlug)
        {
            if (fileSlug != null)
            {
                String host = vobla.Properties.Settings.Default.URL;
                String url = new Uri(new Uri(host), $"f/{fileSlug}").ToString();
                Clipboard.SetDataObject(url);
                this._notifyIcon.ShowBalloonTip(
                    vobla.Properties.Settings.Default.BalloontipTimeout,
                    vobla.Properties.Resources.BalloontipUploadedTitle,
                    vobla.Properties.Resources.BalloontipUploadedText,
                    System.Windows.Forms.ToolTipIcon.Info
                );
            }
            else
            {
                this._notifyIcon.ShowBalloonTip(
                    vobla.Properties.Settings.Default.BalloontipTimeout,
                    vobla.Properties.Resources.BalloontipUploadErrorTitle,
                    vobla.Properties.Resources.BalloontipUploadErrorText,
                    System.Windows.Forms.ToolTipIcon.Error
                );
            }
        }
        #endregion

        #region Events
        private void TrayExit_Click(object sender, EventArgs e)
        {
            this.RemoveKeyHook();
            this.Shutdown();
        }

        private void TrayOpenSite_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start($"{vobla.Properties.Settings.Default.URL}");
        }

        private void SettingsWindow_Hided(object sender, EventArgs e)
        {
            this.updateContextMenu();
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            this._notifyIcon.Dispose();
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            this.DoStartup();
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            this.Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._notifyIcon.Dispose();
            }
        }
        ~App()
        {
            this.Dispose(false);
        }
        #endregion
    }
}
