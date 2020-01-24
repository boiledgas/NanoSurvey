using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NanoSurvey.DB
{
    public partial class SurveyContext : DbContext
    {
        public virtual DbSet<Answer> Answer { get; set; }
        public virtual DbSet<Interview> Interview { get; set; }
        public virtual DbSet<Question> Question { get; set; }
        public virtual DbSet<Result> Result { get; set; }
        public virtual DbSet<ResultAnswer> ResultAnswer { get; set; }
        public virtual DbSet<Survey> Survey { get; set; }
        public virtual DbSet<SurveyQuestion> SurveyQuestion { get; set; }
        public virtual DbSet<SurveyQuestionAnswer> SurveyQuestionAnswer { get; set; }

        public SurveyContext(DbContextOptions<SurveyContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=.\\;Database=NanoSurveyDB;User Id=sa;Password=1qaz@WSX");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<Answer>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
            });

            modelBuilder.Entity<Interview>(entity =>
            {
                entity.HasOne(d => d.Survey)
                    .WithMany(p => p.Interview)
                    .HasForeignKey(d => d.SurveyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_result_Interview_SurveyId");
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
            });

            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasIndex(e => new { e.QuestionId, e.InterviewId })
                    .HasName("IX_result_Result_InterviewId");

                entity.HasOne(d => d.Interview)
                    .WithMany(p => p.Result)
                    .HasForeignKey(d => d.InterviewId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_result_Result_InterviewId");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.Result)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_result_Result_QuestionId");
            });

            modelBuilder.Entity<ResultAnswer>(entity =>
            {
                entity.HasIndex(e => new { e.AnswerId, e.ResultId })
                    .HasName("IX_result_ResultAnswer_ResultId");

                entity.HasOne(d => d.Answer)
                    .WithMany(p => p.ResultAnswer)
                    .HasForeignKey(d => d.AnswerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_result_ResultAnswer_AnswerId");

                entity.HasOne(d => d.Result)
                    .WithMany(p => p.ResultAnswer)
                    .HasForeignKey(d => d.ResultId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_result_ResultAnswer_ResultId");
            });

            modelBuilder.Entity<Survey>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
            });

            modelBuilder.Entity<SurveyQuestion>(entity =>
            {
                entity.HasIndex(e => e.SurveyId);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.NextQuestion)
                    .WithMany(p => p.SurveyQuestionNextQuestion)
                    .HasForeignKey(d => d.NextQuestionId)
                    .HasConstraintName("FK_survey_SurveyQuestionRoute_NextQuestionId");

                entity.HasOne(d => d.PreviousQuestion)
                    .WithMany(p => p.SurveyQuestionPreviousQuestion)
                    .HasForeignKey(d => d.PreviousQuestionId)
                    .HasConstraintName("FK_survey_SurveyQuestionRoute_PreviousQuestionId");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.SurveyQuestionQuestion)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_survey_SurveyQuestion_QuestionId");

                entity.HasOne(d => d.Survey)
                    .WithMany(p => p.SurveyQuestion)
                    .HasForeignKey(d => d.SurveyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_survey_SurveyQuestion_SurveyId");
            });

            modelBuilder.Entity<SurveyQuestionAnswer>(entity =>
            {
                entity.HasIndex(e => new { e.OrderNum, e.SurveyId })
                    .HasName("IX_SurveyQuestionAnswer_QuestionId");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Answer)
                    .WithMany(p => p.SurveyQuestionAnswer)
                    .HasForeignKey(d => d.AnswerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_survey_SurveyQuestionAnswer_AnswerId");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.SurveyQuestionAnswer)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_survey_SurveyQuestionAnswer_QuestionId");

                entity.HasOne(d => d.Survey)
                    .WithMany(p => p.SurveyQuestionAnswer)
                    .HasForeignKey(d => d.SurveyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_survey_SurveyQuestionAnswer_SurveyId");
            });

            OnModelCreatingExt(modelBuilder);
        }

        partial void OnModelCreatingExt(ModelBuilder modelBuilder);
    }
}
