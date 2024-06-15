using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace C.Models;

public partial class CContext : DbContext
{
    public CContext()
    {
    }

    public CContext(DbContextOptions<CContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Calificacione> Calificaciones { get; set; }

    public virtual DbSet<Materia> Materias { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Semestre> Semestres { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=PROOS10;Database=C;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Calificacione>(entity =>
        {
            entity.HasKey(e => e.CalificacionId).HasName("PK__Califica__4CF54ABE5C615E47");

            entity.Property(e => e.CalificacionId).HasColumnName("CalificacionID");
            entity.Property(e => e.MateriaId).HasColumnName("MateriaID");
            entity.Property(e => e.Nota1).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.Nota2).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.Nota3).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.NotaExtra)
                .HasComputedColumnSql("(case when (([Nota1]+[Nota2])+[Nota3])/(3)<(6) then (6)-(([Nota1]+[Nota2])+[Nota3])/(3)  end)", true)
                .HasColumnType("decimal(11, 6)");
            entity.Property(e => e.NotaFinal)
                .HasComputedColumnSql("((([Nota1]+[Nota2])+[Nota3])/(3))", true)
                .HasColumnType("decimal(10, 6)");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Materia).WithMany(p => p.Calificaciones)
                .HasForeignKey(d => d.MateriaId)
                .HasConstraintName("FK__Calificac__Mater__440B1D61");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Calificaciones)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__Calificac__Usuar__4316F928");
        });

        modelBuilder.Entity<Materia>(entity =>
        {
            entity.HasKey(e => e.MateriaId).HasName("PK__Materias__0D019D8173BD5114");

            entity.Property(e => e.MateriaId).HasColumnName("MateriaID");
            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.NombreMateria).HasMaxLength(100);
            entity.Property(e => e.SemestreId).HasColumnName("SemestreID");

            entity.HasOne(d => d.Semestre).WithMany(p => p.Materia)
                .HasForeignKey(d => d.SemestreId)
                .HasConstraintName("FK__Materias__Semest__403A8C7D");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RolId).HasName("PK__Roles__F92302D105011DAF");

            entity.Property(e => e.RolId).HasColumnName("RolID");
            entity.Property(e => e.NombreRol).HasMaxLength(50);
        });

        modelBuilder.Entity<Semestre>(entity =>
        {
            entity.HasKey(e => e.SemestreId).HasName("PK__Semestre__3B06A2D27D94A6B4");

            entity.Property(e => e.SemestreId).HasColumnName("SemestreID");
            entity.Property(e => e.Periodo).HasMaxLength(50);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("PK__Usuarios__2B3DE798C3759DA4");

            entity.HasIndex(e => e.Correo, "UQ__Usuarios__60695A19ABE05A1E").IsUnique();

            entity.HasIndex(e => e.Cedula, "UQ__Usuarios__B4ADFE385F17FE34").IsUnique();

            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
            entity.Property(e => e.Apellido).HasMaxLength(50);
            entity.Property(e => e.Cedula).HasMaxLength(10);
            entity.Property(e => e.Contraseña).HasMaxLength(100);
            entity.Property(e => e.Correo).HasMaxLength(100);
            entity.Property(e => e.Nombre).HasMaxLength(50);
            entity.Property(e => e.RolId).HasColumnName("RolID");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .HasConstraintName("FK__Usuarios__RolID__3B75D760");

            entity.HasMany(d => d.Materia).WithMany(p => p.Estudiantes)
                .UsingEntity<Dictionary<string, object>>(
                    "EstudiantesMateria",
                    r => r.HasOne<Materia>().WithMany()
                        .HasForeignKey("MateriaId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Estudiant__Mater__47DBAE45"),
                    l => l.HasOne<Usuario>().WithMany()
                        .HasForeignKey("EstudianteId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Estudiant__Estud__46E78A0C"),
                    j =>
                    {
                        j.HasKey("EstudianteId", "MateriaId").HasName("PK__Estudian__6FA69AE0E22E69C6");
                        j.ToTable("EstudiantesMaterias");
                        j.IndexerProperty<int>("EstudianteId").HasColumnName("EstudianteID");
                        j.IndexerProperty<int>("MateriaId").HasColumnName("MateriaID");
                    });

            entity.HasMany(d => d.MateriaNavigation).WithMany(p => p.Profesors)
                .UsingEntity<Dictionary<string, object>>(
                    "ProfesoresMateria",
                    r => r.HasOne<Materia>().WithMany()
                        .HasForeignKey("MateriaId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Profesore__Mater__4BAC3F29"),
                    l => l.HasOne<Usuario>().WithMany()
                        .HasForeignKey("ProfesorId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Profesore__Profe__4AB81AF0"),
                    j =>
                    {
                        j.HasKey("ProfesorId", "MateriaId").HasName("PK__Profesor__4D23E9F0876038AD");
                        j.ToTable("ProfesoresMaterias");
                        j.IndexerProperty<int>("ProfesorId").HasColumnName("ProfesorID");
                        j.IndexerProperty<int>("MateriaId").HasColumnName("MateriaID");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
