using Smartway.FileLoaderApi.Entities;

namespace Smartway.FileLoaderApi.Contracts;

public interface ITokenService
{
    string CreateJwtToken(AppUser user);
    string CreateTokenForOneTimeLink();
}
