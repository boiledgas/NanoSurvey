using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NanoSurvey.DB
{
    public partial class ResultContext : DbContext
    {
        public virtual DbSet<Interview> Interview { get; set; }
        public virtual DbSet<Result> Result { get; set; }
        public virtual DbSet<ResultAnswer> ResultAnswer { get; set; }

        public ResultContext(DbContextOptions<ResultContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=.\\;Database=NanoSurveyDB;User ID=sa;Password=1qaz@WSX;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasOne(d => d.Interview)
                    .WithMany(p => p.Result)
                    .HasForeignKey(d => d.InterviewId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_result_Result_InterviewId");
            });

            modelBuilder.Entity<ResultAnswer>(entity =>
            {
                entity.HasIndex(e => new { e.AnswerId, e.ResultId })
                    .HasName("IX_result_ResultAnswer_ResultId");

                entity.HasOne(d => d.Result)
                    .WithMany(p => p.ResultAnswer)
                    .HasForeignKey(d => d.ResultId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_result_ResultAnswer_ResultId");
            });

            OnModelCreatingExt(modelBuilder);
        }

        partial void OnModelCreatingExt(ModelBuilder modelBuilder);
    }
}
