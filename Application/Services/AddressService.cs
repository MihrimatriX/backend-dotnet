using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Application.Services
{
    public class AddressService : IAddressService
    {
        private readonly ApplicationDbContext _context;

        public AddressService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponseDto<List<AddressDto>>> GetUserAddressesAsync(int userId)
        {
            try
            {
                var addresses = await _context.Addresses
                    .Where(a => a.UserId == userId && a.IsActive)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenByDescending(a => a.CreatedAt)
                    .Select(a => new AddressDto
                    {
                        Id = a.Id,
                        UserId = a.UserId,
                        Title = a.Title,
                        FullAddress = a.FullAddress,
                        City = a.City,
                        District = a.District,
                        PostalCode = a.PostalCode,
                        Country = a.Country,
                        IsDefault = a.IsDefault,
                        PhoneNumber = a.PhoneNumber,
                        CreatedAt = a.CreatedAt,
                        UpdatedAt = a.UpdatedAt
                    })
                    .ToListAsync();

                return BaseResponseDto<List<AddressDto>>.SuccessResult("Addresses retrieved successfully", addresses);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<List<AddressDto>>.ErrorResult($"Error retrieving addresses: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<AddressDto>> GetAddressByIdAsync(int addressId, int userId)
        {
            try
            {
                var address = await _context.Addresses
                    .Where(a => a.Id == addressId && a.UserId == userId && a.IsActive)
                    .Select(a => new AddressDto
                    {
                        Id = a.Id,
                        UserId = a.UserId,
                        Title = a.Title,
                        FullAddress = a.FullAddress,
                        City = a.City,
                        District = a.District,
                        PostalCode = a.PostalCode,
                        Country = a.Country,
                        IsDefault = a.IsDefault,
                        PhoneNumber = a.PhoneNumber,
                        CreatedAt = a.CreatedAt,
                        UpdatedAt = a.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (address == null)
                {
                    return BaseResponseDto<AddressDto>.ErrorResult("Address not found");
                }

                return BaseResponseDto<AddressDto>.SuccessResult("Address retrieved successfully", address);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<AddressDto>.ErrorResult($"Error retrieving address: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<AddressDto>> CreateAddressAsync(int userId, CreateAddressDto createAddressDto)
        {
            try
            {
                // If this is set as default, remove default from other addresses
                if (createAddressDto.IsDefault)
                {
                    var existingDefaultAddresses = await _context.Addresses
                        .Where(a => a.UserId == userId && a.IsDefault && a.IsActive)
                        .ToListAsync();

                    foreach (var existingAddress in existingDefaultAddresses)
                    {
                        existingAddress.IsDefault = false;
                        existingAddress.UpdatedAt = DateTime.UtcNow;
                    }
                }

                var address = new Address
                {
                    UserId = userId,
                    Title = createAddressDto.Title,
                    FullAddress = createAddressDto.FullAddress,
                    City = createAddressDto.City,
                    District = createAddressDto.District,
                    PostalCode = createAddressDto.PostalCode,
                    Country = createAddressDto.Country,
                    IsDefault = createAddressDto.IsDefault,
                    PhoneNumber = createAddressDto.PhoneNumber,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();

                var addressDto = new AddressDto
                {
                    Id = address.Id,
                    UserId = address.UserId,
                    Title = address.Title,
                    FullAddress = address.FullAddress,
                    City = address.City,
                    District = address.District,
                    PostalCode = address.PostalCode,
                    Country = address.Country,
                    IsDefault = address.IsDefault,
                    PhoneNumber = address.PhoneNumber,
                    CreatedAt = address.CreatedAt,
                    UpdatedAt = address.UpdatedAt
                };

                return BaseResponseDto<AddressDto>.SuccessResult("Address created successfully", addressDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<AddressDto>.ErrorResult($"Error creating address: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<AddressDto>> UpdateAddressAsync(int addressId, int userId, UpdateAddressDto updateAddressDto)
        {
            try
            {
                var address = await _context.Addresses
                    .Where(a => a.Id == addressId && a.UserId == userId && a.IsActive)
                    .FirstOrDefaultAsync();

                if (address == null)
                {
                    return BaseResponseDto<AddressDto>.ErrorResult("Address not found");
                }

                // If this is set as default, remove default from other addresses
                if (updateAddressDto.IsDefault && !address.IsDefault)
                {
                    var existingDefaultAddresses = await _context.Addresses
                        .Where(a => a.UserId == userId && a.IsDefault && a.IsActive && a.Id != addressId)
                        .ToListAsync();

                    foreach (var existingAddress in existingDefaultAddresses)
                    {
                        existingAddress.IsDefault = false;
                        existingAddress.UpdatedAt = DateTime.UtcNow;
                    }
                }

                address.Title = updateAddressDto.Title;
                address.FullAddress = updateAddressDto.FullAddress;
                address.City = updateAddressDto.City;
                address.District = updateAddressDto.District;
                address.PostalCode = updateAddressDto.PostalCode;
                address.Country = updateAddressDto.Country;
                address.IsDefault = updateAddressDto.IsDefault;
                address.PhoneNumber = updateAddressDto.PhoneNumber;
                address.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var addressDto = new AddressDto
                {
                    Id = address.Id,
                    UserId = address.UserId,
                    Title = address.Title,
                    FullAddress = address.FullAddress,
                    City = address.City,
                    District = address.District,
                    PostalCode = address.PostalCode,
                    Country = address.Country,
                    IsDefault = address.IsDefault,
                    PhoneNumber = address.PhoneNumber,
                    CreatedAt = address.CreatedAt,
                    UpdatedAt = address.UpdatedAt
                };

                return BaseResponseDto<AddressDto>.SuccessResult("Address updated successfully", addressDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<AddressDto>.ErrorResult($"Error updating address: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<string>> DeleteAddressAsync(int addressId, int userId)
        {
            try
            {
                var address = await _context.Addresses
                    .Where(a => a.Id == addressId && a.UserId == userId && a.IsActive)
                    .FirstOrDefaultAsync();

                if (address == null)
                {
                    return BaseResponseDto<string>.ErrorResult("Address not found");
                }

                address.IsActive = false;
                address.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return BaseResponseDto<string>.SuccessResult("Address deleted successfully", "Address deleted successfully");
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult($"Error deleting address: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<AddressDto>> SetDefaultAddressAsync(int addressId, int userId)
        {
            try
            {
                var address = await _context.Addresses
                    .Where(a => a.Id == addressId && a.UserId == userId && a.IsActive)
                    .FirstOrDefaultAsync();

                if (address == null)
                {
                    return BaseResponseDto<AddressDto>.ErrorResult("Address not found");
                }

                // Remove default from other addresses
                var existingDefaultAddresses = await _context.Addresses
                    .Where(a => a.UserId == userId && a.IsDefault && a.IsActive && a.Id != addressId)
                    .ToListAsync();

                foreach (var existingAddress in existingDefaultAddresses)
                {
                    existingAddress.IsDefault = false;
                    existingAddress.UpdatedAt = DateTime.UtcNow;
                }

                address.IsDefault = true;
                address.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var addressDto = new AddressDto
                {
                    Id = address.Id,
                    UserId = address.UserId,
                    Title = address.Title,
                    FullAddress = address.FullAddress,
                    City = address.City,
                    District = address.District,
                    PostalCode = address.PostalCode,
                    Country = address.Country,
                    IsDefault = address.IsDefault,
                    PhoneNumber = address.PhoneNumber,
                    CreatedAt = address.CreatedAt,
                    UpdatedAt = address.UpdatedAt
                };

                return BaseResponseDto<AddressDto>.SuccessResult("Default address set successfully", addressDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<AddressDto>.ErrorResult($"Error setting default address: {ex.Message}");
            }
        }
    }
}
