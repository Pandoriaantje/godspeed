using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Reporting;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Shell.Views;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;

namespace Neurotoxin.Godspeed.Shell
{

    public partial class App : Application
    {
        private Bootstrapper _bootstrapper;

        private static string _applicationVersion;
        public static string ApplicationVersion
        {
            get
            {
                if (string.IsNullOrEmpty(_applicationVersion))
                {
                    var assembly = Assembly.GetAssembly(typeof(App));
                    var assemblyName = assembly.GetName();
                    _applicationVersion = assemblyName.Version.ToString();
                }
                return _applicationVersion;
            }
        }

        private static string _frameworkVersion;

        public static string FrameworkVersion
        {
            get
            {
                if (string.IsNullOrEmpty(_frameworkVersion))
                {
                    var applicationAssembly = Assembly.GetAssembly(typeof(Application));
                    var fvi = FileVersionInfo.GetVersionInfo(applicationAssembly.Location);
                    _frameworkVersion = fvi.ProductVersion;
                }
                return _frameworkVersion;
            }
        }

        public static string DataDirectory { get; set; }
        public static string PostDirectory { get; set; }
        public static bool ShellInitialized { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Dispatcher.CurrentDispatcher.UnhandledException += UnhandledThreadingException;
            ShutdownMode = ShutdownMode.OnMainWindowClose;

#if (DEBUG)
            RunInDebugMode();
#else
            RunInReleaseMode();
#endif
        }

        private void RunInDebugMode()
        {
            _bootstrapper = new Bootstrapper();
            _bootstrapper.Run();
        }

        private void RunInReleaseMode()
        {
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
            try
            {
                RunInDebugMode();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private void UnhandledThreadingException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            HandleException(e.Exception);
        }

        private void HandleException(Exception ex)
        {
            ex = ex is TargetInvocationException ? ex.InnerException : ex;
            var dialog = _bootstrapper.Resolve<ErrorMessage>(new ParameterOverride("exception", ex));
            dialog.ShowDialog();
            Shutdown(ex.GetType().Name.GetHashCode());
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_bootstrapper != null)
            {
                var fileManager = _bootstrapper.Resolve<FileManagerViewModel>();
                if (!fileManager.IsDisposed) fileManager.Dispose();
                var statistics = _bootstrapper.Resolve<StatisticsViewModel>();
                if (e.ApplicationExitCode != 0) statistics.ApplicationCrashed++;
                statistics.PersistData();

                var userSettings = _bootstrapper.Container.Resolve<IUserSettingsProvider>();
                userSettings.PersistData();

                if (!Debugger.IsAttached && userSettings.DisableUserStatisticsParticipation != true)
                {
                    var commandUsage = new StringBuilder();
                    foreach (var kvp in statistics.CommandUsage)
                    {
                        commandUsage.AppendLine(string.Format("{0}={1}", kvp.Key, kvp.Value));
                    }

                    var serverUsage = new StringBuilder();
                    foreach (var kvp in statistics.ServerUsage)
                    {
                        serverUsage.AppendLine(string.Format("{0}={1}", kvp.Key, kvp.Value));
                    }
                }
            }
            base.OnExit(e);
        }

    }
}