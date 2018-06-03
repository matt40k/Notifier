using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Notifier
{
    public partial class MainWindow : Window
    {
        private DateTime lastRaised;
        private FileSystemWatcher fs;
        private bool _authorMode;

        public MainWindow(bool AuthorMode)
        {
            InitializeComponent();
            if (!AuthorMode)
                StartFileWatch();
            else
                EnableAuthorMode();
        }

        private void EnableAuthorMode()
        {
            _authorMode = true;

            this.Visibility = Visibility.Visible;
            this.Topmost = true;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.WindowStyle = WindowStyle.ThreeDBorderWindow;
            this.ShowInTaskbar = true;

            this.messageView.Visibility = Visibility.Hidden;
            this.messageEdit.Visibility = Visibility.Visible;

            this.okButton.Content = "Send";

            this.NotifierType.ItemsSource = GetImages;
            this.NotifierType.SelectedIndex = 0;
            this.NotifierType.Items.Refresh();
            this.NotifierType.Visibility = Visibility.Visible;
        }

        private void StartFileWatch()
        {
            _authorMode = false;
            if (Directory.Exists(ConnectionString))
            {
                fs = new FileSystemWatcher(ConnectionString, ("*" + MessageExtension));
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
                    if (extension == MessageExtension)
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
                            this.messageView.Visibility = Visibility.Visible;
                            this.messageEdit.Visibility = Visibility.Hidden;
                            messageView.Text = newMessage;
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
                string filePath = name + ImageExtension;
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
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri(path);
                img.DecodePixelWidth = 280;
                img.EndInit();
                return img;
            }
            return null;
        }

        private void NotifierType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.NotifierType.SelectedValue != null)
            {
                string imgPath = Path.Combine(ConnectionString, (GetNotifierType + ImageExtension));
                this.image.Source = ImageData(imgPath);
            }
        }

        private string GetNotifierType
        {
            get
            {
                if (this.NotifierType.SelectedValue != null)
                {
                    string type = this.NotifierType.SelectedValue.ToString();
                    if (type == "None")
                        return null;
                    else
                        return type;
                }
                return null;
            }
        }

        private List<string> GetImages
        {
            get
            {
                List<string> imgs = new List<string>();
                imgs.Add("None");
                var images = Directory.GetFiles(ConnectionString, ("*" + ImageExtension), SearchOption.TopDirectoryOnly);
                foreach (var image in images)
                {
                    imgs.Add(Path.GetFileNameWithoutExtension(image));
                }
                return imgs;
            }
        }

        private string ImageExtension
        {
            get
            {
                return ".png";
            }
        }

        private string MessageExtension
        {
            get
            {
                return ".txt";
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (_authorMode)
                SendMessage();
            else
                this.Visibility = Visibility.Hidden;
        }

        private string ConnectionString
        {
            get
            {
                return Properties.Settings.Default.ConnectionString;
            }
        }

        private void SendMessage()
        {
            string user = (Environment.UserName).Replace("\\", "_");
            string dt = DateTime.Now.ToString("_yyyyMMdd_HHmmss");
            string messageFileName = user + dt + MessageExtension;

            string type = GetNotifierType;
            if (!string.IsNullOrWhiteSpace(type))
                messageFileName = type + "_" + messageFileName;

            string messageFilePath = Path.Combine(ConnectionString, messageFileName);
            try
            {
                File.WriteAllText(messageFilePath, messageEdit.Text);
            }
            catch(Exception)
            {

            }

            Application.Current.Shutdown();
        }
    }
}
