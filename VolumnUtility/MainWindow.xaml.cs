using IWshRuntimeLibrary;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Xml.Linq;

namespace VolumnUtility
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Forms.NotifyIcon notifyIcon;

        System.Windows.Forms.MenuItem itemAutoStart;
        System.Windows.Forms.MenuItem itemExit;

        public MainWindow()
        {
            InitializeComponent();

            double width = SystemParameters.WorkArea.Width;
            double height = SystemParameters.WorkArea.Height;
            this.Left = width - this.Width;
            this.Top = height - this.Height;

            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.BalloonTipText = "双击快速静音";
            this.notifyIcon.Text = "音量小工具";
            this.notifyIcon.Visible = true;
            this.notifyIcon.Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("Resources/icon.ico", UriKind.RelativeOrAbsolute)).Stream);
            this.notifyIcon.ShowBalloonTip(1000);
            this.notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;

            itemAutoStart = new System.Windows.Forms.MenuItem("开机启动");
            itemAutoStart.Click += ItemAutoStart_Click;
            itemExit = new System.Windows.Forms.MenuItem("退出");
            itemExit.Click += ItemExit_Click;
            System.Windows.Forms.MenuItem[] items = new System.Windows.Forms.MenuItem[] { itemAutoStart, itemExit };
            this.notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(items);
        }

        private void ItemAutoStart_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MenuItem item = sender as System.Windows.Forms.MenuItem;
            item.Checked = !item.Checked;
            string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (item.Checked)
            {
                try
                {
                    string linkFile = startupPath + "\\VolumnUtility.lnk";
                    var shell = new IWshRuntimeLibrary.WshShell();
                    var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(linkFile);
                    shortcut.TargetPath = Assembly.GetEntryAssembly().Location;
                    shortcut.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    shortcut.Save();

                    XDocument doc = new XDocument();
                    XElement root = new XElement("config");
                    XElement startup = new XElement("autostart");
                    startup.SetValue(1);
                    root.Add(startup);
                    doc.Add(root);
                    doc.Save(AppDomain.CurrentDomain.BaseDirectory + "config.xml");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                string[] files = Directory.GetFiles(startupPath);
                foreach (var file in files)
                {
                    if (Path.GetFileName(file).Trim() == "VolumnUtility.lnk")
                    {
                        System.IO.File.Delete(file);
                        break;
                    }
                }
                XDocument doc = new XDocument();
                XElement root = new XElement("config");
                XElement startup = new XElement("autostart");
                startup.SetValue(0);
                root.Add(startup);
                doc.Add(root);
                doc.Save(AppDomain.CurrentDomain.BaseDirectory + "config.xml");
            }

        }

        private void ItemExit_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void NotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            bool? muted = SystemVolume.GetMasterVolumeMute();
            if (muted != null)
            {
                if (muted.Value)
                {
                    SystemVolume.SetMasterVolumeMute(false);
                    float volume = SystemVolume.GetMasterVolume();
                    tbCurrVolumn.Text = ((int)volume).ToString();
                    HeightTo(volume / 100 * gd.ActualHeight);
                }
                else
                {
                    SystemVolume.SetMasterVolumeMute(true);
                    MuteVolume();
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "config.xml"))
            {
                try
                {
                    XDocument doc = XDocument.Load(AppDomain.CurrentDomain.BaseDirectory + "config.xml");
                    var ele = doc.Root.Element("autostart");
                    string value = ele.Value;
                    if (value == "1")
                    {
                        itemAutoStart.PerformClick();
                    }
                }
                catch
                {

                }
            }
            bool? isMuted = SystemVolume.GetMasterVolumeMute();
            if (isMuted != null && isMuted.Value)
            {
                MuteVolume();
            }
            currGd.Height = gd.ActualHeight * SystemVolume.GetMasterVolume() / 100;
        }

        private void gd_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            bool? muted = SystemVolume.GetMasterVolumeMute();
            if (e.ClickCount == 2)
            {
                if (muted != null)
                {
                    if (muted.Value)
                    {
                        SystemVolume.SetMasterVolumeMute(false);
                        float volume = SystemVolume.GetMasterVolume();
                        tbCurrVolumn.Text = ((int)volume).ToString();
                        HeightTo(volume / 100 * gd.ActualHeight);
                    }
                    else
                    {
                        SystemVolume.SetMasterVolumeMute(true);
                        MuteVolume();
                    }
                }
                e.Handled = true;
                return;
            }
            if (muted.Value)
            {
                SystemVolume.SetMasterVolumeMute(false);
            }
            System.Windows.Point p = e.GetPosition(gd);
            float volumn = (float)((gd.ActualHeight - p.Y) / gd.ActualHeight * 100);
            SystemVolume.SetMasterVolume(volumn);
            tbCurrVolumn.Text = ((int)volumn).ToString();
            HeightTo(volumn / 100 * gd.ActualHeight);
        }

        private void HeightTo(double height)
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.To = height;
            anim.Duration = new Duration(TimeSpan.FromMilliseconds(100));
            currGd.BeginAnimation(Grid.HeightProperty, anim);


            DoubleAnimation opacityAnim = new DoubleAnimation();
            opacityAnim.To = 1;
            opacityAnim.Duration = new Duration(TimeSpan.FromMilliseconds(300));
            opacityAnim.Completed += OpacityAnim_Completed;
            currVolumnGd.BeginAnimation(Grid.OpacityProperty, opacityAnim);
        }

        private void MuteVolume()
        {
            tbCurrVolumn.Text = "X";
            HeightTo(0);
        }

        private void OpacityAnim_Completed(object sender, EventArgs e)
        {
            DoubleAnimation opacityZeroAnim = new DoubleAnimation();
            opacityZeroAnim.To = 0;
            opacityZeroAnim.BeginTime = TimeSpan.FromSeconds(1);
            opacityZeroAnim.Duration = new Duration(TimeSpan.FromMilliseconds(300));
            currVolumnGd.BeginAnimation(Grid.OpacityProperty, opacityZeroAnim);
        }
    }
}
