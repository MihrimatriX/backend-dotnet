using EcommerceBackend.Application.Common;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Application.Services
{
    public class HelpSupportService : IHelpSupportService
    {
        private const string ContactThanks = "Thank you for your message. We will get back to you soon.";

        private readonly ApplicationDbContext _context;
        private readonly ILogger<HelpSupportService> _logger;

        public HelpSupportService(ApplicationDbContext context, ILogger<HelpSupportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>Yayımlanmış makaleler, en yeni önce.</summary>
        public async Task<BaseResponseDto<List<HelpArticleDto>>> GetHelpArticlesAsync(string? category = null, int pageNumber = 1, int pageSize = 10)
        {
            var paging = Paging.Normalize(pageNumber, pageSize);
            var query = _context.HelpArticles.AsNoTracking().Where(ha => ha.IsPublished && ha.IsActive);
            if (!string.IsNullOrEmpty(category))
                query = query.Where(ha => ha.Category == category);

            var articles = await query
                .OrderByDescending(ha => ha.CreatedAt)
                .ThenByDescending(ha => ha.Id)
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .ToListAsync();

            return BaseResponseDto<List<HelpArticleDto>>.SuccessResult(
                "Help articles retrieved successfully",
                articles.Select(ToDto).ToList());
        }

        /// <summary>Her okumada <c>viewCount</c> artar.</summary>
        public async Task<BaseResponseDto<HelpArticleDto>> GetHelpArticleByIdAsync(int articleId)
        {
            var article = await _context.HelpArticles
                .FirstOrDefaultAsync(ha => ha.Id == articleId && ha.IsPublished && ha.IsActive);
            if (article == null)
                return BaseResponseDto<HelpArticleDto>.NotFound("Help article not found", ErrorCodes.ArticleNotFound);

            article.ViewCount++;
            article.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return BaseResponseDto<HelpArticleDto>.SuccessResult("Help article retrieved successfully", ToDto(article));
        }

        public async Task<BaseResponseDto<HelpArticleDto>> CreateHelpArticleAsync(CreateHelpArticleDto createHelpArticleDto)
        {
            var article = new HelpArticle
            {
                Title = createHelpArticleDto.Title,
                Content = createHelpArticleDto.Content,
                Category = createHelpArticleDto.Category,
                Tags = string.Join(",", (createHelpArticleDto.Tags ?? new List<string>())
                    .Select(t => t.Trim())
                    .Where(t => t.Length > 0)),
                ViewCount = 0,
                IsPublished = createHelpArticleDto.IsPublished,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.HelpArticles.Add(article);
            await _context.SaveChangesAsync();

            return BaseResponseDto<HelpArticleDto>.SuccessResult("Help article created successfully", ToDto(article));
        }

        public async Task<BaseResponseDto<SupportTicketDto>> CreateSupportTicketAsync(int userId, CreateSupportTicketDto createSupportTicketDto)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
            if (user == null)
                return BaseResponseDto<SupportTicketDto>.NotFound("User not found", ErrorCodes.UserNotFound);

            var ticket = new SupportTicket
            {
                UserId = userId,
                Subject = createSupportTicketDto.Subject,
                Description = createSupportTicketDto.Description,
                Category = createSupportTicketDto.Category,
                Priority = string.IsNullOrWhiteSpace(createSupportTicketDto.Priority) ? "Medium" : createSupportTicketDto.Priority,
                Status = "Open",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();

            var ticketDto = new SupportTicketDto
            {
                Id = ticket.Id,
                UserId = ticket.UserId,
                UserName = user.FirstName + " " + user.LastName,
                Subject = ticket.Subject,
                Description = ticket.Description,
                Category = ticket.Category,
                Priority = ticket.Priority,
                Status = ticket.Status,
                AssignedTo = ticket.AssignedTo,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                Messages = new List<SupportMessageDto>()
            };

            return BaseResponseDto<SupportTicketDto>.SuccessResult("Support ticket created successfully", ticketDto);
        }

        public async Task<BaseResponseDto<List<SupportTicketDto>>> GetAllSupportTicketsAdminAsync(int pageNumber = 1, int pageSize = 20) =>
            BaseResponseDto<List<SupportTicketDto>>.SuccessResult(
                "Support tickets retrieved successfully",
                await QueryTicketsAsync(_context.SupportTickets.Where(st => st.IsActive), Paging.Normalize(pageNumber, pageSize)));

        public async Task<BaseResponseDto<List<SupportTicketDto>>> GetUserSupportTicketsAsync(int userId, int pageNumber = 1, int pageSize = 10) =>
            BaseResponseDto<List<SupportTicketDto>>.SuccessResult(
                "Support tickets retrieved successfully",
                await QueryTicketsAsync(_context.SupportTickets.Where(st => st.UserId == userId && st.IsActive), Paging.Normalize(pageNumber, pageSize)));

        public async Task<BaseResponseDto<List<FaqDto>>> GetFaqsAsync(string? category = null, int pageNumber = 1, int pageSize = 10)
        {
            var paging = Paging.Normalize(pageNumber, pageSize);
            var query = _context.Faqs.AsNoTracking().Where(f => f.IsPublished && f.IsActive);
            if (!string.IsNullOrEmpty(category))
                query = query.Where(f => f.Category == category);

            var faqs = await query
                .OrderByDescending(f => f.CreatedAt)
                .ThenByDescending(f => f.Id)
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .Select(f => new FaqDto
                {
                    Id = f.Id,
                    Question = f.Question,
                    Answer = f.Answer,
                    Category = f.Category,
                    ViewCount = f.ViewCount,
                    IsPublished = f.IsPublished,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                })
                .ToListAsync();

            return BaseResponseDto<List<FaqDto>>.SuccessResult("FAQs retrieved successfully", faqs);
        }

        /// <summary>Kalıcı kayıt yok; yalnızca loglanır (§4.15).</summary>
        public Task<BaseResponseDto<string>> SubmitContactFormAsync(ContactFormDto contactFormDto)
        {
            _logger.LogInformation(
                "Contact form received: category={Category}, subject={Subject}, email={Email}",
                contactFormDto.Category, contactFormDto.Subject, contactFormDto.Email);

            return Task.FromResult(BaseResponseDto<string>.SuccessResult("Contact form submitted successfully", ContactThanks));
        }

        private static Task<List<SupportTicketDto>> QueryTicketsAsync(IQueryable<SupportTicket> query, Paging paging) =>
            query
                .AsNoTracking()
                .OrderByDescending(st => st.CreatedAt)
                .ThenByDescending(st => st.Id)
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .Select(st => new SupportTicketDto
                {
                    Id = st.Id,
                    UserId = st.UserId,
                    UserName = st.User.FirstName + " " + st.User.LastName,
                    Subject = st.Subject,
                    Description = st.Description,
                    Category = st.Category,
                    Priority = st.Priority,
                    Status = st.Status,
                    AssignedTo = st.AssignedTo,
                    CreatedAt = st.CreatedAt,
                    UpdatedAt = st.UpdatedAt,
                    Messages = st.Messages
                        .Where(m => m.IsActive)
                        .OrderBy(m => m.CreatedAt)
                        .Select(m => new SupportMessageDto
                        {
                            Id = m.Id,
                            TicketId = m.TicketId,
                            UserId = m.UserId,
                            UserName = m.User.FirstName + " " + m.User.LastName,
                            Message = m.Message,
                            IsFromSupport = m.IsFromSupport,
                            CreatedAt = m.CreatedAt
                        })
                        .ToList()
                })
                .ToListAsync();

        /// <summary><c>Tags</c> virgülle ayrılmış saklanır.</summary>
        private static HelpArticleDto ToDto(HelpArticle a) => new()
        {
            Id = a.Id,
            Title = a.Title,
            Content = a.Content,
            Category = a.Category,
            Tags = (a.Tags ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList(),
            ViewCount = a.ViewCount,
            IsPublished = a.IsPublished,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        };
    }
}
