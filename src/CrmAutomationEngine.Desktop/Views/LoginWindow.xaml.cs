using System.Windows;
using CrmAutomationEngine.Desktop.ViewModels;

namespace CrmAutomationEngine.Desktop.Views;

public partial class LoginWindow : Window
{
    public LoginWindow(LoginViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        vm.LoginSucceeded += _ => DialogResult = true;
    }
}
