// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Authentication.Identity.BlazorWebAssembly.Tests;

[TestClass]
public class IdentityTest
{
    [TestMethod]
    public void TestMasaIdentity()
    {
        var services = new ServiceCollection();
        services.AddMasaIdentity();

        Assert.IsTrue(services.Any<ICurrentPrincipalAccessor, BlazorCurrentPrincipalAccessor>(ServiceLifetime.Scoped));
        Assert.IsTrue(services.Any<IUserSetter>(ServiceLifetime.Scoped));
        Assert.IsTrue(services.Any<IUserContext>(ServiceLifetime.Scoped));
        Assert.IsTrue(services.Any<IMultiTenantUserContext>(ServiceLifetime.Scoped));
        Assert.IsTrue(services.Any<IMultiEnvironmentUserContext>(ServiceLifetime.Scoped));
        Assert.IsTrue(services.Any<IIsolatedUserContext>(ServiceLifetime.Scoped));
    }

    [TestMethod]
    public async Task TestMasaIdentity2Async()
    {
        var services = new ServiceCollection();
        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity>()
        {
            new(new List<Claim>()
            {
                new("sub", "1"),
                new(ClaimType.DEFAULT_USER_NAME, "Jim"),
                new(ClaimType.DEFAULT_USER_ROLE, "[\"1\"]")
            })
        });
        Mock<AuthenticationStateProvider> authenticationStateProvider = new();
        authenticationStateProvider
            .Setup(provider => provider.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(claimsPrincipal));

        services.AddScoped(_ => authenticationStateProvider.Object);
        services.AddMasaIdentity(option =>
        {
            option.UserId = "sub";
        });

        var serviceProvider = services.BuildServiceProvider();
        await serviceProvider.InitializeApplicationAsync();

        Assert.IsTrue(services.Any<ICurrentPrincipalAccessor, BlazorCurrentPrincipalAccessor>(ServiceLifetime.Scoped));
        Assert.IsTrue(services.Any<IUserSetter>(ServiceLifetime.Scoped));
        Assert.IsTrue(services.Any<IUserContext>(ServiceLifetime.Scoped));
        Assert.IsTrue(services.Any<IMultiTenantUserContext>(ServiceLifetime.Scoped));
        Assert.IsTrue(services.Any<IMultiEnvironmentUserContext>(ServiceLifetime.Scoped));
        Assert.IsTrue(services.Any<IIsolatedUserContext>(ServiceLifetime.Scoped));

        
        var userContext = serviceProvider.GetService<IUserContext>();
        Assert.IsNotNull(userContext);
        Assert.AreEqual("1", userContext.UserId);
        Assert.AreEqual("Jim", userContext.UserName);

        var userRoles = userContext.GetUserRoles<int>().ToList();
        Assert.AreEqual(1, userRoles.Count);
        Assert.AreEqual(1, userRoles[0]);
    }

    [TestMethod]
    public async Task TestIdentityByYamlAsync()
    {
        var services = new ServiceCollection();
        services.AddMasaIdentity(string.Empty);
        services.AddSerialization(builder => builder.UseYaml());

        var serviceProvider = services.BuildServiceProvider();

        var yamlSerializer = serviceProvider.GetRequiredService<IYamlSerializer>();
        var roles = new List<int>()
        {
            1,
            3,
            7,
            12
        };
        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity>()
        {
            new(new List<Claim>()
            {
                new(ClaimType.DEFAULT_USER_ID, "1"),
                new(ClaimType.DEFAULT_USER_NAME, "Jim"),
                new(ClaimType.DEFAULT_USER_ROLE, yamlSerializer.Serialize(roles))
            })
        });
        Mock<AuthenticationStateProvider> authenticationStateProvider = new();
        authenticationStateProvider
            .Setup(provider => provider.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(claimsPrincipal));

        services.AddScoped(_ => authenticationStateProvider.Object);
        serviceProvider = services.BuildServiceProvider();
        await serviceProvider.InitializeApplicationAsync();

        var userContext = serviceProvider.GetService<IUserContext>();
        Assert.IsNotNull(userContext);

        var userRoles = userContext.GetUserRoles<int>().ToList();
        Assert.AreEqual(4, userRoles.Count);
        Assert.IsTrue(userRoles.Contains(1));
        Assert.IsTrue(userRoles.Contains(3));
        Assert.IsTrue(userRoles.Contains(7));
        Assert.IsTrue(userRoles.Contains(12));
    }

    [TestMethod]
    public async Task TestIdentityByYamlAndCustomOptionsAsync()
    {
        var services = new ServiceCollection();
        services.AddMasaIdentity(string.Empty, option =>
        {
            option.UserId = "sub";
        });
        services.AddSerialization(builder => builder.UseYaml());

        var serviceProvider = services.BuildServiceProvider();

        var yamlSerializer = serviceProvider.GetRequiredService<IYamlSerializer>();
        var roles = new List<int>()
        {
            1,
            3,
            7,
            12
        };
        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity>()
        {
            new(new List<Claim>()
            {
                new("sub", "1"),
                new(ClaimType.DEFAULT_USER_NAME, "Jim"),
                new(ClaimType.DEFAULT_USER_ROLE, yamlSerializer.Serialize(roles))
            })
        });
        Mock<AuthenticationStateProvider> authenticationStateProvider = new();
        authenticationStateProvider
            .Setup(provider => provider.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(claimsPrincipal));

        services.AddScoped(_ => authenticationStateProvider.Object);
        serviceProvider = services.BuildServiceProvider();
        await serviceProvider.InitializeApplicationAsync();

        var userContext = serviceProvider.GetService<IUserContext>();
        Assert.IsNotNull(userContext);

        var userRoles = userContext.GetUserRoles<int>().ToList();
        Assert.AreEqual(4, userRoles.Count);
        Assert.IsTrue(userRoles.Contains(1));
        Assert.IsTrue(userRoles.Contains(3));
        Assert.IsTrue(userRoles.Contains(7));
        Assert.IsTrue(userRoles.Contains(12));
    }
}
