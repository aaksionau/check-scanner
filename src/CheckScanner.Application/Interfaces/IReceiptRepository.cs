using CheckScanner.Domain.Entities;

namespace CheckScanner.Application.Interfaces;

public interface IReceiptRepository
{
    Task<Guid> AddAsync(Receipt receipt, CancellationToken cancellationToken);

    /// <summary>Newest first.</summary>
    Task<IReadOnlyList<Receipt>> GetAllAsync(CancellationToken cancellationToken);
}
