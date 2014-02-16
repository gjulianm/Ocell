using AncoraMVVM.Base.IoC;
using AncoraMVVM.Phone;
using AsyncOAuth;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Marketplace;
using Microsoft.Phone.Shell;
using Ocell.Compatibility;
using Ocell.Controls;
using Ocell.Library;
using Ocell.Library.Twitter;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Navigation;
using Windows.Phone.Speech.VoiceCommands;

namespace Ocell
{
    public partial class App : Application
    {
        /// <summary>
        /// Proporcionar acceso sencillo al marco raíz de la aplicación telefónica.
        /// </summary>
        /// <returns>Marco raíz de la aplicación telefónica.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor para el objeto Application.
        /// </summary>
        public App()
        {
            // Controlador global para excepciones no detectadas.
            UnhandledException += Application_UnhandledException;

            // Inicialización de Silverlight estándar
            InitializeComponent();

            // Inicialización especifica del teléfono
            InitializePhoneApplication();

            // Mostrar información de generación de perfiles gráfica durante la depuración.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Mostrar los contadores de velocidad de marcos actual.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Mostrar las áreas de la aplicación que se están volviendo a dibujar en cada marco.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Habilitar el modo de visualización de análisis de no producción,
                // que muestra áreas de una página que se entregan a la GPU con una superposición coloreada.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Deshabilitar la detección de inactividad de la aplicación estableciendo la propiedad UserIdleDetectionMode del
                // objeto PhoneApplicationService de la aplicación en Disabled.
                // Precaución: solo debe usarse en modo de depuración. Las aplicaciones que deshabiliten la detección de inactividad del usuario seguirán en ejecución
                // y consumirán energía de la batería cuando el usuario no esté usando el teléfono.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            OAuthUtility.ComputeHash = (key, buffer) => { using (var hmac = new HMACSHA1(key)) { return hmac.ComputeHash(buffer); } };

            Dependency.RegisterModule(new PhoneDependencyModule());

            Dependency.Register<IUserProvider, UserProvider>();
            Dependency.Register<IScrollController, DummyScrollController>();
            Dependency.Register<IReadingPositionManager, WP8ReadingPositionManager>();
            Dependency.Register<IInfiniteScroller, WP8InfiniteScroller>();
            Dependency.Register<IListboxCompressionDetector, WP8PullDetector>();
            Dependency.Register<TileManager, WP8TileManager>();

            Dependency.Register<ICryptoManager, CryptoManager>();

            PushNotifications.WPVersion = OSVersion.WP8;

            Config.InitConfig();

            bool isDarkTheme = ((Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible);

            if (Ocell.Library.Config.Background.Value.Type == LightOrDark.Light)
                ThemeManager.ToLightTheme();
            else if (Ocell.Library.Config.Background.Value.Type == LightOrDark.Dark)
                ThemeManager.ToDarkTheme();

            RootFrame.Background = Ocell.Library.Config.Background.Value.GetBrush();

            TrialInformation.RequestTrialInfo = () =>
            {
                var info = new LicenseInformation();
                return info.IsTrial();
            };
        }

        // Código para ejecutar cuando la aplicación se inicia (p.ej. a partir de Inicio)
        // Este código no se ejecutará cuando la aplicación se reactive
        private async void Application_Launching(object sender, LaunchingEventArgs e)
        {
            try
            {
                await VoiceCommandService.InstallCommandSetsFromFileAsync(new Uri("ms-appx:///VoiceReco/VoiceCommandDefinition.xml"));
                TrialInformation.ReloadTrialInfo();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error launching: {0}", ex);
            }
        }

        // Código para ejecutar cuando la aplicación se activa (se trae a primer plano)
        // Este código no se ejecutará cuando la aplicación se inicie por primera vez
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            try
            {
                TrialInformation.ReloadTrialInfo();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error activating: {0}", ex);
            }
        }

        // Código para ejecutar cuando la aplicación se desactiva (se envía a segundo plano)
        // Este código no se ejecutará cuando la aplicación se cierre
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            try
            {
                Controls.ExtendedListBox.RaiseSaveViewports();
                Config.SaveReadPositions();
                Config.SaveAccounts();
                Config.SaveColumns();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error deactivating: {0}", ex);
            }
        }

        // Código para ejecutar cuando la aplicación se cierra (p.ej., al hacer clic en Atrás)
        // Este código no se ejecutará cuando la aplicación se desactive
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            try
            {
                Controls.ExtendedListBox.RaiseSaveViewports();
                Config.SaveReadPositions();
                Config.SaveAccounts();
                Config.SaveColumns();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error closing: {0}", ex);
            }
        }

        // Código para ejecutar si hay un error de navegación
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Ha habido un error de navegación; interrumpir el depurador
                System.Diagnostics.Debugger.Break();
            }

            // TODO: Exception checking.
            // LittleWatson.ReportException(e.Exception, "Ocell App Error");
        }

        // Código para ejecutar en excepciones no controladas
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Se ha producido una excepción no controlada; interrumpir el depurador
                System.Diagnostics.Debugger.Break();
            }

            // TODO: Exception checking.
            // LittleWatson.ReportException(e.ExceptionObject, "Ocell App Error");
        }

        #region Inicialización de la aplicación telefónica

        // Evitar inicialización doble
        private bool phoneApplicationInitialized = false;

        // No agregar ningún código adicional a este método
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Crear el marco pero no establecerlo como RootVisual todavía; esto permite que
            // la pantalla de presentación permanezca activa hasta que la aplicación esté lista para la presentación.
            RootFrame = new TransitionFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Controlar errores de navegación
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Asegurarse de que no volvemos a inicializar
            phoneApplicationInitialized = true;
        }

        // No agregar ningún código adicional a este método
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Establecer el objeto visual raíz para permitir que la aplicación se presente
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Quitar este controlador porque ya no es necesario
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion Inicialización de la aplicación telefónica

    }
}