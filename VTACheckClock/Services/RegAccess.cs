using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using VTACheckClock.Models;
using VTACheckClock.Services.Libs;

namespace VTACheckClock.Services
{
    class RegAccess
    {
        /// <summary>
        /// Obtiene las configuraciones generales almacenadas en el Registro de Windows en un objeto MainSettings.
        /// </summary>
        /// <returns>Objeto MainSettings con la información de configuración.</returns>
        public static MainSettings? GetMainSettings()
        {
            try {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    RegistryKey? la_key = The_key();

                   if (la_key != null) {
                        SimpleAES? aes_crypt = new();
                        byte[]? employees_host = la_key.GetValue("employees_host") as byte[];
                        byte[]? websocket_enabled = SetNullValue(la_key.GetValue("websocket_enabled"));
                        byte[]? websocket_host = SetNullValue(la_key.GetValue("websocket_host"));
                        byte[]? websocket_port = SetNullValue(la_key.GetValue("websocket_port"));
                        byte[]? pusher_app_id = SetNullValue(la_key.GetValue("pusher_app_id"));
                        byte[]? pusher_key = SetNullValue(la_key.GetValue("pusher_key"));
                        byte[]? pusher_secret = SetNullValue(la_key.GetValue("pusher_secret"));
                        byte[]? pusher_cluster = SetNullValue(la_key.GetValue("pusher_cluster"));
                        byte[]? event_name = SetNullValue(la_key.GetValue("event_name"));
                        byte[]? ws_url = SetNullValue(la_key.GetValue("ws_url"));
                        byte[]? db_server = SetNullValue(la_key.GetValue("db_server"));
                        byte[]? db_name = SetNullValue(la_key.GetValue("db_name"));
                        byte[]? db_user = SetNullValue(la_key.GetValue("db_user"));
                        byte[]? db_pass = SetNullValue(la_key.GetValue("db_pass"));
                        byte[]? ftp_url = SetNullValue(la_key.GetValue("ftp_url"));
                        byte[]? ftp_port = SetNullValue(la_key.GetValue("ftp_port"));
                        byte[]? ftp_user = SetNullValue(la_key.GetValue("ftp_user"));
                        byte[]? ftp_pass = SetNullValue(la_key.GetValue("ftp_pass"));
                        byte[]? path_tmp = SetNullValue(la_key.GetValue("path_tmp"));
                        byte[]? logo = SetNullValue(la_key.GetValue("logo"));

                        MainSettings la_resp = new() {
                            Ws_url = ws_url != null ? aes_crypt.DecryptFromBytes(ws_url) : "",
                            Db_server = db_server != null ? aes_crypt.DecryptFromBytes(db_server) : "",
                            Db_name = db_name != null ? aes_crypt.DecryptFromBytes(db_name) : "",
                            Db_user = db_user != null ? aes_crypt.DecryptFromBytes(db_user) : "",
                            Db_pass = db_pass != null ? aes_crypt.DecryptFromBytes(db_pass) : "",
                            Ftp_url = ftp_url != null ? aes_crypt.DecryptFromBytes(ftp_url) : "",
                            Ftp_port = ftp_port != null ? aes_crypt.DecryptFromBytes(ftp_port) : "",
                            Ftp_user = ftp_user != null ? aes_crypt.DecryptFromBytes(ftp_user): "",
                            Ftp_pass = ftp_pass != null ? aes_crypt.DecryptFromBytes(ftp_pass): "",
                            Path_tmp = path_tmp != null ? aes_crypt.DecryptFromBytes(path_tmp): "",
                            Logo = logo != null ? aes_crypt.DecryptFromBytes(logo): "",
                            //These parameters are optionals so can be empty.
                            Employees_host = (employees_host != null) ? aes_crypt.DecryptFromBytes(employees_host) : "",
                            Websocket_enabled = (websocket_enabled != null) ? Convert.ToBoolean(aes_crypt.DecryptFromBytes(websocket_enabled)) : false,
                            Websocket_host = (websocket_host != null) ? aes_crypt.DecryptFromBytes(websocket_host) : "",
                            Websocket_port = (websocket_port != null) ? aes_crypt.DecryptFromBytes(websocket_port) : "",
                            Pusher_app_id = (pusher_app_id != null) ? aes_crypt.DecryptFromBytes(pusher_app_id) : "",
                            Pusher_key = (pusher_key != null) ? aes_crypt.DecryptFromBytes(pusher_key) : "",
                            Pusher_secret = (pusher_secret != null) ? aes_crypt.DecryptFromBytes(pusher_secret) : "",
                            Pusher_cluster = (pusher_cluster != null) ? aes_crypt.DecryptFromBytes(pusher_cluster) : "",
                            Event_name = (event_name != null) ? aes_crypt.DecryptFromBytes(event_name) : ""
                        };

                        la_key.Close();
                        aes_crypt = null;

                        return la_resp;
                   }
                   return null;
                }
                return null;
            } catch {
                return null;
            }
        }

        public static byte[]? SetNullValue(object? objVal)
        {
            return string.IsNullOrEmpty(objVal?.ToString()) ? null : (byte[])objVal;
        }

        /// <summary>
        /// Abre la clave del registro con la que trabajará el sistema (Editor del Registro de Windows).
        /// </summary>
        /// <returns>La clave del registro del sistema.</returns>
        public static RegistryKey? The_key()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                RegistryKey? la_resp = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
                la_resp = la_resp.OpenSubKey(@"SOFTWARE", true);
                la_resp = la_resp.CreateSubKey(GlobalVars.DefRegKey);

                return la_resp;
            }
            return null;
        }

        /// <summary>
        /// Obtiene las configuraciones del Reloj almacenadas en el Registro de Windows en un objeto ClockSettings.
        /// </summary>
        /// <returns>Objeto ClockSettings con la información de configuración.</returns>
        public static ClockSettings? GetClockSettings()
        {
            try {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    RegistryKey? la_key = The_key();

                    if (la_key != null) {
                        SimpleAES? aes_crypt = new();
                        byte[]? timezone = SetNullValue(la_key.GetValue("clock_timezone"));
                        byte[]? clock_office = SetNullValue(la_key.GetValue("clock_office"));
                        byte[]? clock_user = SetNullValue(la_key.GetValue("clock_user"));
                        byte[]? clock_pass = SetNullValue(la_key.GetValue("clock_pass"));
                        byte[]? clock_uuid = SetNullValue(la_key.GetValue("clock_uuid"));

                        ClockSettings la_resp = new() {
                            clock_office = clock_office != null ? int.Parse(aes_crypt.DecryptFromBytes(clock_office)): 0,
                            clock_user = clock_user != null ? aes_crypt.DecryptFromBytes(clock_user): null,
                            clock_pass = clock_pass != null ? aes_crypt.DecryptFromBytes(clock_pass): null,
                            clock_uuid = clock_uuid != null ? aes_crypt.DecryptFromBytes(clock_uuid): null,
                            clock_timezone = (timezone != null) ? aes_crypt.DecryptFromBytes(timezone) : ""
                        };

                        la_key.Close();
                        aes_crypt = null;

                        return la_resp;
                    }
                    return null;
                }
                return null;
            } catch {
                return null;
            }
        }

        /// <summary>
        /// Recupera las configuraciones de la aplicación desde el Registro de Windows.
        /// </summary>
        /// <param name="msetts">Objeto de configuraciones generales que se proporciona como variable de salida.</param>
        /// <param name="csetts">Objeto de configuraciones del reloj que se proporciona como variable de salida.</param>
        /// <returns>True si la operación se realizó con éxito. False de lo contrario.</returns>
        public static bool GetRegSettings(out MainSettings? msetts, out ClockSettings? csetts)
        {
            try {
                msetts = null;
                csetts = null;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    RegistryKey? la_key = The_key();

                    if (la_key != null) {
                        SimpleAES? aes_crypt = new();
                        byte[]? employees_host = SetNullValue(la_key.GetValue("employees_host"));
                        byte[]? websocket_enabled = SetNullValue(la_key.GetValue("websocket_enabled"));
                        byte[]? websocket_host = SetNullValue(la_key.GetValue("websocket_host"));
                        byte[]? websocket_port = SetNullValue(la_key.GetValue("websocket_port"));
                        byte[]? pusher_app_id = SetNullValue(la_key.GetValue("pusher_app_id"));
                        byte[]? pusher_key = SetNullValue(la_key.GetValue("pusher_key"));
                        byte[]? pusher_secret = SetNullValue(la_key.GetValue("pusher_secret"));
                        byte[]? pusher_cluster = SetNullValue(la_key.GetValue("pusher_cluster"));
                        byte[]? event_name = SetNullValue(la_key.GetValue("event_name"));
                        byte[]? ws_url = SetNullValue(la_key.GetValue("ws_url"));
                        byte[]? ftp_url = SetNullValue(la_key.GetValue("ftp_url"));
                        byte[]? ftp_port = SetNullValue(la_key.GetValue("ftp_port"));
                        byte[]? ftp_user = SetNullValue(la_key.GetValue("ftp_user"));
                        byte[]? ftp_pass = SetNullValue(la_key.GetValue("ftp_pass"));
                        byte[]? path_tmp = SetNullValue(la_key.GetValue("path_tmp"));
                        byte[]? logo = SetNullValue(la_key.GetValue("logo"));

                        msetts = new MainSettings {
                            Ws_url = ws_url != null ? aes_crypt.DecryptFromBytes(ws_url) : "",
                            Ftp_url = ftp_url != null ? aes_crypt.DecryptFromBytes(ftp_url) : "",
                            Ftp_port = ftp_port != null ? aes_crypt.DecryptFromBytes(ftp_port) : "",
                            Ftp_user = ftp_user != null ? aes_crypt.DecryptFromBytes(ftp_user) : "",
                            Ftp_pass = ftp_pass != null ? aes_crypt.DecryptFromBytes(ftp_pass) : "",
                            Path_tmp = path_tmp != null ? aes_crypt.DecryptFromBytes(path_tmp) : "",
                            Logo = logo != null ? aes_crypt.DecryptFromBytes(logo) : "",
                            //These parameters are optionals so can be empty.
                            Employees_host = (employees_host != null) ? aes_crypt.DecryptFromBytes(employees_host) : "",
                            Websocket_enabled = (websocket_enabled != null) && Convert.ToBoolean(aes_crypt.DecryptFromBytes(websocket_enabled)),
                            Websocket_host = (websocket_host != null) ? aes_crypt.DecryptFromBytes(websocket_host) : "",
                            Websocket_port = (websocket_port != null) ? aes_crypt.DecryptFromBytes(websocket_port) : "",
                            Pusher_app_id = (pusher_app_id != null) ? aes_crypt.DecryptFromBytes(pusher_app_id) : "",
                            Pusher_key = (pusher_key != null) ? aes_crypt.DecryptFromBytes(pusher_key) : "",
                            Pusher_secret = (pusher_secret != null) ? aes_crypt.DecryptFromBytes(pusher_secret) : "",
                            Pusher_cluster = (pusher_cluster != null) ? aes_crypt.DecryptFromBytes(pusher_cluster) : "",
                            Event_name = (event_name != null) ? aes_crypt.DecryptFromBytes(event_name) : ""
                        };

                        if (GlobalVars.VTAttModule == 1) {
                            byte[]? timezone = SetNullValue(la_key.GetValue("clock_timezone"));
                            byte[]? clock_office = SetNullValue(la_key.GetValue("clock_office"));
                            byte[]? clock_user = SetNullValue(la_key.GetValue("clock_user"));
                            byte[]? clock_pass = SetNullValue(la_key.GetValue("clock_pass"));
                            byte[]? clock_uuid = SetNullValue(la_key.GetValue("clock_uuid"));

                            csetts = new ClockSettings {
                                clock_office = clock_office != null ? int.Parse(aes_crypt.DecryptFromBytes(clock_office)) : 0,
                                clock_user = clock_user != null ? aes_crypt.DecryptFromBytes(clock_user) : null,
                                clock_pass = clock_pass != null ? aes_crypt.DecryptFromBytes(clock_pass) : null,
                                clock_uuid = clock_uuid != null ? aes_crypt.DecryptFromBytes(clock_uuid) : null,
                                clock_timezone = (timezone != null) ? aes_crypt.DecryptFromBytes(timezone) : ""
                            };

                            GlobalVars.TimeZone = csetts.clock_timezone;
                        } else {
                            csetts = null;
                        }

                        la_key.Close();
                        aes_crypt = null;

                        return true;
                    }
                    return false;
                }
                return false;
            } catch {
                msetts = null;
                csetts = null;
                return false;
            }
        }

        /// <summary>
        /// Obtiene la información de la oficina de trabajo del Registro de Windows.
        /// </summary>
        /// <returns>Objeto con la información de la oficina configurada.</returns>
        public static OfficeData GetOffRegData()
        {
            int.TryParse(GetRegValue("office_id"), out int off_id);

            return new OfficeData {
                Offid = off_id,
                Offname = GetRegValue("office_name"),
                Offdesc = GetRegValue("office_desc")
            };
        }

        /// <summary>
        /// Método universal para recuperar valores del Registro de Windows.
        /// </summary>
        /// <param name="nom_val">Nombre del valor que se recuperará.</param>
        /// <returns>Cadena de texto con el valor solicitado, o null en caso de fallo.</returns>
        public static string? GetRegValue(string nom_val)
        {
            try {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    RegistryKey? la_key = The_key();

                    if (la_key != null) {
                        string? la_resp;
                        byte[]? val_name = SetNullValue(la_key.GetValue(nom_val));

                        SimpleAES? aes_crypt = new();

                        la_resp = val_name != null ? aes_crypt.DecryptFromBytes(val_name): null;
                        la_key.Close();
                        aes_crypt = null;

                        return la_resp;
                    }
                }
                return null;
            } catch {
                return string.Empty;
            }
        }

        /// <summary>
        /// Recupera los parámetros del sistema almacenados en el Registro de Windows.
        /// </summary>
        /// <returns>Cadena de texto con la lista de parámetros.</returns>
        public static string? GetSysParams()
        {
            return GetRegValue("db_params");
        }

        #region Guardar configuraciones
        /// <summary>
        /// Método universal para la escritura de valores al Registro de Windows.
        /// </summary>
        /// <param name="nom_val">Nombre del valor que será escrito.</param>
        /// <param name="val_val">Valor asignado.</param>
        /// <returns>True si la operación se completó con éxito. False de lo contrario.</returns>
        public static bool SetRegValue(string nom_val, string? val_val)
        {
            bool la_resp = false;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                RegistryKey? la_key = The_key();

                if (la_key != null) {
                    SimpleAES? aes_crypt = new();

                    try {
                        la_key.SetValue(nom_val, aes_crypt.EncryptToBytes(val_val));
                        la_resp = true;
                    } catch {
                        la_resp = false;
                    } finally  {
                        la_key.Close();
                    }
                }
            }

            return la_resp;
        }

        public static bool SetDBConSettings(MainSettings msettings)
        {
            bool la_resp;
            try {
                SetRegValue("db_server", msettings.Db_server);
                SetRegValue("db_name", msettings.Db_name);
                SetRegValue("db_user", msettings.Db_user);
                SetRegValue("db_pass", msettings.Db_pass);
                la_resp = true;
            } catch {
                la_resp = false;
            }
            return la_resp;
        }

        /// <summary>
        /// Guarda todas las configuraciones del sistema en el Registro de Windows.
        /// </summary>
        /// <param name="msettings">Objeto MainSettings con las configuraciones generales que se guardarán.</param>
        /// <param name="csettings">Objeto ClockSettings con las configuraciones del Reloj que se guardarán.</param>
        /// <returns>True si las configuraciones fueron escritas correctamente.</returns>
        public static bool SetRegSettings(MainSettings msettings, ClockSettings? csettings)
        {
            bool la_resp = false;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                RegistryKey? la_key = The_key();

                if (la_key != null) {
                    SimpleAES? aes_crypt = new();

                    try {
                        la_key.SetValue("path_tmp", aes_crypt.EncryptToBytes(msettings.Path_tmp));
                        la_key.SetValue("logo", aes_crypt.EncryptToBytes(msettings.Logo));
                        la_key.SetValue("ftp_url", aes_crypt.EncryptToBytes(msettings.Ftp_url));
                        la_key.SetValue("ftp_port", aes_crypt.EncryptToBytes(msettings.Ftp_port));
                        la_key.SetValue("ftp_user", aes_crypt.EncryptToBytes(msettings.Ftp_user));
                        la_key.SetValue("ftp_pass", aes_crypt.EncryptToBytes(msettings.Ftp_pass));
                        la_key.SetValue("employees_host", aes_crypt.EncryptToBytes(msettings.Employees_host));
                        la_key.SetValue("websocket_enabled", aes_crypt.EncryptToBytes(msettings.Websocket_enabled.ToString()));
                        la_key.SetValue("websocket_host", aes_crypt.EncryptToBytes(msettings.Websocket_host));
                        la_key.SetValue("websocket_port", aes_crypt.EncryptToBytes(msettings.Websocket_port));
                        la_key.SetValue("pusher_app_id", aes_crypt.EncryptToBytes(msettings.Pusher_app_id));
                        la_key.SetValue("pusher_key", aes_crypt.EncryptToBytes(msettings.Pusher_key));
                        la_key.SetValue("pusher_secret", aes_crypt.EncryptToBytes(msettings.Pusher_secret));
                        la_key.SetValue("pusher_cluster", aes_crypt.EncryptToBytes(msettings.Pusher_cluster));
                        la_key.SetValue("event_name", aes_crypt.EncryptToBytes(msettings.Event_name));

                        if (GlobalVars.VTAttModule == 1) {
                            la_key.SetValue("clock_office", aes_crypt.EncryptToBytes(csettings.clock_office.ToString()));
                            la_key.SetValue("clock_user", aes_crypt.EncryptToBytes(csettings.clock_user));
                            la_key.SetValue("clock_pass", aes_crypt.EncryptToBytes(csettings.clock_pass));
                            la_key.SetValue("clock_timezone", aes_crypt.EncryptToBytes(csettings.clock_timezone));
                        }

                        la_resp = true;
                    } catch {
                        la_resp = false;
                    }
                    finally {
                        la_key.Close();
                    }
                }
            }
            return la_resp;
        }

        /// <summary>
        /// Guarda la URL del Servicio Web correspondiente en el registro, encriptada.
        /// </summary>
        /// <param name="the_url">URL del Servicio Web.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public static bool SetWSSettings(string the_url)
        {
            return SetRegValue("ws_url", the_url);
        }

        /// <summary>
        /// Almacena los parámetros del sistema en el Registro de Windows.
        /// </summary>
        /// <param name="params_str">Cadena de texto con la lista de parámetros.</param>
        /// <returns>True si la operación se realizó con éxito. False de lo contrario.</returns>
        public static bool SaveSysParams(string params_str)
        {
            return SetRegValue("db_params", params_str);
        }

        /// <summary>
        /// Almacena en el Registro de Windows la fecha y hora de la última sincronización exitosa con el Servicio Web.
        /// </summary>
        /// <param name="el_timestamp">Objeto DateTime que se guardará.</param>
        /// <returns>True si la operación se realizó con éxito. False de lo contrario.</returns>
        public static bool SaveLastSync(DateTime el_timestamp)
        {
            return SetRegValue("last_sync", (el_timestamp.ToString("yyyyMMddHHmmss")));
        }

        /// <summary>
        /// Guarda la información de la oficina de trabajo en el Registro de Windows.
        /// </summary>
        /// <param name="la_office">Objeto con la información de la oficina para guardar.</param>
        /// <returns>True si la operación se completó con éxito. False de lo contrario.</returns>
        public static bool SaveOffRegData(OfficeData la_office)
        {
            bool r1 = SetRegValue("office_id", la_office.Offid.ToString());
            bool r2 = SetRegValue("office_name", la_office.Offname);
            bool r3 = SetRegValue("office_desc", la_office.Offdesc);

            return (r1 && r2 && r3);
        }
        #endregion
    }
}
