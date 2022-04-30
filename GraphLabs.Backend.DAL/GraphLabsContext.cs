using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL;

public class GraphLabsContext:DbContext
{
    private readonly Lazy<IUserInfoService> _userInfoService;
    public GraphLabsContext(DbContextOptions<GraphLabsContext> options)
        : base(options)
    {
        _userInfoService = new Lazy<IUserInfoService>(this.GetService<IUserInfoService>);
        // Database.EnsureDeleted();
        // Database.EnsureCreated();
    }
    
    private DbConnection _dbConnection;
    
    public override DatabaseFacade Database 
    {
        get
        {
            var db = base.Database;
            if (_dbConnection == null)
            {
                _dbConnection = db.GetDbConnection();
                _dbConnection.StateChange += DbConnectionOnStateChange; 
            }

            return db;
        }
    }
    
    private void DbConnectionOnStateChange(object sender, StateChangeEventArgs e)
    {
        if (e.OriginalState == ConnectionState.Closed && e.CurrentState == ConnectionState.Open)
        {
            var enableRls = _userInfoService.Value.UserRole == nameof(Student) ? 1 : 0;
            using (var cmd = _dbConnection.CreateCommand())
            {
                cmd.CommandText = $"set backend.enableRls = {enableRls};";
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            using (var cmd = _dbConnection.CreateCommand())
            {
                cmd.CommandText = $"set backend.userId = '{_userInfoService.Value.UserId}';";
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }
    }

    public DbSet<User> Users { get; protected set; } = null!;
    public DbSet<Student> Students { get; protected set; } = null!;
    public DbSet<Teacher> Teachers { get; protected set; } = null!;
    public DbSet<Subject> Subjects { get; protected set; } = null!;
    public DbSet<Test> Tests { get; protected set; } = null!;
    public DbSet<Section> Sections { get; protected set; } = null!;
    public DbSet<TestAnswer> TestAnswers { get; protected set; } = null!;
    public DbSet<TestParticipation> TestParticipation { get; protected set; } = null!;
    public DbSet<TestQuestion> TestQuestions { get; protected set; } = null!;
    
    public DbSet<TaskModule> TaskModules { get; protected set; }
    public DbSet<TaskVariant> TaskVariants { get; protected set; }
    public DbSet<TaskVariantLog> TaskVariantLogs { get; protected set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(UserConfigure);
        modelBuilder.Entity<Student>(StudentConfigure);
        modelBuilder.Entity<Teacher>();
        modelBuilder.Entity<Subject>(SubjectConfigure);
        modelBuilder.Entity<Test>(TestConfigure);
        modelBuilder.Entity<Section>(SectionConfigure);
        modelBuilder.Entity<TestQuestion>(TestQuestionConfigure);
        modelBuilder.Entity<TestAnswer>(TestAnswerConfigure);
        modelBuilder.Entity<TestParticipation>(TestParticipationConfigure);
        
        modelBuilder.Entity<TaskVariant>(TaskVariantConfigure);
        modelBuilder.Entity<TaskModule>(TaskModuleConfigure);
        modelBuilder.Entity<TaskVariantLog>(TaskVariantLogConfigure);
    }
    
    private void UserConfigure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.FirstName).IsRequired();
        builder.Property(u => u.LastName).IsRequired();
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.PasswordSalt).IsRequired();
    }
    
    private void StudentConfigure(EntityTypeBuilder<Student> builder)
    {
        builder.HasIndex(s => s.Group);
    }
    
    private void SubjectConfigure(EntityTypeBuilder<Subject> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.NameSubject).IsRequired();
    }
    
    private void TestConfigure(EntityTypeBuilder<Test> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.NameTest).IsRequired();
        builder.HasOne(t => t.Teacher)
            .WithMany(teacher => teacher.Tests);
            
        builder.HasOne(t => t.Subject)
            .WithMany(s => s.Tests);
           
    }
    
    private void SectionConfigure(EntityTypeBuilder<Section> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.NumberSection).IsRequired();
    }

    private void TestQuestionConfigure(EntityTypeBuilder<TestQuestion> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Text).IsRequired();
        builder.Property(t => t.difficulty).IsRequired();
        builder.HasCheckConstraint("difficulty", "difficulty >= 0 AND difficulty <= 10");   //стоит согласовать ограничения
        builder.HasOne(t => t.Section)
            .WithMany(s => s.TestQuestions);
            
        builder.HasOne(t => t.Test)
            .WithMany(test => test.TestQuestions);
        builder.Property(t => t.Photo).HasMaxLength(512);
    }

    private void TestAnswerConfigure(EntityTypeBuilder<TestAnswer> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Text).IsRequired();
        builder.Property(t => t.IsCorrect).IsRequired();
        builder.HasOne(t => t.TestQuestion)
            .WithMany(t => t.TestAnswers);
    }
    
    private void TestParticipationConfigure(EntityTypeBuilder<TestParticipation> builder)
    {
        builder.HasKey(t => t.Id);
        builder.HasOne(t => t.Test)
            .WithMany(t => t.TestParticipation);
        builder.Property(t => t.DateOpen).IsRequired();
        builder.Property(t => t.DateClose).IsRequired();
        builder.Property(t => t.TimeStart).IsRequired();
        builder.Property(t => t.TimeFinish).IsRequired();
        builder.HasOne(t => t.Student)
            .WithMany(s => s.TestParticipation);
        builder.Property(t => t.IsPassed).HasDefaultValue(false);
        builder.Property(t => t.Result).HasDefaultValue(0);
    }

    private void TaskVariantConfigure(EntityTypeBuilder<TaskVariant> builder)
    {
        builder.HasKey(v => v.Id);
        builder.HasOne(v => v.TaskModule)
            .WithMany(t => t.Variants)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void TaskModuleConfigure(EntityTypeBuilder<TaskModule> builder)
    {
        builder.HasKey(t => t.Id);
    }

    private void TaskVariantLogConfigure(EntityTypeBuilder<TaskVariantLog> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(l => l.Action).IsRequired();
        builder.HasOne(l => l.Variant)
            .WithMany(v => v.Logs)
            .IsRequired()
            .HasForeignKey(l => l.VariantId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.Student)
            .WithMany(s => s.Logs)
            .IsRequired()
            .HasForeignKey(l => l.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(l => l.DateTime);
    }
    
    public override void Dispose()
    {
        if (_dbConnection != null)
        {
            _dbConnection.StateChange -= DbConnectionOnStateChange;
            _dbConnection = null;
        }

        base.Dispose();
    }
}