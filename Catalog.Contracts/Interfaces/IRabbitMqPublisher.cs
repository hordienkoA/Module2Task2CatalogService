namespace CartService.BLL.Interfaces
{
    public interface IRabbitMqPublisher
    {
        Task Publish<T>(T message, string queueName);
    }
}
