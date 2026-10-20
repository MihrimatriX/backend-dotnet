using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface IHelpSupportService
    {
        // Yardım makaleleri
        Task<BaseResponseDto<List<HelpArticleDto>>> GetHelpArticlesAsync(string? category = null, int pageNumber = 1, int pageSize = 10);
        Task<BaseResponseDto<HelpArticleDto>> GetHelpArticleByIdAsync(int articleId);
        Task<BaseResponseDto<HelpArticleDto>> CreateHelpArticleAsync(CreateHelpArticleDto createHelpArticleDto);

        // Destek talepleri
        Task<BaseResponseDto<List<SupportTicketDto>>> GetAllSupportTicketsAdminAsync(int pageNumber = 1, int pageSize = 20);
        Task<BaseResponseDto<List<SupportTicketDto>>> GetUserSupportTicketsAsync(int userId, int pageNumber = 1, int pageSize = 10);
        Task<BaseResponseDto<SupportTicketDto>> CreateSupportTicketAsync(int userId, CreateSupportTicketDto createSupportTicketDto);

        // SSS
        Task<BaseResponseDto<List<FaqDto>>> GetFaqsAsync(string? category = null, int pageNumber = 1, int pageSize = 10);

        // İletişim formu
        Task<BaseResponseDto<string>> SubmitContactFormAsync(ContactFormDto contactFormDto);
    }
}
