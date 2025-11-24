namespace CartService.API.MessageBroker.Messages
{
    public class CatalogItemUpdatedEvent
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
