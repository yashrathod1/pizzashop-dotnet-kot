namespace pizzashop_repository.ViewModels;

public class UpdatePreparedItemsViewModel
{
    public int OrderId { get; set; }
    public List<PreparedItemViewModel> Items { get; set; }
}

public class PreparedItemViewModel
{
    public int ItemId { get; set; }
    public int PreparedQuantity { get; set; }
}
