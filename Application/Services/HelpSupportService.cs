using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Application.Services
{
    public class HelpSupportService : IHelpSupportService
    {
        private readonly ApplicationDbContext _context;

        public HelpSupportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponseDto<List<HelpArticleDto>>> GetHelpArticlesAsync(string? category = null, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.HelpArticles
                    .Where(ha => ha.IsPublished && ha.IsActive);

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(ha => ha.Category == category);
                }

                var articles = await query
                    .OrderByDescending(ha => ha.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(ha => new HelpArticleDto
                    {
                        Id = ha.Id,
                        Title = ha.Title,
                        Content = ha.Content,
                        Category = ha.Category,
                        Tags = new List<string>(),
                        ViewCount = ha.ViewCount,
                        IsPublished = ha.IsPublished,
                        CreatedAt = ha.CreatedAt,
                        UpdatedAt = ha.UpdatedAt
                    })
                    .ToListAsync();

                return BaseResponseDto<List<HelpArticleDto>>.SuccessResult("Help articles retrieved successfully", articles);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<List<HelpArticleDto>>.ErrorResult($"Error retrieving help articles: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<HelpArticleDto>> GetHelpArticleByIdAsync(int articleId)
        {
            try
            {
                var article = await _context.HelpArticles
                    .Where(ha => ha.Id == articleId && ha.IsPublished && ha.IsActive)
                    .FirstOrDefaultAsync();

                if (article == null)
                {
                    return BaseResponseDto<HelpArticleDto>.ErrorResult("Help article not found");
                }

                // Increment view count
                article.ViewCount++;
                article.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var articleDto = new HelpArticleDto
                {
                    Id = article.Id,
                    Title = article.Title,
                    Content = article.Content,
                    Category = article.Category,
                    Tags = article.Tags.Split(',').Where(t => !string.IsNullOrEmpty(t)).ToList(),
                    ViewCount = article.ViewCount,
                    IsPublished = article.IsPublished,
                    CreatedAt = article.CreatedAt,
                    UpdatedAt = article.UpdatedAt
                };

                return BaseResponseDto<HelpArticleDto>.SuccessResult("Help article retrieved successfully", articleDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<HelpArticleDto>.ErrorResult($"Error retrieving help article: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<HelpArticleDto>> CreateHelpArticleAsync(CreateHelpArticleDto createHelpArticleDto)
        {
            try
            {
                var article = new HelpArticle
                {
                    Title = createHelpArticleDto.Title,
                    Content = createHelpArticleDto.Content,
                    Category = createHelpArticleDto.Category,
                    Tags = string.Join(",", createHelpArticleDto.Tags),
                    ViewCount = 0,
                    IsPublished = createHelpArticleDto.IsPublished,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.HelpArticles.Add(article);
                await _context.SaveChangesAsync();

                var articleDto = new HelpArticleDto
                {
                    Id = article.Id,
                    Title = article.Title,
                    Content = article.Content,
                    Category = article.Category,
                    Tags = article.Tags.Split(',').Where(t => !string.IsNullOrEmpty(t)).ToList(),
                    ViewCount = article.ViewCount,
                    IsPublished = article.IsPublished,
                    CreatedAt = article.CreatedAt,
                    UpdatedAt = article.UpdatedAt
                };

                return BaseResponseDto<HelpArticleDto>.SuccessResult("Help article created successfully", articleDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<HelpArticleDto>.ErrorResult($"Error creating help article: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<SupportTicketDto>> CreateSupportTicketAsync(int userId, CreateSupportTicketDto createSupportTicketDto)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId && u.IsActive)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return BaseResponseDto<SupportTicketDto>.ErrorResult("User not found");
                }

                var ticket = new SupportTicket
                {
                    UserId = userId,
                    Subject = createSupportTicketDto.Subject,
                    Description = createSupportTicketDto.Description,
                    Category = createSupportTicketDto.Category,
                    Priority = createSupportTicketDto.Priority,
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
            catch (Exception ex)
            {
                return BaseResponseDto<SupportTicketDto>.ErrorResult($"Error creating support ticket: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<List<SupportTicketDto>>> GetUserSupportTicketsAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var tickets = await _context.SupportTickets
                    .Where(st => st.UserId == userId && st.IsActive)
                    .OrderByDescending(st => st.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
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

                return BaseResponseDto<List<SupportTicketDto>>.SuccessResult("Support tickets retrieved successfully", tickets);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<List<SupportTicketDto>>.ErrorResult($"Error retrieving support tickets: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<List<FaqDto>>> GetFaqsAsync(string? category = null, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Faqs
                    .Where(f => f.IsPublished && f.IsActive);

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(f => f.Category == category);
                }

                var faqs = await query
                    .OrderByDescending(f => f.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
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
            catch (Exception ex)
            {
                return BaseResponseDto<List<FaqDto>>.ErrorResult($"Error retrieving FAQs: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<string>> SubmitContactFormAsync(ContactFormDto contactFormDto)
        {
            try
            {
                // TODO: Implement contact form submission logic
                // This could save to database, send email, etc.
                
                return BaseResponseDto<string>.SuccessResult("Contact form submitted successfully", "Thank you for your message. We will get back to you soon.");
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult($"Error submitting contact form: {ex.Message}");
            }
        }

        // Implement other methods with similar patterns...
        public async Task<BaseResponseDto<HelpArticleDto>> UpdateHelpArticleAsync(int articleId, UpdateHelpArticleDto updateHelpArticleDto)
        {
            // Implementation similar to other update methods
            return BaseResponseDto<HelpArticleDto>.ErrorResult("Not implemented yet");
        }

        public async Task<BaseResponseDto<string>> DeleteHelpArticleAsync(int articleId)
        {
            // Implementation similar to other delete methods
            return BaseResponseDto<string>.ErrorResult("Not implemented yet");
        }

        public async Task<BaseResponseDto<List<HelpArticleDto>>> SearchHelpArticlesAsync(string searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            // Implementation for search functionality
            return BaseResponseDto<List<HelpArticleDto>>.ErrorResult("Not implemented yet");
        }

        public async Task<BaseResponseDto<SupportTicketDto>> GetSupportTicketByIdAsync(int ticketId, int userId)
        {
            // Implementation for getting specific ticket
            return BaseResponseDto<SupportTicketDto>.ErrorResult("Not implemented yet");
        }

        public async Task<BaseResponseDto<SupportTicketDto>> UpdateSupportTicketAsync(int ticketId, int userId, UpdateSupportTicketDto updateSupportTicketDto)
        {
            // Implementation for updating ticket
            return BaseResponseDto<SupportTicketDto>.ErrorResult("Not implemented yet");
        }

        public async Task<BaseResponseDto<string>> CloseSupportTicketAsync(int ticketId, int userId)
        {
            // Implementation for closing ticket
            return BaseResponseDto<string>.ErrorResult("Not implemented yet");
        }

        public async Task<BaseResponseDto<List<SupportMessageDto>>> GetSupportTicketMessagesAsync(int ticketId, int userId)
        {
            // Implementation for getting ticket messages
            return BaseResponseDto<List<SupportMessageDto>>.ErrorResult("Not implemented yet");
        }

        public async Task<BaseResponseDto<SupportMessageDto>> AddSupportMessageAsync(int ticketId, int userId, CreateSupportMessageDto createSupportMessageDto)
        {
            // Implementation for adding message to ticket
            return BaseResponseDto<SupportMessageDto>.ErrorResult("Not implemented yet");
        }

        public async Task<BaseResponseDto<FaqDto>> GetFaqByIdAsync(int faqId)
        {
            // Implementation for getting specific FAQ
            return BaseResponseDto<FaqDto>.ErrorResult("Not implemented yet");
        }

        public async Task<BaseResponseDto<FaqDto>> CreateFaqAsync(CreateFaqDto createFaqDto)
        {
            // Implementation for creating FAQ
            return BaseResponseDto<FaqDto>.ErrorResult("Not implemented yet");
        }

        public async Task<BaseResponseDto<FaqDto>> UpdateFaqAsync(int faqId, UpdateFaqDto updateFaqDto)
        {
            // Implementation for updating FAQ
            return BaseResponseDto<FaqDto>.ErrorResult("Not implemented yet");
        }

        public async Task<BaseResponseDto<string>> DeleteFaqAsync(int faqId)
        {
            // Implementation for deleting FAQ
            return BaseResponseDto<string>.ErrorResult("Not implemented yet");
        }

        public async Task<BaseResponseDto<object>> GetHelpSupportStatsAsync()
        {
            // Implementation for getting statistics
            return BaseResponseDto<object>.ErrorResult("Not implemented yet");
        }
    }
}
