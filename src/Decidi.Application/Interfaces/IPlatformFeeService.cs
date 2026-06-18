using Decidi.Application.DTOs.Payments;

namespace Decidi.Application.Interfaces;

public interface IPlatformFeeService
{
    Task<PlatformFeeDto> GetCurrentAsync();
}
