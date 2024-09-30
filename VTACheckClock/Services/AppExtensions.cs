using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTACheckClock.Services.Libs;

namespace VTACheckClock.Services
{
    public static class AppExtensions
    {
        public static void Restart(this IClassicDesktopStyleApplicationLifetime app)
        {
            app.MainWindow = null; // Liberar la ventana principal
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown; // Configurar el modo de cierre en "apagado explícito"
            app.Exit += (sender, args) => {
                if (GlobalVars.IsRestart) {
                    var full_app_ddl_path = Environment.GetCommandLineArgs()[0];
                    var full_app_exec_path = Process.GetCurrentProcess().MainModule?.FileName;

                    // Crea una nueva instancia del proceso actual
                    var newProcess = new Process {
                        StartInfo = new ProcessStartInfo {
                            FileName = full_app_exec_path,
                            //UseShellExecute = true
                        }
                    };

                    // Inicia el nuevo proceso
                    newProcess.Start();
                }
            };

            // Termina la instancia actual de la aplicación
            app.Shutdown();
        }
    }
}
