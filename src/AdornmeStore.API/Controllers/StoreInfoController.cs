using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdornmeStore.API.Controllers;

[ApiController]
[Route("api/store")]
public class StoreInfoController : ControllerBase
{
    // =====================================================
    // ABOUT US
    // =====================================================

    [HttpGet("about")]
    [AllowAnonymous]
    public IActionResult GetAbout()
    {
        return Ok(new
        {
            brandName = "Adornme",

            title = "About Adornme",

            description =
                "Adornme is an online jewellery store offering thoughtfully designed jewellery for everyday elegance and special occasions.",

            values = new[]
            {
                "Quality",
                "Elegant designs",
                "Customer satisfaction",
                "Reliable shopping experience"
            }
        });
    }


    // =====================================================
    // SUPPORT
    // =====================================================

    [HttpGet("support")]
    [AllowAnonymous]
    public IActionResult GetSupport()
    {
        return Ok(new
        {
            title = "Customer Support",

            email = "support@adornme.com",

            phone = "+91-XXXXXXXXXX",

            workingHours = "Monday - Saturday, 10:00 AM - 6:00 PM",

            sections = new[]
            {
                new
                {
                    title = "Order Support",
                    description =
                        "Get help with order status, cancellation and delivery."
                },

                new
                {
                    title = "Product Support",
                    description =
                        "Get help regarding product details, availability and pricing."
                },

                new
                {
                    title = "Payment Support",
                    description =
                        "Get help with payment or transaction related issues."
                },

                new
                {
                    title = "Return Support",
                    description =
                        "Contact support regarding eligible returns and returned orders."
                }
            }
        });
    }
}