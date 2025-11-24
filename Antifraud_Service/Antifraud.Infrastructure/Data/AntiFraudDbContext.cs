using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antifraud.Infrastructure.Data
{
    public class AntiFraudDbContext : DbContext
    {
        public AntiFraudDbContext(DbContextOptions<AntiFraudDbContext> options) : base(options)
        {
        }

        public DbSet<TransactionReadModel> Transactions => Set<TransactionReadModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var entity = modelBuilder.Entity<TransactionReadModel>();
            entity.ToTable("Transactions");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).ValueGeneratedNever();
            entity.Property(t => t.SourceAccountId).IsRequired();
            entity.Property(t => t.TargetAccountId).IsRequired();
            entity.Property(t => t.TransferTypeId).IsRequired();
            entity.Property(t => t.Value).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(t => t.CreatedAt).IsRequired();
        }
    }
}
