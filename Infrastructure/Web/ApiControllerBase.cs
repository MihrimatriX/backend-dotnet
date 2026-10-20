using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceBackend.Infrastructure.Web;

/// <summary>
/// Ortak controller tabanı: kimlik bilgisi okuma ve servis sonucunu HTTP cevabına çevirme.
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Token'daki <c>sub</c>; [Authorize] uçlarda her zaman vardır.</summary>
    protected int CurrentUserId =>
        User.GetUserId() ?? throw new UnauthorizedAccessException("Token does not contain a valid subject.");

    protected bool IsAdmin => User.IsInRole(UserRoles.Admin);

    /// <summary>
    /// Zarfı döner: servis bir durum kodu belirlediyse o (ör. 404, idempotent tekrarda 200), yoksa başarıda
    /// <paramref name="successStatusCode"/>, hatada 400.
    /// </summary>
    protected ObjectResult Respond<T>(BaseResponseDto<T> result, int successStatusCode = StatusCodes.Status200OK) =>
        StatusCode(result.StatusCode ?? (result.Success ? successStatusCode : StatusCodes.Status400BadRequest), result);

    protected ObjectResult RespondCreated<T>(BaseResponseDto<T> result) => Respond(result, StatusCodes.Status201Created);
}
