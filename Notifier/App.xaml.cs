using System.Windows;

namespace Notifier
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            bool AuthorMode = false;
            if (e.Args.Length == 1)
                if (e.Args[0].ToUpper() == "/AUTHOR")
                    AuthorMode = true;

            MainWindow wnd = new MainWindow(AuthorMode);
            wnd.Show();
        }
    }
}
