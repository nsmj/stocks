using Microsoft.EntityFrameworkCore;
using Stocks.Bo;
using Stocks.Data;
using Stocks.Extraction;
using Stocks.Extraction.Strategies;
using Stocks.Interfaces;
using Stocks.Models.TiposOperacao;
using Stocks.UseCases;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<BancoContext>(options =>
    options.UseSqlite("Data Source=Data/Banco.db")
);

builder.Services.AddScoped<ILucroVendasAbaixo20kQuery, LucroVendasAbaixo20kQuery>();
builder.Services.AddScoped<IIrrfQuery, IrrfQuery>();
builder.Services.AddScoped<IIrrfService, IrrfService>();
builder.Services.AddScoped<IPdfExtractor, PdfExtractor>();
builder.Services.AddScoped<IIrpfRowsBuilder, IrpfRowsBuilder>();
builder.Services.AddScoped<ICalcularPrejuizoAcumuladoUseCase, CalcularPrejuizoAcumuladoUseCase>();
builder.Services.AddScoped<IPosicaoFimAnoQuery, PosicaoFimAnoQuery>();
builder.Services.AddScoped<IEstrategiaImportacaoFactory, EstrategiaImportacaoFactory>();
builder.Services.AddScoped<IImportarArquivosUseCase, ImportarArquivosUseCase>();
builder.Services.AddScoped<ICalcularResultadosUseCase, CalcularResultadosUseCase>();

builder.Services.AddKeyedScoped<IOperacaoListable, SwingTrade>("SwingTrade");
builder.Services.AddKeyedScoped<IOperacaoListable, DayTrade>("DayTrade");
builder.Services.AddKeyedScoped<IOperacaoListable, Fii>("Fii");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();
