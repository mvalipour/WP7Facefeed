using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FaceFeed.Model;
using System.Threading;
using System.IO.IsolatedStorage;
using FaceFeed.Tasks;
using Microsoft.Phone.Scheduler;

namespace FaceFeed
{
    public partial class App : Application
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                
                //PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }


            FBDataAccess.Failed += (a, b) => LoadingManager.ShowMessage(b.Data);
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            SetupBackgroundAgent();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }

            e.Handled = true;

            LoadingManager.GenericError(e.Exception);
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
            e.Handled = true;
            LoadingManager.GenericError(e.ExceptionObject);
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion

        public static App MyApp
        {
            get { return Current as App; }
        }

        public void NavigateToLogin()
        {
            RootFrame.NavigateTo("/Pages/LoginE.xaml");
        }

        static string APP_VERSION = "1.3";
        public static bool IsWhatsNewShown
        {
            get
            {
                return (bool)(IsolatedStorageSettings.ApplicationSettings.TryGet("WhatsNew." + APP_VERSION) ?? false);
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["WhatsNew." + APP_VERSION] = value;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        static string APP_ALERT_KEY = "MarketplaceOnly";
        public static bool IsAlertShown
        {
            get
            {
                return (bool)(IsolatedStorageSettings.ApplicationSettings.TryGet("Alert." + APP_ALERT_KEY) ?? false);
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["Alert." + APP_ALERT_KEY] = value;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        internal static void ShowAlert()
        {
            MessageBox.Show("لطفا این نرم افزار و تمامی نرم افزارهای ما را از طریق مارکت پلیس نصب کنید. در غیر این صورت شما تغییرات نرم افزار و ویژگی های جدید را دریافت نخواهید کرد.", "توجه", MessageBoxButton.OK);
        }

        const string _BackgroundAgentTaskName = "BackgroundTask";
        public static void SetupBackgroundAgent()
        {
            try
            {
                var existingTask = ScheduledActionService.Find(_BackgroundAgentTaskName);
                if (existingTask == null)
                {
                    existingTask = new PeriodicTask(_BackgroundAgentTaskName)
                    {
                        Description = "Notifies any new notification and updates the live tile of any item pinned to the home screen.",
                    };
                    ScheduledActionService.Add(existingTask);
                }
                else
                {
                    // renew the task
                    ScheduledActionService.Remove(_BackgroundAgentTaskName);
                    ScheduledActionService.Add(existingTask);
                }


                //test
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    ScheduledActionService.LaunchForTest(_BackgroundAgentTaskName, TimeSpan.FromSeconds(10));
                }
            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("BNS Error: The action is disabled"))
                {
                    //MessageBox.Show("Background agents for this application have been disabled by the user.");
                }

                if (exception.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added."))
                {
                    // No user action required. The system prompts the user when the hard limit of periodic tasks has been reached.
                }
            }
            catch (Exception ex)
            {
                // nothing!
            }
        }

    }
}