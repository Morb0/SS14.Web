using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SS14.Auth.Shared.Data;

namespace SS14.Web.Areas.Admin.Pages.Users;

public class EditPassword : PageModel
{
    private readonly UserManager<SpaceUser> _userManager;

    public SpaceUser SpaceUser { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }
    
    public sealed class InputModel
    {
        [Required]
        [Display(Name = "New Password")] 
        [DataType(DataType.Password)]
        public string Password { get; set; }   
    }

    public EditPassword(UserManager<SpaceUser> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        SpaceUser = await _userManager.FindByIdAsync(id.ToString());

        if (SpaceUser == null)
            return NotFound("That user does not exist!");

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(Guid id)
    {
        SpaceUser = await _userManager.FindByIdAsync(id.ToString());
        if (SpaceUser == null)
            return NotFound("That user does not exist!");

       
        if (!ModelState.IsValid)
            return Page();

        // Yes you need to do this insanity to change a database column.
        // Truly Microsoft's finest over here.
        var token = await _userManager.GeneratePasswordResetTokenAsync(SpaceUser);
        var result = await _userManager.ResetPasswordAsync(SpaceUser, token, Input.Password);

        if (result.Succeeded)
        {
            TempData["StatusMessage"] = "Password updated!";
        
            return RedirectToPage("./ViewUser", new { id });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        
        return Page();
    }
}