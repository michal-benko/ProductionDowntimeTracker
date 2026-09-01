using ProductionDowntimeTracker.api.DTOs;

namespace ProductionDowntimeTracker.api.Services
{
    public interface IOpcUaService
    {
        Task<OpcUaStatusDto> ReadStatusAsync();
    }
}
