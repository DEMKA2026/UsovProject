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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UsovProject
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Entity context;
        public MainWindow()
        {
            InitializeComponent();
            context = new Entity();
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string enteredLogin = tbLogin.Text.Trim();
            string enteredPassword = pbPassword.Password;
            User current = FindUser(enteredLogin, enteredPassword);
            if (current == null)
            {
                ShowError();
                return;
            }

            OpenProductsPage(current);
        }
        private User FindUser(string login, string password)
        {
            User result = context.User
                .FirstOrDefault(u =>
                    u.Login == login &&
                    u.Password == password);

            return result;
        }
        private void ShowError()
        {
            MessageBox.Show(
                "Неверный логин или пароль",
                "Ошибка входа",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        private void OpenProductsPage(User user)
        {
            ProductsSpisok window = new ProductsSpisok(user);

            window.Show();

            Close();
        }
        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            OpenProductsPage(null);
        }
    }
}