using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WifiShare.Logic
{
    public class WifiEntity : INotifyPropertyChanged
    {
        private string password = string.Empty;
        private string passwordHidden = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                if (password != value && value != null)
                {
                    password = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string PasswordHidden
        {
            get
            {
                return passwordHidden;
            }
            set
            {
                if (passwordHidden != value && value != null)
                {
                    passwordHidden = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool IsHidden { get; set; } = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string GetPassword()
        {
            return IsHidden ? passwordHidden : password;
        }
    }
}
