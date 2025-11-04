using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Services;
using System.Security.Claims;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<BaseResponseDto<List<AddressDto>>>> GetUserAddresses(int userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId != userId)
                {
                    return Forbid("You can only access your own addresses");
                }

                var result = await _addressService.GetUserAddressesAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<List<AddressDto>>.ErrorResult($"Error retrieving user addresses: {ex.Message}"));
            }
        }

        [HttpGet("{addressId}")]
        public async Task<ActionResult<BaseResponseDto<AddressDto>>> GetAddress(int addressId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _addressService.GetAddressByIdAsync(addressId, currentUserId);
                
                if (result.Success && result.Data != null)
                {
                    return Ok(result);
                }
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<AddressDto>.ErrorResult($"Error retrieving address: {ex.Message}"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponseDto<AddressDto>>> CreateAddress([FromBody] CreateAddressDto createAddressDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _addressService.CreateAddressAsync(currentUserId, createAddressDto);
                
                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetAddress), new { addressId = result.Data?.Id }, result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<AddressDto>.ErrorResult($"Error creating address: {ex.Message}"));
            }
        }

        [HttpPut("{addressId}")]
        public async Task<ActionResult<BaseResponseDto<AddressDto>>> UpdateAddress(int addressId, [FromBody] UpdateAddressDto updateAddressDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _addressService.UpdateAddressAsync(addressId, currentUserId, updateAddressDto);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<AddressDto>.ErrorResult($"Error updating address: {ex.Message}"));
            }
        }

        [HttpDelete("{addressId}")]
        public async Task<ActionResult<BaseResponseDto<string>>> DeleteAddress(int addressId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _addressService.DeleteAddressAsync(addressId, currentUserId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<string>.ErrorResult($"Error deleting address: {ex.Message}"));
            }
        }

        [HttpPut("{addressId}/default")]
        public async Task<ActionResult<BaseResponseDto<AddressDto>>> SetDefaultAddress(int addressId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _addressService.SetDefaultAddressAsync(addressId, currentUserId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<AddressDto>.ErrorResult($"Error setting default address: {ex.Message}"));
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Invalid user ID");
        }
    }
}
