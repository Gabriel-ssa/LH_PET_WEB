using Microsoft.EntityFrameworkCore;
using LH_PET_WEB.Models;
using Microsoft.AspNetCore.Mvc;
using LH_PET_WEB.Controllers;

namespace LH_PET_WEB.Data
{
    public class ContextoBanco : DbContext
    {
        public ContextoBanco(DbContextOptions<ContextoBanco> options) : base(options)
        {
        }
        
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }

        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Agendamento> Agendamento { get; set; }
        public DbSet<Atendimento> Atendimento { get; set; }
        public DbSet<ConfiguracaoClinica> Configuracoes { get; set; }
        public DbSet<Venda> Vendas { get; set; }
        public DbSet<ItemVenda> ItensVenda { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Produto>().Property(p => p.Preco).HasPrecision(10, 2);
            modelBuilder.Entity<Venda>().Property(v => v.total).HasPrecision(10, 2);
            modelBuilder.Entity<ItemVenda>().Property(i => i.PrecoUnitario).HasPrecision(10, 2);
            modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Cliente>().HasIndex(c => c.Cpf).IsUnique();
            modelBuilder.Entity<Fornecedor>().HasIndex(f => f.Cnpj).IsUnique();
            modelBuilder.Entity<Atendimento>().HasIndex(a => a.AgendamentoId).IsUnique();
        }
        
    }
}