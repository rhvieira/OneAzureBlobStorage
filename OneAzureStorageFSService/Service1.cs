using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneAzureStorageFSService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }
        Process _hostprocess = null;
        ProcessStartInfo _processinfo = null;
        protected override void OnStart(string[] args)
        {
            string storagedata = "OneAzureStorageFS.exe";

            Task t = new Task(() =>
            {
                while(true)
                {

                    if (_hostprocess == null )
                    {
                        _processinfo = new ProcessStartInfo();
                        _processinfo.FileName = AppDomain.CurrentDomain.BaseDirectory + storagedata;
                        _processinfo.UseShellExecute = false;
                  

                        _hostprocess = Process.Start(_processinfo);
                    }
                    else
                    {
                        if(_hostprocess.HasExited)
                        {
                            _hostprocess = Process.Start(_processinfo);
                        }
                    }

                    Thread.Sleep(2000);
                }
            });
            t.Start();
        }

        protected override void OnStop()
        {
            if (!_hostprocess.HasExited)
            {
                _hostprocess.Kill();
            }
        }
    }
}
