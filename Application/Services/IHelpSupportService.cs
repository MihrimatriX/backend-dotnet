using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface IHelpSupportService
    {
        // Help Articles
        Task<BaseResponseDto<List<HelpArticleDto>>> GetHelpArticlesAsync(string? category = null, int pageNumber = 1, int pageSize = 10);
        Task<BaseResponseDto<HelpArticleDto>> GetHelpArticleByIdAsync(int articleId);
        Task<BaseResponseDto<HelpArticleDto>> CreateHelpArticleAsync(CreateHelpArticleDto createHelpArticleDto);
        Task<BaseResponseDto<HelpArticleDto>> UpdateHelpArticleAsync(int articleId, UpdateHelpArticleDto updateHelpArticleDto);
        Task<BaseResponseDto<string>> DeleteHelpArticleAsync(int articleId);
        Task<BaseResponseDto<List<HelpArticleDto>>> SearchHelpArticlesAsync(string searchTerm, int pageNumber = 1, int pageSize = 10);

        // Support Tickets
        Task<BaseResponseDto<List<SupportTicketDto>>> GetUserSupportTicketsAsync(int userId, int pageNumber = 1, int pageSize = 10);
        Task<BaseResponseDto<SupportTicketDto>> GetSupportTicketByIdAsync(int ticketId, int userId);
        Task<BaseResponseDto<SupportTicketDto>> CreateSupportTicketAsync(int userId, CreateSupportTicketDto createSupportTicketDto);
        Task<BaseResponseDto<SupportTicketDto>> UpdateSupportTicketAsync(int ticketId, int userId, UpdateSupportTicketDto updateSupportTicketDto);
        Task<BaseResponseDto<string>> CloseSupportTicketAsync(int ticketId, int userId);

        // Support Messages
        Task<BaseResponseDto<List<SupportMessageDto>>> GetSupportTicketMessagesAsync(int ticketId, int userId);
        Task<BaseResponseDto<SupportMessageDto>> AddSupportMessageAsync(int ticketId, int userId, CreateSupportMessageDto createSupportMessageDto);

        // FAQ
        Task<BaseResponseDto<List<FaqDto>>> GetFaqsAsync(string? category = null, int pageNumber = 1, int pageSize = 10);
        Task<BaseResponseDto<FaqDto>> GetFaqByIdAsync(int faqId);
        Task<BaseResponseDto<FaqDto>> CreateFaqAsync(CreateFaqDto createFaqDto);
        Task<BaseResponseDto<FaqDto>> UpdateFaqAsync(int faqId, UpdateFaqDto updateFaqDto);
        Task<BaseResponseDto<string>> DeleteFaqAsync(int faqId);

        // Contact Form
        Task<BaseResponseDto<string>> SubmitContactFormAsync(ContactFormDto contactFormDto);

        // Statistics
        Task<BaseResponseDto<object>> GetHelpSupportStatsAsync();
    }
}
