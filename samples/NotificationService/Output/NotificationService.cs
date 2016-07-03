namespace MyNamespace
{
    public class NotificationService
    {
        public NotificationBinding NotificationPort { get; } = new NotificationBinding("producerName");
    }
}