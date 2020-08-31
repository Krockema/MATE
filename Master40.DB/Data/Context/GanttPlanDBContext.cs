using Master40.DB.GanttPlanModel;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.Data.Context
{
    public partial class GanttPlanDBContext : DbContext
    {
        public GanttPlanDBContext()
        {
        }

        public GanttPlanDBContext(DbContextOptions<GanttPlanDBContext> options)
            : base(options)
        {
        }

        public static GanttPlanDBContext GetContext(string connectionString)
        {
            return new GanttPlanDBContext(options: new DbContextOptionsBuilder<GanttPlanDBContext>()
                .UseSqlServer(connectionString: connectionString)
                .Options);
        }

        public virtual DbSet<GptblAttachment> GptblAttachment { get; set; }
        public virtual DbSet<GptblAttachmentUdf> GptblAttachmentUdf { get; set; }
        public virtual DbSet<GptblBom> GptblBom { get; set; }
        public virtual DbSet<GptblBomItem> GptblBomItem { get; set; }
        public virtual DbSet<GptblBomItemUdf> GptblBomItemUdf { get; set; }
        public virtual DbSet<GptblBomUdf> GptblBomUdf { get; set; }
        public virtual DbSet<GptblCalendar> GptblCalendar { get; set; }
        public virtual DbSet<GptblCalendarInterval> GptblCalendarInterval { get; set; }
        public virtual DbSet<GptblCalendarIntervalUdf> GptblCalendarIntervalUdf { get; set; }
        public virtual DbSet<GptblCalendarUdf> GptblCalendarUdf { get; set; }
        public virtual DbSet<GptblChart> GptblChart { get; set; }
        public virtual DbSet<GptblChartColorfilter> GptblChartColorfilter { get; set; }
        public virtual DbSet<GptblChartColorfilterItem> GptblChartColorfilterItem { get; set; }
        public virtual DbSet<GptblChartColorfilterUdf> GptblChartColorfilterUdf { get; set; }
        public virtual DbSet<GptblChartRowfilter> GptblChartRowfilter { get; set; }
        public virtual DbSet<GptblChartRowfilterItem> GptblChartRowfilterItem { get; set; }
        public virtual DbSet<GptblChartRowfilterUdf> GptblChartRowfilterUdf { get; set; }
        public virtual DbSet<GptblChartSeries> GptblChartSeries { get; set; }
        public virtual DbSet<GptblChartSeriesUdf> GptblChartSeriesUdf { get; set; }
        public virtual DbSet<GptblChartText> GptblChartText { get; set; }
        public virtual DbSet<GptblChartTextItem> GptblChartTextItem { get; set; }
        public virtual DbSet<GptblChartTextUdf> GptblChartTextUdf { get; set; }
        public virtual DbSet<GptblChartUdf> GptblChartUdf { get; set; }
        public virtual DbSet<GptblConfirmation> GptblConfirmation { get; set; }
        public virtual DbSet<GptblConfirmationResource> GptblConfirmationResource { get; set; }
        public virtual DbSet<GptblConfirmationResourceInterval> GptblConfirmationResourceInterval { get; set; }
        public virtual DbSet<GptblConfirmationResourceIntervalUdf> GptblConfirmationResourceIntervalUdf { get; set; }
        public virtual DbSet<GptblConfirmationResourceUdf> GptblConfirmationResourceUdf { get; set; }
        public virtual DbSet<GptblConfirmationUdf> GptblConfirmationUdf { get; set; }
        public virtual DbSet<GptblGanttcolorscheme> GptblGanttcolorscheme { get; set; }
        public virtual DbSet<GptblGanttcolorschemeFilter> GptblGanttcolorschemeFilter { get; set; }
        public virtual DbSet<GptblGanttcolorschemeFilterItem> GptblGanttcolorschemeFilterItem { get; set; }
        public virtual DbSet<GptblGanttcolorschemeFilterUdf> GptblGanttcolorschemeFilterUdf { get; set; }
        public virtual DbSet<GptblGanttcolorschemeUdf> GptblGanttcolorschemeUdf { get; set; }
        public virtual DbSet<GptblJournal> GptblJournal { get; set; }
        public virtual DbSet<GptblLogdberror> GptblLogdberror { get; set; }
        public virtual DbSet<GptblMaterial> GptblMaterial { get; set; }
        public virtual DbSet<GptblMaterialOptimizationgroup> GptblMaterialOptimizationgroup { get; set; }
        public virtual DbSet<GptblMaterialOptimizationgroupUdf> GptblMaterialOptimizationgroupUdf { get; set; }
        public virtual DbSet<GptblMaterialProductionversion> GptblMaterialProductionversion { get; set; }
        public virtual DbSet<GptblMaterialProductionversionUdf> GptblMaterialProductionversionUdf { get; set; }
        public virtual DbSet<GptblMaterialUdf> GptblMaterialUdf { get; set; }
        public virtual DbSet<GptblMaterialUnitconversion> GptblMaterialUnitconversion { get; set; }
        public virtual DbSet<GptblMaterialUnitconversionUdf> GptblMaterialUnitconversionUdf { get; set; }
        public virtual DbSet<GptblModelparameter> GptblModelparameter { get; set; }
        public virtual DbSet<GptblModelparameterUdf> GptblModelparameterUdf { get; set; }
        public virtual DbSet<GptblObjecttype> GptblObjecttype { get; set; }
        public virtual DbSet<GptblObjecttypeUdf> GptblObjecttypeUdf { get; set; }
        public virtual DbSet<GptblOptimizationgroup> GptblOptimizationgroup { get; set; }
        public virtual DbSet<GptblOptimizationgroupUdf> GptblOptimizationgroupUdf { get; set; }
        public virtual DbSet<GptblPlanningparameter> GptblPlanningparameter { get; set; }
        public virtual DbSet<GptblPlanningparameterUdf> GptblPlanningparameterUdf { get; set; }
        public virtual DbSet<GptblPriority> GptblPriority { get; set; }
        public virtual DbSet<GptblPriorityLatenesscost> GptblPriorityLatenesscost { get; set; }
        public virtual DbSet<GptblPriorityLatenesscostUdf> GptblPriorityLatenesscostUdf { get; set; }
        public virtual DbSet<GptblPriorityUdf> GptblPriorityUdf { get; set; }
        public virtual DbSet<GptblProductioncontroller> GptblProductioncontroller { get; set; }
        public virtual DbSet<GptblProductioncontrollerUdf> GptblProductioncontrollerUdf { get; set; }
        public virtual DbSet<GptblProductionorder> GptblProductionorder { get; set; }
        public virtual DbSet<GptblProductionorderOperationActivity> GptblProductionorderOperationActivity { get; set; }
        public virtual DbSet<GptblProductionorderOperationActivityFixedresource> GptblProductionorderOperationActivityFixedresource { get; set; }
        public virtual DbSet<GptblProductionorderOperationActivityFixedresourceUdf> GptblProductionorderOperationActivityFixedresourceUdf { get; set; }
        public virtual DbSet<GptblProductionorderOperationActivityMaterialrelation> GptblProductionorderOperationActivityMaterialrelation { get; set; }
        public virtual DbSet<GptblProductionorderOperationActivityMaterialrelationUdf> GptblProductionorderOperationActivityMaterialrelationUdf { get; set; }
        public virtual DbSet<GptblProductionorderOperationActivityResource> GptblProductionorderOperationActivityResource { get; set; }
        public virtual DbSet<GptblProductionorderOperationActivityResourceInterval> GptblProductionorderOperationActivityResourceInterval { get; set; }
        public virtual DbSet<GptblProductionorderOperationActivityResourceIntervalUdf> GptblProductionorderOperationActivityResourceIntervalUdf { get; set; }
        public virtual DbSet<GptblProductionorderOperationActivityResourceUdf> GptblProductionorderOperationActivityResourceUdf { get; set; }
        public virtual DbSet<GptblProductionorderOperationActivityUdf> GptblProductionorderOperationActivityUdf { get; set; }
        public virtual DbSet<GptblProductionorderUdf> GptblProductionorderUdf { get; set; }
        public virtual DbSet<GptblPrt> GptblPrt { get; set; }
        public virtual DbSet<GptblPrtCalendarinterval> GptblPrtCalendarinterval { get; set; }
        public virtual DbSet<GptblPrtCalendarintervalUdf> GptblPrtCalendarintervalUdf { get; set; }
        public virtual DbSet<GptblPrtShiftmodel> GptblPrtShiftmodel { get; set; }
        public virtual DbSet<GptblPrtShiftmodelUdf> GptblPrtShiftmodelUdf { get; set; }
        public virtual DbSet<GptblPrtUdf> GptblPrtUdf { get; set; }
        public virtual DbSet<GptblPrtWorkcenter> GptblPrtWorkcenter { get; set; }
        public virtual DbSet<GptblPrtWorkcenterUdf> GptblPrtWorkcenterUdf { get; set; }
        public virtual DbSet<GptblPurchaseorder> GptblPurchaseorder { get; set; }
        public virtual DbSet<GptblPurchaseorderUdf> GptblPurchaseorderUdf { get; set; }
        public virtual DbSet<GptblReport> GptblReport { get; set; }
        public virtual DbSet<GptblReportColorfilter> GptblReportColorfilter { get; set; }
        public virtual DbSet<GptblReportColorfilterItem> GptblReportColorfilterItem { get; set; }
        public virtual DbSet<GptblReportColorfilterUdf> GptblReportColorfilterUdf { get; set; }
        public virtual DbSet<GptblReportColumn> GptblReportColumn { get; set; }
        public virtual DbSet<GptblReportColumnUdf> GptblReportColumnUdf { get; set; }
        public virtual DbSet<GptblReportOption> GptblReportOption { get; set; }
        public virtual DbSet<GptblReportOptionItem> GptblReportOptionItem { get; set; }
        public virtual DbSet<GptblReportOptionUdf> GptblReportOptionUdf { get; set; }
        public virtual DbSet<GptblReportRowfilter> GptblReportRowfilter { get; set; }
        public virtual DbSet<GptblReportRowfilterItem> GptblReportRowfilterItem { get; set; }
        public virtual DbSet<GptblReportRowfilterUdf> GptblReportRowfilterUdf { get; set; }
        public virtual DbSet<GptblReportUdf> GptblReportUdf { get; set; }
        public virtual DbSet<GptblResultinfo> GptblResultinfo { get; set; }
        public virtual DbSet<GptblResultinfoUdf> GptblResultinfoUdf { get; set; }
        public virtual DbSet<GptblRouting> GptblRouting { get; set; }
        public virtual DbSet<GptblRoutingOperation> GptblRoutingOperation { get; set; }
        public virtual DbSet<GptblRoutingOperationActivity> GptblRoutingOperationActivity { get; set; }
        public virtual DbSet<GptblRoutingOperationActivityBomItem> GptblRoutingOperationActivityBomItem { get; set; }
        public virtual DbSet<GptblRoutingOperationActivityBomItemUdf> GptblRoutingOperationActivityBomItemUdf { get; set; }
        public virtual DbSet<GptblRoutingOperationActivityPrt> GptblRoutingOperationActivityPrt { get; set; }
        public virtual DbSet<GptblRoutingOperationActivityPrtUdf> GptblRoutingOperationActivityPrtUdf { get; set; }
        public virtual DbSet<GptblRoutingOperationActivityResourcereservation> GptblRoutingOperationActivityResourcereservation { get; set; }
        public virtual DbSet<GptblRoutingOperationActivityResourcereservationUdf> GptblRoutingOperationActivityResourcereservationUdf { get; set; }
        public virtual DbSet<GptblRoutingOperationActivityUdf> GptblRoutingOperationActivityUdf { get; set; }
        public virtual DbSet<GptblRoutingOperationActivityWorkcenterfactor> GptblRoutingOperationActivityWorkcenterfactor { get; set; }
        public virtual DbSet<GptblRoutingOperationActivityWorkcenterfactorUdf> GptblRoutingOperationActivityWorkcenterfactorUdf { get; set; }
        public virtual DbSet<GptblRoutingOperationOperationrelation> GptblRoutingOperationOperationrelation { get; set; }
        public virtual DbSet<GptblRoutingOperationOperationrelationUdf> GptblRoutingOperationOperationrelationUdf { get; set; }
        public virtual DbSet<GptblRoutingOperationOptimizationgroup> GptblRoutingOperationOptimizationgroup { get; set; }
        public virtual DbSet<GptblRoutingOperationOptimizationgroupUdf> GptblRoutingOperationOptimizationgroupUdf { get; set; }
        public virtual DbSet<GptblRoutingOperationUdf> GptblRoutingOperationUdf { get; set; }
        public virtual DbSet<GptblRoutingOperationWorkcenter> GptblRoutingOperationWorkcenter { get; set; }
        public virtual DbSet<GptblRoutingOperationWorkcenterUdf> GptblRoutingOperationWorkcenterUdf { get; set; }
        public virtual DbSet<GptblRoutingOperationWorker> GptblRoutingOperationWorker { get; set; }
        public virtual DbSet<GptblRoutingOperationWorkerUdf> GptblRoutingOperationWorkerUdf { get; set; }
        public virtual DbSet<GptblRoutingScrap> GptblRoutingScrap { get; set; }
        public virtual DbSet<GptblRoutingScrapItem> GptblRoutingScrapItem { get; set; }
        public virtual DbSet<GptblRoutingScrapItemUdf> GptblRoutingScrapItemUdf { get; set; }
        public virtual DbSet<GptblRoutingScrapUdf> GptblRoutingScrapUdf { get; set; }
        public virtual DbSet<GptblRoutingUdf> GptblRoutingUdf { get; set; }
        public virtual DbSet<GptblSalesorder> GptblSalesorder { get; set; }
        public virtual DbSet<GptblSalesorderMaterialrelation> GptblSalesorderMaterialrelation { get; set; }
        public virtual DbSet<GptblSalesorderMaterialrelationUdf> GptblSalesorderMaterialrelationUdf { get; set; }
        public virtual DbSet<GptblSalesorderUdf> GptblSalesorderUdf { get; set; }
        public virtual DbSet<GptblSetupmatrix> GptblSetupmatrix { get; set; }
        public virtual DbSet<GptblSetupmatrixItem> GptblSetupmatrixItem { get; set; }
        public virtual DbSet<GptblSetupmatrixItemUdf> GptblSetupmatrixItemUdf { get; set; }
        public virtual DbSet<GptblSetupmatrixUdf> GptblSetupmatrixUdf { get; set; }
        public virtual DbSet<GptblShift> GptblShift { get; set; }
        public virtual DbSet<GptblShiftInterval> GptblShiftInterval { get; set; }
        public virtual DbSet<GptblShiftIntervalUdf> GptblShiftIntervalUdf { get; set; }
        public virtual DbSet<GptblShiftUdf> GptblShiftUdf { get; set; }
        public virtual DbSet<GptblShiftmodel> GptblShiftmodel { get; set; }
        public virtual DbSet<GptblShiftmodelShift> GptblShiftmodelShift { get; set; }
        public virtual DbSet<GptblShiftmodelShiftUdf> GptblShiftmodelShiftUdf { get; set; }
        public virtual DbSet<GptblShiftmodelUdf> GptblShiftmodelUdf { get; set; }
        public virtual DbSet<GptblStockquantityposting> GptblStockquantityposting { get; set; }
        public virtual DbSet<GptblStockquantitypostingUdf> GptblStockquantitypostingUdf { get; set; }
        public virtual DbSet<GptblSystemconfiguration> GptblSystemconfiguration { get; set; }
        public virtual DbSet<GptblSystemlock> GptblSystemlock { get; set; }
        public virtual DbSet<GptblUnit> GptblUnit { get; set; }
        public virtual DbSet<GptblUnitUdf> GptblUnitUdf { get; set; }
        public virtual DbSet<GptblUnitUnitconversion> GptblUnitUnitconversion { get; set; }
        public virtual DbSet<GptblUnitUnitconversionUdf> GptblUnitUnitconversionUdf { get; set; }
        public virtual DbSet<GptblUser> GptblUser { get; set; }
        public virtual DbSet<GptblUserPermission> GptblUserPermission { get; set; }
        public virtual DbSet<GptblUserPermissionUdf> GptblUserPermissionUdf { get; set; }
        public virtual DbSet<GptblUserProductioncontroller> GptblUserProductioncontroller { get; set; }
        public virtual DbSet<GptblUserProductioncontrollerUdf> GptblUserProductioncontrollerUdf { get; set; }
        public virtual DbSet<GptblUserUdf> GptblUserUdf { get; set; }
        public virtual DbSet<GptblUserUsertemplate> GptblUserUsertemplate { get; set; }
        public virtual DbSet<GptblUserUsertemplateUdf> GptblUserUsertemplateUdf { get; set; }
        public virtual DbSet<GptblUsersettings> GptblUsersettings { get; set; }
        public virtual DbSet<GptblUsersettingsUdf> GptblUsersettingsUdf { get; set; }
        public virtual DbSet<GptblWorkcenter> GptblWorkcenter { get; set; }
        public virtual DbSet<GptblWorkcenterCalendarinterval> GptblWorkcenterCalendarinterval { get; set; }
        public virtual DbSet<GptblWorkcenterCalendarintervalUdf> GptblWorkcenterCalendarintervalUdf { get; set; }
        public virtual DbSet<GptblWorkcenterCost> GptblWorkcenterCost { get; set; }
        public virtual DbSet<GptblWorkcenterCostUdf> GptblWorkcenterCostUdf { get; set; }
        public virtual DbSet<GptblWorkcenterParallelworkcenter> GptblWorkcenterParallelworkcenter { get; set; }
        public virtual DbSet<GptblWorkcenterParallelworkcenterUdf> GptblWorkcenterParallelworkcenterUdf { get; set; }
        public virtual DbSet<GptblWorkcenterShiftmodel> GptblWorkcenterShiftmodel { get; set; }
        public virtual DbSet<GptblWorkcenterShiftmodelUdf> GptblWorkcenterShiftmodelUdf { get; set; }
        public virtual DbSet<GptblWorkcenterUdf> GptblWorkcenterUdf { get; set; }
        public virtual DbSet<GptblWorkcenterWorker> GptblWorkcenterWorker { get; set; }
        public virtual DbSet<GptblWorkcenterWorkerUdf> GptblWorkcenterWorkerUdf { get; set; }
        public virtual DbSet<GptblWorkcentergroup> GptblWorkcentergroup { get; set; }
        public virtual DbSet<GptblWorkcentergroupCalendarinterval> GptblWorkcentergroupCalendarinterval { get; set; }
        public virtual DbSet<GptblWorkcentergroupCalendarintervalUdf> GptblWorkcentergroupCalendarintervalUdf { get; set; }
        public virtual DbSet<GptblWorkcentergroupCost> GptblWorkcentergroupCost { get; set; }
        public virtual DbSet<GptblWorkcentergroupCostUdf> GptblWorkcentergroupCostUdf { get; set; }
        public virtual DbSet<GptblWorkcentergroupShiftmodel> GptblWorkcentergroupShiftmodel { get; set; }
        public virtual DbSet<GptblWorkcentergroupShiftmodelUdf> GptblWorkcentergroupShiftmodelUdf { get; set; }
        public virtual DbSet<GptblWorkcentergroupUdf> GptblWorkcentergroupUdf { get; set; }
        public virtual DbSet<GptblWorkcentergroupWorkcenter> GptblWorkcentergroupWorkcenter { get; set; }
        public virtual DbSet<GptblWorkcentergroupWorkcenterUdf> GptblWorkcentergroupWorkcenterUdf { get; set; }
        public virtual DbSet<GptblWorker> GptblWorker { get; set; }
        public virtual DbSet<GptblWorkerActivityqualification> GptblWorkerActivityqualification { get; set; }
        public virtual DbSet<GptblWorkerActivityqualificationUdf> GptblWorkerActivityqualificationUdf { get; set; }
        public virtual DbSet<GptblWorkerCalendarinterval> GptblWorkerCalendarinterval { get; set; }
        public virtual DbSet<GptblWorkerCalendarintervalUdf> GptblWorkerCalendarintervalUdf { get; set; }
        public virtual DbSet<GptblWorkerShiftmodel> GptblWorkerShiftmodel { get; set; }
        public virtual DbSet<GptblWorkerShiftmodelUdf> GptblWorkerShiftmodelUdf { get; set; }
        public virtual DbSet<GptblWorkerUdf> GptblWorkerUdf { get; set; }
        public virtual DbSet<GptblWorkerWorkcenterqualification> GptblWorkerWorkcenterqualification { get; set; }
        public virtual DbSet<GptblWorkerWorkcenterqualificationUdf> GptblWorkerWorkcenterqualificationUdf { get; set; }
        public virtual DbSet<GptblWorkergroup> GptblWorkergroup { get; set; }
        public virtual DbSet<GptblWorkergroupActivityqualification> GptblWorkergroupActivityqualification { get; set; }
        public virtual DbSet<GptblWorkergroupActivityqualificationUdf> GptblWorkergroupActivityqualificationUdf { get; set; }
        public virtual DbSet<GptblWorkergroupCalendarinterval> GptblWorkergroupCalendarinterval { get; set; }
        public virtual DbSet<GptblWorkergroupCalendarintervalUdf> GptblWorkergroupCalendarintervalUdf { get; set; }
        public virtual DbSet<GptblWorkergroupPoolcapacity> GptblWorkergroupPoolcapacity { get; set; }
        public virtual DbSet<GptblWorkergroupPoolcapacityUdf> GptblWorkergroupPoolcapacityUdf { get; set; }
        public virtual DbSet<GptblWorkergroupShiftmodel> GptblWorkergroupShiftmodel { get; set; }
        public virtual DbSet<GptblWorkergroupShiftmodelUdf> GptblWorkergroupShiftmodelUdf { get; set; }
        public virtual DbSet<GptblWorkergroupUdf> GptblWorkergroupUdf { get; set; }
        public virtual DbSet<GptblWorkergroupWorkcenterqualification> GptblWorkergroupWorkcenterqualification { get; set; }
        public virtual DbSet<GptblWorkergroupWorkcenterqualificationUdf> GptblWorkergroupWorkcenterqualificationUdf { get; set; }
        public virtual DbSet<GptblWorkergroupWorker> GptblWorkergroupWorker { get; set; }
        public virtual DbSet<GptblWorkergroupWorkerUdf> GptblWorkergroupWorkerUdf { get; set; }
        public virtual DbSet<Material> Material { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("SERVER=(localdb)\\MSSQLLocalDB;DATABASE=GanttPlanImportTestDB;Trusted_connection=Yes;UID=;PWD=");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GptblAttachment>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.AttachmentId })
                    .HasName("PK_attachment");

                entity.ToTable("gptbl_attachment");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AttachmentId)
                    .HasColumnName("attachment_id")
                    .HasMaxLength(50);

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ObjectId)
                    .HasColumnName("object_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ObjecttypeId)
                    .HasColumnName("objecttype_id")
                    .HasMaxLength(250);

                entity.Property(e => e.Path).HasColumnName("path");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblAttachmentUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.AttachmentId, e.UdfId })
                    .HasName("PK_attachment_udf");

                entity.ToTable("gptbl_attachment_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AttachmentId)
                    .HasColumnName("attachment_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value");
            });

            modelBuilder.Entity<GptblBom>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.BomId })
                    .HasName("PK_bom");

                entity.ToTable("gptbl_bom");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.BomId)
                    .HasColumnName("bom_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.MasterBomId)
                    .HasColumnName("master_bom_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<GptblBomItem>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.BomId, e.ItemId, e.AlternativeId })
                    .HasName("PK_bom_item");

                entity.ToTable("gptbl_bom_item");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.BomId)
                    .HasColumnName("bom_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ItemId)
                    .HasColumnName("item_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Group).HasColumnName("group");

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.PreparationTime).HasColumnName("preparation_time");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.QuantityUnitId)
                    .HasColumnName("quantity_unit_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Standard).HasColumnName("standard");
            });

            modelBuilder.Entity<GptblBomItemUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.BomId, e.ItemId, e.AlternativeId })
                    .HasName("PK_bom_item_udf");

                entity.ToTable("gptbl_bom_item_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.BomId)
                    .HasColumnName("bom_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ItemId)
                    .HasColumnName("item_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblBomUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.BomId })
                    .HasName("PK_bom_udf");

                entity.ToTable("gptbl_bom_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.BomId)
                    .HasColumnName("bom_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblCalendar>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.CalendarId })
                    .HasName("PK_calendar");

                entity.ToTable("gptbl_calendar");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.CalendarId)
                    .HasColumnName("calendar_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<GptblCalendarInterval>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.CalendarId, e.DateFrom });

                entity.ToTable("gptbl_calendar_interval");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.CalendarId)
                    .HasColumnName("calendar_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.DateTo)
                    .HasColumnName("date_to")
                    .HasColumnType("datetime");

                entity.Property(e => e.IntervalType).HasColumnName("interval_type");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.RepetitionType).HasColumnName("repetition_type");
            });

            modelBuilder.Entity<GptblCalendarIntervalUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.CalendarId, e.DateFrom })
                    .HasName("PK_calendar_interval_udf");

                entity.ToTable("gptbl_calendar_interval_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.CalendarId)
                    .HasColumnName("calendar_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblCalendarUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.CalendarId })
                    .HasName("PK_calendar_udf");

                entity.ToTable("gptbl_calendar_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.CalendarId)
                    .HasColumnName("calendar_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblChart>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ChartId });

                entity.ToTable("gptbl_chart");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChartId)
                    .HasColumnName("chart_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AllInOne).HasColumnName("all_in_one");

                entity.Property(e => e.ArgumentDataMemberId).HasColumnName("argument_data_member_id");

                entity.Property(e => e.Category).HasColumnName("category");

                entity.Property(e => e.ChartType).HasColumnName("chart_type");

                entity.Property(e => e.ColorPaletteName).HasColumnName("color_palette_name");

                entity.Property(e => e.CustomChartTitle).HasColumnName("custom_chart_title");

                entity.Property(e => e.CustomXaxisTitle).HasColumnName("custom_xaxis_title");

                entity.Property(e => e.CustomYaxisTitle).HasColumnName("custom_yaxis_title");

                entity.Property(e => e.HeaderAsArgument).HasColumnName("header_as_argument");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ReportId)
                    .HasColumnName("report_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ReportType).HasColumnName("report_type");

                entity.Property(e => e.Rotated).HasColumnName("rotated");

                entity.Property(e => e.ShowLabels).HasColumnName("show_labels");

                entity.Property(e => e.ShowLegend).HasColumnName("show_legend");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblChartColorfilter>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ChartId, e.FilterId });

                entity.ToTable("gptbl_chart_colorfilter");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChartId)
                    .HasColumnName("chart_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterId)
                    .HasColumnName("filter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Color).HasColumnName("color");

                entity.Property(e => e.FilterOperator).HasColumnName("filter_operator");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");

                entity.Property(e => e.PropertyId).HasColumnName("property_id");
            });

            modelBuilder.Entity<GptblChartColorfilterItem>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("gptbl_chart_colorfilter_item");

                entity.Property(e => e.ChartId)
                    .IsRequired()
                    .HasColumnName("chart_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterId)
                    .IsRequired()
                    .HasColumnName("filter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterMatchingType).HasColumnName("filter_matching_type");

                entity.Property(e => e.PropertyId).HasColumnName("property_id");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblChartColorfilterUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ChartId, e.FilterId })
                    .HasName("PK_chart_colorfilter_udf");

                entity.ToTable("gptbl_chart_colorfilter_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChartId)
                    .HasColumnName("chart_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterId)
                    .HasColumnName("filter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblChartRowfilter>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ChartId, e.FilterId });

                entity.ToTable("gptbl_chart_rowfilter");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChartId)
                    .HasColumnName("chart_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterId)
                    .HasColumnName("filter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterOperator).HasColumnName("filter_operator");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");
            });

            modelBuilder.Entity<GptblChartRowfilterItem>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("gptbl_chart_rowfilter_item");

                entity.Property(e => e.ChartId)
                    .IsRequired()
                    .HasColumnName("chart_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterId)
                    .IsRequired()
                    .HasColumnName("filter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterMatchingType).HasColumnName("filter_matching_type");

                entity.Property(e => e.PropertyId).HasColumnName("property_id");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblChartRowfilterUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ChartId, e.FilterId })
                    .HasName("PK_chart_rowfilter_udf");

                entity.ToTable("gptbl_chart_rowfilter_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChartId)
                    .HasColumnName("chart_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterId)
                    .HasColumnName("filter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblChartSeries>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ChartId, e.SeriesId });

                entity.ToTable("gptbl_chart_series");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChartId)
                    .HasColumnName("chart_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SeriesId)
                    .HasColumnName("series_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChartType).HasColumnName("chart_type");

                entity.Property(e => e.Color).HasColumnName("color");

                entity.Property(e => e.ReportColumnName).HasColumnName("report_column_name");

                entity.Property(e => e.ReportColumnPropertyId).HasColumnName("report_column_property_id");
            });

            modelBuilder.Entity<GptblChartSeriesUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ChartId, e.SeriesId })
                    .HasName("PK_chart_series_udf");

                entity.ToTable("gptbl_chart_series_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChartId)
                    .HasColumnName("chart_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SeriesId)
                    .HasColumnName("series_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblChartText>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ChartId, e.TextId })
                    .HasName("PK_GP_ReportChartText");

                entity.ToTable("gptbl_chart_text");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChartId)
                    .HasColumnName("chart_id")
                    .HasMaxLength(50);

                entity.Property(e => e.TextId)
                    .HasColumnName("text_id")
                    .HasMaxLength(50);

                entity.Property(e => e.TextType).HasColumnName("text_type");
            });

            modelBuilder.Entity<GptblChartTextItem>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("gptbl_chart_text_item");

                entity.Property(e => e.ChartId)
                    .IsRequired()
                    .HasColumnName("chart_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PropertyId).HasColumnName("property_id");

                entity.Property(e => e.PropertyName).HasColumnName("property_name");

                entity.Property(e => e.PropertyNameType).HasColumnName("property_name_type");

                entity.Property(e => e.TextId)
                    .IsRequired()
                    .HasColumnName("text_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblChartTextUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ChartId, e.TextId })
                    .HasName("PK_chart_text_udf");

                entity.ToTable("gptbl_chart_text_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChartId)
                    .HasColumnName("chart_id")
                    .HasMaxLength(50);

                entity.Property(e => e.TextId)
                    .HasColumnName("text_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblChartUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ChartId })
                    .HasName("PK_chart_udf");

                entity.ToTable("gptbl_chart_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChartId)
                    .HasColumnName("chart_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblConfirmation>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ConfirmationId })
                    .HasName("PK_confirmation");

                entity.ToTable("gptbl_confirmation");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ConfirmationId)
                    .HasColumnName("confirmation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityEnd)
                    .HasColumnName("activity_end")
                    .HasColumnType("datetime");

                entity.Property(e => e.ActivityStart)
                    .HasColumnName("activity_start")
                    .HasColumnType("datetime");

                entity.Property(e => e.ConfirmationDate)
                    .HasColumnName("confirmation_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.ConfirmationType).HasColumnName("confirmation_type");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ProductionorderActivityId).HasColumnName("productionorder_activity_id");

                entity.Property(e => e.ProductionorderAlternativeId)
                    .HasColumnName("productionorder_alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionorderId)
                    .HasColumnName("productionorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionorderOperationId)
                    .HasColumnName("productionorder_operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionorderSplitId).HasColumnName("productionorder_split_id");

                entity.Property(e => e.QuantityFinished).HasColumnName("quantity_finished");

                entity.Property(e => e.QuantityFinishedUnitId)
                    .HasColumnName("quantity_finished_unit_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblConfirmationResource>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ResourceType, e.ConfirmationId, e.ResourceId })
                    .HasName("PK_confirmation_resource");

                entity.ToTable("gptbl_confirmation_resource");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.ConfirmationId)
                    .HasColumnName("confirmation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ResourceId)
                    .HasColumnName("resource_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Allocation).HasColumnName("allocation");

                entity.Property(e => e.GroupId)
                    .HasColumnName("group_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblConfirmationResourceInterval>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ResourceType, e.ConfirmationId, e.ResourceId, e.DateFrom })
                    .HasName("PK_confirmation_resource_interval");

                entity.ToTable("gptbl_confirmation_resource_interval");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.ConfirmationId)
                    .HasColumnName("confirmation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ResourceId)
                    .HasColumnName("resource_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.DateTo)
                    .HasColumnName("date_to")
                    .HasColumnType("datetime");

                entity.Property(e => e.IntervalAllocationType).HasColumnName("interval_allocation_type");
            });

            modelBuilder.Entity<GptblConfirmationResourceIntervalUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ResourceType, e.ConfirmationId, e.ResourceId, e.DateFrom })
                    .HasName("PK_confirmation_resource_interval_udf");

                entity.ToTable("gptbl_confirmation_resource_interval_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.ConfirmationId)
                    .HasColumnName("confirmation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ResourceId)
                    .HasColumnName("resource_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblConfirmationResourceUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ResourceType, e.ConfirmationId, e.ResourceId })
                    .HasName("PK_confirmation_resource_udf");

                entity.ToTable("gptbl_confirmation_resource_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.ConfirmationId)
                    .HasColumnName("confirmation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ResourceId)
                    .HasColumnName("resource_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblConfirmationUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ConfirmationId })
                    .HasName("PK_confirmation_udf");

                entity.ToTable("gptbl_confirmation_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ConfirmationId)
                    .HasColumnName("confirmation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblGanttcolorscheme>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.GanttcolorschemeId });

                entity.ToTable("gptbl_ganttcolorscheme");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.GanttcolorschemeId)
                    .HasColumnName("ganttcolorscheme_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Category).HasColumnName("category");

                entity.Property(e => e.DefaultColor).HasColumnName("default_color");

                entity.Property(e => e.DefaultGanttcolorschemeId)
                    .HasColumnName("default_ganttcolorscheme_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DefaultRandomcolorPropertyId).HasColumnName("default_randomcolor_property_id");

                entity.Property(e => e.DefaultType).HasColumnName("default_type");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblGanttcolorschemeFilter>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.GanttcolorschemeId, e.FilterId });

                entity.ToTable("gptbl_ganttcolorscheme_filter");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.GanttcolorschemeId)
                    .HasColumnName("ganttcolorscheme_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterId)
                    .HasColumnName("filter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Color).HasColumnName("color");

                entity.Property(e => e.FilterOperator).HasColumnName("filter_operator");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");
            });

            modelBuilder.Entity<GptblGanttcolorschemeFilterItem>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("gptbl_ganttcolorscheme_filter_item");

                entity.Property(e => e.CaseSensitive).HasColumnName("case_sensitive");

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterId)
                    .IsRequired()
                    .HasColumnName("filter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterMatchingType).HasColumnName("filter_matching_type");

                entity.Property(e => e.GanttcolorschemeId)
                    .IsRequired()
                    .HasColumnName("ganttcolorscheme_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PropertyId).HasColumnName("property_id");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblGanttcolorschemeFilterUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.GanttcolorschemeId, e.FilterId })
                    .HasName("PK_ganttcolorscheme_filter_udf");

                entity.ToTable("gptbl_ganttcolorscheme_filter_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.GanttcolorschemeId)
                    .HasColumnName("ganttcolorscheme_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterId)
                    .HasColumnName("filter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblGanttcolorschemeUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.GanttcolorschemeId })
                    .HasName("PK_ganttcolorscheme_udf");

                entity.ToTable("gptbl_ganttcolorscheme_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.GanttcolorschemeId)
                    .HasColumnName("ganttcolorscheme_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblJournal>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.JournalId })
                    .HasName("PK_journal");

                entity.ToTable("gptbl_journal");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.JournalId)
                    .HasColumnName("journal_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ActionType).HasColumnName("action_type");

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Host).HasColumnName("host");

                entity.Property(e => e.ObjectId)
                    .HasColumnName("object_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ObjecttypeId)
                    .HasColumnName("objecttype_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SessionId).HasColumnName("session_id");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblLogdberror>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("gptbl_logdberror");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Info).HasColumnName("info");

                entity.Property(e => e.Level).HasColumnName("level");

                entity.Property(e => e.Source).HasColumnName("source");
            });

            modelBuilder.Entity<GptblMaterial>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.MaterialId })
                    .HasName("PK_material");

                entity.ToTable("gptbl_material");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .HasMaxLength(50);

                entity.Property(e => e.CheckStockQuantity).HasColumnName("check_stock_quantity");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.InhouseProduction).HasColumnName("inhouse_production");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.LotSizeMax).HasColumnName("lot_size_max");

                entity.Property(e => e.LotSizeMin).HasColumnName("lot_size_min");

                entity.Property(e => e.LotSizeOpt).HasColumnName("lot_size_opt");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ProductioncontrollerId)
                    .HasColumnName("productioncontroller_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PurchaseTimeQuantityDependent).HasColumnName("purchase_time_quantity_dependent");

                entity.Property(e => e.PurchaseTimeQuantityIndependent).HasColumnName("purchase_time_quantity_independent");

                entity.Property(e => e.QuantityRoundingValue).HasColumnName("quantity_rounding_value");

                entity.Property(e => e.QuantityUnitId)
                    .HasColumnName("quantity_unit_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ReduceStockQuantity).HasColumnName("reduce_stock_quantity");

                entity.Property(e => e.SafetyStockUsage).HasColumnName("safety_stock_usage");

                entity.Property(e => e.SafetyStockValue).HasColumnName("safety_stock_value");

                entity.Property(e => e.ValueProduction).HasColumnName("value_production");

                entity.Property(e => e.ValuePurchase).HasColumnName("value_purchase");

                entity.Property(e => e.ValueSales).HasColumnName("value_sales");

                entity.Property(e => e.WaitingTimeMax).HasColumnName("waiting_time_max");
            });

            modelBuilder.Entity<GptblMaterialOptimizationgroup>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.MaterialId, e.OptimizationgroupType })
                    .HasName("PK_material_optimizationgroup");

                entity.ToTable("gptbl_material_optimizationgroup");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OptimizationgroupType).HasColumnName("optimizationgroup_type");

                entity.Property(e => e.OptimizationgroupId)
                    .HasColumnName("optimizationgroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OptimizationgroupValue).HasColumnName("optimizationgroup_value");
            });

            modelBuilder.Entity<GptblMaterialOptimizationgroupUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.MaterialId, e.OptimizationgroupType })
                    .HasName("PK_material_optimizationgroup_udf");

                entity.ToTable("gptbl_material_optimizationgroup_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OptimizationgroupType).HasColumnName("optimizationgroup_type");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblMaterialProductionversion>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.MaterialId, e.ProductionversionId })
                    .HasName("PK_material_productionversion");

                entity.ToTable("gptbl_material_productionversion");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionversionId)
                    .HasColumnName("productionversion_id")
                    .HasMaxLength(50);

                entity.Property(e => e.BomId)
                    .HasColumnName("bom_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ValidFrom)
                    .HasColumnName("valid_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.ValidUntil)
                    .HasColumnName("valid_until")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<GptblMaterialProductionversionUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.MaterialId, e.ProductionversionId, e.UdfId })
                    .HasName("PK_material_productionversion_udf");

                entity.ToTable("gptbl_material_productionversion_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionversionId)
                    .HasColumnName("productionversion_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value)
                    .HasColumnName("value")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<GptblMaterialUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.MaterialId })
                    .HasName("PK_material_udf");

                entity.ToTable("gptbl_material_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblMaterialUnitconversion>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.MaterialId, e.UnitId, e.ConversionUnitId })
                    .HasName("PK_material_unitconversion");

                entity.ToTable("gptbl_material_unitconversion");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UnitId)
                    .HasColumnName("unit_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ConversionUnitId)
                    .HasColumnName("conversion_unit_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ConversionFactor).HasColumnName("conversion_factor");
            });

            modelBuilder.Entity<GptblMaterialUnitconversionUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.MaterialId, e.UnitId, e.ConversionUnitId })
                    .HasName("PK_material_unitconversion_udf");

                entity.ToTable("gptbl_material_unitconversion_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UnitId)
                    .HasColumnName("unit_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ConversionUnitId)
                    .HasColumnName("conversion_unit_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblModelparameter>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ModelparameterId })
                    .HasName("PK_modelparameter");

                entity.ToTable("gptbl_modelparameter");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ModelparameterId)
                    .HasColumnName("modelparameter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActualTime)
                    .HasColumnName("actual_time")
                    .HasColumnType("datetime");

                entity.Property(e => e.ActualTimeFromSystemTime).HasColumnName("actual_time_from_system_time");

                entity.Property(e => e.AllowChangeWorkerActivityTimeMin).HasColumnName("allow_change_worker_activity_time_min");

                entity.Property(e => e.AllowMultipleMachineWork).HasColumnName("allow_multiple_machine_work");

                entity.Property(e => e.AllowOverlapActivityTypeSetup).HasColumnName("allow_overlap_activity_type_setup");

                entity.Property(e => e.AllowOverlapActivityTypeWait).HasColumnName("allow_overlap_activity_type_wait");

                entity.Property(e => e.AutoCalculatePeriods).HasColumnName("auto_calculate_periods");

                entity.Property(e => e.AutoConfirmChildProductionorders).HasColumnName("auto_confirm_child_productionorders");

                entity.Property(e => e.CapacityPeriodEnd)
                    .HasColumnName("capacity_period_end")
                    .HasColumnType("datetime");

                entity.Property(e => e.CapacityPeriodPast).HasColumnName("capacity_period_past");

                entity.Property(e => e.CapacityPeriodPlanning).HasColumnName("capacity_period_planning");

                entity.Property(e => e.CapacityPeriodStart)
                    .HasColumnName("capacity_period_start")
                    .HasColumnType("datetime");

                entity.Property(e => e.CapitalCommitmentInterestRate).HasColumnName("capital_commitment_interest_rate");

                entity.Property(e => e.DataPeriodEnd)
                    .HasColumnName("data_period_end")
                    .HasColumnType("datetime");

                entity.Property(e => e.DataPeriodPlanning).HasColumnName("data_period_planning");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.ObjectiveFunctionType).HasColumnName("objective_function_type");

                entity.Property(e => e.SchedulePrt).HasColumnName("schedule_prt");

                entity.Property(e => e.ScheduleWorker).HasColumnName("schedule_worker");

                entity.Property(e => e.SchedulingStatusLate).HasColumnName("scheduling_status_late");

                entity.Property(e => e.SchedulingStatusOntime).HasColumnName("scheduling_status_ontime");

                entity.Property(e => e.StockPriorityId)
                    .HasColumnName("stock_priority_id")
                    .HasMaxLength(50);

                entity.Property(e => e.TimeBufferPurchaseorder).HasColumnName("time_buffer_purchaseorder");

                entity.Property(e => e.TimeBufferSalesorder).HasColumnName("time_buffer_salesorder");

                entity.Property(e => e.WorkerIntervalTimeMin).HasColumnName("worker_interval_time_min");
            });

            modelBuilder.Entity<GptblModelparameterUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ModelparameterId })
                    .HasName("PK_modelparameter_udf");

                entity.ToTable("gptbl_modelparameter_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ModelparameterId)
                    .HasColumnName("modelparameter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblObjecttype>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ObjecttypeId })
                    .HasName("PK_objecttype");

                entity.ToTable("gptbl_objecttype");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ObjecttypeId)
                    .HasColumnName("objecttype_id")
                    .HasMaxLength(250);

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");
            });

            modelBuilder.Entity<GptblObjecttypeUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ObjecttypeId, e.UdfId })
                    .HasName("PK_objecttype_udf");

                entity.ToTable("gptbl_objecttype_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ObjecttypeId)
                    .HasColumnName("objecttype_id")
                    .HasMaxLength(250);

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Category).HasColumnName("category");

                entity.Property(e => e.Datatype).HasColumnName("datatype");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.Range).HasColumnName("range");
            });

            modelBuilder.Entity<GptblOptimizationgroup>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.OptimizationgroupId })
                    .HasName("PK_optimizationgroup");

                entity.ToTable("gptbl_optimizationgroup");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OptimizationgroupId)
                    .HasColumnName("optimizationgroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ValueDifferenceMax).HasColumnName("value_difference_max");
            });

            modelBuilder.Entity<GptblOptimizationgroupUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.OptimizationgroupId })
                    .HasName("PK_optimizationgroup_udf");

                entity.ToTable("gptbl_optimizationgroup_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OptimizationgroupId)
                    .HasColumnName("optimizationgroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblPlanningparameter>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.PlanningparameterId })
                    .HasName("PK_planningparameter");

                entity.ToTable("gptbl_planningparameter");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PlanningparameterId)
                    .HasColumnName("planningparameter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ImportanceCapitalCommitment).HasColumnName("importance_capital_commitment");

                entity.Property(e => e.ImportanceLateness).HasColumnName("importance_lateness");

                entity.Property(e => e.ImportanceProcessing).HasColumnName("importance_processing");

                entity.Property(e => e.ImportanceSetup).HasColumnName("importance_setup");

                entity.Property(e => e.ImportanceThroughputTime).HasColumnName("importance_throughput_time");

                entity.Property(e => e.ImportanceUtilization).HasColumnName("importance_utilization");

                entity.Property(e => e.ImportanceWorker).HasColumnName("importance_worker");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.LotsizeOptimization).HasColumnName("lotsize_optimization");

                entity.Property(e => e.MrpCheckInhouseProduction).HasColumnName("mrp_check_inhouse_production");

                entity.Property(e => e.MrpCheckPurchase).HasColumnName("mrp_check_purchase");

                entity.Property(e => e.MrpCreateInhouseProduction).HasColumnName("mrp_create_inhouse_production");

                entity.Property(e => e.MrpCreatePurchase).HasColumnName("mrp_create_purchase");

                entity.Property(e => e.MrpRelinkProductionorders).HasColumnName("mrp_relink_productionorders");

                entity.Property(e => e.MrpRelinkPurchaseorders).HasColumnName("mrp_relink_purchaseorders");

                entity.Property(e => e.MrpRelinkStockreservations).HasColumnName("mrp_relink_stockreservations");

                entity.Property(e => e.OptimizationRunCount).HasColumnName("optimization_run_count");

                entity.Property(e => e.PlanningMode).HasColumnName("planning_mode");

                entity.Property(e => e.PlanningTypes).HasColumnName("planning_types");

                entity.Property(e => e.ResultCount).HasColumnName("result_count");

                entity.Property(e => e.StablePeriod).HasColumnName("stable_period");

                entity.Property(e => e.StrategyType).HasColumnName("strategy_type");
            });

            modelBuilder.Entity<GptblPlanningparameterUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.PlanningparameterId })
                    .HasName("PK_planningparameter_udf");

                entity.ToTable("gptbl_planningparameter_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PlanningparameterId)
                    .HasColumnName("planningparameter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblPriority>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.PriorityId })
                    .HasName("PK_priority");

                entity.ToTable("gptbl_priority");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PriorityId)
                    .HasColumnName("priority_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");
            });

            modelBuilder.Entity<GptblPriorityLatenesscost>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.PriorityId, e.Start })
                    .HasName("PK_priority_latenesscost");

                entity.ToTable("gptbl_priority_latenesscost");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PriorityId)
                    .HasColumnName("priority_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Start).HasColumnName("start");

                entity.Property(e => e.CostsAbsolute).HasColumnName("costs_absolute");

                entity.Property(e => e.CostsAbsoluteInterval).HasColumnName("costs_absolute_interval");

                entity.Property(e => e.CostsRelative).HasColumnName("costs_relative");

                entity.Property(e => e.CostsRelativeInterval).HasColumnName("costs_relative_interval");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<GptblPriorityLatenesscostUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.PriorityId, e.Start })
                    .HasName("PK_priority_latenesscost_udf");

                entity.ToTable("gptbl_priority_latenesscost_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PriorityId)
                    .HasColumnName("priority_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Start).HasColumnName("start");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblPriorityUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.PriorityId })
                    .HasName("PK_priority_udf");

                entity.ToTable("gptbl_priority_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PriorityId)
                    .HasColumnName("priority_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblProductioncontroller>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ProductioncontrollerId })
                    .HasName("PK_productioncontroller");

                entity.ToTable("gptbl_productioncontroller");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductioncontrollerId)
                    .HasColumnName("productioncontroller_id")
                    .HasMaxLength(50);

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.PlantId)
                    .HasColumnName("plant_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblProductioncontrollerUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ProductioncontrollerId, e.UdfId })
                    .HasName("PK_productioncontroller_udf");

                entity.ToTable("gptbl_productioncontroller_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductioncontrollerId)
                    .HasColumnName("productioncontroller_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblProductionorder>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ProductionorderId })
                    .HasName("PK_productionorder");

                entity.ToTable("gptbl_productionorder");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionorderId)
                    .HasColumnName("productionorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Approved).HasColumnName("approved");

                entity.Property(e => e.BomId)
                    .HasColumnName("bom_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateEnd)
                    .HasColumnName("date_end")
                    .HasColumnType("datetime");

                entity.Property(e => e.DateStart)
                    .HasColumnName("date_start")
                    .HasColumnType("datetime");

                entity.Property(e => e.Duedate)
                    .HasColumnName("duedate")
                    .HasColumnType("datetime");

                entity.Property(e => e.EarliestStartDate)
                    .HasColumnName("earliest_start_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.FixedProperties).HasColumnName("fixed_properties");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.InfoDateEndInitial)
                    .HasColumnName("info_date_end_initial")
                    .HasColumnType("datetime");

                entity.Property(e => e.InfoDateStartInitial)
                    .HasColumnName("info_date_start_initial")
                    .HasColumnType("datetime");

                entity.Property(e => e.InfoDebug).HasColumnName("info_debug");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Locked).HasColumnName("locked");

                entity.Property(e => e.LotParentId)
                    .HasColumnName("lot_parent_id")
                    .HasMaxLength(50);

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.PlanningType).HasColumnName("planning_type");

                entity.Property(e => e.PriorityId)
                    .HasColumnName("priority_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductioncontrollerId)
                    .HasColumnName("productioncontroller_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionversionId)
                    .HasColumnName("productionversion_id")
                    .HasMaxLength(50);

                entity.Property(e => e.QuantityGross).HasColumnName("quantity_gross");

                entity.Property(e => e.QuantityNet).HasColumnName("quantity_net");

                entity.Property(e => e.QuantityUnitId)
                    .HasColumnName("quantity_unit_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SchedulingStatus).HasColumnName("scheduling_status");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.ValueProduction).HasColumnName("value_production");

                entity.Property(e => e.ValueSales).HasColumnName("value_sales");
            });

            modelBuilder.Entity<GptblProductionorderOperationActivity>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ProductionorderId, e.OperationId, e.AlternativeId, e.ActivityId, e.SplitId })
                    .HasName("PK_productionorder_operation_activity");

                entity.ToTable("gptbl_productionorder_operation_activity");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionorderId)
                    .HasColumnName("productionorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.ActivityType).HasColumnName("activity_type");

                entity.Property(e => e.ConfirmationType).HasColumnName("confirmation_type");

                entity.Property(e => e.DateEnd)
                    .HasColumnName("date_end")
                    .HasColumnType("datetime");

                entity.Property(e => e.DateStart)
                    .HasColumnName("date_start")
                    .HasColumnType("datetime");

                entity.Property(e => e.DateStartFix)
                    .HasColumnName("date_start_fix")
                    .HasColumnType("datetime");

                entity.Property(e => e.DurationFix).HasColumnName("duration_fix");

                entity.Property(e => e.EarliestStartDate)
                    .HasColumnName("earliest_start_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.InfoDateEarliestEndInitial)
                    .HasColumnName("info_date_earliest_end_initial")
                    .HasColumnType("datetime");

                entity.Property(e => e.InfoDateEarliestEndMaterial)
                    .HasColumnName("info_date_earliest_end_material")
                    .HasColumnType("datetime");

                entity.Property(e => e.InfoDateEarliestEndScheduling)
                    .HasColumnName("info_date_earliest_end_scheduling")
                    .HasColumnType("datetime");

                entity.Property(e => e.InfoDateEarliestStartInitial)
                    .HasColumnName("info_date_earliest_start_initial")
                    .HasColumnType("datetime");

                entity.Property(e => e.InfoDateEarliestStartMaterial)
                    .HasColumnName("info_date_earliest_start_material")
                    .HasColumnType("datetime");

                entity.Property(e => e.InfoDateEarliestStartScheduling)
                    .HasColumnName("info_date_earliest_start_scheduling")
                    .HasColumnType("datetime");

                entity.Property(e => e.InfoDateEndInitial)
                    .HasColumnName("info_date_end_initial")
                    .HasColumnType("datetime");

                entity.Property(e => e.InfoDateLatestEndInitial)
                    .HasColumnName("info_date_latest_end_initial")
                    .HasColumnType("datetime");

                entity.Property(e => e.InfoDateLatestEndMaterial)
                    .HasColumnName("info_date_latest_end_material")
                    .HasColumnType("datetime");

                entity.Property(e => e.InfoDateLatestEndScheduling)
                    .HasColumnName("info_date_latest_end_scheduling")
                    .HasColumnType("datetime");

                entity.Property(e => e.InfoDateLatestStartInitial)
                    .HasColumnName("info_date_latest_start_initial")
                    .HasColumnType("datetime");

                entity.Property(e => e.InfoDateLatestStartMaterial)
                    .HasColumnName("info_date_latest_start_material")
                    .HasColumnType("datetime");

                entity.Property(e => e.InfoDateLatestStartScheduling)
                    .HasColumnName("info_date_latest_start_scheduling")
                    .HasColumnType("datetime");

                entity.Property(e => e.InfoDateStartInitial)
                    .HasColumnName("info_date_start_initial")
                    .HasColumnType("datetime");

                entity.Property(e => e.InfoDebug).HasColumnName("info_debug");

                entity.Property(e => e.InfoDuration).HasColumnName("info_duration");

                entity.Property(e => e.InfoNote).HasColumnName("info_note");

                entity.Property(e => e.InfoSetup).HasColumnName("info_setup");

                entity.Property(e => e.InfoTimeBufferInitial).HasColumnName("info_time_buffer_initial");

                entity.Property(e => e.InfoTimeBufferLatestEnd).HasColumnName("info_time_buffer_latest_end");

                entity.Property(e => e.InfoTimeBufferMaterial).HasColumnName("info_time_buffer_material");

                entity.Property(e => e.InfoTimeBufferScheduling).HasColumnName("info_time_buffer_scheduling");

                entity.Property(e => e.InfoWorkerUtilization).HasColumnName("info_worker_utilization");

                entity.Property(e => e.JobParallelId).HasColumnName("job_parallel_id");

                entity.Property(e => e.JobSequentialId).HasColumnName("job_sequential_id");

                entity.Property(e => e.LastConfirmationDate)
                    .HasColumnName("last_confirmation_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.OperationType).HasColumnName("operation_type");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");

                entity.Property(e => e.QuantityFinishedNet).HasColumnName("quantity_finished_net");

                entity.Property(e => e.QuantityGross).HasColumnName("quantity_gross");

                entity.Property(e => e.QuantityNet).HasColumnName("quantity_net");

                entity.Property(e => e.SchedulingLevel).HasColumnName("scheduling_level");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.ValueProduction).HasColumnName("value_production");

                entity.Property(e => e.ValueProductionAccumulated).HasColumnName("value_production_accumulated");
            });

            modelBuilder.Entity<GptblProductionorderOperationActivityFixedresource>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ProductionorderId, e.OperationId, e.AlternativeId, e.ActivityId, e.SplitId, e.ResourceId, e.ResourceType })
                    .HasName("PK_productionorder_operation_activity_fixedresource");

                entity.ToTable("gptbl_productionorder_operation_activity_fixedresource");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionorderId)
                    .HasColumnName("productionorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.ResourceId)
                    .HasColumnName("resource_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.GroupId)
                    .HasColumnName("group_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblProductionorderOperationActivityFixedresourceUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ProductionorderId, e.OperationId, e.AlternativeId, e.ActivityId, e.SplitId, e.ResourceId, e.ResourceType })
                    .HasName("PK_productionorder_operation_activity_fixedresource_udf");

                entity.ToTable("gptbl_productionorder_operation_activity_fixedresource_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionorderId)
                    .HasColumnName("productionorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.ResourceId)
                    .HasColumnName("resource_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblProductionorderOperationActivityMaterialrelation>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ProductionorderId, e.OperationId, e.AlternativeId, e.ActivityId, e.SplitId, e.MaterialrelationType, e.ChildId, e.ChildOperationId, e.ChildAlternativeId, e.ChildSplitId, e.ChildActivityId })
                    .HasName("PK_productionorder_operation_activity_materialrelation");

                entity.ToTable("gptbl_productionorder_operation_activity_materialrelation");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionorderId)
                    .HasColumnName("productionorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.MaterialrelationType).HasColumnName("materialrelation_type");

                entity.Property(e => e.ChildId)
                    .HasColumnName("child_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChildOperationId)
                    .HasColumnName("child_operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChildAlternativeId)
                    .HasColumnName("child_alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChildSplitId).HasColumnName("child_split_id");

                entity.Property(e => e.ChildActivityId).HasColumnName("child_activity_id");

                entity.Property(e => e.Fixed).HasColumnName("fixed");

                entity.Property(e => e.InfoDateAvailability)
                    .HasColumnName("info_date_availability")
                    .HasColumnType("datetime");

                entity.Property(e => e.InfoTimeBuffer).HasColumnName("info_time_buffer");

                entity.Property(e => e.OverlapValue).HasColumnName("overlap_value");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.QuantityUnitId)
                    .HasColumnName("quantity_unit_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblProductionorderOperationActivityMaterialrelationUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ProductionorderId, e.OperationId, e.AlternativeId, e.ActivityId, e.SplitId, e.MaterialrelationType, e.ChildId, e.ChildOperationId, e.ChildAlternativeId, e.ChildSplitId, e.ChildActivityId, e.UdfId })
                    .HasName("PK_productionorder_operation_activity_materialrelation_udf");

                entity.ToTable("gptbl_productionorder_operation_activity_materialrelation_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionorderId)
                    .HasColumnName("productionorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.MaterialrelationType).HasColumnName("materialrelation_type");

                entity.Property(e => e.ChildId)
                    .HasColumnName("child_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChildOperationId)
                    .HasColumnName("child_operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChildAlternativeId)
                    .HasColumnName("child_alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChildSplitId).HasColumnName("child_split_id");

                entity.Property(e => e.ChildActivityId).HasColumnName("child_activity_id");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblProductionorderOperationActivityResource>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ProductionorderId, e.OperationId, e.AlternativeId, e.ActivityId, e.SplitId, e.ResourceId, e.ResourceType })
                    .HasName("PK_productionorder_operation_activity_resource");

                entity.ToTable("gptbl_productionorder_operation_activity_resource");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionorderId)
                    .HasColumnName("productionorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.ResourceId)
                    .HasColumnName("resource_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.Allocation).HasColumnName("allocation");

                entity.Property(e => e.GroupId)
                    .HasColumnName("group_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblProductionorderOperationActivityResourceInterval>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ProductionorderId, e.OperationId, e.AlternativeId, e.ActivityId, e.SplitId, e.DateFrom, e.ResourceId, e.ResourceType })
                    .HasName("PK_productionorder_operation_activity_resource_interval");

                entity.ToTable("gptbl_productionorder_operation_activity_resource_interval");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionorderId)
                    .HasColumnName("productionorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.ResourceId)
                    .HasColumnName("resource_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.DateTo)
                    .HasColumnName("date_to")
                    .HasColumnType("datetime");

                entity.Property(e => e.IntervalAllocationType).HasColumnName("interval_allocation_type");
            });

            modelBuilder.Entity<GptblProductionorderOperationActivityResourceIntervalUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ProductionorderId, e.OperationId, e.AlternativeId, e.ActivityId, e.SplitId, e.DateFrom, e.ResourceId, e.ResourceType })
                    .HasName("PK_productionorder_operation_activity_resource_interval_udf");

                entity.ToTable("gptbl_productionorder_operation_activity_resource_interval_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionorderId)
                    .HasColumnName("productionorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.ResourceId)
                    .HasColumnName("resource_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblProductionorderOperationActivityResourceUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ProductionorderId, e.OperationId, e.AlternativeId, e.ActivityId, e.SplitId, e.ResourceId, e.ResourceType })
                    .HasName("PK_productionorder_operation_activity_resource_udf");

                entity.ToTable("gptbl_productionorder_operation_activity_resource_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionorderId)
                    .HasColumnName("productionorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.ResourceId)
                    .HasColumnName("resource_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblProductionorderOperationActivityUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ProductionorderId, e.OperationId, e.AlternativeId, e.ActivityId, e.SplitId })
                    .HasName("PK_productionorder_operation_activity_udf");

                entity.ToTable("gptbl_productionorder_operation_activity_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionorderId)
                    .HasColumnName("productionorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblProductionorderUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ProductionorderId })
                    .HasName("PK_productionorder_udf");

                entity.ToTable("gptbl_productionorder_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionorderId)
                    .HasColumnName("productionorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblPrt>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.PrtId })
                    .HasName("PK_prt");

                entity.ToTable("gptbl_prt");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PrtId)
                    .HasColumnName("prt_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AllocationMax).HasColumnName("allocation_max");

                entity.Property(e => e.AllowChangeWorkcenter).HasColumnName("allow_change_workcenter");

                entity.Property(e => e.CapacityType).HasColumnName("capacity_type");

                entity.Property(e => e.CostRateProcessing).HasColumnName("cost_rate_processing");

                entity.Property(e => e.CostRateSetup).HasColumnName("cost_rate_setup");

                entity.Property(e => e.FactorProcessingSpeed).HasColumnName("factor_processing_speed");

                entity.Property(e => e.GlobalCalendarId)
                    .HasColumnName("global_calendar_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.InterruptionTimeMax).HasColumnName("interruption_time_max");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.MaintenanceIntervalQuantity).HasColumnName("maintenance_interval_quantity");

                entity.Property(e => e.MaintenanceIntervalQuantityUnitId)
                    .HasColumnName("maintenance_interval_quantity_unit_id")
                    .HasMaxLength(50);

                entity.Property(e => e.MaintenanceIntervalTime).HasColumnName("maintenance_interval_time");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ParallelAllocationCriteria).HasColumnName("parallel_allocation_criteria");

                entity.Property(e => e.SetupTime).HasColumnName("setup_time");

                entity.Property(e => e.SynchronousStart).HasColumnName("synchronous_start");

                entity.Property(e => e.ValidUntil)
                    .HasColumnName("valid_until")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<GptblPrtCalendarinterval>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.PrtId, e.DateFrom })
                    .HasName("PK_prt_calendarinterval");

                entity.ToTable("gptbl_prt_calendarinterval");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PrtId)
                    .HasColumnName("prt_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.DateTo)
                    .HasColumnName("date_to")
                    .HasColumnType("datetime");

                entity.Property(e => e.IntervalType)
                    .HasColumnName("interval_type")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.RepetitionType)
                    .HasColumnName("repetition_type")
                    .HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<GptblPrtCalendarintervalUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.PrtId, e.DateFrom, e.UdfId })
                    .HasName("PK_prt_calendarinterval_udf");

                entity.ToTable("gptbl_prt_calendarinterval_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PrtId)
                    .HasColumnName("prt_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblPrtShiftmodel>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.PrtId, e.StartDate })
                    .HasName("PK_prt_shiftmodel");

                entity.ToTable("gptbl_prt_shiftmodel");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PrtId)
                    .HasColumnName("prt_id")
                    .HasMaxLength(50);

                entity.Property(e => e.StartDate)
                    .HasColumnName("start_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.ShiftmodelId)
                    .HasColumnName("shiftmodel_id")
                    .HasMaxLength(50);

                entity.Property(e => e.StartShiftIndex)
                    .HasColumnName("start_shift_index")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.ValidUntil)
                    .HasColumnName("valid_until")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'30001231 23:59:59')");
            });

            modelBuilder.Entity<GptblPrtShiftmodelUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.PrtId, e.UdfId })
                    .HasName("PK_prt_shiftmodel_udf");

                entity.ToTable("gptbl_prt_shiftmodel_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PrtId)
                    .HasColumnName("prt_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblPrtUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.PrtId })
                    .HasName("PK_prt_udf");

                entity.ToTable("gptbl_prt_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PrtId)
                    .HasColumnName("prt_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblPrtWorkcenter>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.PrtId, e.WorkcenterId })
                    .HasName("PK_prt_workcenter");

                entity.ToTable("gptbl_prt_workcenter");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PrtId)
                    .HasColumnName("prt_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FactorSetupTime).HasColumnName("factor_setup_time");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");
            });

            modelBuilder.Entity<GptblPrtWorkcenterUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.PrtId, e.WorkcenterId })
                    .HasName("PK_prt_workcenter_udf");

                entity.ToTable("gptbl_prt_workcenter_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PrtId)
                    .HasColumnName("prt_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblPurchaseorder>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.PurchaseorderId })
                    .HasName("PK_purchaseorder");

                entity.ToTable("gptbl_purchaseorder");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PurchaseorderId)
                    .HasColumnName("purchaseorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DeliveryDate)
                    .HasColumnName("delivery_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.InfoOrderDate)
                    .HasColumnName("info_order_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Locked).HasColumnName("locked");

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ProductioncontrollerId)
                    .HasColumnName("productioncontroller_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PurchaseorderType).HasColumnName("purchaseorder_type");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.QuantityDelivered).HasColumnName("quantity_delivered");

                entity.Property(e => e.QuantityUnitId)
                    .HasColumnName("quantity_unit_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Status).HasColumnName("status");
            });

            modelBuilder.Entity<GptblPurchaseorderUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.PurchaseorderId })
                    .HasName("PK_purchaseorder_udf");

                entity.ToTable("gptbl_purchaseorder_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PurchaseorderId)
                    .HasColumnName("purchaseorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblReport>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ReportId });

                entity.ToTable("gptbl_report");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ReportId)
                    .HasColumnName("report_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Category).HasColumnName("category");

                entity.Property(e => e.DateFrom).HasColumnName("date_from");

                entity.Property(e => e.DateTo).HasColumnName("date_to");

                entity.Property(e => e.FixedColumnsCount).HasColumnName("fixed_columns_count");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.OrderAsc).HasColumnName("order_asc");

                entity.Property(e => e.PagebreakAfterObject).HasColumnName("pagebreak_after_object");

                entity.Property(e => e.ReportType).HasColumnName("report_type");

                entity.Property(e => e.SelectedObjectId).HasColumnName("selected_object_id");

                entity.Property(e => e.SortingColumn).HasColumnName("sorting_column");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblReportColorfilter>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ReportId, e.FilterId });

                entity.ToTable("gptbl_report_colorfilter");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ReportId)
                    .HasColumnName("report_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterId)
                    .HasColumnName("filter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.BgColor).HasColumnName("bg_color");

                entity.Property(e => e.FilterOperator).HasColumnName("filter_operator");

                entity.Property(e => e.FilterResultCountMax).HasColumnName("filter_result_count_max");

                entity.Property(e => e.FontColor).HasColumnName("font_color");

                entity.Property(e => e.FullColumnColoring).HasColumnName("full_column_coloring");

                entity.Property(e => e.FullRowColoring).HasColumnName("full_row_coloring");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");

                entity.Property(e => e.PropertyId).HasColumnName("property_id");
            });

            modelBuilder.Entity<GptblReportColorfilterItem>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("gptbl_report_colorfilter_item");

                entity.Property(e => e.CaseSensitive).HasColumnName("case_sensitive");

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterId)
                    .IsRequired()
                    .HasColumnName("filter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterMatchingType).HasColumnName("filter_matching_type");

                entity.Property(e => e.PropertyId).HasColumnName("property_id");

                entity.Property(e => e.ReportId)
                    .IsRequired()
                    .HasColumnName("report_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblReportColorfilterUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ReportId, e.FilterId })
                    .HasName("PK_report_colorfilter_udf");

                entity.ToTable("gptbl_report_colorfilter_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ReportId)
                    .HasColumnName("report_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterId)
                    .HasColumnName("filter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblReportColumn>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ReportId, e.Index });

                entity.ToTable("gptbl_report_column");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ReportId)
                    .HasColumnName("report_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Index).HasColumnName("index");

                entity.Property(e => e.Formula).HasColumnName("formula");

                entity.Property(e => e.PropertyId).HasColumnName("property_id");

                entity.Property(e => e.PropertyName).HasColumnName("property_name");
            });

            modelBuilder.Entity<GptblReportColumnUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ReportId, e.Index })
                    .HasName("PK_report_column_udf");

                entity.ToTable("gptbl_report_column_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ReportId)
                    .HasColumnName("report_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Index).HasColumnName("index");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblReportOption>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ReportId, e.OptionId });

                entity.ToTable("gptbl_report_option");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ReportId)
                    .HasColumnName("report_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OptionId)
                    .HasColumnName("option_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblReportOptionItem>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("gptbl_report_option_item");

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OptionId)
                    .IsRequired()
                    .HasColumnName("option_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ReportId)
                    .IsRequired()
                    .HasColumnName("report_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblReportOptionUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ReportId, e.OptionId })
                    .HasName("PK_report_option_udf");

                entity.ToTable("gptbl_report_option_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ReportId)
                    .HasColumnName("report_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OptionId)
                    .HasColumnName("option_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblReportRowfilter>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ReportId, e.FilterId });

                entity.ToTable("gptbl_report_rowfilter");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ReportId)
                    .HasColumnName("report_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterId)
                    .HasColumnName("filter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterOperator).HasColumnName("filter_operator");

                entity.Property(e => e.FilterResultCountMax).HasColumnName("filter_result_count_max");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");
            });

            modelBuilder.Entity<GptblReportRowfilterItem>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("gptbl_report_rowfilter_item");

                entity.Property(e => e.CaseSensitive).HasColumnName("case_sensitive");

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterId)
                    .IsRequired()
                    .HasColumnName("filter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterMatchingType).HasColumnName("filter_matching_type");

                entity.Property(e => e.PropertyId).HasColumnName("property_id");

                entity.Property(e => e.ReportId)
                    .IsRequired()
                    .HasColumnName("report_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblReportRowfilterUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ReportId, e.FilterId })
                    .HasName("PK_report_rowfilter_udf");

                entity.ToTable("gptbl_report_rowfilter_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ReportId)
                    .HasColumnName("report_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FilterId)
                    .HasColumnName("filter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblReportUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ReportId })
                    .HasName("PK_report_udf");

                entity.ToTable("gptbl_report_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ReportId)
                    .HasColumnName("report_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblResultinfo>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ResultinfoId })
                    .HasName("PK_resultinfo");

                entity.ToTable("gptbl_resultinfo");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ResultinfoId)
                    .HasColumnName("resultinfo_id")
                    .HasMaxLength(50);

                entity.Property(e => e.CountProductionorderEarly).HasColumnName("count_productionorder_early");

                entity.Property(e => e.CountProductionorderIncomplete).HasColumnName("count_productionorder_incomplete");

                entity.Property(e => e.CountProductionorderLate).HasColumnName("count_productionorder_late");

                entity.Property(e => e.CountProductionorderOntime).HasColumnName("count_productionorder_ontime");

                entity.Property(e => e.CountSalesorderIncomplete).HasColumnName("count_salesorder_incomplete");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.OptimizationRun).HasColumnName("optimization_run");

                entity.Property(e => e.Timestamp)
                    .HasColumnName("timestamp")
                    .HasColumnType("datetime");

                entity.Property(e => e.ValueCapitalCommitment).HasColumnName("value_capital_commitment");

                entity.Property(e => e.ValueLateness).HasColumnName("value_lateness");

                entity.Property(e => e.ValueObjectiveFunction).HasColumnName("value_objective_function");

                entity.Property(e => e.ValueProcessing).HasColumnName("value_processing");

                entity.Property(e => e.ValueSetup).HasColumnName("value_setup");

                entity.Property(e => e.ValueThroughputTime).HasColumnName("value_throughput_time");

                entity.Property(e => e.ValueTotal).HasColumnName("value_total");

                entity.Property(e => e.ValueUtilization).HasColumnName("value_utilization");

                entity.Property(e => e.ValueWorker).HasColumnName("value_worker");
            });

            modelBuilder.Entity<GptblResultinfoUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ResultinfoId })
                    .HasName("PK_resultinfo_udf");

                entity.ToTable("gptbl_resultinfo_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ResultinfoId)
                    .HasColumnName("resultinfo_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblRouting>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.RoutingId })
                    .HasName("PK_routing");

                entity.ToTable("gptbl_routing");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AllowOverlap).HasColumnName("allow_overlap");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Locked).HasColumnName("locked");

                entity.Property(e => e.MasterRoutingId)
                    .HasColumnName("master_routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<GptblRoutingOperation>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId })
                    .HasName("PK_routing_operation");

                entity.ToTable("gptbl_routing_operation");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.AllowInterruptions).HasColumnName("allow_interruptions");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.OperationType).HasColumnName("operation_type");

                entity.Property(e => e.QuantityRoundingValue).HasColumnName("quantity_rounding_value");

                entity.Property(e => e.SplitMax).HasColumnName("split_max");

                entity.Property(e => e.SplitMin).HasColumnName("split_min");

                entity.Property(e => e.WorkerRequirementType).HasColumnName("worker_requirement_type");
            });

            modelBuilder.Entity<GptblRoutingOperationActivity>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.ActivityId })
                    .HasName("PK_routing_operation_activity");

                entity.ToTable("gptbl_routing_operation_activity");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.ActivityType).HasColumnName("activity_type");

                entity.Property(e => e.ConfirmationRequired).HasColumnName("confirmation_required");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.InterruptionTimeMax).HasColumnName("interruption_time_max");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ParallelCountMax).HasColumnName("parallel_count_max");

                entity.Property(e => e.TimeQuantityDependent).HasColumnName("time_quantity_dependent");

                entity.Property(e => e.TimeQuantityIndependent).HasColumnName("time_quantity_independent");

                entity.Property(e => e.WorkcenterAllocation).HasColumnName("workcenter_allocation");
            });

            modelBuilder.Entity<GptblRoutingOperationActivityBomItem>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.RoutingId, e.OperationId, e.AlternativeId, e.ActivityId, e.SplitId, e.BomId, e.BomItemId })
                    .HasName("PK_routing_operation_activity_bom_item");

                entity.ToTable("gptbl_routing_operation_activity_bom_item");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.BomId)
                    .HasColumnName("bom_id")
                    .HasMaxLength(50);

                entity.Property(e => e.BomItemId)
                    .HasColumnName("bom_item_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblRoutingOperationActivityBomItemUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.RoutingId, e.OperationId, e.AlternativeId, e.ActivityId, e.SplitId, e.BomId, e.BomItemId, e.UdfId })
                    .HasName("PK_routing_operation_activity_bom_item_udf");

                entity.ToTable("gptbl_routing_operation_activity_bom_item_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.BomId)
                    .HasColumnName("bom_id")
                    .HasMaxLength(50);

                entity.Property(e => e.BomItemId)
                    .HasColumnName("bom_item_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblRoutingOperationActivityPrt>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.RoutingId, e.OperationId, e.ActivityId, e.AlternativeId, e.SplitId, e.PrtId, e.GroupId })
                    .HasName("PK_routing_operation_activity_prt");

                entity.ToTable("gptbl_routing_operation_activity_prt");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.PrtId)
                    .HasColumnName("prt_id")
                    .HasMaxLength(50);

                entity.Property(e => e.GroupId)
                    .HasColumnName("group_id")
                    .HasMaxLength(50);

                entity.Property(e => e.PrtAllocation).HasColumnName("prt_allocation");
            });

            modelBuilder.Entity<GptblRoutingOperationActivityPrtUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.RoutingId, e.OperationId, e.ActivityId, e.AlternativeId, e.SplitId, e.PrtId, e.GroupId })
                    .HasName("PK_routing_operation_activity_prt_udf");

                entity.ToTable("gptbl_routing_operation_activity_prt_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.PrtId)
                    .HasColumnName("prt_id")
                    .HasMaxLength(50);

                entity.Property(e => e.GroupId)
                    .HasColumnName("group_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblRoutingOperationActivityResourcereservation>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.ActivityId, e.ReservationId, e.ReservationType, e.ResourceType, e.ResourceId })
                    .HasName("PK_routing_operation_activity_resourcereservation");

                entity.ToTable("gptbl_routing_operation_activity_resourcereservation");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.ReservationId)
                    .HasColumnName("reservation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ReservationType).HasColumnName("reservation_type");

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.ResourceId)
                    .HasColumnName("resource_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblRoutingOperationActivityResourcereservationUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.ActivityId, e.ReservationId, e.ReservationType, e.ResourceType, e.ResourceId, e.UdfId })
                    .HasName("PK_routing_operation_activity_resourcereservation_udf");

                entity.ToTable("gptbl_routing_operation_activity_resourcereservation_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.ReservationId)
                    .HasColumnName("reservation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ReservationType).HasColumnName("reservation_type");

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.ResourceId)
                    .HasColumnName("resource_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblRoutingOperationActivityUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.ActivityId })
                    .HasName("PK_routing_operation_activity_udf");

                entity.ToTable("gptbl_routing_operation_activity_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblRoutingOperationActivityWorkcenterfactor>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.RoutingId, e.OperationId, e.ActivityId, e.AlternativeId, e.SplitId, e.WorkcenterId })
                    .HasName("PK_routing_operation_activity_workcenterfactor");

                entity.ToTable("gptbl_routing_operation_activity_workcenterfactor");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FactorWorkcenterTime).HasColumnName("factor_workcenter_time");
            });

            modelBuilder.Entity<GptblRoutingOperationActivityWorkcenterfactorUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.RoutingId, e.OperationId, e.ActivityId, e.AlternativeId, e.SplitId, e.WorkcenterId })
                    .HasName("PK_routing_operation_activity_workcenterfactor_udf");

                entity.ToTable("gptbl_routing_operation_activity_workcenterfactor_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblRoutingOperationOperationrelation>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.SuccessorOperationId, e.SuccessorAlternativeId, e.SuccessorSplitId })
                    .HasName("PK_routing_operation_operationrelation");

                entity.ToTable("gptbl_routing_operation_operationrelation");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.SuccessorOperationId)
                    .HasColumnName("successor_operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SuccessorAlternativeId)
                    .HasColumnName("successor_alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SuccessorSplitId).HasColumnName("successor_split_id");

                entity.Property(e => e.OverlapType).HasColumnName("overlap_type");

                entity.Property(e => e.OverlapValue).HasColumnName("overlap_value");

                entity.Property(e => e.TimeBufferMax).HasColumnName("time_buffer_max");

                entity.Property(e => e.TimeBufferMin).HasColumnName("time_buffer_min");
            });

            modelBuilder.Entity<GptblRoutingOperationOperationrelationUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.SuccessorOperationId, e.SuccessorAlternativeId, e.SuccessorSplitId })
                    .HasName("PK_routing_operation_operationrelation_udf");

                entity.ToTable("gptbl_routing_operation_operationrelation_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.SuccessorOperationId)
                    .HasColumnName("successor_operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SuccessorAlternativeId)
                    .HasColumnName("successor_alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SuccessorSplitId).HasColumnName("successor_split_id");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblRoutingOperationOptimizationgroup>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.OptimizationgroupType })
                    .HasName("PK_routing_operation_optimizationgroup");

                entity.ToTable("gptbl_routing_operation_optimizationgroup");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.OptimizationgroupType).HasColumnName("optimizationgroup_type");

                entity.Property(e => e.OptimizationgroupId)
                    .HasColumnName("optimizationgroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OptimizationgroupValue).HasColumnName("optimizationgroup_value");
            });

            modelBuilder.Entity<GptblRoutingOperationOptimizationgroupUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.OptimizationgroupType })
                    .HasName("PK_routing_operation_optimizationgroup_udf");

                entity.ToTable("gptbl_routing_operation_optimizationgroup_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.OptimizationgroupType).HasColumnName("optimizationgroup_type");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblRoutingOperationUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId })
                    .HasName("PK_routing_operation_udf");

                entity.ToTable("gptbl_routing_operation_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblRoutingOperationWorkcenter>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.WorkcenterId })
                    .HasName("PK_routing_operation_workcenter");

                entity.ToTable("gptbl_routing_operation_workcenter");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Delta).HasColumnName("delta");

                entity.Property(e => e.LotSizeMax).HasColumnName("lot_size_max");

                entity.Property(e => e.LotSizeMin).HasColumnName("lot_size_min");
            });

            modelBuilder.Entity<GptblRoutingOperationWorkcenterUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.WorkcenterId })
                    .HasName("PK_routing_operation_workcenter_udf");

                entity.ToTable("gptbl_routing_operation_workcenter_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblRoutingOperationWorker>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.GroupId })
                    .HasName("PK_routing_operation_worker");

                entity.ToTable("gptbl_routing_operation_worker");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.GroupId)
                    .HasColumnName("group_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityType).HasColumnName("activity_type");

                entity.Property(e => e.ActivityqualificationId).HasColumnName("activityqualification_id");

                entity.Property(e => e.ChangeWorkerType).HasColumnName("change_worker_type");

                entity.Property(e => e.WorkcenterId).HasColumnName("workcenter_id");

                entity.Property(e => e.WorkerRequirementCount).HasColumnName("worker_requirement_count");

                entity.Property(e => e.WorkerRequirementCountMax).HasColumnName("worker_requirement_count_max");

                entity.Property(e => e.WorkerRequirementUtilization).HasColumnName("worker_requirement_utilization");
            });

            modelBuilder.Entity<GptblRoutingOperationWorkerUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.GroupId })
                    .HasName("PK_routing_operation_worker_udf");

                entity.ToTable("gptbl_routing_operation_worker_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.OperationId)
                    .HasColumnName("operation_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AlternativeId)
                    .HasColumnName("alternative_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.GroupId)
                    .HasColumnName("group_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblRoutingScrap>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.RoutingId })
                    .HasName("PK_routing_scrap");

                entity.ToTable("gptbl_routing_scrap");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UnitId)
                    .HasColumnName("unit_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblRoutingScrapItem>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.RoutingId, e.QuantityLimit })
                    .HasName("PK_routing_scrap_item");

                entity.ToTable("gptbl_routing_scrap_item");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.QuantityLimit).HasColumnName("quantity_limit");

                entity.Property(e => e.ScrapRate).HasColumnName("scrap_rate");
            });

            modelBuilder.Entity<GptblRoutingScrapItemUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.RoutingId, e.QuantityLimit })
                    .HasName("PK_routing_scrap_item_udf");

                entity.ToTable("gptbl_routing_scrap_item_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.QuantityLimit).HasColumnName("quantity_limit");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblRoutingScrapUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.RoutingId })
                    .HasName("PK_routing_scrap_udf");

                entity.ToTable("gptbl_routing_scrap_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblRoutingUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.RoutingId })
                    .HasName("PK_routing_udf");

                entity.ToTable("gptbl_routing_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblSalesorder>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.SalesorderId })
                    .HasName("PK_salesorder");

                entity.ToTable("gptbl_salesorder");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SalesorderId)
                    .HasColumnName("salesorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Duedate)
                    .HasColumnName("duedate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Locked).HasColumnName("locked");

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.PlanningType).HasColumnName("planning_type");

                entity.Property(e => e.PriorityId)
                    .HasColumnName("priority_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductioncontrollerId)
                    .HasColumnName("productioncontroller_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.QuantityDelivered).HasColumnName("quantity_delivered");

                entity.Property(e => e.QuantityUnitId)
                    .HasColumnName("quantity_unit_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SalesorderType).HasColumnName("salesorder_type");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.ValueSales).HasColumnName("value_sales");
            });

            modelBuilder.Entity<GptblSalesorderMaterialrelation>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.SalesorderId, e.ChildId, e.MaterialrelationType })
                    .HasName("PK_salesorder_materialrelation");

                entity.ToTable("gptbl_salesorder_materialrelation");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SalesorderId)
                    .HasColumnName("salesorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChildId)
                    .HasColumnName("child_id")
                    .HasMaxLength(50);

                entity.Property(e => e.MaterialrelationType).HasColumnName("materialrelation_type");

                entity.Property(e => e.Fixed).HasColumnName("fixed");

                entity.Property(e => e.InfoDateAvailability)
                    .HasColumnName("info_date_availability")
                    .HasColumnType("datetime");

                entity.Property(e => e.InfoTimeBuffer).HasColumnName("info_time_buffer");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.QuantityUnitId)
                    .HasColumnName("quantity_unit_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblSalesorderMaterialrelationUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.SalesorderId, e.ChildId, e.MaterialrelationType })
                    .HasName("PK_salesorder_materialrelation_udf");

                entity.ToTable("gptbl_salesorder_materialrelation_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SalesorderId)
                    .HasColumnName("salesorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChildId)
                    .HasColumnName("child_id")
                    .HasMaxLength(50);

                entity.Property(e => e.MaterialrelationType).HasColumnName("materialrelation_type");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblSalesorderUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.SalesorderId })
                    .HasName("PK_salesorder_udf");

                entity.ToTable("gptbl_salesorder_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SalesorderId)
                    .HasColumnName("salesorder_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblSetupmatrix>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.SetupmatrixId })
                    .HasName("PK_setupmatrix");

                entity.ToTable("gptbl_setupmatrix");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SetupmatrixId)
                    .HasColumnName("setupmatrix_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<GptblSetupmatrixItem>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.FromOptimizationgroupId, e.ToOptimizationgroupId, e.SetupmatrixId })
                    .HasName("PK_setupmatrix_item");

                entity.ToTable("gptbl_setupmatrix_item");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FromOptimizationgroupId)
                    .HasColumnName("from_optimizationgroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ToOptimizationgroupId)
                    .HasColumnName("to_optimizationgroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SetupmatrixId)
                    .HasColumnName("setupmatrix_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Comment).HasColumnName("comment");

                entity.Property(e => e.SetupCosts).HasColumnName("setup_costs");

                entity.Property(e => e.SetupTime).HasColumnName("setup_time");
            });

            modelBuilder.Entity<GptblSetupmatrixItemUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.FromOptimizationgroupId, e.ToOptimizationgroupId, e.SetupmatrixId })
                    .HasName("PK_setupmatrix_item_udf");

                entity.ToTable("gptbl_setupmatrix_item_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.FromOptimizationgroupId)
                    .HasColumnName("from_optimizationgroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ToOptimizationgroupId)
                    .HasColumnName("to_optimizationgroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SetupmatrixId)
                    .HasColumnName("setupmatrix_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblSetupmatrixUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.SetupmatrixId })
                    .HasName("PK_setupmatrix_udf");

                entity.ToTable("gptbl_setupmatrix_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SetupmatrixId)
                    .HasColumnName("setupmatrix_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblShift>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ShiftId })
                    .HasName("PK_shift");

                entity.ToTable("gptbl_shift");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ShiftId)
                    .HasColumnName("shift_id")
                    .HasMaxLength(50);

                entity.Property(e => e.BgBlue).HasColumnName("bg_blue");

                entity.Property(e => e.BgGreen).HasColumnName("bg_green");

                entity.Property(e => e.BgRed).HasColumnName("bg_red");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<GptblShiftInterval>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ShiftId, e.DateFrom, e.WeekdayType });

                entity.ToTable("gptbl_shift_interval");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ShiftId)
                    .HasColumnName("shift_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.WeekdayType).HasColumnName("weekday_type");

                entity.Property(e => e.DateTo)
                    .HasColumnName("date_to")
                    .HasColumnType("datetime");

                entity.Property(e => e.IntervalType).HasColumnName("interval_type");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<GptblShiftIntervalUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ShiftId, e.DateFrom, e.WeekdayType })
                    .HasName("PK_shift_interval_udf");

                entity.ToTable("gptbl_shift_interval_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ShiftId)
                    .HasColumnName("shift_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.WeekdayType).HasColumnName("weekday_type");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblShiftUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ShiftId })
                    .HasName("PK_shift_udf");

                entity.ToTable("gptbl_shift_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ShiftId)
                    .HasColumnName("shift_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblShiftmodel>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ShiftmodelId })
                    .HasName("PK_shiftmodel");

                entity.ToTable("gptbl_shiftmodel");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ShiftmodelId)
                    .HasColumnName("shiftmodel_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<GptblShiftmodelShift>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.Index, e.ShiftId, e.ShiftmodelId })
                    .HasName("PK_shiftmodel_shift");

                entity.ToTable("gptbl_shiftmodel_shift");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Index).HasColumnName("index");

                entity.Property(e => e.ShiftId)
                    .HasColumnName("shift_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ShiftmodelId)
                    .HasColumnName("shiftmodel_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblShiftmodelShiftUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.Index, e.ShiftId, e.ShiftmodelId })
                    .HasName("PK_shiftmodel_shift_udf");

                entity.ToTable("gptbl_shiftmodel_shift_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Index).HasColumnName("index");

                entity.Property(e => e.ShiftId)
                    .HasColumnName("shift_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ShiftmodelId)
                    .HasColumnName("shiftmodel_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblShiftmodelUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ShiftmodelId })
                    .HasName("PK_shiftmodel_udf");

                entity.ToTable("gptbl_shiftmodel_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ShiftmodelId)
                    .HasColumnName("shiftmodel_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblStockquantityposting>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.StockquantitypostingId })
                    .HasName("PK_stockquantityposting");

                entity.ToTable("gptbl_stockquantityposting");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.StockquantitypostingId)
                    .HasColumnName("stockquantityposting_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.InfoObjectId)
                    .HasColumnName("info_object_id")
                    .HasMaxLength(50);

                entity.Property(e => e.InfoObjecttypeId).HasColumnName("info_objecttype_id");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.PostingType).HasColumnName("posting_type");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.QuantityUnitId)
                    .HasColumnName("quantity_unit_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblStockquantitypostingUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.StockquantitypostingId })
                    .HasName("PK_stockquantityposting_udf");

                entity.ToTable("gptbl_stockquantityposting_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.StockquantitypostingId)
                    .HasColumnName("stockquantityposting_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblSystemconfiguration>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.SystemconfigurationId })
                    .HasName("PK_systemconfiguration");

                entity.ToTable("gptbl_systemconfiguration");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.SystemconfigurationId)
                    .HasColumnName("systemconfiguration_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblSystemlock>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ObjectId, e.ObjecttypeId });

                entity.ToTable("gptbl_systemlock");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ObjectId)
                    .HasColumnName("object_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ObjecttypeId)
                    .HasColumnName("objecttype_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Host).HasColumnName("host");

                entity.Property(e => e.ProcessId).HasColumnName("process_id");

                entity.Property(e => e.ProductioncontrollerIds).HasColumnName("productioncontroller_ids");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblUnit>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.UnitId })
                    .HasName("PK_unit");

                entity.ToTable("gptbl_unit");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UnitId)
                    .HasColumnName("unit_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<GptblUnitUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.UnitId })
                    .HasName("PK_unit_udf");

                entity.ToTable("gptbl_unit_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UnitId)
                    .HasColumnName("unit_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblUnitUnitconversion>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.UnitId, e.ConversionUnitId })
                    .HasName("PK_unit_unitconversion");

                entity.ToTable("gptbl_unit_unitconversion");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UnitId)
                    .HasColumnName("unit_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ConversionUnitId)
                    .HasColumnName("conversion_unit_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ConversionFactor).HasColumnName("conversion_factor");
            });

            modelBuilder.Entity<GptblUnitUnitconversionUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.UnitId, e.ConversionUnitId })
                    .HasName("PK_unit_unitconversion_udf");

                entity.ToTable("gptbl_unit_unitconversion_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UnitId)
                    .HasColumnName("unit_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ConversionUnitId)
                    .HasColumnName("conversion_unit_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblUser>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.UserId });

                entity.ToTable("gptbl_user");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Forename).HasColumnName("forename");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.Password).HasColumnName("password");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.Surname).HasColumnName("surname");
            });

            modelBuilder.Entity<GptblUserPermission>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.ObjecttypeId, e.Permissiontype, e.UserId });

                entity.ToTable("gptbl_user_permission");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ObjecttypeId)
                    .HasColumnName("objecttype_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Permissiontype).HasColumnName("permissiontype");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblUserPermissionUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.ObjecttypeId, e.Permissiontype, e.UserId })
                    .HasName("PK_user_permission_udf");

                entity.ToTable("gptbl_user_permission_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ObjecttypeId)
                    .HasColumnName("objecttype_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Permissiontype).HasColumnName("permissiontype");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblUserProductioncontroller>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.UserId, e.ProductioncontrollerId, e.Permissiontype })
                    .HasName("PK_user_productioncontroller");

                entity.ToTable("gptbl_user_productioncontroller");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductioncontrollerId)
                    .HasColumnName("productioncontroller_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Permissiontype).HasColumnName("permissiontype");
            });

            modelBuilder.Entity<GptblUserProductioncontrollerUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.UserId, e.ProductioncontrollerId, e.Permissiontype })
                    .HasName("PK_user_productioncontroller_udf");

                entity.ToTable("gptbl_user_productioncontroller_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductioncontrollerId)
                    .HasColumnName("productioncontroller_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Permissiontype).HasColumnName("permissiontype");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblUserUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.UserId })
                    .HasName("PK_user_udf");

                entity.ToTable("gptbl_user_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblUserUsertemplate>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.UserId, e.TemplateUserId })
                    .HasName("PK_user_usertemplate");

                entity.ToTable("gptbl_user_usertemplate");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasMaxLength(50);

                entity.Property(e => e.TemplateUserId)
                    .HasColumnName("template_user_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblUserUsertemplateUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.UserId, e.TemplateUserId })
                    .HasName("PK_user_usertemplate_udf");

                entity.ToTable("gptbl_user_usertemplate_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasMaxLength(50);

                entity.Property(e => e.TemplateUserId)
                    .HasColumnName("template_user_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblUsersettings>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.UsersettingsId })
                    .HasName("PK_usersettings");

                entity.ToTable("gptbl_usersettings");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UsersettingsId)
                    .HasColumnName("usersettings_id")
                    .HasMaxLength(50);

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.TypeId).HasColumnName("type_id");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblUsersettingsUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.UsersettingsId, e.UdfId })
                    .HasName("PK_usersettings_udf");

                entity.ToTable("gptbl_usersettings_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UsersettingsId)
                    .HasColumnName("usersettings_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkcenter>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkcenterId })
                    .HasName("PK_workcenter");

                entity.ToTable("gptbl_workcenter");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AllocationMax).HasColumnName("allocation_max");

                entity.Property(e => e.CapacityType).HasColumnName("capacity_type");

                entity.Property(e => e.FactorSpeed).HasColumnName("factor_speed");

                entity.Property(e => e.GlobalCalendarId)
                    .HasColumnName("global_calendar_id")
                    .HasMaxLength(50);

                entity.Property(e => e.IdleTimePeriod).HasColumnName("idle_time_period");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.InterruptionTimeMax).HasColumnName("interruption_time_max");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.LotSizeMax).HasColumnName("lot_size_max");

                entity.Property(e => e.LotSizeMin).HasColumnName("lot_size_min");

                entity.Property(e => e.LotSizeUnitId)
                    .HasColumnName("lot_size_unit_id")
                    .HasMaxLength(50);

                entity.Property(e => e.MandatoryTimeInterval).HasColumnName("mandatory_time_interval");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ParallelAllocationCriteria).HasColumnName("parallel_allocation_criteria");

                entity.Property(e => e.ParallelSchedulingType).HasColumnName("parallel_scheduling_type");

                entity.Property(e => e.ScheduleWorker).HasColumnName("schedule_worker");

                entity.Property(e => e.SetupMandatoryOptimizationCriteria).HasColumnName("setup_mandatory_optimization_criteria");

                entity.Property(e => e.SetupSchedulingType).HasColumnName("setup_scheduling_type");

                entity.Property(e => e.SetupStaticTimeNeedlessCriteria).HasColumnName("setup_static_time_needless_criteria");

                entity.Property(e => e.SetupmatrixDefaultCosts).HasColumnName("setupmatrix_default_costs");

                entity.Property(e => e.SetupmatrixDefaultTime).HasColumnName("setupmatrix_default_time");

                entity.Property(e => e.SetupmatrixId)
                    .HasColumnName("setupmatrix_id")
                    .HasMaxLength(50);

                entity.Property(e => e.StablePeriod).HasColumnName("stable_period");

                entity.Property(e => e.ValidUntil)
                    .HasColumnName("valid_until")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<GptblWorkcenterCalendarinterval>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkcenterId, e.DateFrom })
                    .HasName("PK_workcenter_calendarinterval");

                entity.ToTable("gptbl_workcenter_calendarinterval");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.DateTo)
                    .HasColumnName("date_to")
                    .HasColumnType("datetime");

                entity.Property(e => e.IntervalType)
                    .HasColumnName("interval_type")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.RepetitionType)
                    .HasColumnName("repetition_type")
                    .HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<GptblWorkcenterCalendarintervalUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkcenterId, e.DateFrom, e.UdfId })
                    .HasName("PK_workcenter_calendarinterval_udf");

                entity.ToTable("gptbl_workcenter_calendarinterval_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkcenterCost>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkcenterId, e.ValidFrom })
                    .HasName("PK_workcenter_cost");

                entity.ToTable("gptbl_workcenter_cost");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ValidFrom)
                    .HasColumnName("valid_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.CostRateIdleTime).HasColumnName("cost_rate_idle_time");

                entity.Property(e => e.CostRateProcessing).HasColumnName("cost_rate_processing");

                entity.Property(e => e.CostRateSetup).HasColumnName("cost_rate_setup");
            });

            modelBuilder.Entity<GptblWorkcenterCostUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.WorkcenterId, e.ValidFrom })
                    .HasName("PK_workcenter_cost_udf");

                entity.ToTable("gptbl_workcenter_cost_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ValidFrom)
                    .HasColumnName("valid_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkcenterParallelworkcenter>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkcenterId, e.ParallelWorkcenterId })
                    .HasName("PK_workcenter_parallelworkcenter");

                entity.ToTable("gptbl_workcenter_parallelworkcenter");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ParallelWorkcenterId)
                    .HasColumnName("parallel_workcenter_id")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GptblWorkcenterParallelworkcenterUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.WorkcenterId, e.ParallelWorkcenterId })
                    .HasName("PK_workcenter_parallelworkcenter_udf");

                entity.ToTable("gptbl_workcenter_parallelworkcenter_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ParallelWorkcenterId)
                    .HasColumnName("parallel_workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkcenterShiftmodel>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkcenterId, e.StartDate })
                    .HasName("PK_workcenter_shiftmodel");

                entity.ToTable("gptbl_workcenter_shiftmodel");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.StartDate)
                    .HasColumnName("start_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.ShiftmodelId)
                    .HasColumnName("shiftmodel_id")
                    .HasMaxLength(50);

                entity.Property(e => e.StartShiftIndex)
                    .HasColumnName("start_shift_index")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.ValidUntil)
                    .HasColumnName("valid_until")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'30001231 23:59:59')");
            });

            modelBuilder.Entity<GptblWorkcenterShiftmodelUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkcenterId, e.UdfId })
                    .HasName("PK_workcenter_shiftmodel_udf");

                entity.ToTable("gptbl_workcenter_shiftmodel_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkcenterUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.WorkcenterId })
                    .HasName("PK_workcenter_udf");

                entity.ToTable("gptbl_workcenter_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkcenterWorker>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkcenterId, e.GroupId })
                    .HasName("PK_workcenter_worker");

                entity.ToTable("gptbl_workcenter_worker");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.GroupId)
                    .HasColumnName("group_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityType).HasColumnName("activity_type");

                entity.Property(e => e.ActivityqualificationId)
                    .HasColumnName("activityqualification_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ChangeWorkerType).HasColumnName("change_worker_type");

                entity.Property(e => e.WorkerRequirementCount).HasColumnName("worker_requirement_count");

                entity.Property(e => e.WorkerRequirementCountMax).HasColumnName("worker_requirement_count_max");

                entity.Property(e => e.WorkerRequirementUtilization).HasColumnName("worker_requirement_utilization");
            });

            modelBuilder.Entity<GptblWorkcenterWorkerUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.WorkcenterId, e.GroupId })
                    .HasName("PK_workcenter_worker_udf");

                entity.ToTable("gptbl_workcenter_worker_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.GroupId)
                    .HasColumnName("group_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkcentergroup>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkcentergroupId })
                    .HasName("PK_workcentergroup");

                entity.ToTable("gptbl_workcentergroup");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcentergroupId)
                    .HasColumnName("workcentergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AllocationMax).HasColumnName("allocation_max");

                entity.Property(e => e.CapacityType).HasColumnName("capacity_type");

                entity.Property(e => e.GlobalCalendarId)
                    .HasColumnName("global_calendar_id")
                    .HasMaxLength(50);

                entity.Property(e => e.IdleTimePeriod).HasColumnName("idle_time_period");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.LineType).HasColumnName("line_type");

                entity.Property(e => e.MandatoryTimeInterval).HasColumnName("mandatory_time_interval");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ParallelAllocationCriteria).HasColumnName("parallel_allocation_criteria");

                entity.Property(e => e.ParallelSchedulingType).HasColumnName("parallel_scheduling_type");

                entity.Property(e => e.ValidUntil)
                    .HasColumnName("valid_until")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<GptblWorkcentergroupCalendarinterval>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkcentergroupId, e.DateFrom })
                    .HasName("PK_workcentergroup_calendarinterval");

                entity.ToTable("gptbl_workcentergroup_calendarinterval");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcentergroupId)
                    .HasColumnName("workcentergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.DateTo)
                    .HasColumnName("date_to")
                    .HasColumnType("datetime");

                entity.Property(e => e.IntervalType)
                    .HasColumnName("interval_type")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.RepetitionType)
                    .HasColumnName("repetition_type")
                    .HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<GptblWorkcentergroupCalendarintervalUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkcentergroupId, e.DateFrom, e.UdfId })
                    .HasName("PK_workcentergroup_calendarinterval_udf");

                entity.ToTable("gptbl_workcentergroup_calendarinterval_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcentergroupId)
                    .HasColumnName("workcentergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkcentergroupCost>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkcentergroupId, e.ValidFrom })
                    .HasName("PK_workcentergroup_cost");

                entity.ToTable("gptbl_workcentergroup_cost");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcentergroupId)
                    .HasColumnName("workcentergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ValidFrom)
                    .HasColumnName("valid_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.CostRateIdleTime).HasColumnName("cost_rate_idle_time");

                entity.Property(e => e.CostRateProcessing).HasColumnName("cost_rate_processing");

                entity.Property(e => e.CostRateSetup).HasColumnName("cost_rate_setup");
            });

            modelBuilder.Entity<GptblWorkcentergroupCostUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.WorkcentergroupId, e.ValidFrom })
                    .HasName("PK_workcentergroup_cost_udf");

                entity.ToTable("gptbl_workcentergroup_cost_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcentergroupId)
                    .HasColumnName("workcentergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ValidFrom)
                    .HasColumnName("valid_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkcentergroupShiftmodel>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkcentergroupId, e.StartDate })
                    .HasName("PK_workcentergroup_shiftmodel");

                entity.ToTable("gptbl_workcentergroup_shiftmodel");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcentergroupId)
                    .HasColumnName("workcentergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.StartDate)
                    .HasColumnName("start_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.ShiftmodelId)
                    .HasColumnName("shiftmodel_id")
                    .HasMaxLength(50);

                entity.Property(e => e.StartShiftIndex)
                    .HasColumnName("start_shift_index")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.ValidUntil)
                    .HasColumnName("valid_until")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'30001231 23:59:59')");
            });

            modelBuilder.Entity<GptblWorkcentergroupShiftmodelUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkcentergroupId, e.UdfId })
                    .HasName("PK_workcentergroup_shiftmodel_udf");

                entity.ToTable("gptbl_workcentergroup_shiftmodel_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcentergroupId)
                    .HasColumnName("workcentergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkcentergroupUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.WorkcentergroupId })
                    .HasName("PK_workcentergroup_udf");

                entity.ToTable("gptbl_workcentergroup_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcentergroupId)
                    .HasColumnName("workcentergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkcentergroupWorkcenter>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkcenterId, e.WorkcentergroupId })
                    .HasName("PK_workcentergroup_workcenter");

                entity.ToTable("gptbl_workcentergroup_workcenter");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcentergroupId)
                    .HasColumnName("workcentergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.GroupType).HasColumnName("group_type");
            });

            modelBuilder.Entity<GptblWorkcentergroupWorkcenterUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.WorkcenterId, e.WorkcentergroupId })
                    .HasName("PK_workcentergroup_workcenter_udf");

                entity.ToTable("gptbl_workcentergroup_workcenter_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcentergroupId)
                    .HasColumnName("workcentergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorker>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkerId })
                    .HasName("PK_worker");

                entity.ToTable("gptbl_worker");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkerId)
                    .HasColumnName("worker_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AllocationMax).HasColumnName("allocation_max");

                entity.Property(e => e.CapacityType).HasColumnName("capacity_type");

                entity.Property(e => e.CostRate).HasColumnName("cost_rate");

                entity.Property(e => e.GlobalCalendarId)
                    .HasColumnName("global_calendar_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ProcessingTimePenalty).HasColumnName("processing_time_penalty");

                entity.Property(e => e.SetupTimePenalty).HasColumnName("setup_time_penalty");

                entity.Property(e => e.ValidUntil)
                    .HasColumnName("valid_until")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<GptblWorkerActivityqualification>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkerId, e.ActivityqualificationId, e.ValidFrom })
                    .HasName("PK_worker_activityqualification");

                entity.ToTable("gptbl_worker_activityqualification");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkerId)
                    .HasColumnName("worker_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityqualificationId)
                    .HasColumnName("activityqualification_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ValidFrom)
                    .HasColumnName("valid_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");

                entity.Property(e => e.ValidUntil)
                    .HasColumnName("valid_until")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<GptblWorkerActivityqualificationUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkerId, e.ActivityqualificationId, e.ValidFrom, e.UdfId })
                    .HasName("PK_worker_activityqualification_udf");

                entity.ToTable("gptbl_worker_activityqualification_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkerId)
                    .HasColumnName("worker_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityqualificationId)
                    .HasColumnName("activityqualification_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ValidFrom)
                    .HasColumnName("valid_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkerCalendarinterval>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkerId, e.DateFrom })
                    .HasName("PK_worker_calendarinterval");

                entity.ToTable("gptbl_worker_calendarinterval");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkerId)
                    .HasColumnName("worker_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.DateTo)
                    .HasColumnName("date_to")
                    .HasColumnType("datetime");

                entity.Property(e => e.IntervalType)
                    .HasColumnName("interval_type")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.RepetitionType)
                    .HasColumnName("repetition_type")
                    .HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<GptblWorkerCalendarintervalUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkerId, e.DateFrom, e.UdfId })
                    .HasName("PK_worker_calendarinterval_udf");

                entity.ToTable("gptbl_worker_calendarinterval_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkerId)
                    .HasColumnName("worker_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkerShiftmodel>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkerId, e.StartDate })
                    .HasName("PK_worker_shiftmodel");

                entity.ToTable("gptbl_worker_shiftmodel");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkerId)
                    .HasColumnName("worker_id")
                    .HasMaxLength(50);

                entity.Property(e => e.StartDate)
                    .HasColumnName("start_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.ShiftmodelId)
                    .HasColumnName("shiftmodel_id")
                    .HasMaxLength(50);

                entity.Property(e => e.StartShiftIndex)
                    .HasColumnName("start_shift_index")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.ValidUntil)
                    .HasColumnName("valid_until")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'30001231 23:59:59')");
            });

            modelBuilder.Entity<GptblWorkerShiftmodelUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkerId, e.UdfId })
                    .HasName("PK_worker_shiftmodel_udf");

                entity.ToTable("gptbl_worker_shiftmodel_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkerId)
                    .HasColumnName("worker_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkerUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.WorkerId })
                    .HasName("PK_worker_udf");

                entity.ToTable("gptbl_worker_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkerId)
                    .HasColumnName("worker_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkerWorkcenterqualification>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkerId, e.WorkcenterId, e.ValidFrom, e.WorkcenterqualificationType })
                    .HasName("PK_worker_workcenterqualification");

                entity.ToTable("gptbl_worker_workcenterqualification");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkerId)
                    .HasColumnName("worker_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ValidFrom)
                    .HasColumnName("valid_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.WorkcenterqualificationType).HasColumnName("workcenterqualification_type");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");

                entity.Property(e => e.ValidUntil)
                    .HasColumnName("valid_until")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<GptblWorkerWorkcenterqualificationUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.WorkerId, e.WorkcenterId, e.ValidFrom, e.WorkcenterqualificationType })
                    .HasName("PK_worker_workcenterqualification_udf");

                entity.ToTable("gptbl_worker_workcenterqualification_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkerId)
                    .HasColumnName("worker_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ValidFrom)
                    .HasColumnName("valid_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.WorkcenterqualificationType).HasColumnName("workcenterqualification_type");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkergroup>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkergroupId })
                    .HasName("PK_workergroup");

                entity.ToTable("gptbl_workergroup");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkergroupId)
                    .HasColumnName("workergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.AllocationMax).HasColumnName("allocation_max");

                entity.Property(e => e.CapacityType).HasColumnName("capacity_type");

                entity.Property(e => e.CostRate).HasColumnName("cost_rate");

                entity.Property(e => e.GlobalCalendarId)
                    .HasColumnName("global_calendar_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'19700102 00:00:00')");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.PoolType).HasColumnName("pool_type");

                entity.Property(e => e.ProcessingTimePenalty).HasColumnName("processing_time_penalty");

                entity.Property(e => e.SetupTimePenalty).HasColumnName("setup_time_penalty");

                entity.Property(e => e.ValidUntil)
                    .HasColumnName("valid_until")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<GptblWorkergroupActivityqualification>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkergroupId, e.ActivityqualificationId, e.ValidFrom })
                    .HasName("PK_workergroup_activityqualification");

                entity.ToTable("gptbl_workergroup_activityqualification");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkergroupId)
                    .HasColumnName("workergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityqualificationId)
                    .HasColumnName("activityqualification_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ValidFrom)
                    .HasColumnName("valid_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");

                entity.Property(e => e.ValidUntil)
                    .HasColumnName("valid_until")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<GptblWorkergroupActivityqualificationUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkergroupId, e.ActivityqualificationId, e.ValidFrom, e.UdfId })
                    .HasName("PK_workergroup_activityqualification_udf");

                entity.ToTable("gptbl_workergroup_activityqualification_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkergroupId)
                    .HasColumnName("workergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ActivityqualificationId)
                    .HasColumnName("activityqualification_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ValidFrom)
                    .HasColumnName("valid_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkergroupCalendarinterval>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkergroupId, e.DateFrom })
                    .HasName("PK_workergroup_calendarinterval");

                entity.ToTable("gptbl_workergroup_calendarinterval");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkergroupId)
                    .HasColumnName("workergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.DateTo)
                    .HasColumnName("date_to")
                    .HasColumnType("datetime");

                entity.Property(e => e.IntervalType)
                    .HasColumnName("interval_type")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.RepetitionType)
                    .HasColumnName("repetition_type")
                    .HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<GptblWorkergroupCalendarintervalUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkergroupId, e.DateFrom, e.UdfId })
                    .HasName("PK_workergroup_calendarinterval_udf");

                entity.ToTable("gptbl_workergroup_calendarinterval_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkergroupId)
                    .HasColumnName("workergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkergroupPoolcapacity>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkergroupId, e.DateFrom })
                    .HasName("PK_workergroup_poolcapacity");

                entity.ToTable("gptbl_workergroup_poolcapacity");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkergroupId)
                    .HasColumnName("workergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.Quantity).HasColumnName("quantity");
            });

            modelBuilder.Entity<GptblWorkergroupPoolcapacityUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.WorkergroupId, e.DateFrom })
                    .HasName("PK_workergroup_poolcapacity_udf");

                entity.ToTable("gptbl_workergroup_poolcapacity_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkergroupId)
                    .HasColumnName("workergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.DateFrom)
                    .HasColumnName("date_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkergroupShiftmodel>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkergroupId, e.StartDate })
                    .HasName("PK_workergroup_shiftmodel");

                entity.ToTable("gptbl_workergroup_shiftmodel");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkergroupId)
                    .HasColumnName("workergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.StartDate)
                    .HasColumnName("start_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.ShiftmodelId)
                    .HasColumnName("shiftmodel_id")
                    .HasMaxLength(50);

                entity.Property(e => e.StartShiftIndex)
                    .HasColumnName("start_shift_index")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.ValidUntil)
                    .HasColumnName("valid_until")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(N'30001231 23:59:59')");
            });

            modelBuilder.Entity<GptblWorkergroupShiftmodelUdf>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkergroupId, e.UdfId })
                    .HasName("PK_workergroup_shiftmodel_udf");

                entity.ToTable("gptbl_workergroup_shiftmodel_udf");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkergroupId)
                    .HasColumnName("workergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkergroupUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.WorkergroupId })
                    .HasName("PK_workergroup_udf");

                entity.ToTable("gptbl_workergroup_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkergroupId)
                    .HasColumnName("workergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkergroupWorkcenterqualification>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkergroupId, e.WorkcenterId, e.ValidFrom, e.WorkcenterqualificationType })
                    .HasName("PK_workergroup_workcenterqualification");

                entity.ToTable("gptbl_workergroup_workcenterqualification");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkergroupId)
                    .HasColumnName("workergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ValidFrom)
                    .HasColumnName("valid_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.WorkcenterqualificationType).HasColumnName("workcenterqualification_type");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");

                entity.Property(e => e.ValidUntil)
                    .HasColumnName("valid_until")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<GptblWorkergroupWorkcenterqualificationUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.WorkergroupId, e.WorkcenterId, e.ValidFrom, e.WorkcenterqualificationType })
                    .HasName("PK_workergroup_workcenterqualification_udf");

                entity.ToTable("gptbl_workergroup_workcenterqualification_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkergroupId)
                    .HasColumnName("workergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ValidFrom)
                    .HasColumnName("valid_from")
                    .HasColumnType("datetime");

                entity.Property(e => e.WorkcenterqualificationType).HasColumnName("workcenterqualification_type");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<GptblWorkergroupWorker>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.WorkerId, e.WorkergroupId })
                    .HasName("PK_workergroup_worker");

                entity.ToTable("gptbl_workergroup_worker");

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkerId)
                    .HasColumnName("worker_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkergroupId)
                    .HasColumnName("workergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.GroupType).HasColumnName("group_type");
            });

            modelBuilder.Entity<GptblWorkergroupWorkerUdf>(entity =>
            {
                entity.HasKey(e => new { e.UdfId, e.ClientId, e.WorkerId, e.WorkergroupId })
                    .HasName("PK_workergroup_worker_udf");

                entity.ToTable("gptbl_workergroup_worker_udf");

                entity.Property(e => e.UdfId)
                    .HasColumnName("udf_id")
                    .HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .HasColumnName("client_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkerId)
                    .HasColumnName("worker_id")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkergroupId)
                    .HasColumnName("workergroup_id")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<Material>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("material");

                entity.Property(e => e.CheckStockQuantity).HasColumnName("check_stock_quantity");

                entity.Property(e => e.ClientId).HasColumnName("client_id");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.InhouseProduction).HasColumnName("inhouse_production");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.Property(e => e.LotSizeMax).HasColumnName("lot_size_max");

                entity.Property(e => e.LotSizeMin).HasColumnName("lot_size_min");

                entity.Property(e => e.LotSizeOpt).HasColumnName("lot_size_opt");

                entity.Property(e => e.MaterialId).HasColumnName("material_id");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ProductioncontrollerId).HasColumnName("productioncontroller_id");

                entity.Property(e => e.PurchaseTimeQuantityDependent).HasColumnName("purchase_time_quantity_dependent");

                entity.Property(e => e.PurchaseTimeQuantityIndependent).HasColumnName("purchase_time_quantity_independent");

                entity.Property(e => e.QuantityRoundingValue).HasColumnName("quantity_rounding_value");

                entity.Property(e => e.QuantityUnitId).HasColumnName("quantity_unit_id");

                entity.Property(e => e.ReduceStockQuantity).HasColumnName("reduce_stock_quantity");

                entity.Property(e => e.ResultId).HasColumnName("result_id");

                entity.Property(e => e.SafetyStockUsage).HasColumnName("safety_stock_usage");

                entity.Property(e => e.SafetyStockValue).HasColumnName("safety_stock_value");

                entity.Property(e => e.SessionId).HasColumnName("session_id");

                entity.Property(e => e.ValueProduction).HasColumnName("value_production");

                entity.Property(e => e.ValuePurchase).HasColumnName("value_purchase");

                entity.Property(e => e.ValueSales).HasColumnName("value_sales");

                entity.Property(e => e.WaitingTimeMax).HasColumnName("waiting_time_max");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
