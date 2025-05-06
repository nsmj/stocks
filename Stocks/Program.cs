using Microsoft.EntityFrameworkCore;
using Stocks.Bo;
using Stocks.Data;
using Stocks.Extraction;
using Stocks.Interfaces;
using Stocks.Models.TiposOperacao;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<BancoContext>(options =>
    options.UseSqlite("Data Source=Data/Banco.db")
);

builder.Services.AddScoped<LucroVendasAbaixo20kBo>();
builder.Services.AddScoped<IrrfBo>();
builder.Services.AddScoped<IrpfRowsBuilder>();
builder.Services.AddScoped<CalculadoraPrejuizoAcumuladoBo>();
builder.Services.AddScoped<PosicaoFimAnoBo>();
builder.Services.AddScoped<FileProcessor>();

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

app.Run();
