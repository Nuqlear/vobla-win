using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Windows;

namespace vobla
{
    public partial class App : Application, IDisposable
    {
        private System.Windows.Forms.NotifyIcon notifyIcon = null;
        private SettingsWindow settingsWindow = null;
        private AreaSelector asForm = null;

        #region Init
        private void DoStartup()
        {
            this.AddNotifyIcon();
            this.ShowSettingsWindow(null, null);
            
            this.AddKeyHook();
            this.Exit += App_Exit;
        }

        private void ShowSettingsWindow(object sender, EventArgs e)
        {
            if (this.settingsWindow == null)
            {
                this.settingsWindow = new SettingsWindow();
                this.settingsWindow.Hided += this.SettingsWindow_Hided;
            }
            this.settingsWindow.Show();
            this.updateContextMenu();
        }

        #region NotifyIcon
        private void AddNotifyIcon()
        {
            // setup icon
            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.Text = vobla.Properties.Resources.NotifyText;
            this.notifyIcon.Icon = vobla.Properties.Resources.favicon;

            // show icon
            this.notifyIcon.Visible = true;
        }

        private void updateContextMenu()
        {
            this.notifyIcon.ContextMenu = this.CreateNotifyIconContextMenu();
        }

        private System.Windows.Forms.ContextMenu CreateNotifyIconContextMenu()
        {
            System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem itemExit = new System.Windows.Forms.MenuItem()
            {
                Text = vobla.Properties.Resources.NotifyExit,
                Name = vobla.Properties.Resources.NotifyExit
            };
            itemExit.Click += TrayExit_Click;
            if (!this.settingsWindow.IsVisible)
            {
                System.Windows.Forms.MenuItem itemSettings = new System.Windows.Forms.MenuItem()
                {
                    Index = 0,
                    Text = vobla.Properties.Resources.NotifySettings,
                    Name = vobla.Properties.Resources.NotifySettings
                };
                itemSettings.Click += ShowSettingsWindow;
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
            HotkeyManager.hotKeyPressedEvent += HotKeyHelper_HotKeyPressed;

            this.AddKeyHook(
                WinApiConstants.VK_4, 
                WinApiConstants.MOD_CTRL | WinApiConstants.MOD_SHIFT
            );
        }

        private void AddKeyHook(uint vkCode, uint modKeys)
        {
            HotkeyManager.AddGlobalKeyHook(this.settingsWindow, modKeys, vkCode);
        }

        private void RemoveKeyHook()
        {
            HotkeyManager.RemoveGlobalKeyHook(this.settingsWindow);
        }

        private void HotKeyHelper_HotKeyPressed(object sender, EventArgs e)
        {
            this.ScreenshotArea();
        }
        #endregion

        #region Screenshots
        private void ScreenshotArea()
        {
            if (this.asForm == null)
            {
                this.asForm = new AreaSelector();
                this.asForm.Show();
                this.asForm.areaSelectedEvent += AreaSelectedHandler;
            }
        }

        private async void AreaSelectedHandler(Rectangle rect)
        {
            this.asForm = null;
            if (!rect.IsEmpty)
            {
                Image img = CaptureScreen.CaptureRectangle(rect);
                var result = await ApiRequests.Instance.ImagePost(img);
                FileUploaded(result);
            }
            
        }

        private void FileUploaded(string fileRelUrl)
        {
            if (fileRelUrl != null)
            {
                String host = vobla.Properties.Settings.Default.URL;
                String url = new Uri(new Uri(host), fileRelUrl).ToString();
                Clipboard.SetDataObject(url);
                this.notifyIcon.ShowBalloonTip(
                    vobla.Properties.Settings.Default.BalloontipTimeout,
                    vobla.Properties.Resources.BalloontipUploadedTitle,
                    vobla.Properties.Resources.BalloontipUploadedText,
                    System.Windows.Forms.ToolTipIcon.Info
                );
            }
            else
            {
                this.notifyIcon.ShowBalloonTip(
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

        private void SettingsWindow_Hided(object sender, EventArgs e)
        {
            this.updateContextMenu();
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            this.notifyIcon.Dispose();
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
                this.notifyIcon.Dispose();
            }
        }
        ~App()
        {
            this.Dispose(false);
        }
        #endregion
    }
}
