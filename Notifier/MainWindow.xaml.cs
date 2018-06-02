using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

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
                    FileInfo createdFile = new FileInfo(newMessagePath);
                    string extension = createdFile.Extension;
                    if (extension == ".txt")
                    {
                        string imageName = null;

                        if ((Eventocc.Name).Contains("-"))
                        {
                            imageName = (Eventocc.Name).Substring(0, (Eventocc.Name).IndexOf("-"));
                        }

                        lastRaised = DateTime.Now;
                        System.Threading.Thread.Sleep(100);
                        string newMessage = File.ReadAllText(newMessagePath);
                        this.Dispatcher.Invoke(() =>
                        {
                            messageBlock.Text = newMessage;
                            this.Visibility = Visibility.Visible;
                            this.Topmost = true;
                            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;


                            string imgPath = ImagePath(imageName);
                            if (!string.IsNullOrWhiteSpace(imgPath))
                                this.image.Source = ImageData(imgPath);
                        });
                    }
                }
            }
            catch (Exception)
            {
                
            }
            finally
            {

            }
        }

        private string ImagePath(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                string filePath = name + ".png";
                if (File.Exists(filePath))
                    return filePath;
                filePath = Path.Combine(ConnectionString, filePath);
                if (File.Exists(filePath))
                    return filePath;
            }
            return null;
        }

        private BitmapImage ImageData(string path)
        {
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri(path);
            img.DecodePixelWidth = 280;
            img.EndInit();
            return img;
        }
    }
}
