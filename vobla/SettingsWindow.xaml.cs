using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Interop;

namespace vobla
{
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        public event EventHandler Hided;

        private bool _logged;
        public bool logged
        {
            get { return _logged; }
            set
            {
                _logged = value;
                OnPropertyChanged("logged");
            }
        }
        private string _email;
        public string email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged("email");
            }
        }
        private string _captureAreaHotkey;
        public string captureAreaHotkey
        {
            get { return _captureAreaHotkey; }
            set
            {
                _captureAreaHotkey = value;
                OnPropertyChanged("captureAreaHotkey");
            }
        }
        private string _captureScreenHotkey;
        public string captureScreenHotkey
        {
            get { return _captureScreenHotkey; }
            set
            {
                _captureScreenHotkey = value;
                OnPropertyChanged("captureScreenHotkey");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SettingsWindow()
        {
            this.email = UserModel.Email;
            this.logged = UserModel.IsLoggedIn();
            this.ConvertKeyCodes();
            InitializeComponent();
            // force init window's HWND
            InitHwnd();
        }

        public void ConvertKeyCodes()
        {
            var keysConverter = new System.Windows.Forms.KeysConverter();
            var areaKey = (System.Windows.Forms.Keys)vobla.Properties.Settings.Default.CaptureAreaVKCode;
            var areaModifierKeys = (System.Windows.Input.ModifierKeys)vobla.Properties.Settings.Default.CaptureAreaVKModifier;
            this.captureAreaHotkey = areaModifierKeys.ToString() + " + " + keysConverter.ConvertToString(areaKey);

            var screenKey = (System.Windows.Forms.Keys)vobla.Properties.Settings.Default.CaptureScreenVKCode;
            var screeModifierKeys = (System.Windows.Input.ModifierKeys)vobla.Properties.Settings.Default.CaptureScreenVKModifier;
            this.captureScreenHotkey = screeModifierKeys.ToString() + " + " + keysConverter.ConvertToString(screenKey);
        }

        public void InitHwnd()
        {
            var helper = new WindowInteropHelper(this);
            helper.EnsureHandle();
        }

        private ICommand mLoginCommand;
        public ICommand LoginCommand
        {
            get
            {
                if (mLoginCommand == null)
                    mLoginCommand = new AsyncRelayCommand(this.Login);
                return mLoginCommand;
            }
            set
            {
                mLoginCommand = value;
            }
        }

        public async Task Login(Object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null || passwordBox.Password.Length == 0 || this.email.Length == 0)
                return;
            var password = passwordBox.Password;
            var dict = await ApiRequests.Instance.LoginPost(this.email, password);
            if (dict.ContainsKey("token"))
            {
                var token = dict["token"];
                ApiRequests.Instance.SetToken(token);
                UserModel.Token = token;
                UserModel.Email = this.email;
                this.logged = UserModel.IsLoggedIn();
            }
        }

        private void logoutB_Click(object sender, RoutedEventArgs e)
        {
            UserModel.Clear();
            this.logged = UserModel.IsLoggedIn();
            this.email = UserModel.Email;
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            this.Hided(null, null);
        }

        private async void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var grid = (Grid)sender;
                var passwordBox = (PasswordBox)(grid.Children.Cast<UIElement>().First(
                    child => Grid.GetRow(child) == 1 && Grid.GetColumn(child) == 1
                ));
                await this.Login(passwordBox);
            }
        }
    }
}
