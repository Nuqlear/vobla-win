using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Http;
using System.Net.Http.Headers;
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
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler == null) return;
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public SettingsWindow()
        {
            this.email = UserModel.Email;
            this.logged = UserModel.IsLoggedIn();
            InitializeComponent();
            InitHwnd();
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
    }
}
