using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RPPP_WebApp.Model;

public partial class Rppp15Context : DbContext
{
    public Rppp15Context()
    {
    }

    public Rppp15Context(DbContextOptions<Rppp15Context> options)
        : base(options)
    {
    }

    public virtual DbSet<ClassUsability> ClassUsabilities { get; set; }

    public virtual DbSet<Cuisine> Cuisines { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<ExpenseType> ExpenseTypes { get; set; }

    public virtual DbSet<Harvest> Harvests { get; set; }

    public virtual DbSet<Infrastructure> Infrastructures { get; set; }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<Lease> Leases { get; set; }

    public virtual DbSet<Measure> Measures { get; set; }

    public virtual DbSet<MeasureType> MeasureTypes { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<PlantClass> PlantClasses { get; set; }

    public virtual DbSet<Plot> Plots { get; set; }

    public virtual DbSet<Purchase> Purchases { get; set; }

    public virtual DbSet<Recipe> Recipes { get; set; }

    public virtual DbSet<SoilQuality> SoilQualities { get; set; }

    public virtual DbSet<SoilType> SoilTypes { get; set; }

    public virtual DbSet<Subsidy> Subsidies { get; set; }

    public virtual DbSet<Sunlight> Sunlights { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<Usability> Usabilities { get; set; }

    public virtual DbSet<Vegetation> Vegetations { get; set; }

    public virtual DbSet<Worker> Workers { get; set; }

    public virtual DbSet<WorkerType> WorkerTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClassUsability>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ClassUsability");

            entity.HasOne(d => d.PlantClass).WithMany()
                .HasForeignKey(d => d.PlantClassId)
                .HasConstraintName("FK_ClassUsability_PlantClass");

            entity.HasOne(d => d.Usability).WithMany()
                .HasForeignKey(d => d.UsabilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassUsability_Usability");
        });

        modelBuilder.Entity<Cuisine>(entity =>
        {
            entity.HasKey(e => e.IdCuisine).HasName("Cuisine_PK");

            entity.ToTable("Cuisine");

            entity.Property(e => e.Caption)
                .IsRequired()
                .HasColumnType("text");
            entity.Property(e => e.Description).HasColumnType("text");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.IdCustomer);

            entity.ToTable("Customer");

            entity.Property(e => e.IdCustomer).ValueGeneratedNever();
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.IdExpense).HasName("Expense_PK");

            entity.ToTable("Expense");

            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PaidOn).HasColumnType("datetime");

            entity.HasOne(d => d.ExpenseType).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.ExpenseTypeId)
                .HasConstraintName("FK_Expense_ExpenseType");

            entity.HasOne(d => d.Vegetation).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.VegetationId)
                .HasConstraintName("FK_Expense_Vegetation");
        });

        modelBuilder.Entity<ExpenseType>(entity =>
        {
            entity.HasKey(e => e.IdExpenseType).HasName("ExpenseType_PK");

            entity.ToTable("ExpenseType");

            entity.Property(e => e.Caption)
                .IsRequired()
                .HasMaxLength(127)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Harvest>(entity =>
        {
            entity.HasKey(e => e.IdHarvest);

            entity.ToTable("Harvest");

            entity.Property(e => e.Tag)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Vegetation).WithMany(p => p.Harvests)
                .HasForeignKey(d => d.VegetationId)
                .HasConstraintName("FK_Harvest_Vegitation");
        });

        modelBuilder.Entity<Infrastructure>(entity =>
        {
            entity.HasKey(e => e.Caption).HasName("Infrastructure_PK");

            entity.ToTable("Infrastructure");

            entity.Property(e => e.Caption)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("caption");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("description");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.IdIngredient);

            entity.ToTable("Ingredient");

            entity.HasOne(d => d.PlantClass).WithMany(p => p.Ingredients)
                .HasForeignKey(d => d.PlantClassId)
                .HasConstraintName("FK_Ingredient_PlantClass");

            entity.HasOne(d => d.Recipe).WithMany(p => p.Ingredients)
                .HasForeignKey(d => d.RecipeId)
                .HasConstraintName("FK_Ingredient_Recipe");
        });

        modelBuilder.Entity<Lease>(entity =>
        {
            entity.HasKey(e => e.IdLease).HasName("Lease_PK");

            entity.ToTable("Lease");

            entity.Property(e => e.Link)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.YearlyPay).HasColumnType("money");
        });

        modelBuilder.Entity<Measure>(entity =>
        {
            entity.HasKey(e => e.IdMeasure).HasName("Measure_PK");

            entity.ToTable("Measure");

            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PerformedOn).HasColumnType("datetime");

            entity.HasOne(d => d.MeasureType).WithMany(p => p.Measures)
                .HasForeignKey(d => d.MeasureTypeId)
                .HasConstraintName("FK_Measure_MeasureType");

            entity.HasOne(d => d.Vegetation).WithMany(p => p.Measures)
                .HasForeignKey(d => d.VegetationId)
                .HasConstraintName("FK_Measure_Vegetation");

            entity.HasOne(d => d.Worker).WithMany(p => p.Measures)
                .HasForeignKey(d => d.WorkerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Measure_FK");
        });

        modelBuilder.Entity<MeasureType>(entity =>
        {
            entity.HasKey(e => e.IdMeasureType).HasName("MeasureType_PK");

            entity.ToTable("MeasureType");

            entity.Property(e => e.Caption)
                .IsRequired()
                .HasMaxLength(127)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.IdOrder);

            entity.ToTable("Order");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(300)
                .IsUnicode(false);

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Order_Customer");
        });

        modelBuilder.Entity<PlantClass>(entity =>
        {
            entity.HasKey(e => e.IdPlantClass).HasName("PlantClass_PK");

            entity.ToTable("PlantClass");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(127)
                .IsUnicode(false);
            entity.Property(e => e.Passport).HasColumnType("text");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PlantClass_PlantClass");
        });

        modelBuilder.Entity<Plot>(entity =>
        {
            entity.HasKey(e => e.IdPlot).HasName("Plot_PK");

            entity.ToTable("Plot");

            entity.Property(e => e.Area).HasColumnName("area");
            entity.Property(e => e.Infrastructure)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.InsertIntoRppp15DboPlot)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("INSERT INTO RPPP15.dbo.Plot");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(8, 6)")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("longitude");
            entity.Property(e => e.SoilQuality)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SoilType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Sunlight)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Tag)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("tag");

            entity.HasOne(d => d.InfrastructureNavigation).WithMany(p => p.Plots)
                .HasForeignKey(d => d.Infrastructure)
                .HasConstraintName("FK_Infrastructure_Plot");

            entity.HasOne(d => d.Lease).WithMany(p => p.Plots)
                .HasForeignKey(d => d.LeaseId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Lease_Plot");

            entity.HasOne(d => d.SoilQualityNavigation).WithMany(p => p.Plots)
                .HasForeignKey(d => d.SoilQuality)
                .HasConstraintName("FK_SoliQuality_Plot");

            entity.HasOne(d => d.SoilTypeNavigation).WithMany(p => p.Plots)
                .HasForeignKey(d => d.SoilType)
                .HasConstraintName("FK_SoilType_Plot");

            entity.HasOne(d => d.Subsidy).WithMany(p => p.Plots)
                .HasForeignKey(d => d.SubsidyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Subsidy_Plot");

            entity.HasOne(d => d.SunlightNavigation).WithMany(p => p.Plots)
                .HasForeignKey(d => d.Sunlight)
                .HasConstraintName("FK_Sunlight_Plot");
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasKey(e => e.IdPurchase);

            entity.ToTable("Purchase");

            entity.Property(e => e.Gain).HasColumnType("money");
            entity.Property(e => e.Tag)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Harvest).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.HarvestId)
                .HasConstraintName("FK_Purchase_Harvest");

            entity.HasOne(d => d.Order).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_Purchase_Order");
        });

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(e => e.IdRecipe);

            entity.ToTable("Recipe");

            entity.Property(e => e.Caption)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .IsRequired()
                .HasColumnType("text");

            entity.HasOne(d => d.Cuisine).WithMany(p => p.Recipes)
                .HasForeignKey(d => d.CuisineId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Recipe_FK");
        });

        modelBuilder.Entity<SoilQuality>(entity =>
        {
            entity.HasKey(e => e.Caption).HasName("SoilQuality_PK");

            entity.ToTable("SoilQuality");

            entity.Property(e => e.Caption)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("caption");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("description");
        });

        modelBuilder.Entity<SoilType>(entity =>
        {
            entity.HasKey(e => e.Caption).HasName("SoilType_PK");

            entity.ToTable("SoilType");

            entity.Property(e => e.Caption)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("caption");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("description");
        });

        modelBuilder.Entity<Subsidy>(entity =>
        {
            entity.HasKey(e => e.IdSubsidy).HasName("Subsidy_PK");

            entity.ToTable("Subsidy");

            entity.Property(e => e.Amount).HasColumnType("money");
        });

        modelBuilder.Entity<Sunlight>(entity =>
        {
            entity.HasKey(e => e.Caption).HasName("Sunlight_PK");

            entity.ToTable("Sunlight");

            entity.Property(e => e.Caption)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("caption");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("description");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.IdTask).HasName("Task_PK");

            entity.ToTable("Task");

            entity.Property(e => e.Deadline).HasColumnType("datetime");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Usability>(entity =>
        {
            entity.HasKey(e => e.IdUsability);

            entity.ToTable("Usability");

            entity.Property(e => e.IdUsability).ValueGeneratedNever();
            entity.Property(e => e.Caption)
                .IsRequired()
                .HasColumnType("text");
            entity.Property(e => e.Description).HasColumnType("text");
        });

        modelBuilder.Entity<Vegetation>(entity =>
        {
            entity.HasKey(e => e.IdVegetation).HasName("Vegetation_PK");

            entity.ToTable("Vegetation");

            entity.Property(e => e.PlantedOn).HasColumnType("datetime");
            entity.Property(e => e.RemovedOn).HasColumnType("datetime");

            entity.HasOne(d => d.PlantClass).WithMany(p => p.Vegetations)
                .HasForeignKey(d => d.PlantClassId)
                .HasConstraintName("FK_Vegetation_PlantClass");

            entity.HasOne(d => d.Plot).WithMany(p => p.Vegetations)
                .HasForeignKey(d => d.PlotId)
                .HasConstraintName("FK_Vegetation_Plot");
        });

        modelBuilder.Entity<Worker>(entity =>
        {
            entity.HasKey(e => e.IdWorker).HasName("Worker_PK");

            entity.ToTable("Worker");

            entity.Property(e => e.DailyWage).HasColumnType("money");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Notes)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Tag)
                .HasMaxLength(127)
                .IsUnicode(false);

            entity.HasOne(d => d.WorkerType).WithMany(p => p.Workers)
                .HasForeignKey(d => d.WorkerTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Worker_WorkerType");
        });

        modelBuilder.Entity<WorkerType>(entity =>
        {
            entity.HasKey(e => e.IdWorkerType).HasName("WorkerType_PK");

            entity.ToTable("WorkerType");

            entity.Property(e => e.Caption)
                .IsRequired()
                .HasMaxLength(127)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
