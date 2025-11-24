using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction.Core.Entities;

namespace Transaction.Infrastructure.Data
{
    public class TransactionDbContext : DbContext
    {
        public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options)
        {
        }

        public DbSet<TransactionEntity> Transactions => Set<TransactionEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
          
            var entity = modelBuilder.Entity<TransactionEntity>();
            
            entity.ToTable("Transactions");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).ValueGeneratedNever();
            entity.Property(t => t.ExternalId).IsRequired();
            entity.Property(t => t.SourceAccountId).IsRequired();
            entity.Property(t => t.TargetAccountId).IsRequired();
            entity.Property(t => t.TransferTypeId).IsRequired();
            entity.Property(t => t.Value).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(t => t.Status).HasConversion<int>().IsRequired();
            entity.Property(t => t.CreatedAt).IsRequired();
        }
    }
}
