// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Authentication.OpenIdConnect.EFCore.Caches;

public class SyncCache
{
    IClientCache? _clientCache;
    IApiResourceCache? _apiResourceCache;
    IApiScopeCache? _apiScopeCache;
    IIdentityResourceCache? _identityResourceCache;
    DbContext _context;

    public SyncCache(OidcDbContext context, IServiceProvider serviceProvider)
    {
        _clientCache = serviceProvider.GetService<IClientCache>();
        _apiResourceCache = serviceProvider.GetService<IApiResourceCache>();
        _apiScopeCache = serviceProvider.GetService<IApiScopeCache>();
        _identityResourceCache = serviceProvider.GetService<IIdentityResourceCache>();
        _context = context;
    }

    public async Task SyncApiResourceCacheAsync(Guid id)
    {
        if (_apiResourceCache is null) return;

        var apiResource = await ApiResourceQuery().FirstOrDefaultAsync(apiResource => apiResource.Id == id);
        if (apiResource is null) return;
        await _apiResourceCache.SetAsync(apiResource);
    }

    public async Task SyncApiScopeCacheAsync(Guid id)
    {
        if (_apiScopeCache is null) return;

        var apiScope = await ApiScopeQuery().FirstOrDefaultAsync(apiScope => apiScope.Id == id);
        if (apiScope is null) return;
        await _apiScopeCache.SetAsync(apiScope);
    }

    public async Task SyncIdentityResourceCacheAsync(params Guid[] ids)
    {
        if (_identityResourceCache is null) return;

        var identityResources = await IdentityResourceQuery().Where(idrs => ids.Contains(idrs.Id)).ToListAsync();
        if (identityResources.Count == 0) return;
        await _identityResourceCache.SetRangeAsync(identityResources);
    }

    public async Task SyncClientCacheAsync(Guid id)
    {
        if (_clientCache is null) return;

        var client = await ClientQuery().FirstOrDefaultAsync(client => client.Id == id);
        if (client is null) return;
        await _clientCache.SetAsync(client);
    }

    [ExcludeFromCodeCoverage]
    public async Task RemoveApiResourceCacheAsync(ApiResource apiResource)
    {
        if (_apiResourceCache is null) return;

        await _apiResourceCache.RemoveAsync(apiResource);
    }

    [ExcludeFromCodeCoverage]
    public async Task RemoveApiScopeCacheAsync(ApiScope apiScope)
    {
        if (_apiScopeCache is null) return;

        await _apiScopeCache.RemoveAsync(apiScope);
    }

    [ExcludeFromCodeCoverage]
    public async Task RemoveIdentityResourceCacheAsync(IdentityResource identityResource)
    {
        if (_identityResourceCache is null) return;

        await _identityResourceCache.RemoveAsync(identityResource);
    }

    [ExcludeFromCodeCoverage]
    public async Task RemoveClientCacheAsync(Client client)
    {
        if (_clientCache is null) return;

        await _clientCache.RemoveAsync(client);
    }

    [ExcludeFromCodeCoverage]
    public async Task ResetAsync()
    {
        if (_clientCache is not null)
        {
            var clients = await ClientQuery().ToListAsync();
            await _clientCache.ResetAsync(clients);
        }
        if (_apiScopeCache is not null)
        {
            var apiScopes = await ApiScopeQuery().ToListAsync();
            await _apiScopeCache.ResetAsync(apiScopes);
        }
        if (_apiResourceCache is not null)
        {
            var apiResources = await ApiResourceQuery().ToListAsync();
            await _apiResourceCache.ResetAsync(apiResources);
        }
        if (_identityResourceCache is not null)
        {
            var identityResource = await IdentityResourceQuery().ToListAsync();
            await _identityResourceCache.ResetAsync(identityResource);
        }
    }

    private IQueryable<IdentityResource> IdentityResourceQuery()
    {
        return _context.Set<IdentityResource>()
                    .Include(idrs => idrs.UserClaims)
                    .ThenInclude(uc => uc.UserClaim)
                    .Include(idrs => idrs.Properties);
    }

    private IQueryable<ApiScope> ApiScopeQuery()
    {
        return _context.Set<ApiScope>()
                    .Include(apiScope => apiScope.UserClaims)
                    .ThenInclude(apiScope => apiScope.UserClaim)
                    .Include(apiScope => apiScope.Properties);
    }

    private IQueryable<ApiResource> ApiResourceQuery()
    {
        return _context.Set<ApiResource>()
                    .Include(apiResource => apiResource.UserClaims)
                    .ThenInclude(userClaim => userClaim.UserClaim)
                    .Include(apiResource => apiResource.Properties)
                    .Include(apiResource => apiResource.ApiScopes)
                    .ThenInclude(apiScope => apiScope.ApiScope);
    }

    private IQueryable<Client> ClientQuery()
    {
        return _context.Set<Client>()
            .Include(c => c.AllowedGrantTypes)
            .Include(c => c.RedirectUris)
            .Include(c => c.PostLogoutRedirectUris)
            .Include(c => c.Properties)
            .Include(c => c.Claims)
            .Include(c => c.IdentityProviderRestrictions)
            .Include(c => c.AllowedCorsOrigins)
            .Include(c => c.ClientSecrets)
            .Include(c => c.AllowedScopes)
            .AsSplitQuery();
    }
}
