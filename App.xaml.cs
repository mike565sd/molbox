using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace molbox
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        MainViewModel viewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow app = new MainWindow();
            viewModel = new MainViewModel();

            app.DataContext = viewModel;
            app.Show();
        }
    }
}
