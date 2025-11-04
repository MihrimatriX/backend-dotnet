using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface IAddressService
    {
        Task<BaseResponseDto<List<AddressDto>>> GetUserAddressesAsync(int userId);
        Task<BaseResponseDto<AddressDto>> GetAddressByIdAsync(int addressId, int userId);
        Task<BaseResponseDto<AddressDto>> CreateAddressAsync(int userId, CreateAddressDto createAddressDto);
        Task<BaseResponseDto<AddressDto>> UpdateAddressAsync(int addressId, int userId, UpdateAddressDto updateAddressDto);
        Task<BaseResponseDto<string>> DeleteAddressAsync(int addressId, int userId);
        Task<BaseResponseDto<AddressDto>> SetDefaultAddressAsync(int addressId, int userId);
    }
}
