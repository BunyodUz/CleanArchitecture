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

    public Task<string?> LogoutButtonText()
        => Page.Locator("a:has-text('Log out')").TextContentAsync();

    public Task AssertErrorVisible()
        => Assertions.Expect(Page.GetByText("Invalid username or password")).ToBeVisibleAsync();
}
