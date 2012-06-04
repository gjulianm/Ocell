using System.Windows;
using Microsoft.Phone.Scheduler;

namespace ScheduledTaskAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;

        /// <remarks>
        /// Constructor de ScheduledAgent que inicializa el controlador UnhandledException
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // Suscribirse al controlador de excepciones administradas
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }
        }

        /// Código para ejecutar en excepciones no controladas
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Se ha producido una excepción no controlada; interrumpir el depurador
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Agente que ejecuta una tarea programada
        /// </summary>
        /// <param name="task">
        /// Tarea invocada
        /// </param>
        /// <remarks>
        /// Se llama a este método cuando se invoca una tarea periódica o con uso intensivo de recursos
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            //TODO: Agregar código para realizar la tarea en segundo plano

            NotifyComplete();
        }
    }
}