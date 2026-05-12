using Pixtract.Domain.Entities;

namespace Pixtract.Application.Interfaces;

public interface IExportService
{
    byte[] ExportCurrent(ExtractionRequest request);
    byte[] ExportHistory(List<ExtractionRequest> requests);
}
