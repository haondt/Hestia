namespace Hestia.Persistence.Models
{
    public class FoodLogSectionDataModel
    {
        public int Id { get; set; }

        public required int FoodLogId { get; set; }
        public FoodLogDataModel FoodLog { get; set; } = default!;

        public required string Name { get; set; }
        public required int Order { get; set; }
        public ICollection<FoodLogItemDataModel> Items { get; set; } = [];

    }
}
