using Prism.Events;

namespace VTACheckClock.Models
{
    /// <summary>
    /// Patrón de Mensajes o eventos para notificar a la Vista y se puedan realizar ciertas operaciones con los controles.
    /// <para>Se envia desde el ViewModel una notificacion y la Vista se subscribe.</para>
    /// </summary>
    public class NotifyView : PubSubEvent<string> {
        //public string? Value { get; set; }
    }
}
