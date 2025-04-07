using Microsoft.AspNetCore.Mvc;
using MyWebSite.Models;
using MyWebSite.Services;
using System.Security.Claims;

public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PaymentController(IPaymentService paymentService, IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _paymentService = paymentService;
    }
    public async Task<IActionResult> CreateOrder()
    {
        var url = await _paymentService.doPayment("10000", "");
        return Redirect(url);
    }

    public async Task<ActionResult> ConfirmPaymentClient(MomoViewModel result)
    {
        try
        {
            int carId = (int)TempData["CarId"];
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _context.SaveChangesAsync();


            return View("SuccessPayment");
        }
        catch (Exception ex)
        {
            return View("SuccessPayment");
        }
    }
}