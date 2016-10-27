
namespace vobla
{
    static class UserModel
    {
        public static string Email
        {
            get { return vobla.Properties.Settings.Default.Email; }
            set
            {
                if (value != vobla.Properties.Settings.Default.Email)
                {
                    vobla.Properties.Settings.Default.Email = value;
                    vobla.Properties.Settings.Default.Save();
                }
            }
        }

        public static string Token
        {
            get { return vobla.Properties.Settings.Default.Token; }
            set
            {
                if (value != vobla.Properties.Settings.Default.Token)
                {
                    vobla.Properties.Settings.Default.Token = value;
                    vobla.Properties.Settings.Default.Save();
                }
            }
        }

        public static bool IsLoggedIn()
        {

            return UserModel.Email.Length > 0 && UserModel.Token.Length > 0;
        }

        public static void Clear()
        {
            UserModel.Email = "";
            UserModel.Token = "";
        }
    }
}
