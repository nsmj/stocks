using Microsoft.EntityFrameworkCore;
using Stocks.Models;

namespace Stocks.Data;

public partial class BancoContext : DbContext
{
    public BancoContext() { }

    public BancoContext(DbContextOptions<BancoContext> options)
        : base(options) { }

    public virtual DbSet<Ativo> Ativos { get; set; }

    public virtual DbSet<Evento> Eventos { get; set; }

    public virtual DbSet<Irrf> Irrfs { get; set; }

    public virtual DbSet<Operacao> Operacoes { get; set; }

    public virtual DbSet<PosicaoFimAno> PosicoesFimAno { get; set; }

    public virtual DbSet<Provento> Proventos { get; set; }

    public virtual DbSet<TipoAtivo> TiposAtivo { get; set; }

    public virtual DbSet<TipoEvento> TiposEvento { get; set; }

    public virtual DbSet<TipoOperacao> TiposOperacao { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ativo>(entity =>
        {
            entity.ToTable("ativo");

            entity.HasIndex(e => e.Codigo, "IX_ativo_codigo").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cnpj).IsRequired().HasColumnName("cnpj");
            entity.Property(e => e.CnpjFontePagadora).HasColumnName("cnpj_fonte_pagadora");
            entity.Property(e => e.Codigo).IsRequired().HasColumnName("codigo");
            entity.Property(e => e.DescricaoNota).IsRequired().HasColumnName("descricao_nota");
            entity
                .Property(e => e.FatorPrecoTeto)
                .HasColumnType("DECIMAL(10, 5)")
                .HasColumnName("fator_preco_teto");
            entity.Property(e => e.Nome).IsRequired().HasColumnName("nome");
            entity.Property(e => e.Posicao).HasColumnName("posicao");
            entity
                .Property(e => e.PrecoMedio)
                .HasColumnType("DECIMAL(10, 5)")
                .HasColumnName("preco_medio");
            entity
                .Property(e => e.PrecoTetoMedio)
                .HasColumnType("DECIMAL(10, 5)")
                .HasColumnName("preco_teto_medio");
            entity
                .Property(e => e.PrecoTetoProjetivo)
                .HasColumnType("DECIMAL(10, 5)")
                .HasColumnName("preco_teto_projetivo");
            entity.Property(e => e.TipoAcao).HasColumnName("tipo_acao");
            entity.Property(e => e.TipoAtivoId).HasColumnName("tipo_ativo_id");

            entity
                .HasOne(d => d.TipoAtivo)
                .WithMany(p => p.Ativos)
                .HasForeignKey(d => d.TipoAtivoId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Evento>(entity =>
        {
            entity.ToTable("evento");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AtivoId).HasColumnName("ativo_id");
            entity.Property(e => e.Data).HasColumnType("datetime").HasColumnName("data");
            entity.Property(e => e.Fator).HasColumnName("fator");
            entity.Property(e => e.Valor).HasColumnName("valor");
            entity.Property(e => e.TipoEventoId).HasColumnName("tipo_evento_id");

            entity
                .HasOne(d => d.Ativo)
                .WithMany(p => p.Eventos)
                .HasForeignKey(d => d.AtivoId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity
                .HasOne(d => d.TipoEvento)
                .WithMany(p => p.Eventos)
                .HasForeignKey(d => d.TipoEventoId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Irrf>(entity =>
        {
            entity.ToTable("irrf");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Data).HasColumnType("datetime").HasColumnName("data");
            entity.Property(e => e.TipoOperacaoId).HasColumnName("tipo_operacao_id");
            entity.Property(e => e.Valor).HasColumnType("DECIMAL(10, 5)").HasColumnName("valor");

            entity
                .HasOne(d => d.TipoOperacao)
                .WithMany(p => p.Irrfs)
                .HasForeignKey(d => d.TipoOperacaoId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Operacao>(entity =>
        {
            entity.ToTable("operacao");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AtivoId).HasColumnName("ativo_id");
            entity.Property(e => e.Compra).HasColumnType("numeric").HasColumnName("compra");
            entity.Property(e => e.Data).HasColumnType("datetime").HasColumnName("data");
            entity
                .Property(e => e.LucroLiquido)
                .HasColumnType("DECIMAL(10, 5)")
                .HasColumnName("lucro_liquido");
            entity
                .Property(e => e.PrecoAtivo)
                .HasColumnType("DECIMAL(10, 5)")
                .HasColumnName("preco_ativo");
            entity.Property(e => e.Quantidade).HasColumnName("quantidade");
            entity.Property(e => e.Taxas).HasColumnType("DECIMAL(10, 5)").HasColumnName("taxas");
            entity.Property(e => e.TipoOperacaoId).HasColumnName("tipo_operacao_id");
            entity
                .Property(e => e.ValorTotal)
                .HasColumnType("DECIMAL(10, 5)")
                .HasColumnName("valor_total");

            entity
                .HasOne(d => d.Ativo)
                .WithMany(p => p.Operacoes)
                .HasForeignKey(d => d.AtivoId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity
                .HasOne(d => d.TipoOperacao)
                .WithMany(p => p.Operacoes)
                .HasForeignKey(d => d.TipoOperacaoId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<PosicaoFimAno>(entity =>
        {
            entity.ToTable("posicao_fim_ano");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ano).HasColumnName("ano");
            entity.Property(e => e.AtivoId).HasColumnName("ativo_id");
            entity.Property(e => e.Posicao).HasColumnName("posicao");
            entity
                .Property(e => e.PrecoMedio)
                .HasColumnType("DECIMAL(10, 5)")
                .HasColumnName("preco_medio");

            entity
                .HasOne(d => d.Ativo)
                .WithMany(p => p.PosicoesFimAno)
                .HasForeignKey(d => d.AtivoId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Provento>(entity =>
        {
            entity.ToTable("provento");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AtivoId).HasColumnName("ativo_id");
            entity.Property(e => e.Data).HasColumnType("datetime").HasColumnName("data");
            entity.Property(e => e.Valor).HasColumnType("DECIMAL(10, 5)").HasColumnName("valor");

            entity
                .HasOne(d => d.Ativo)
                .WithMany(p => p.Proventos)
                .HasForeignKey(d => d.AtivoId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TipoAtivo>(entity =>
        {
            entity.ToTable("tipo_ativo");

            entity.HasIndex(e => e.Nome, "IX_tipo_ativo_nome").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nome).IsRequired().HasColumnName("nome");
        });

        modelBuilder.Entity<TipoEvento>(entity =>
        {
            entity.ToTable("tipo_evento");

            entity.HasIndex(e => e.Nome, "IX_tipo_evento_nome").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nome).IsRequired().HasColumnName("nome");
        });

        modelBuilder.Entity<TipoOperacao>(entity =>
        {
            entity.ToTable("tipo_operacao");

            entity.HasIndex(e => e.Nome, "IX_tipo_operacao_nome").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nome).IsRequired().HasColumnName("nome");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
