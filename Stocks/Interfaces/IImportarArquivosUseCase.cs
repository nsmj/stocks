using Microsoft.AspNetCore.Http;

namespace Stocks.Interfaces;

public interface IImportarArquivosUseCase
{
    Task ExecutarAsync(IFormFile arquivo);
}
