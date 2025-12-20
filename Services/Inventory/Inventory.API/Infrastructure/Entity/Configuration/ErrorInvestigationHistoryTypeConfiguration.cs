namespace BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.Configuration
{
    public class ErrorInvestigationHistoryTypeConfiguration : IEntityTypeConfiguration<ErrorInvestigationHistory>
    {
        public void Configure(EntityTypeBuilder<ErrorInvestigationHistory> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ComponentCode).IsRequired().HasMaxLength(50);
            builder.Property(x => x.ComponentName).HasMaxLength(250);
            builder.Property(x => x.PositionCode).HasMaxLength(50);
            builder.Property(x => x.AdjustmentNo).IsRequired();
            builder.Property(x => x.OldValue).IsRequired();
            builder.Property(x => x.NewValue).IsRequired();
            builder.Property(x => x.ErrorCategory).IsRequired();
            builder.Property(x => x.ErrorDetails).IsRequired().HasMaxLength(500);
            builder.Property(x => x.InvestigatorId).IsRequired();
            builder.Property(x => x.ConfirmationTime).IsRequired();
            builder.Property(x => x.ConfirmationImage1).HasMaxLength(500);
            builder.Property(x => x.ConfirmationImage2).HasMaxLength(500);
            builder.Property(x => x.ErrorInvestigationId).IsRequired();
            builder.Property(x => x.IsDelete).IsRequired();
            builder.Property(x => x.InvestigatorUserCode).HasMaxLength(50);

            builder.HasIndex(x => x.ComponentCode)
                .IsClustered(false)
                .HasDatabaseName($"IDX_{nameof(ErrorInvestigationHistory)}_{nameof(ErrorInvestigationHistory.ComponentCode)}");

            builder.HasOne(x => x.ErrorInvestigation)
                .WithMany(x => x.ErrorInvestigationHistories)
                .HasForeignKey(x => x.ErrorInvestigationId)
                .OnDelete(DeleteBehavior.NoAction);

            // Add Indexes:
            builder.HasIndex(x => x.ErrorCategory)
                .IsClustered(false)
                .HasDatabaseName($"IDX_{nameof(ErrorInvestigationHistory)}_{nameof(ErrorInvestigationHistory.ErrorCategory)}");
        }
    }

    public class ErrorInvestigationTypeConfiguration : IEntityTypeConfiguration<ErrorInvestigation>
    {
        public void Configure(EntityTypeBuilder<ErrorInvestigation> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.InventoryId).IsRequired();
            builder.Property(x => x.ComponentCode).IsRequired().HasMaxLength(50);
            builder.Property(x => x.ComponentName).HasMaxLength(250);
            builder.Property(x => x.Status).IsRequired();
            builder.Property(x => x.IsDelete).IsRequired();
            builder.Property(x => x.AdjustmentNo).IsRequired();
            builder.Property(x => x.InvestigatorUserCode).HasMaxLength(50);


            builder.HasIndex(x => x.ComponentCode)
                .IsClustered(false)
                .HasDatabaseName($"IDX_{nameof(ErrorInvestigation)}_{nameof(ErrorInvestigation.ComponentCode)}");

            builder.HasOne(x => x.Inventory)
                .WithMany(x => x.ErrorInvestigations)
                .HasForeignKey(x => x.InventoryId)
                .OnDelete(DeleteBehavior.NoAction);

            // Add Indexes:
            builder.HasIndex(x => x.Status)
                .IsClustered(false)
                .HasDatabaseName($"IDX_{nameof(ErrorInvestigation)}_{nameof(ErrorInvestigation.Status)}");

        }
    }

    public class ErrorInvestigationInventoryDocTypeConfiguration : IEntityTypeConfiguration<ErrorInvestigationInventoryDoc>
    {
        public void Configure(EntityTypeBuilder<ErrorInvestigationInventoryDoc> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ErrorInvestigationId).IsRequired();
            builder.Property(x => x.InventoryDocId).IsRequired();
            builder.Property(x => x.DocType).IsRequired();
            builder.Property(x => x.DocCode).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Plant).IsRequired().HasMaxLength(50);
            builder.Property(x => x.WareHouseLocation).IsRequired().HasMaxLength(50);
            builder.Property(x => x.PositionCode).HasMaxLength(50);
            builder.Property(x => x.TotalQuantity);
            builder.Property(x => x.AccountQuantity);
            builder.Property(x => x.ErrorQuantity);
            builder.Property(x => x.ErrorMoney);
            builder.Property(x => x.UnitPrice);
            builder.Property(x => x.DocCode).IsRequired().HasMaxLength(50);
            builder.Property(x => x.DocType).IsRequired();
            builder.Property(x => x.ModelCode).HasMaxLength(50);
            builder.Property(x => x.AttachModule).HasMaxLength(50);


            builder.HasOne(x => x.ErrorInvestigation)
                .WithMany(x => x.ErrorInvestigationInventoryDocs)
                .HasForeignKey(x => x.ErrorInvestigationId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.InventoryDoc)
                .WithMany(x => x.ErrorInvestigationInventoryDocs)
                .HasForeignKey(x => x.InventoryDocId)
                .OnDelete(DeleteBehavior.NoAction);

            // Add Indexes:
            builder.HasIndex(x => x.Plant)
                .IsClustered(false)
                .HasDatabaseName($"IDX_{nameof(ErrorInvestigationInventoryDoc)}_{nameof(ErrorInvestigationInventoryDoc.Plant)}");
            builder.HasIndex(x => x.WareHouseLocation)
                .IsClustered(false)
                .HasDatabaseName($"IDX_{nameof(ErrorInvestigationInventoryDoc)}_{nameof(ErrorInvestigationInventoryDoc.WareHouseLocation)}");
            builder.HasIndex(x => x.AccountQuantity)
                .IsClustered(false)
                .HasDatabaseName($"IDX_{nameof(ErrorInvestigationInventoryDoc)}_{nameof(ErrorInvestigationInventoryDoc.AccountQuantity)}");
            builder.HasIndex(x => x.PositionCode)
                .IsClustered(false)
                .HasDatabaseName($"IDX_{nameof(ErrorInvestigationInventoryDoc)}_{nameof(ErrorInvestigationInventoryDoc.PositionCode)}");
            builder.HasIndex(x => x.ErrorQuantity)
                .IsClustered(false)
                .HasDatabaseName($"IDX_{nameof(ErrorInvestigationInventoryDoc)}_{nameof(ErrorInvestigationInventoryDoc.ErrorQuantity)}");
            builder.HasIndex(x => x.AssignedAccount)
                .IsClustered(false)
                .HasDatabaseName($"IDX_{nameof(ErrorInvestigationInventoryDoc)}_{nameof(ErrorInvestigationInventoryDoc.AssignedAccount)}");

        }
    }

    public class GeneralSettingTypeConfiguration : IEntityTypeConfiguration<GeneralSetting>
    {
        public void Configure(EntityTypeBuilder<GeneralSetting> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Key1).HasMaxLength(50);
            builder.Property(x => x.Value1).HasMaxLength(500);
            builder.Property(x => x.Key2).HasMaxLength(50);
            builder.Property(x => x.Value2).HasMaxLength(500);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
        }
    }


}
