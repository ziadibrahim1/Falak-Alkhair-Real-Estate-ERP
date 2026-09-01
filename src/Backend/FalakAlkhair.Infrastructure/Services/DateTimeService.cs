using FalakAlkhair.Application.Common.Interfaces;

namespace FalakAlkhair.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.UtcNow;
}
