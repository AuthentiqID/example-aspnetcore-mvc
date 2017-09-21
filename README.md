# Authentiq Sample for ASP.NET Core MVC

This package is an example on how to add [Authentiq](https://www.authentiq.com/developers/?utm_source=github&utm_medium=readme&utm_campaign=example-aspnetcore-mvc) authentication to you MVC App.


## Requirements

* .[NET Core 2.0 SDK](https://www.microsoft.com/net/download/core)


## To run this project

1. Run the application from the command line:

```bash
    dotnet run
```

2. Visit `http://localhost:5002` in your web browser to view this example website.


## Register a new client with Authentiq

This example site uses a pre-configured test client at Authentiq in order you can run this example at `http://localhost:5002`, if you want to integrate Authentiq to your own site you will have to register your own client.

1. First go to: [Authentiq Dashboard](https://dashboard.authentiq.com) and sign in.
2. Create a new "Server side" application.
3. Fill out any required fields such as the client name and provide your logo URL.
4. Fill your URL in the "Redirect URIs": `https://YOUR_SITE.COM/signin-authentiq`
5. Fill your URL in the "Post logout Redirect URIs": `https://YOUR_SITE.COM/signout-callback-authentiq`
6. Fill your URL for the "Backchannel logout URL": `https://YOUR_SITE.COM/signout-callback-authentiq`
7. Once you have registered, enter the created application credentials into the [appsettings.json](appsettings.json) file, under the Authentiq section.


## Important parts of this example

### 1. Register the Cookie and OIDC Authentication handlers

```csharp
// Startup.cs

public void ConfigureServices(IServiceCollection services)
{
  // Add authentication services
  services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
  })
  .AddCookie()
  .AddOpenIdConnect("Authentiq", options => {
    // Set the authority to the Authentiq Provider
    options.Authority = "https://connect.authentiq.io";

    // Configure the Authentiq Client ID and Client Secret
    options.ClientId = Configuration["Authentiq:ClientId"];
    options.ClientSecret = Configuration["Authentiq:ClientSecret"];

    // Set response type to: code id_token
    options.ResponseType = "code id_token";

    // Configure the Claims Issuer to be Authentiq
    options.ClaimsIssuer = "Authentiq";

    // Configure the scopes requested from user
    // Check the supported [Identity Claims for Authentiq](http://developers.authentiq.io/#identity-claims)
    options.Scope.Add("openid");
    options.Scope.Add("aq:push");

    // email shall be required and verified (signed)
    options.Scope.Add("email~rs");

    // Request additional scopes which can be opted out by the user
    //options.Scope.Add("phone");
    //options.Scope.Add("address");
    //options.Scope.Add("aq:location");
    //options.Scope.Add("profile");

    // Set the callback path, so that Authentiq will call back to http://localhost:5002/signin-authentiq 
    // check that you have added this full URL in the Authentiq dashboard at "Redirect URIs"
    options.CallbackPath = new PathString("/signin-authentiq");
    
    // The UserInfo endpoint does not return any additional claims next to the ones returned in the id_token
    options.GetClaimsFromUserInfoEndpoint = false;

    options.SaveTokens = true;
  });

  // Add framework services.
  services.AddMvc();
}
```

### 2. Register the Authentication middleware

```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

    loggerFactory.AddConsole(Configuration.GetSection("Logging"));
    loggerFactory.AddDebug();

    if (env.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
    }
    else
    {
      app.UseExceptionHandler("/Home/Error");
    }

  app.UseAuthentication();
  app.UseStaticFiles();
  app.UseMvcWithDefaultRoute();
}
```

### 3. Log the user in

```csharp
// Controllers/HomeController.cs

public async Task Login()
{
  await HttpContext.ChallengeAsync("Authentiq");
}
```

### 4. Log the user out

To log the user out, we have to call the `SignOutAsync` method for both the Authentiq OIDC middleware as well as the Cookie middleware.

```csharp
// Controllers/HomeController.cs

public async Task Logout()
{
  await HttpContext.SignOutAsync("Authentiq");
  await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
}
```
