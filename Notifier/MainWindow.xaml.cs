using System;
using System.IO;
using System.Windows;


namespace Notifier
{
    public partial class MainWindow : Window
    {
        DateTime lastRaised;
        FileSystemWatcher fs;

        public MainWindow()
        {
            InitializeComponent();
            StartFileWatch();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private string ConnectionString
        {
            get
            {
                return Properties.Settings.Default.ConnectionString;
            }
        }

        private void StartFileWatch()
        {
            if (Directory.Exists(ConnectionString))
            {
                fs = new FileSystemWatcher(ConnectionString, "*.*");
                fs.EnableRaisingEvents = true;
                fs.IncludeSubdirectories = false;
                //This event will check for any new addition of files to the watching folder
                fs.Created += new FileSystemEventHandler(newfile);
            }
        }

        protected void newfile(object fscreated, FileSystemEventArgs Eventocc)
        {
            try
            {
                if (DateTime.Now.Subtract(lastRaised).TotalMilliseconds > 1000)
                {
                    string newMessagePath = Path.Combine(ConnectionString, Eventocc.Name);
                    lastRaised = DateTime.Now;
                    System.Threading.Thread.Sleep(100);
                    string newMessage = File.ReadAllText(newMessagePath);
                    this.Dispatcher.Invoke(() =>
                    {
                        messageBlock.Text = newMessage;
                        this.Visibility = Visibility.Visible;
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {

            }
        }
    }
}
