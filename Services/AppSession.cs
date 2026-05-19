using PharmaDesk.Models;
namespace PharmaDesk.Services;
public class AppSession
{
    public User? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser is not null;
    public bool IsAdmin => CurrentUser?.Role?.Name == "Admin";
    public bool IsPharmacist => CurrentUser?.Role?.Name == "Pharmacist";
    public bool CanManageMedicines => IsAdmin || IsPharmacist;
    public void SignIn(User user) => CurrentUser = user;
    public void SignOut() => CurrentUser = null;
}
