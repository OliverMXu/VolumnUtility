using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace VolumnUtility
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Process process = Process.GetCurrentProcess();
            if (Process.GetProcessesByName(process.ProcessName).Length > 1)
            {
                MessageBox.Show("已有相同实例正在运行");
                Application.Current.Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}
