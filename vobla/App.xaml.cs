using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Squirrel;

namespace vobla
{
    public partial class App : Application, IDisposable
    {
        private System.Windows.Forms.NotifyIcon notifyIcon = null;
        private SettingsWindow settingsWindow = null;
        private AreaSelector asForm = null;
        private UpdateManager updateManager = null;

        #region Init
        private void DoStartup()
        {
            updateManager = new UpdateManager(new Uri(new Uri(vobla.Properties.Settings.Default.URL), "releases/win").ToString());
            updateManager.CheckForUpdate();
            this.AddNotifyIcon();
            if (UserModel.IsLoggedIn())
            {
                ApiRequests.Instance.SetToken(UserModel.Token);
                // running task synchronously
                var res = Task.Run(async () => await ApiRequests.Instance.SyncPost()).Result;
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

        private void InitSettingsWindow()
        {
            if (this.settingsWindow == null)
            {
                this.settingsWindow = new SettingsWindow();
                this.settingsWindow.Hided += this.SettingsWindow_Hided;
            }
            if (!UserModel.IsLoggedIn())
            {
                this.settingsWindow.Show();
            }
        }

        public void ShowSettingsWindow()
        {
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
            
            this.notifyIcon.MouseDoubleClick += (object sender, System.Windows.Forms.MouseEventArgs e) => {
                ShowSettingsWindow();
            };
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
                Text = vobla.Properties.Resources.NotifyExit
            };
            itemExit.Click += TrayExit_Click;
            string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToString();
            string version = System.Reflection.Assembly.GetExecutingAssembly()
                .GetName().Version.ToString();
            System.Windows.Forms.MenuItem voblaInfo = new System.Windows.Forms.MenuItem()
            {
                Checked = true,
                Index = 0,
                Enabled = false,
                Text = appName + " " + version
            };
            contextMenu.MenuItems.Add(voblaInfo);
            if (!this.settingsWindow.IsVisible)
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
            HotkeyManager.hotKeyPressedEvent += HotKeyHelper_HotKeyPressed;
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
            HotkeyManager.AddGlobalKeyHook(this.settingsWindow, modKeys, vkCode);
        }

        private void RemoveKeyHook()
        {
            HotkeyManager.RemoveGlobalKeyHook(this.settingsWindow);
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
            if (this.asForm == null && UserModel.IsLoggedIn())
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

        private void FileUploaded(string fileSlug)
        {
            if (fileSlug != null)
            {
                String host = vobla.Properties.Settings.Default.URL;
                String url = new Uri(new Uri(host), fileSlug).ToString();
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
                        updateManager.Dispose();
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
