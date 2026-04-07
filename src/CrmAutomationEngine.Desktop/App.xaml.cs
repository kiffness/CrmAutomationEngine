using System.IO;
using System.Windows;
using CrmAutomationEngine.Desktop.Services;
using CrmAutomationEngine.Desktop.ViewModels;
using CrmAutomationEngine.Desktop.Views;
using Microsoft.Extensions.Configuration;

namespace CrmAutomationEngine.Desktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var api = new ApiClient(config);

        var contacts    = new ContactsViewModel(api);
        var templates   = new TemplatesViewModel(api);
        var automations = new AutomationsViewModel(api);
        var jobs        = new JobsViewModel(api);
        var main        = new MainViewModel(contacts, templates, automations, jobs);

        var window = new MainWindow { DataContext = main };
        window.Show();
    }
}
