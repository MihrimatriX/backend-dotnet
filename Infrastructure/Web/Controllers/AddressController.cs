using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Services;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class AddressController : ApiControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<BaseResponseDto<List<AddressDto>>>> GetUserAddresses(int userId)
        {
            if (CurrentUserId != userId)
                return Respond(BaseResponseDto<List<AddressDto>>.Forbidden("You can only access your own addresses"));

            return Respond(await _addressService.GetUserAddressesAsync(userId));
        }

        [HttpGet("{addressId}")]
        public async Task<ActionResult<BaseResponseDto<AddressDto>>> GetAddress(int addressId) =>
            Respond(await _addressService.GetAddressByIdAsync(addressId, CurrentUserId));

        [HttpPost]
        public async Task<ActionResult<BaseResponseDto<AddressDto>>> CreateAddress([FromBody] CreateAddressDto createAddressDto) =>
            RespondCreated(await _addressService.CreateAddressAsync(CurrentUserId, createAddressDto));

        [HttpPut("{addressId}")]
        public async Task<ActionResult<BaseResponseDto<AddressDto>>> UpdateAddress(int addressId, [FromBody] UpdateAddressDto updateAddressDto) =>
            Respond(await _addressService.UpdateAddressAsync(addressId, CurrentUserId, updateAddressDto));

        [HttpDelete("{addressId}")]
        public async Task<ActionResult<BaseResponseDto<string>>> DeleteAddress(int addressId) =>
            Respond(await _addressService.DeleteAddressAsync(addressId, CurrentUserId));

        [HttpPut("{addressId}/default")]
        public async Task<ActionResult<BaseResponseDto<AddressDto>>> SetDefaultAddress(int addressId) =>
            Respond(await _addressService.SetDefaultAddressAsync(addressId, CurrentUserId));
    }
}
