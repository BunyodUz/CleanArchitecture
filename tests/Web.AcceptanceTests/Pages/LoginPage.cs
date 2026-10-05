namespace CleanArchitecture.Web.AcceptanceTests.Pages;

// Login itself happens on Keycloak's hosted login page (this app redirects there), not on
// a page this app serves — the "#username"/"#password"/"#kc-login" selectors below are
// Keycloak's default theme field ids, not this app's markup.
public class LoginPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/login";

    public Task SetEmail(string email)
        => Page.FillAsync("#username", email);

    public Task SetPassword(string password)
        => Page.FillAsync("#password", password);

    public Task ClickLogin()
        => Page.Locator("#kc-login").ClickAsync();

    // "Log out" lives in the account menu in the top bar, so open that menu first.
    public async Task<string?> LogoutButtonText()
    {
        await Page.Locator("button[aria-label='Account menu']").ClickAsync();
        return await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Log out" }).TextContentAsync();
    }

    public Task AssertErrorVisible()
        => Assertions.Expect(Page.GetByText("Invalid username or password")).ToBeVisibleAsync();
}
