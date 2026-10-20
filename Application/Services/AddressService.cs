using EcommerceBackend.Application.Common;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Application.Services
{
    public class AddressService : IAddressService
    {
        private const string DefaultCountry = "Turkey";

        private readonly ApplicationDbContext _context;

        public AddressService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Varsayılan önce, sonra en yeni.</summary>
        public async Task<BaseResponseDto<List<AddressDto>>> GetUserAddressesAsync(int userId)
        {
            var addresses = await _context.Addresses
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.IsActive)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ThenByDescending(a => a.Id)
                .ToListAsync();

            return BaseResponseDto<List<AddressDto>>.SuccessResult(
                "Addresses retrieved successfully",
                addresses.Select(ToDto).ToList());
        }

        public async Task<BaseResponseDto<AddressDto>> GetAddressByIdAsync(int addressId, int userId)
        {
            var address = await FindOwnedAsync(addressId, userId);
            return address == null
                ? AddressNotFound<AddressDto>()
                : BaseResponseDto<AddressDto>.SuccessResult("Address retrieved successfully", ToDto(address));
        }

        /// <summary>Kullanıcının ilk adresi otomatik varsayılan olur; <c>isDefault:true</c> diğerlerini kaldırır.</summary>
        public async Task<BaseResponseDto<AddressDto>> CreateAddressAsync(int userId, CreateAddressDto createAddressDto)
        {
            var isFirst = !await _context.Addresses.AnyAsync(a => a.UserId == userId && a.IsActive);
            var makeDefault = createAddressDto.IsDefault || isFirst;
            if (makeDefault)
                await ClearDefaultAsync(userId, exceptId: null);

            var address = new Address
            {
                UserId = userId,
                Title = createAddressDto.Title,
                FullAddress = createAddressDto.FullAddress,
                City = createAddressDto.City,
                District = createAddressDto.District,
                PostalCode = createAddressDto.PostalCode,
                Country = NormalizeCountry(createAddressDto.Country),
                IsDefault = makeDefault,
                PhoneNumber = createAddressDto.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return BaseResponseDto<AddressDto>.SuccessResult("Address created successfully", ToDto(address));
        }

        /// <summary><c>country</c> boşsa "Turkey"; <c>isDefault</c> gönderilmezse değişmez.</summary>
        public async Task<BaseResponseDto<AddressDto>> UpdateAddressAsync(int addressId, int userId, UpdateAddressDto updateAddressDto)
        {
            var address = await FindOwnedAsync(addressId, userId);
            if (address == null)
                return AddressNotFound<AddressDto>();

            if (updateAddressDto.IsDefault is { } isDefault)
            {
                if (isDefault)
                    await ClearDefaultAsync(userId, exceptId: addressId);
                address.IsDefault = isDefault;
            }

            address.Title = updateAddressDto.Title;
            address.FullAddress = updateAddressDto.FullAddress;
            address.City = updateAddressDto.City;
            address.District = updateAddressDto.District;
            address.PostalCode = updateAddressDto.PostalCode;
            address.Country = NormalizeCountry(updateAddressDto.Country);
            address.PhoneNumber = updateAddressDto.PhoneNumber;
            address.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return BaseResponseDto<AddressDto>.SuccessResult("Address updated successfully", ToDto(address));
        }

        /// <summary>Yumuşak silme; varsayılan silinirse kalan en yeni adres varsayılan olur.</summary>
        public async Task<BaseResponseDto<string>> DeleteAddressAsync(int addressId, int userId)
        {
            var address = await FindOwnedAsync(addressId, userId);
            if (address == null)
                return AddressNotFound<string>();

            address.IsActive = false;
            address.UpdatedAt = DateTime.UtcNow;

            if (address.IsDefault)
            {
                address.IsDefault = false;
                var successor = await _context.Addresses
                    .Where(a => a.UserId == userId && a.IsActive && a.Id != addressId)
                    .OrderByDescending(a => a.CreatedAt)
                    .ThenByDescending(a => a.Id)
                    .FirstOrDefaultAsync();
                if (successor != null)
                {
                    successor.IsDefault = true;
                    successor.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            return BaseResponseDto<string>.SuccessResult("Address deleted successfully", "Address deleted successfully");
        }

        public async Task<BaseResponseDto<AddressDto>> SetDefaultAddressAsync(int addressId, int userId)
        {
            var address = await FindOwnedAsync(addressId, userId);
            if (address == null)
                return AddressNotFound<AddressDto>();

            await ClearDefaultAsync(userId, exceptId: addressId);
            address.IsDefault = true;
            address.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return BaseResponseDto<AddressDto>.SuccessResult("Default address set successfully", ToDto(address));
        }

        public static AddressDto ToDto(Address a) => new()
        {
            Id = a.Id,
            UserId = a.UserId,
            Title = a.Title,
            FullAddress = a.FullAddress,
            City = a.City,
            District = a.District,
            PostalCode = a.PostalCode,
            Country = string.IsNullOrWhiteSpace(a.Country) ? DefaultCountry : a.Country,
            IsDefault = a.IsDefault,
            PhoneNumber = a.PhoneNumber,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        };

        /// <summary>Başkasının adresi de "bulunamadı" sayılır (404).</summary>
        private Task<Address?> FindOwnedAsync(int addressId, int userId) =>
            _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && a.IsActive);

        private async Task ClearDefaultAsync(int userId, int? exceptId)
        {
            var defaults = await _context.Addresses
                .Where(a => a.UserId == userId && a.IsDefault && a.IsActive && (exceptId == null || a.Id != exceptId))
                .ToListAsync();

            foreach (var existing in defaults)
            {
                existing.IsDefault = false;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        private static string NormalizeCountry(string? country) =>
            string.IsNullOrWhiteSpace(country) ? DefaultCountry : country.Trim();

        private static BaseResponseDto<T> AddressNotFound<T>() =>
            BaseResponseDto<T>.NotFound("Address not found", ErrorCodes.AddressNotFound);
    }
}
