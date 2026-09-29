using Household.Api.Features.Account.Contracts;
using Household.Api.Features.Account.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationIdentityUser> _userManager;
    private readonly SignInManager<ApplicationIdentityUser> _signInManager;

    public AccountService(UserManager<ApplicationIdentityUser> userManager, SignInManager<ApplicationIdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<bool> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)return false;

        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true
        );

        return result.Succeeded;
    }

    public async Task<bool> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existingUser= await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null) return false;

        var user=new ApplicationIdentityUser
        {
            UserName=request.Email,
            Email=request.Email
        };

        var result = await _userManager.CreateAsync(
            user,
            request.Password
        );

        return result.Succeeded;
    }

    public async Task<bool> SendConfirmationAsync(ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user= await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return false;
        if (user.EmailConfirmed) return false;

        var token=await _userManager.GenerateEmailConfirmationTokenAsync(user);

        //TODO: build confirmation URL
        //TODO: send using configured email provider

        return !token.IsNullOrEmpty();
    }
}