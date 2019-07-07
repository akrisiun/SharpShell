using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Windows.Forms;

namespace ShellDebugger
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;

            Application.Run(new ShellDebuggerForm());
        }

        static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            Trace.WriteLine(String.Format("FirstChanceException {0}", e.Exception.Message));
        }


        //https://github.com/shellscape/Shellscape.Common/blob/aa5465929e842e4bcc88c29c1cc369122c307bc6/Shellscape.Common/Program.cs
        //private static void InitRemoting()
        //{
        //    ChannelServices.RegisterChannel(new IpcChannel(JumplistChannelName), false);
        //    RemotingConfiguration.RegisterWellKnownServiceType(typeof(RemotingSingleton), JumplistObjectName, WellKnownObjectMode.Singleton);
        //}

        //private static void CallRunningInstance(string[] arguments)
        //{

        //    if (arguments.Length == 0)
        //    {
        //        return;
        //    }

        //    object proxy = RemotingServices.Connect(typeof(RemotingSingleton), "ipc://" + JumplistChannelName + "/" + JumplistObjectName);
        //    RemotingSingleton service = proxy as RemotingSingleton;

        //    try
        //    {
        //        service.Run(arguments);

        //        if (RemoteCallMade != null)
        //        {
        //            RemoteCallMade(arguments);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Utilities.ErrorHelper.Report(ex);
        //    }
        //}

    }



}
