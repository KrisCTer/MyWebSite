namespace MyWebSite.Repositories
{
    public interface ILoyalCustomerRepository
    {
       Task CheckAndAddLoyalCustomerAsync(string userId);
    }
}
