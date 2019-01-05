using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Master40.DB.GanttplanDB.Models;

namespace Master40.DB.Data.Context
{
    public partial class GPSzenarioContext : DbContext
    {
        public GPSzenarioContext()
        {
        }

        public GPSzenarioContext(DbContextOptions<GPSzenarioContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Bom> Bom { get; set; }
        public virtual DbSet<BomItem> BomItem { get; set; }
        public virtual DbSet<Calendar> Calendar { get; set; }
        public virtual DbSet<CalendarInterval> CalendarInterval { get; set; }
        public virtual DbSet<Chart> Chart { get; set; }
        public virtual DbSet<ChartColorfilter> ChartColorfilter { get; set; }
        public virtual DbSet<ChartColorfilterItem> ChartColorfilterItem { get; set; }
        public virtual DbSet<ChartRowfilter> ChartRowfilter { get; set; }
        public virtual DbSet<ChartRowfilterItem> ChartRowfilterItem { get; set; }
        public virtual DbSet<ChartSeries> ChartSeries { get; set; }
        public virtual DbSet<ChartText> ChartText { get; set; }
        public virtual DbSet<ChartTextItem> ChartTextItem { get; set; }
        public virtual DbSet<Config> Config { get; set; }
        public virtual DbSet<Confirmation> Confirmation { get; set; }
        public virtual DbSet<ConfirmationResource> ConfirmationResource { get; set; }
        public virtual DbSet<ConfirmationResourceInterval> ConfirmationResourceInterval { get; set; }
        public virtual DbSet<Ganttcolorscheme> Ganttcolorscheme { get; set; }
        public virtual DbSet<GanttcolorschemeFilter> GanttcolorschemeFilter { get; set; }
        public virtual DbSet<GanttcolorschemeFilterItem> GanttcolorschemeFilterItem { get; set; }
        public virtual DbSet<Idtemplate> Idtemplate { get; set; }
        public virtual DbSet<Journal> Journal { get; set; }
        public virtual DbSet<Material> Material { get; set; }
        public virtual DbSet<MaterialOptimizationgroup> MaterialOptimizationgroup { get; set; }
        public virtual DbSet<MaterialRouting> MaterialRouting { get; set; }
        public virtual DbSet<MaterialUdf> MaterialUdf { get; set; }
        public virtual DbSet<MaterialUnitconversion> MaterialUnitconversion { get; set; }
        public virtual DbSet<Modelparameter> Modelparameter { get; set; }
        public virtual DbSet<Objecttype> Objecttype { get; set; }
        public virtual DbSet<ObjecttypeUdf> ObjecttypeUdf { get; set; }
        public virtual DbSet<Optimizationgroup> Optimizationgroup { get; set; }
        public virtual DbSet<Planningparameter> Planningparameter { get; set; }
        public virtual DbSet<Plant> Plant { get; set; }
        public virtual DbSet<Priority> Priority { get; set; }
        public virtual DbSet<PriorityLatenesscost> PriorityLatenesscost { get; set; }
        public virtual DbSet<Productioncontroller> Productioncontroller { get; set; }
        public virtual DbSet<Productionorder> Productionorder { get; set; }
        public virtual DbSet<ProductionorderOperationActivity> ProductionorderOperationActivity { get; set; }
        public virtual DbSet<ProductionorderOperationActivityFixedresource> ProductionorderOperationActivityFixedresource { get; set; }
        public virtual DbSet<ProductionorderOperationActivityMaterialrelation> ProductionorderOperationActivityMaterialrelation { get; set; }
        public virtual DbSet<ProductionorderOperationActivityResource> ProductionorderOperationActivityResource { get; set; }
        public virtual DbSet<ProductionorderOperationActivityResourceInterval> ProductionorderOperationActivityResourceInterval { get; set; }
        public virtual DbSet<Prt> Prt { get; set; }
        public virtual DbSet<PrtUdf> PrtUdf { get; set; }
        public virtual DbSet<PrtWorkcenter> PrtWorkcenter { get; set; }
        public virtual DbSet<Purchaseorder> Purchaseorder { get; set; }
        public virtual DbSet<Report> Report { get; set; }
        public virtual DbSet<ReportColorfilter> ReportColorfilter { get; set; }
        public virtual DbSet<ReportColorfilterItem> ReportColorfilterItem { get; set; }
        public virtual DbSet<ReportColumn> ReportColumn { get; set; }
        public virtual DbSet<ReportOption> ReportOption { get; set; }
        public virtual DbSet<ReportOptionItem> ReportOptionItem { get; set; }
        public virtual DbSet<ReportRowfilter> ReportRowfilter { get; set; }
        public virtual DbSet<ReportRowfilterItem> ReportRowfilterItem { get; set; }
        public virtual DbSet<Resourcestatus> Resourcestatus { get; set; }
        public virtual DbSet<Resultinfo> Resultinfo { get; set; }
        public virtual DbSet<Routing> Routing { get; set; }
        public virtual DbSet<RoutingOperation> RoutingOperation { get; set; }
        public virtual DbSet<RoutingOperationActivity> RoutingOperationActivity { get; set; }
        public virtual DbSet<RoutingOperationActivityBomItem> RoutingOperationActivityBomItem { get; set; }
        public virtual DbSet<RoutingOperationActivityPrt> RoutingOperationActivityPrt { get; set; }
        public virtual DbSet<RoutingOperationActivityResourcereservation> RoutingOperationActivityResourcereservation { get; set; }
        public virtual DbSet<RoutingOperationActivityWorkcenterfactor> RoutingOperationActivityWorkcenterfactor { get; set; }
        public virtual DbSet<RoutingOperationOperationrelation> RoutingOperationOperationrelation { get; set; }
        public virtual DbSet<RoutingOperationOptimizationgroup> RoutingOperationOptimizationgroup { get; set; }
        public virtual DbSet<RoutingOperationWorkcenter> RoutingOperationWorkcenter { get; set; }
        public virtual DbSet<RoutingOperationWorker> RoutingOperationWorker { get; set; }
        public virtual DbSet<RoutingScrap> RoutingScrap { get; set; }
        public virtual DbSet<RoutingScrapItem> RoutingScrapItem { get; set; }
        public virtual DbSet<Salesorder> Salesorder { get; set; }
        public virtual DbSet<SalesorderMaterialrelation> SalesorderMaterialrelation { get; set; }
        public virtual DbSet<Setupmatrix> Setupmatrix { get; set; }
        public virtual DbSet<SetupmatrixItem> SetupmatrixItem { get; set; }
        public virtual DbSet<SetupmatrixItemUdf> SetupmatrixItemUdf { get; set; }
        public virtual DbSet<Shift> Shift { get; set; }
        public virtual DbSet<ShiftInterval> ShiftInterval { get; set; }
        public virtual DbSet<Shiftmodel> Shiftmodel { get; set; }
        public virtual DbSet<ShiftmodelShift> ShiftmodelShift { get; set; }
        public virtual DbSet<Stock> Stock { get; set; }
        public virtual DbSet<Stockquantityposting> Stockquantityposting { get; set; }
        public virtual DbSet<Unit> Unit { get; set; }
        public virtual DbSet<UnitUnitconversion> UnitUnitconversion { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserPermission> UserPermission { get; set; }
        public virtual DbSet<UserProductioncontroller> UserProductioncontroller { get; set; }
        public virtual DbSet<UserUsertemplate> UserUsertemplate { get; set; }
        public virtual DbSet<Userview> Userview { get; set; }
        public virtual DbSet<UserviewItem> UserviewItem { get; set; }
        public virtual DbSet<Workcenter> Workcenter { get; set; }
        public virtual DbSet<WorkcenterCost> WorkcenterCost { get; set; }
        public virtual DbSet<WorkcenterParallelworkcenter> WorkcenterParallelworkcenter { get; set; }
        public virtual DbSet<WorkcenterUdf> WorkcenterUdf { get; set; }
        public virtual DbSet<WorkcenterWorker> WorkcenterWorker { get; set; }
        public virtual DbSet<Workcentergroup> Workcentergroup { get; set; }
        public virtual DbSet<WorkcentergroupCost> WorkcentergroupCost { get; set; }
        public virtual DbSet<WorkcentergroupWorkcenter> WorkcentergroupWorkcenter { get; set; }
        public virtual DbSet<Worker> Worker { get; set; }
        public virtual DbSet<WorkerActivityqualification> WorkerActivityqualification { get; set; }
        public virtual DbSet<WorkerUdf> WorkerUdf { get; set; }
        public virtual DbSet<WorkerWorkcenterqualification> WorkerWorkcenterqualification { get; set; }
        public virtual DbSet<Workergroup> Workergroup { get; set; }
        public virtual DbSet<WorkergroupActivityqualification> WorkergroupActivityqualification { get; set; }
        public virtual DbSet<WorkergroupPoolcapacity> WorkergroupPoolcapacity { get; set; }
        public virtual DbSet<WorkergroupWorkcenterqualification> WorkergroupWorkcenterqualification { get; set; }
        public virtual DbSet<WorkergroupWorker> WorkergroupWorker { get; set; }
        public virtual DbSet<Workingtimemodel> Workingtimemodel { get; set; }
        public virtual DbSet<WorkingtimemodelShiftmodel> WorkingtimemodelShiftmodel { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlite("Filename=C:\\source\\repo\\Master-4.0\\Master40.DB\\GanttplanDB\\GPSzenario.gpsx");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity<Bom>(entity =>
            {
                entity.ToTable("bom");

                entity.Property(e => e.BomId)
                    .HasColumnName("bom_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.MasterBomId).HasColumnName("master_bom_id");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<BomItem>(entity =>
            {
                entity.HasKey(e => new { e.BomId, e.ItemId, e.AlternativeId });

                entity.ToTable("bom_item");

                entity.Property(e => e.BomId).HasColumnName("bom_id");

                entity.Property(e => e.ItemId).HasColumnName("item_id");

                entity.Property(e => e.AlternativeId).HasColumnName("alternative_id");

                entity.Property(e => e.Group).HasColumnName("group");

                entity.Property(e => e.MaterialId).HasColumnName("material_id");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.PreparationTime).HasColumnName("preparation_time");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.QuantityUnitId).HasColumnName("quantity_unit_id");

                entity.Property(e => e.Standard).HasColumnName("standard");
            });

            modelBuilder.Entity<Calendar>(entity =>
            {
                entity.ToTable("calendar");

                entity.Property(e => e.CalendarId)
                    .HasColumnName("calendar_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<CalendarInterval>(entity =>
            {
                entity.HasKey(e => new { e.CalendarId, e.DateFrom });

                entity.ToTable("calendar_interval");

                entity.Property(e => e.CalendarId).HasColumnName("calendar_id");

                entity.Property(e => e.DateFrom).HasColumnName("date_from");

                entity.Property(e => e.DateTo).HasColumnName("date_to");

                entity.Property(e => e.IntervalType).HasColumnName("interval_type");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.RepetitionType).HasColumnName("repetition_type");
            });

            modelBuilder.Entity<Chart>(entity =>
            {
                entity.ToTable("chart");

                entity.Property(e => e.ChartId)
                    .HasColumnName("chart_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.AllInOne).HasColumnName("all_in_one");

                entity.Property(e => e.ArgumentDataMemberId).HasColumnName("argument_data_member_id");

                entity.Property(e => e.Category).HasColumnName("category");

                entity.Property(e => e.ChartType).HasColumnName("chart_type");

                entity.Property(e => e.ColorPaletteName).HasColumnName("color_palette_name");

                entity.Property(e => e.CustomChartTitle).HasColumnName("custom_chart_title");

                entity.Property(e => e.CustomXaxisTitle).HasColumnName("custom_xaxis_title");

                entity.Property(e => e.CustomYaxisTitle).HasColumnName("custom_yaxis_title");

                entity.Property(e => e.HeaderAsArgument).HasColumnName("header_as_argument");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ReportId).HasColumnName("report_id");

                entity.Property(e => e.ReportType).HasColumnName("report_type");

                entity.Property(e => e.Rotated).HasColumnName("rotated");

                entity.Property(e => e.ShowLabels).HasColumnName("show_labels");

                entity.Property(e => e.ShowLegend).HasColumnName("show_legend");

                entity.Property(e => e.UserId).HasColumnName("user_id");
            });

            modelBuilder.Entity<ChartColorfilter>(entity =>
            {
                entity.HasKey(e => new { e.ChartId, e.FilterId });

                entity.ToTable("chart_colorfilter");

                entity.Property(e => e.ChartId).HasColumnName("chart_id");

                entity.Property(e => e.FilterId).HasColumnName("filter_id");

                entity.Property(e => e.Color).HasColumnName("color");

                entity.Property(e => e.FilterOperator).HasColumnName("filter_operator");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");

                entity.Property(e => e.PropertyId).HasColumnName("property_id");
            });

            modelBuilder.Entity<ChartColorfilterItem>(entity =>
            {
                entity.HasKey(e => new { e.ChartId, e.FilterId, e.PropertyId });

                entity.ToTable("chart_colorfilter_item");

                entity.Property(e => e.ChartId).HasColumnName("chart_id");

                entity.Property(e => e.FilterId).HasColumnName("filter_id");

                entity.Property(e => e.PropertyId).HasColumnName("property_id");

                entity.Property(e => e.FilterMatchingType).HasColumnName("filter_matching_type");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<ChartRowfilter>(entity =>
            {
                entity.HasKey(e => new { e.ChartId, e.FilterId });

                entity.ToTable("chart_rowfilter");

                entity.Property(e => e.ChartId).HasColumnName("chart_id");

                entity.Property(e => e.FilterId).HasColumnName("filter_id");

                entity.Property(e => e.FilterOperator).HasColumnName("filter_operator");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");
            });

            modelBuilder.Entity<ChartRowfilterItem>(entity =>
            {
                entity.HasKey(e => new { e.ChartId, e.FilterId, e.PropertyId });

                entity.ToTable("chart_rowfilter_item");

                entity.Property(e => e.ChartId).HasColumnName("chart_id");

                entity.Property(e => e.FilterId).HasColumnName("filter_id");

                entity.Property(e => e.PropertyId).HasColumnName("property_id");

                entity.Property(e => e.FilterMatchingType).HasColumnName("filter_matching_type");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<ChartSeries>(entity =>
            {
                entity.HasKey(e => new { e.SeriesId, e.ChartId });

                entity.ToTable("chart_series");

                entity.Property(e => e.SeriesId).HasColumnName("series_id");

                entity.Property(e => e.ChartId).HasColumnName("chart_id");

                entity.Property(e => e.ChartType).HasColumnName("chart_type");

                entity.Property(e => e.Color).HasColumnName("color");

                entity.Property(e => e.ReportColumnName).HasColumnName("report_column_name");

                entity.Property(e => e.ReportColumnPropertyId).HasColumnName("report_column_property_id");
            });

            modelBuilder.Entity<ChartText>(entity =>
            {
                entity.HasKey(e => new { e.ChartId, e.TextId });

                entity.ToTable("chart_text");

                entity.Property(e => e.ChartId).HasColumnName("chart_id");

                entity.Property(e => e.TextId).HasColumnName("text_id");

                entity.Property(e => e.TextType).HasColumnName("text_type");
            });

            modelBuilder.Entity<ChartTextItem>(entity =>
            {
                entity.HasKey(e => new { e.ChartId, e.TextId, e.PropertyId });

                entity.ToTable("chart_text_item");

                entity.Property(e => e.ChartId).HasColumnName("chart_id");

                entity.Property(e => e.TextId).HasColumnName("text_id");

                entity.Property(e => e.PropertyId).HasColumnName("property_id");

                entity.Property(e => e.PropertyName).HasColumnName("property_name");

                entity.Property(e => e.PropertyNameType).HasColumnName("property_name_type");
            });

            modelBuilder.Entity<Config>(entity =>
            {
                entity.HasKey(e => e.PropertyName);

                entity.Property(e => e.PropertyName).ValueGeneratedNever();
            });

            modelBuilder.Entity<Confirmation>(entity =>
            {
                entity.ToTable("confirmation");

                entity.Property(e => e.ConfirmationId)
                    .HasColumnName("confirmation_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.ActivityEnd).HasColumnName("activity_end");

                entity.Property(e => e.ActivityStart).HasColumnName("activity_start");

                entity.Property(e => e.ConfirmationDate).HasColumnName("confirmation_date");

                entity.Property(e => e.ConfirmationType).HasColumnName("confirmation_type");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ProductionorderActivityId).HasColumnName("productionorder_activity_id");

                entity.Property(e => e.ProductionorderAlternativeId).HasColumnName("productionorder_alternative_id");

                entity.Property(e => e.ProductionorderId).HasColumnName("productionorder_id");

                entity.Property(e => e.ProductionorderOperationId).HasColumnName("productionorder_operation_id");

                entity.Property(e => e.ProductionorderSplitId).HasColumnName("productionorder_split_id");

                entity.Property(e => e.QuantityFinished).HasColumnName("quantity_finished");

                entity.Property(e => e.QuantityFinishedUnitId).HasColumnName("quantity_finished_unit_id");
            });

            modelBuilder.Entity<ConfirmationResource>(entity =>
            {
                entity.HasKey(e => new { e.ConfirmationId, e.ResourceId, e.ResourceType });

                entity.ToTable("confirmation_resource");

                entity.Property(e => e.ConfirmationId).HasColumnName("confirmation_id");

                entity.Property(e => e.ResourceId).HasColumnName("resource_id");

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.Allocation).HasColumnName("allocation");

                entity.Property(e => e.GroupId).HasColumnName("group_id");
            });

            modelBuilder.Entity<ConfirmationResourceInterval>(entity =>
            {
                entity.HasKey(e => new { e.ConfirmationId, e.ResourceId, e.ResourceType, e.DateFrom });

                entity.ToTable("confirmation_resource_interval");

                entity.Property(e => e.ConfirmationId).HasColumnName("confirmation_id");

                entity.Property(e => e.ResourceId).HasColumnName("resource_id");

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.DateFrom).HasColumnName("date_from");

                entity.Property(e => e.DateTo).HasColumnName("date_to");

                entity.Property(e => e.IntervalAllocationType).HasColumnName("interval_allocation_type");
            });

            modelBuilder.Entity<Ganttcolorscheme>(entity =>
            {
                entity.ToTable("ganttcolorscheme");

                entity.Property(e => e.GanttcolorschemeId)
                    .HasColumnName("ganttcolorscheme_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Category).HasColumnName("category");

                entity.Property(e => e.DefaultColor).HasColumnName("default_color");

                entity.Property(e => e.DefaultGanttcolorschemeId).HasColumnName("default_ganttcolorscheme_id");

                entity.Property(e => e.DefaultRandomcolorPropertyId).HasColumnName("default_randomcolor_property_id");

                entity.Property(e => e.DefaultType).HasColumnName("default_type");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.UserId).HasColumnName("user_id");
            });

            modelBuilder.Entity<GanttcolorschemeFilter>(entity =>
            {
                entity.HasKey(e => new { e.GanttcolorschemeId, e.FilterId });

                entity.ToTable("ganttcolorscheme_filter");

                entity.Property(e => e.GanttcolorschemeId).HasColumnName("ganttcolorscheme_id");

                entity.Property(e => e.FilterId).HasColumnName("filter_id");

                entity.Property(e => e.Color).HasColumnName("color");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");
            });

            modelBuilder.Entity<GanttcolorschemeFilterItem>(entity =>
            {
                entity.HasKey(e => new { e.GanttcolorschemeId, e.FilterId, e.PropertyId });

                entity.ToTable("ganttcolorscheme_filter_item");

                entity.Property(e => e.GanttcolorschemeId).HasColumnName("ganttcolorscheme_id");

                entity.Property(e => e.FilterId).HasColumnName("filter_id");

                entity.Property(e => e.PropertyId).HasColumnName("property_id");

                entity.Property(e => e.CaseSensitive).HasColumnName("case_sensitive");

                entity.Property(e => e.FilterMatchingType).HasColumnName("filter_matching_type");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<Idtemplate>(entity =>
            {
                entity.HasKey(e => e.ObjecttypeId);

                entity.ToTable("idtemplate");

                entity.Property(e => e.ObjecttypeId)
                    .HasColumnName("objecttype_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.ValueStart).HasColumnName("value_start");
            });

            modelBuilder.Entity<Journal>(entity =>
            {
                entity.ToTable("journal");

                entity.Property(e => e.JournalId)
                    .HasColumnName("journal_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.ActionType).HasColumnName("action_type");

                entity.Property(e => e.Date).HasColumnName("date");

                entity.Property(e => e.Host).HasColumnName("host");

                entity.Property(e => e.ObjectId).HasColumnName("object_id");

                entity.Property(e => e.ObjecttypeId).HasColumnName("objecttype_id");

                entity.Property(e => e.SessionId).HasColumnName("session_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<Material>(entity =>
            {
                entity.ToTable("material");

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.CheckStockQuantity).HasColumnName("check_stock_quantity");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.InhouseProduction).HasColumnName("inhouse_production");

                entity.Property(e => e.LotSizeMax).HasColumnName("lot_size_max");

                entity.Property(e => e.LotSizeMin).HasColumnName("lot_size_min");

                entity.Property(e => e.LotSizeOpt).HasColumnName("lot_size_opt");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.PurchaseTimeQuantityDependent).HasColumnName("purchase_time_quantity_dependent");

                entity.Property(e => e.PurchaseTimeQuantityIndependent).HasColumnName("purchase_time_quantity_independent");

                entity.Property(e => e.QuantityRoundingValue).HasColumnName("quantity_rounding_value");

                entity.Property(e => e.QuantityUnitId).HasColumnName("quantity_unit_id");

                entity.Property(e => e.ReduceStockQuantity).HasColumnName("reduce_stock_quantity");

                entity.Property(e => e.SafetyStockUsage).HasColumnName("safety_stock_usage");

                entity.Property(e => e.SafetyStockValue).HasColumnName("safety_stock_value");

                entity.Property(e => e.ValueProduction).HasColumnName("value_production");

                entity.Property(e => e.ValuePurchase).HasColumnName("value_purchase");

                entity.Property(e => e.ValueSales).HasColumnName("value_sales");

                entity.Property(e => e.WaitingTimeMax).HasColumnName("waiting_time_max");
            });

            modelBuilder.Entity<MaterialOptimizationgroup>(entity =>
            {
                entity.HasKey(e => new { e.MaterialId, e.OptimizationgroupType });

                entity.ToTable("material_optimizationgroup");

                entity.Property(e => e.MaterialId).HasColumnName("material_id");

                entity.Property(e => e.OptimizationgroupType).HasColumnName("optimizationgroup_type");

                entity.Property(e => e.OptimizationgroupId).HasColumnName("optimizationgroup_id");

                entity.Property(e => e.OptimizationgroupValue).HasColumnName("optimizationgroup_value");
            });

            modelBuilder.Entity<MaterialRouting>(entity =>
            {
                entity.HasKey(e => new { e.MaterialId, e.RoutingId, e.ValidFrom });

                entity.ToTable("material_routing");

                entity.Property(e => e.MaterialId).HasColumnName("material_id");

                entity.Property(e => e.RoutingId).HasColumnName("routing_id");

                entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
            });

            modelBuilder.Entity<MaterialUdf>(entity =>
            {
                entity.HasKey(e => new { e.MaterialId, e.UdfId });

                entity.ToTable("material_udf");

                entity.Property(e => e.MaterialId).HasColumnName("material_id");

                entity.Property(e => e.UdfId).HasColumnName("udf_id");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<MaterialUnitconversion>(entity =>
            {
                entity.HasKey(e => new { e.MaterialId, e.UnitId, e.ConversionUnitId });

                entity.ToTable("material_unitconversion");

                entity.Property(e => e.MaterialId).HasColumnName("material_id");

                entity.Property(e => e.UnitId).HasColumnName("unit_id");

                entity.Property(e => e.ConversionUnitId).HasColumnName("conversion_unit_id");

                entity.Property(e => e.ConversionFactor).HasColumnName("conversion_factor");
            });

            modelBuilder.Entity<Modelparameter>(entity =>
            {
                entity.ToTable("modelparameter");

                entity.Property(e => e.ModelparameterId)
                    .HasColumnName("modelparameter_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.ActualTime).HasColumnName("actual_time");

                entity.Property(e => e.ActualTimeFromSystemTime).HasColumnName("actual_time_from_system_time");

                entity.Property(e => e.AllowChangeWorkerActivityTimeMin).HasColumnName("allow_change_worker_activity_time_min");

                entity.Property(e => e.AllowMultipleMachineWork).HasColumnName("allow_multiple_machine_work");

                entity.Property(e => e.AllowOverlapActivityTypeSetup).HasColumnName("allow_overlap_activity_type_setup");

                entity.Property(e => e.AllowOverlapActivityTypeWait).HasColumnName("allow_overlap_activity_type_wait");

                entity.Property(e => e.AutoCalculatePeriods).HasColumnName("auto_calculate_periods");

                entity.Property(e => e.AutoConfirmChildProductionorders).HasColumnName("auto_confirm_child_productionorders");

                entity.Property(e => e.CapacityPeriodEnd).HasColumnName("capacity_period_end");

                entity.Property(e => e.CapacityPeriodPast).HasColumnName("capacity_period_past");

                entity.Property(e => e.CapacityPeriodPlanning).HasColumnName("capacity_period_planning");

                entity.Property(e => e.CapacityPeriodStart).HasColumnName("capacity_period_start");

                entity.Property(e => e.CapitalCommitmentInterestRate).HasColumnName("capital_commitment_interest_rate");

                entity.Property(e => e.DataPeriodEnd).HasColumnName("data_period_end");

                entity.Property(e => e.DataPeriodPlanning).HasColumnName("data_period_planning");

                entity.Property(e => e.ObjectiveFunctionType).HasColumnName("objective_function_type");

                entity.Property(e => e.SchedulePrt).HasColumnName("schedule_prt");

                entity.Property(e => e.ScheduleWorker).HasColumnName("schedule_worker");

                entity.Property(e => e.SchedulingStatusLate).HasColumnName("scheduling_status_late");

                entity.Property(e => e.SchedulingStatusOntime).HasColumnName("scheduling_status_ontime");

                entity.Property(e => e.StockPriorityId).HasColumnName("stock_priority_id");

                entity.Property(e => e.TimeBufferPurchaseorder).HasColumnName("time_buffer_purchaseorder");

                entity.Property(e => e.TimeBufferSalesorder).HasColumnName("time_buffer_salesorder");

                entity.Property(e => e.WorkerIntervalTimeMin).HasColumnName("worker_interval_time_min");
            });

            modelBuilder.Entity<Objecttype>(entity =>
            {
                entity.ToTable("objecttype");

                entity.Property(e => e.ObjecttypeId)
                    .HasColumnName("objecttype_id")
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<ObjecttypeUdf>(entity =>
            {
                entity.HasKey(e => new { e.ObjecttypeId, e.UdfId });

                entity.ToTable("objecttype_udf");

                entity.Property(e => e.ObjecttypeId).HasColumnName("objecttype_id");

                entity.Property(e => e.UdfId).HasColumnName("udf_id");

                entity.Property(e => e.Category).HasColumnName("category");

                entity.Property(e => e.Datatype).HasColumnName("datatype");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.Range).HasColumnName("range");
            });

            modelBuilder.Entity<Optimizationgroup>(entity =>
            {
                entity.ToTable("optimizationgroup");

                entity.Property(e => e.OptimizationgroupId)
                    .HasColumnName("optimizationgroup_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ValueDifferenceMax).HasColumnName("value_difference_max");
            });

            modelBuilder.Entity<Planningparameter>(entity =>
            {
                entity.ToTable("planningparameter");

                entity.Property(e => e.PlanningparameterId)
                    .HasColumnName("planningparameter_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.ImportanceCapitalCommitment).HasColumnName("importance_capital_commitment");

                entity.Property(e => e.ImportanceLateness).HasColumnName("importance_lateness");

                entity.Property(e => e.ImportanceProcessing).HasColumnName("importance_processing");

                entity.Property(e => e.ImportanceSetup).HasColumnName("importance_setup");

                entity.Property(e => e.ImportanceThroughputTime).HasColumnName("importance_throughput_time");

                entity.Property(e => e.ImportanceUtilization).HasColumnName("importance_utilization");

                entity.Property(e => e.ImportanceWorker).HasColumnName("importance_worker");

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

            modelBuilder.Entity<Plant>(entity =>
            {
                entity.ToTable("plant");

                entity.Property(e => e.PlantId)
                    .HasColumnName("plant_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Default).HasColumnName("default");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<Priority>(entity =>
            {
                entity.ToTable("priority");

                entity.Property(e => e.PriorityId)
                    .HasColumnName("priority_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");
            });

            modelBuilder.Entity<PriorityLatenesscost>(entity =>
            {
                entity.HasKey(e => new { e.PriorityId, e.Start });

                entity.ToTable("priority_latenesscost");

                entity.Property(e => e.PriorityId).HasColumnName("priority_id");

                entity.Property(e => e.Start).HasColumnName("start");

                entity.Property(e => e.CostsAbsolute).HasColumnName("costs_absolute");

                entity.Property(e => e.CostsAbsoluteInterval).HasColumnName("costs_absolute_interval");

                entity.Property(e => e.CostsRelative).HasColumnName("costs_relative");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<Productioncontroller>(entity =>
            {
                entity.ToTable("productioncontroller");

                entity.Property(e => e.ProductioncontrollerId)
                    .HasColumnName("productioncontroller_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.PlantId).HasColumnName("plant_id");
            });

            modelBuilder.Entity<Productionorder>(entity =>
            {
                entity.ToTable("productionorder");

                entity.Property(e => e.ProductionorderId)
                    .HasColumnName("productionorder_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Approved).HasColumnName("approved");

                entity.Property(e => e.DateEnd).HasColumnName("date_end");

                entity.Property(e => e.DateStart).HasColumnName("date_start");

                entity.Property(e => e.Duedate).HasColumnName("duedate");

                entity.Property(e => e.EarliestStartDate).HasColumnName("earliest_start_date");

                entity.Property(e => e.FixedProperties).HasColumnName("fixed_properties");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.InfoDateEndInitial).HasColumnName("info_date_end_initial");

                entity.Property(e => e.InfoDateStartInitial).HasColumnName("info_date_start_initial");

                entity.Property(e => e.InfoDebug).HasColumnName("info_debug");

                entity.Property(e => e.Locked).HasColumnName("locked");

                entity.Property(e => e.MaterialId).HasColumnName("material_id");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.PlanningType).HasColumnName("planning_type");

                entity.Property(e => e.PriorityId).HasColumnName("priority_id");

                entity.Property(e => e.QuantityGross).HasColumnName("quantity_gross");

                entity.Property(e => e.QuantityNet).HasColumnName("quantity_net");

                entity.Property(e => e.QuantityUnitId).HasColumnName("quantity_unit_id");

                entity.Property(e => e.RoutingId).HasColumnName("routing_id");

                entity.Property(e => e.SchedulingStatus).HasColumnName("scheduling_status");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.ValueProduction).HasColumnName("value_production");

                entity.Property(e => e.ValueSales).HasColumnName("value_sales");
            });

            modelBuilder.Entity<ProductionorderOperationActivity>(entity =>
            {
                entity.HasKey(e => new { e.ProductionorderId, e.OperationId, e.AlternativeId, e.ActivityId, e.SplitId });

                entity.ToTable("productionorder_operation_activity");

                entity.Property(e => e.ProductionorderId).HasColumnName("productionorder_id");

                entity.Property(e => e.OperationId).HasColumnName("operation_id");

                entity.Property(e => e.AlternativeId).HasColumnName("alternative_id");

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.ActivityType).HasColumnName("activity_type");

                entity.Property(e => e.DateEnd).HasColumnName("date_end");

                entity.Property(e => e.DateStart).HasColumnName("date_start");

                entity.Property(e => e.DateStartFix).HasColumnName("date_start_fix");

                entity.Property(e => e.DurationFix).HasColumnName("duration_fix");

                entity.Property(e => e.EarliestStartDate).HasColumnName("earliest_start_date");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.InfoDateEarliestEndInitial).HasColumnName("info_date_earliest_end_initial");

                entity.Property(e => e.InfoDateEarliestEndMaterial).HasColumnName("info_date_earliest_end_material");

                entity.Property(e => e.InfoDateEarliestEndScheduling).HasColumnName("info_date_earliest_end_scheduling");

                entity.Property(e => e.InfoDateEarliestStartInitial).HasColumnName("info_date_earliest_start_initial");

                entity.Property(e => e.InfoDateEarliestStartMaterial).HasColumnName("info_date_earliest_start_material");

                entity.Property(e => e.InfoDateEarliestStartScheduling).HasColumnName("info_date_earliest_start_scheduling");

                entity.Property(e => e.InfoDateEndInitial).HasColumnName("info_date_end_initial");

                entity.Property(e => e.InfoDateLatestEndInitial).HasColumnName("info_date_latest_end_initial");

                entity.Property(e => e.InfoDateLatestEndMaterial).HasColumnName("info_date_latest_end_material");

                entity.Property(e => e.InfoDateLatestEndScheduling).HasColumnName("info_date_latest_end_scheduling");

                entity.Property(e => e.InfoDateLatestStartInitial).HasColumnName("info_date_latest_start_initial");

                entity.Property(e => e.InfoDateLatestStartMaterial).HasColumnName("info_date_latest_start_material");

                entity.Property(e => e.InfoDateLatestStartScheduling).HasColumnName("info_date_latest_start_scheduling");

                entity.Property(e => e.InfoDateStartInitial).HasColumnName("info_date_start_initial");

                entity.Property(e => e.InfoDebug).HasColumnName("info_debug");

                entity.Property(e => e.InfoDuration).HasColumnName("info_duration");

                entity.Property(e => e.InfoNote).HasColumnName("info_note");

                entity.Property(e => e.InfoSetup).HasColumnName("info_setup");

                entity.Property(e => e.InfoTimeBufferInitial).HasColumnName("info_time_buffer_initial");

                entity.Property(e => e.InfoTimeBufferLatestEnd).HasColumnName("info_time_buffer_latest_end");

                entity.Property(e => e.InfoTimeBufferMaterial).HasColumnName("info_time_buffer_material");

                entity.Property(e => e.InfoTimeBufferScheduling).HasColumnName("info_time_buffer_scheduling");

                entity.Property(e => e.InfoValueWorkcenter).HasColumnName("info_value_workcenter");

                entity.Property(e => e.InfoValueWorker).HasColumnName("info_value_worker");

                entity.Property(e => e.InfoWorkerUtilization).HasColumnName("info_worker_utilization");

                entity.Property(e => e.JobParallelId).HasColumnName("job_parallel_id");

                entity.Property(e => e.JobSequentialId).HasColumnName("job_sequential_id");

                entity.Property(e => e.LastConfirmationDate).HasColumnName("last_confirmation_date");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.OperationType).HasColumnName("operation_type");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");

                entity.Property(e => e.QuantityFinishedPercentage).HasColumnName("quantity_finished_percentage");

                entity.Property(e => e.QuantityGross).HasColumnName("quantity_gross");

                entity.Property(e => e.QuantityNet).HasColumnName("quantity_net");

                entity.Property(e => e.SchedulingLevel).HasColumnName("scheduling_level");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.ValueProduction).HasColumnName("value_production");

                entity.Property(e => e.ValueProductionAccumulated).HasColumnName("value_production_accumulated");
            });

            modelBuilder.Entity<ProductionorderOperationActivityFixedresource>(entity =>
            {
                entity.HasKey(e => new { e.ProductionorderId, e.OperationId, e.AlternativeId, e.ActivityId, e.SplitId, e.ResourceId, e.ResourceType });

                entity.ToTable("productionorder_operation_activity_fixedresource");

                entity.Property(e => e.ProductionorderId).HasColumnName("productionorder_id");

                entity.Property(e => e.OperationId).HasColumnName("operation_id");

                entity.Property(e => e.AlternativeId).HasColumnName("alternative_id");

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.ResourceId).HasColumnName("resource_id");

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.GroupId).HasColumnName("group_id");
            });

            modelBuilder.Entity<ProductionorderOperationActivityMaterialrelation>(entity =>
            {
                entity.HasKey(e => new { e.ProductionorderId, e.OperationId, e.ActivityId, e.SplitId, e.MaterialrelationType, e.ChildId, e.ChildOperationId, e.ChildActivityId, e.ChildSplitId });

                entity.ToTable("productionorder_operation_activity_materialrelation");

                entity.Property(e => e.ProductionorderId).HasColumnName("productionorder_id");

                entity.Property(e => e.OperationId).HasColumnName("operation_id");

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.MaterialrelationType).HasColumnName("materialrelation_type");

                entity.Property(e => e.ChildId).HasColumnName("child_id");

                entity.Property(e => e.ChildOperationId).HasColumnName("child_operation_id");

                entity.Property(e => e.ChildActivityId).HasColumnName("child_activity_id");

                entity.Property(e => e.ChildSplitId).HasColumnName("child_split_id");

                entity.Property(e => e.Fixed).HasColumnName("fixed");

                entity.Property(e => e.InfoDateAvailability).HasColumnName("info_date_availability");

                entity.Property(e => e.InfoTimeBuffer).HasColumnName("info_time_buffer");

                entity.Property(e => e.OverlapValue).HasColumnName("overlap_value");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.QuantityUnitId).HasColumnName("quantity_unit_id");
            });

            modelBuilder.Entity<ProductionorderOperationActivityResource>(entity =>
            {
                entity.HasKey(e => new { e.ProductionorderId, e.OperationId, e.AlternativeId, e.ActivityId, e.SplitId, e.ResourceId, e.ResourceType });

                entity.ToTable("productionorder_operation_activity_resource");

                entity.Property(e => e.ProductionorderId).HasColumnName("productionorder_id");

                entity.Property(e => e.OperationId).HasColumnName("operation_id");

                entity.Property(e => e.AlternativeId).HasColumnName("alternative_id");

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.ResourceId).HasColumnName("resource_id");

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.Allocation).HasColumnName("allocation");

                entity.Property(e => e.GroupId).HasColumnName("group_id");
            });

            modelBuilder.Entity<ProductionorderOperationActivityResourceInterval>(entity =>
            {
                entity.HasKey(e => new { e.ProductionorderId, e.OperationId, e.AlternativeId, e.ActivityId, e.SplitId, e.ResourceId, e.ResourceType, e.DateFrom });

                entity.ToTable("productionorder_operation_activity_resource_interval");

                entity.Property(e => e.ProductionorderId).HasColumnName("productionorder_id");

                entity.Property(e => e.OperationId).HasColumnName("operation_id");

                entity.Property(e => e.AlternativeId).HasColumnName("alternative_id");

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.ResourceId).HasColumnName("resource_id");

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.DateFrom).HasColumnName("date_from");

                entity.Property(e => e.DateTo).HasColumnName("date_to");

                entity.Property(e => e.IntervalAllocationType).HasColumnName("interval_allocation_type");
            });

            modelBuilder.Entity<Prt>(entity =>
            {
                entity.ToTable("prt");

                entity.Property(e => e.PrtId)
                    .HasColumnName("prt_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.AllocationMax).HasColumnName("allocation_max");

                entity.Property(e => e.AllowChangeWorkcenter).HasColumnName("allow_change_workcenter");

                entity.Property(e => e.CapacityType).HasColumnName("capacity_type");

                entity.Property(e => e.FactorProcessingSpeed).HasColumnName("factor_processing_speed");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.MaintenanceIntervalQuantity).HasColumnName("maintenance_interval_quantity");

                entity.Property(e => e.MaintenanceIntervalQuantityUnitId).HasColumnName("maintenance_interval_quantity_unit_id");

                entity.Property(e => e.MaintenanceIntervalTime).HasColumnName("maintenance_interval_time");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.SetupTime).HasColumnName("setup_time");
            });

            modelBuilder.Entity<PrtUdf>(entity =>
            {
                entity.HasKey(e => new { e.PrtId, e.UdfId });

                entity.ToTable("prt_udf");

                entity.Property(e => e.PrtId).HasColumnName("prt_id");

                entity.Property(e => e.UdfId).HasColumnName("udf_id");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<PrtWorkcenter>(entity =>
            {
                entity.HasKey(e => new { e.PrtId, e.WorkcenterId });

                entity.ToTable("prt_workcenter");

                entity.Property(e => e.PrtId).HasColumnName("prt_id");

                entity.Property(e => e.WorkcenterId).HasColumnName("workcenter_id");

                entity.Property(e => e.FactorSetupTime).HasColumnName("factor_setup_time");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");
            });

            modelBuilder.Entity<Purchaseorder>(entity =>
            {
                entity.ToTable("purchaseorder");

                entity.Property(e => e.PurchaseorderId)
                    .HasColumnName("purchaseorder_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.DeliveryDate).HasColumnName("delivery_date");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.InfoOrderDate).HasColumnName("info_order_date");

                entity.Property(e => e.Locked).HasColumnName("locked");

                entity.Property(e => e.MaterialId).HasColumnName("material_id");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.PurchaseorderType).HasColumnName("purchaseorder_type");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.QuantityDelivered).HasColumnName("quantity_delivered");

                entity.Property(e => e.QuantityUnitId).HasColumnName("quantity_unit_id");

                entity.Property(e => e.Status).HasColumnName("status");
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.ToTable("report");

                entity.Property(e => e.ReportId)
                    .HasColumnName("report_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Category).HasColumnName("category");

                entity.Property(e => e.DateFrom).HasColumnName("date_from");

                entity.Property(e => e.DateTo).HasColumnName("date_to");

                entity.Property(e => e.FixedColumnsCount).HasColumnName("fixed_columns_count");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.OrderAsc).HasColumnName("order_asc");

                entity.Property(e => e.PagebreakAfterObject).HasColumnName("pagebreak_after_object");

                entity.Property(e => e.ReportType).HasColumnName("report_type");

                entity.Property(e => e.SelectedObjectId).HasColumnName("selected_object_id");

                entity.Property(e => e.SortingColumn).HasColumnName("sorting_column");

                entity.Property(e => e.UserId).HasColumnName("user_id");
            });

            modelBuilder.Entity<ReportColorfilter>(entity =>
            {
                entity.HasKey(e => new { e.ReportId, e.FilterId });

                entity.ToTable("report_colorfilter");

                entity.Property(e => e.ReportId).HasColumnName("report_id");

                entity.Property(e => e.FilterId).HasColumnName("filter_id");

                entity.Property(e => e.BgColor).HasColumnName("bg_color");

                entity.Property(e => e.FilterOperator).HasColumnName("filter_operator");

                entity.Property(e => e.FilterResultCountMax).HasColumnName("filter_result_count_max");

                entity.Property(e => e.FontColor).HasColumnName("font_color");

                entity.Property(e => e.FullColumnColoring).HasColumnName("full_column_coloring");

                entity.Property(e => e.FullRowColoring).HasColumnName("full_row_coloring");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");

                entity.Property(e => e.PropertyId).HasColumnName("property_id");
            });

            modelBuilder.Entity<ReportColorfilterItem>(entity =>
            {
                entity.HasKey(e => new { e.ReportId, e.FilterId, e.PropertyId });

                entity.ToTable("report_colorfilter_item");

                entity.Property(e => e.ReportId).HasColumnName("report_id");

                entity.Property(e => e.FilterId).HasColumnName("filter_id");

                entity.Property(e => e.PropertyId).HasColumnName("property_id");

                entity.Property(e => e.CaseSensitive).HasColumnName("case_sensitive");

                entity.Property(e => e.FilterMatchingType).HasColumnName("filter_matching_type");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<ReportColumn>(entity =>
            {
                entity.HasKey(e => new { e.ReportId, e.Index });

                entity.ToTable("report_column");

                entity.Property(e => e.ReportId).HasColumnName("report_id");

                entity.Property(e => e.Index).HasColumnName("index");

                entity.Property(e => e.Formula).HasColumnName("formula");

                entity.Property(e => e.PropertyId).HasColumnName("property_id");

                entity.Property(e => e.PropertyName).HasColumnName("property_name");
            });

            modelBuilder.Entity<ReportOption>(entity =>
            {
                entity.HasKey(e => new { e.ReportId, e.OptionId });

                entity.ToTable("report_option");

                entity.Property(e => e.ReportId).HasColumnName("report_id");

                entity.Property(e => e.OptionId).HasColumnName("option_id");
            });

            modelBuilder.Entity<ReportOptionItem>(entity =>
            {
                entity.HasKey(e => new { e.ReportId, e.OptionId });

                entity.ToTable("report_option_item");

                entity.Property(e => e.ReportId).HasColumnName("report_id");

                entity.Property(e => e.OptionId).HasColumnName("option_id");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<ReportRowfilter>(entity =>
            {
                entity.HasKey(e => new { e.ReportId, e.FilterId });

                entity.ToTable("report_rowfilter");

                entity.Property(e => e.ReportId).HasColumnName("report_id");

                entity.Property(e => e.FilterId).HasColumnName("filter_id");

                entity.Property(e => e.FilterOperator).HasColumnName("filter_operator");

                entity.Property(e => e.FilterResultCountMax).HasColumnName("filter_result_count_max");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");
            });

            modelBuilder.Entity<ReportRowfilterItem>(entity =>
            {
                entity.HasKey(e => new { e.ReportId, e.FilterId, e.PropertyId });

                entity.ToTable("report_rowfilter_item");

                entity.Property(e => e.ReportId).HasColumnName("report_id");

                entity.Property(e => e.FilterId).HasColumnName("filter_id");

                entity.Property(e => e.PropertyId).HasColumnName("property_id");

                entity.Property(e => e.CaseSensitive).HasColumnName("case_sensitive");

                entity.Property(e => e.FilterMatchingType).HasColumnName("filter_matching_type");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<Resourcestatus>(entity =>
            {
                entity.HasKey(e => new { e.ResourceId, e.ResourceType });

                entity.ToTable("resourcestatus");

                entity.Property(e => e.ResourceId).HasColumnName("resource_id");

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.BgBlue).HasColumnName("bg_blue");

                entity.Property(e => e.BgGreen).HasColumnName("bg_green");

                entity.Property(e => e.BgRed).HasColumnName("bg_red");

                entity.Property(e => e.Date).HasColumnName("date");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.UsedQuantity).HasColumnName("used_quantity");

                entity.Property(e => e.UsedQuantityUnit).HasColumnName("used_quantity_unit");

                entity.Property(e => e.UsedTime).HasColumnName("used_time");
            });

            modelBuilder.Entity<Resultinfo>(entity =>
            {
                entity.ToTable("resultinfo");

                entity.Property(e => e.ResultinfoId)
                    .HasColumnName("resultinfo_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.CountProductionorderEarly).HasColumnName("count_productionorder_early");

                entity.Property(e => e.CountProductionorderIncomplete).HasColumnName("count_productionorder_incomplete");

                entity.Property(e => e.CountProductionorderLate).HasColumnName("count_productionorder_late");

                entity.Property(e => e.CountProductionorderOntime).HasColumnName("count_productionorder_ontime");

                entity.Property(e => e.CountSalesorderIncomplete).HasColumnName("count_salesorder_incomplete");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.OptimizationRun).HasColumnName("optimization_run");

                entity.Property(e => e.Timestamp).HasColumnName("timestamp");

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

            modelBuilder.Entity<Routing>(entity =>
            {
                entity.ToTable("routing");

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.AllowOverlap).HasColumnName("allow_overlap");

                entity.Property(e => e.BomId).HasColumnName("bom_id");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.Locked).HasColumnName("locked");

                entity.Property(e => e.MasterRoutingId).HasColumnName("master_routing_id");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<RoutingOperation>(entity =>
            {
                entity.HasKey(e => new { e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId });

                entity.ToTable("routing_operation");

                entity.Property(e => e.RoutingId).HasColumnName("routing_id");

                entity.Property(e => e.OperationId).HasColumnName("operation_id");

                entity.Property(e => e.AlternativeId).HasColumnName("alternative_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.AllowInterruptions).HasColumnName("allow_interruptions");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.OperationType).HasColumnName("operation_type");

                entity.Property(e => e.SplitMax).HasColumnName("split_max");

                entity.Property(e => e.SplitMin).HasColumnName("split_min");

                entity.Property(e => e.WorkerRequirementType).HasColumnName("worker_requirement_type");
            });

            modelBuilder.Entity<RoutingOperationActivity>(entity =>
            {
                entity.HasKey(e => new { e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.ActivityId });

                entity.ToTable("routing_operation_activity");

                entity.Property(e => e.RoutingId).HasColumnName("routing_id");

                entity.Property(e => e.OperationId).HasColumnName("operation_id");

                entity.Property(e => e.AlternativeId).HasColumnName("alternative_id");

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

            modelBuilder.Entity<RoutingOperationActivityBomItem>(entity =>
            {
                entity.HasKey(e => new { e.RoutingId, e.OperationId, e.SplitId, e.ActivityId, e.BomItemId });

                entity.ToTable("routing_operation_activity_bom_item");

                entity.Property(e => e.RoutingId).HasColumnName("routing_id");

                entity.Property(e => e.OperationId).HasColumnName("operation_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.BomItemId).HasColumnName("bom_item_id");
            });

            modelBuilder.Entity<RoutingOperationActivityPrt>(entity =>
            {
                entity.HasKey(e => new { e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.ActivityId, e.GroupId, e.PrtId });

                entity.ToTable("routing_operation_activity_prt");

                entity.Property(e => e.RoutingId).HasColumnName("routing_id");

                entity.Property(e => e.OperationId).HasColumnName("operation_id");

                entity.Property(e => e.AlternativeId).HasColumnName("alternative_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.GroupId).HasColumnName("group_id");

                entity.Property(e => e.PrtId).HasColumnName("prt_id");

                entity.Property(e => e.PrtAllocation).HasColumnName("prt_allocation");
            });

            modelBuilder.Entity<RoutingOperationActivityResourcereservation>(entity =>
            {
                entity.HasKey(e => new { e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.ActivityId, e.ReservationId, e.ReservationType, e.ResourceType, e.ResourceId });

                entity.ToTable("routing_operation_activity_resourcereservation");

                entity.Property(e => e.RoutingId).HasColumnName("routing_id");

                entity.Property(e => e.OperationId).HasColumnName("operation_id");

                entity.Property(e => e.AlternativeId).HasColumnName("alternative_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.ReservationId).HasColumnName("reservation_id");

                entity.Property(e => e.ReservationType).HasColumnName("reservation_type");

                entity.Property(e => e.ResourceType).HasColumnName("resource_type");

                entity.Property(e => e.ResourceId).HasColumnName("resource_id");
            });

            modelBuilder.Entity<RoutingOperationActivityWorkcenterfactor>(entity =>
            {
                entity.HasKey(e => new { e.WorkcenterId, e.ActivityId, e.SplitId, e.AlternativeId, e.OperationId, e.RoutingId });

                entity.ToTable("routing_operation_activity_workcenterfactor");

                entity.Property(e => e.WorkcenterId).HasColumnName("workcenter_id");

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.AlternativeId).HasColumnName("alternative_id");

                entity.Property(e => e.OperationId).HasColumnName("operation_id");

                entity.Property(e => e.RoutingId).HasColumnName("routing_id");

                entity.Property(e => e.FactorWorkcenterTime).HasColumnName("factor_workcenter_time");
            });

            modelBuilder.Entity<RoutingOperationOperationrelation>(entity =>
            {
                entity.HasKey(e => new { e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.SuccessorOperationId, e.SuccessorAlternativeId, e.SuccessorSplitId });

                entity.ToTable("routing_operation_operationrelation");

                entity.Property(e => e.RoutingId).HasColumnName("routing_id");

                entity.Property(e => e.OperationId).HasColumnName("operation_id");

                entity.Property(e => e.AlternativeId).HasColumnName("alternative_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.SuccessorOperationId).HasColumnName("successor_operation_id");

                entity.Property(e => e.SuccessorAlternativeId).HasColumnName("successor_alternative_id");

                entity.Property(e => e.SuccessorSplitId).HasColumnName("successor_split_id");

                entity.Property(e => e.OverlapType).HasColumnName("overlap_type");

                entity.Property(e => e.OverlapValue).HasColumnName("overlap_value");

                entity.Property(e => e.TimeBufferMax).HasColumnName("time_buffer_max");

                entity.Property(e => e.TimeBufferMin).HasColumnName("time_buffer_min");
            });

            modelBuilder.Entity<RoutingOperationOptimizationgroup>(entity =>
            {
                entity.HasKey(e => new { e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.OptimizationgroupType });

                entity.ToTable("routing_operation_optimizationgroup");

                entity.Property(e => e.RoutingId).HasColumnName("routing_id");

                entity.Property(e => e.OperationId).HasColumnName("operation_id");

                entity.Property(e => e.AlternativeId).HasColumnName("alternative_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.OptimizationgroupType).HasColumnName("optimizationgroup_type");

                entity.Property(e => e.OptimizationgroupId).HasColumnName("optimizationgroup_id");

                entity.Property(e => e.OptimizationgroupValue).HasColumnName("optimizationgroup_value");
            });

            modelBuilder.Entity<RoutingOperationWorkcenter>(entity =>
            {
                entity.HasKey(e => new { e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.WorkcenterId });

                entity.ToTable("routing_operation_workcenter");

                entity.Property(e => e.RoutingId).HasColumnName("routing_id");

                entity.Property(e => e.OperationId).HasColumnName("operation_id");

                entity.Property(e => e.AlternativeId).HasColumnName("alternative_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.WorkcenterId).HasColumnName("workcenter_id");

                entity.Property(e => e.Delta).HasColumnName("delta");

                entity.Property(e => e.LotSizeMax).HasColumnName("lot_size_max");

                entity.Property(e => e.LotSizeMin).HasColumnName("lot_size_min");
            });

            modelBuilder.Entity<RoutingOperationWorker>(entity =>
            {
                entity.HasKey(e => new { e.RoutingId, e.OperationId, e.AlternativeId, e.SplitId, e.GroupId });

                entity.ToTable("routing_operation_worker");

                entity.Property(e => e.RoutingId).HasColumnName("routing_id");

                entity.Property(e => e.OperationId).HasColumnName("operation_id");

                entity.Property(e => e.AlternativeId).HasColumnName("alternative_id");

                entity.Property(e => e.SplitId).HasColumnName("split_id");

                entity.Property(e => e.GroupId).HasColumnName("group_id");

                entity.Property(e => e.ActivityType).HasColumnName("activity_type");

                entity.Property(e => e.ActivityqualificationId).HasColumnName("activityqualification_id");

                entity.Property(e => e.ChangeWorkerType).HasColumnName("change_worker_type");

                entity.Property(e => e.WorkcenterId).HasColumnName("workcenter_id");

                entity.Property(e => e.WorkerRequirementCount).HasColumnName("worker_requirement_count");

                entity.Property(e => e.WorkerRequirementCountMax).HasColumnName("worker_requirement_count_max");

                entity.Property(e => e.WorkerRequirementUtilization).HasColumnName("worker_requirement_utilization");
            });

            modelBuilder.Entity<RoutingScrap>(entity =>
            {
                entity.HasKey(e => e.RoutingId);

                entity.ToTable("routing_scrap");

                entity.Property(e => e.RoutingId)
                    .HasColumnName("routing_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.UnitId).HasColumnName("unit_id");
            });

            modelBuilder.Entity<RoutingScrapItem>(entity =>
            {
                entity.HasKey(e => new { e.RoutingId, e.QuantityLimit });

                entity.ToTable("routing_scrap_item");

                entity.Property(e => e.RoutingId).HasColumnName("routing_id");

                entity.Property(e => e.QuantityLimit).HasColumnName("quantity_limit");

                entity.Property(e => e.ScrapRate).HasColumnName("scrap_rate");
            });

            modelBuilder.Entity<Salesorder>(entity =>
            {
                entity.ToTable("salesorder");

                entity.Property(e => e.SalesorderId)
                    .HasColumnName("salesorder_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Duedate).HasColumnName("duedate");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.Locked).HasColumnName("locked");

                entity.Property(e => e.MaterialId).HasColumnName("material_id");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.PlanningType).HasColumnName("planning_type");

                entity.Property(e => e.PriorityId).HasColumnName("priority_id");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.QuantityDelivered).HasColumnName("quantity_delivered");

                entity.Property(e => e.QuantityUnitId).HasColumnName("quantity_unit_id");

                entity.Property(e => e.SalesorderType).HasColumnName("salesorder_type");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.ValueSales).HasColumnName("value_sales");
            });

            modelBuilder.Entity<SalesorderMaterialrelation>(entity =>
            {
                entity.HasKey(e => new { e.SalesorderId, e.ChildId, e.MaterialrelationType });

                entity.ToTable("salesorder_materialrelation");

                entity.Property(e => e.SalesorderId).HasColumnName("salesorder_id");

                entity.Property(e => e.ChildId).HasColumnName("child_id");

                entity.Property(e => e.MaterialrelationType).HasColumnName("materialrelation_type");

                entity.Property(e => e.Fixed).HasColumnName("fixed");

                entity.Property(e => e.InfoDateAvailability).HasColumnName("info_date_availability");

                entity.Property(e => e.InfoTimeBuffer).HasColumnName("info_time_buffer");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.QuantityUnitId).HasColumnName("quantity_unit_id");
            });

            modelBuilder.Entity<Setupmatrix>(entity =>
            {
                entity.ToTable("setupmatrix");

                entity.Property(e => e.SetupmatrixId)
                    .HasColumnName("setupmatrix_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<SetupmatrixItem>(entity =>
            {
                entity.HasKey(e => new { e.SetupmatrixId, e.FromOptimizationgroupId, e.ToOptimizationgroupId });

                entity.ToTable("setupmatrix_item");

                entity.Property(e => e.SetupmatrixId).HasColumnName("setupmatrix_id");

                entity.Property(e => e.FromOptimizationgroupId).HasColumnName("from_optimizationgroup_id");

                entity.Property(e => e.ToOptimizationgroupId).HasColumnName("to_optimizationgroup_id");

                entity.Property(e => e.SetupTime).HasColumnName("setup_time");
            });

            modelBuilder.Entity<SetupmatrixItemUdf>(entity =>
            {
                entity.HasKey(e => new { e.SetupmatrixId, e.UdfId, e.FromOptimizationgroupId, e.ToOptimizationgroupId });

                entity.ToTable("setupmatrix_item_udf");

                entity.Property(e => e.SetupmatrixId).HasColumnName("setupmatrix_id");

                entity.Property(e => e.UdfId).HasColumnName("udf_id");

                entity.Property(e => e.FromOptimizationgroupId).HasColumnName("from_optimizationgroup_id");

                entity.Property(e => e.ToOptimizationgroupId).HasColumnName("to_optimizationgroup_id");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<Shift>(entity =>
            {
                entity.ToTable("shift");

                entity.Property(e => e.ShiftId)
                    .HasColumnName("shift_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.BgBlue).HasColumnName("bg_blue");

                entity.Property(e => e.BgGreen).HasColumnName("bg_green");

                entity.Property(e => e.BgRed).HasColumnName("bg_red");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<ShiftInterval>(entity =>
            {
                entity.HasKey(e => new { e.ShiftId, e.WeekdayType, e.DateFrom });

                entity.ToTable("shift_interval");

                entity.Property(e => e.ShiftId).HasColumnName("shift_id");

                entity.Property(e => e.WeekdayType).HasColumnName("weekday_type");

                entity.Property(e => e.DateFrom).HasColumnName("date_from");

                entity.Property(e => e.DateTo).HasColumnName("date_to");

                entity.Property(e => e.IntervalType).HasColumnName("interval_type");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<Shiftmodel>(entity =>
            {
                entity.ToTable("shiftmodel");

                entity.Property(e => e.ShiftmodelId)
                    .HasColumnName("shiftmodel_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<ShiftmodelShift>(entity =>
            {
                entity.HasKey(e => new { e.ShiftmodelId, e.ShiftId, e.Index });

                entity.ToTable("shiftmodel_shift");

                entity.Property(e => e.ShiftmodelId).HasColumnName("shiftmodel_id");

                entity.Property(e => e.ShiftId).HasColumnName("shift_id");

                entity.Property(e => e.Index).HasColumnName("index");
            });

            modelBuilder.Entity<Stock>(entity =>
            {
                entity.ToTable("stock");

                entity.Property(e => e.StockId)
                    .HasColumnName("stock_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.InfoStockAvg).HasColumnName("info_stock_avg");

                entity.Property(e => e.InfoStockFinal).HasColumnName("info_stock_final");

                entity.Property(e => e.InfoStockInitial).HasColumnName("info_stock_initial");

                entity.Property(e => e.InfoStockInitialDate).HasColumnName("info_stock_initial_date");

                entity.Property(e => e.InfoStockMax).HasColumnName("info_stock_max");

                entity.Property(e => e.InfoStockMin).HasColumnName("info_stock_min");
            });

            modelBuilder.Entity<Stockquantityposting>(entity =>
            {
                entity.ToTable("stockquantityposting");

                entity.Property(e => e.StockquantitypostingId)
                    .HasColumnName("stockquantityposting_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Date).HasColumnName("date");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.InfoObjectId).HasColumnName("info_object_id");

                entity.Property(e => e.InfoObjecttypeId).HasColumnName("info_objecttype_id");

                entity.Property(e => e.MaterialId).HasColumnName("material_id");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.PostingType).HasColumnName("posting_type");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.QuantityUnitId).HasColumnName("quantity_unit_id");
            });

            modelBuilder.Entity<Unit>(entity =>
            {
                entity.ToTable("unit");

                entity.Property(e => e.UnitId)
                    .HasColumnName("unit_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<UnitUnitconversion>(entity =>
            {
                entity.HasKey(e => new { e.UnitId, e.ConversionUnitId });

                entity.ToTable("unit_unitconversion");

                entity.Property(e => e.UnitId).HasColumnName("unit_id");

                entity.Property(e => e.ConversionUnitId).HasColumnName("conversion_unit_id");

                entity.Property(e => e.ConversionFactor).HasColumnName("conversion_factor");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Forename).HasColumnName("forename");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.Password).HasColumnName("password");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.Surname).HasColumnName("surname");
            });

            modelBuilder.Entity<UserPermission>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.ObjecttypeId, e.Permissiontype });

                entity.ToTable("user_permission");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.ObjecttypeId).HasColumnName("objecttype_id");

                entity.Property(e => e.Permissiontype).HasColumnName("permissiontype");
            });

            modelBuilder.Entity<UserProductioncontroller>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.ProductioncontrollerId, e.Permissiontype });

                entity.ToTable("user_productioncontroller");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.ProductioncontrollerId).HasColumnName("productioncontroller_id");

                entity.Property(e => e.Permissiontype).HasColumnName("permissiontype");
            });

            modelBuilder.Entity<UserUsertemplate>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.TemplateUserId });

                entity.ToTable("user_usertemplate");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.TemplateUserId).HasColumnName("template_user_id");
            });

            modelBuilder.Entity<Userview>(entity =>
            {
                entity.ToTable("userview");

                entity.Property(e => e.UserviewId)
                    .HasColumnName("userview_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Category).HasColumnName("category");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.ViewType).HasColumnName("view_type");
            });

            modelBuilder.Entity<UserviewItem>(entity =>
            {
                entity.HasKey(e => new { e.UserviewId, e.ItemId, e.ItemType });

                entity.ToTable("userview_item");

                entity.Property(e => e.UserviewId).HasColumnName("userview_id");

                entity.Property(e => e.ItemId).HasColumnName("item_id");

                entity.Property(e => e.ItemType).HasColumnName("item_type");

                entity.Property(e => e.ItemIndex).HasColumnName("item_index");
            });

            modelBuilder.Entity<Workcenter>(entity =>
            {
                entity.ToTable("workcenter");

                entity.Property(e => e.WorkcenterId)
                    .HasColumnName("workcenter_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.AllocationMax).HasColumnName("allocation_max");

                entity.Property(e => e.CapacityType).HasColumnName("capacity_type");

                entity.Property(e => e.FactorSpeed).HasColumnName("factor_speed");

                entity.Property(e => e.IdleTimePeriod).HasColumnName("idle_time_period");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.InterruptionTimeMax).HasColumnName("interruption_time_max");

                entity.Property(e => e.LotSizeMax).HasColumnName("lot_size_max");

                entity.Property(e => e.LotSizeMin).HasColumnName("lot_size_min");

                entity.Property(e => e.LotSizeUnitId).HasColumnName("lot_size_unit_id");

                entity.Property(e => e.MandatoryTimeInterval).HasColumnName("mandatory_time_interval");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ParallelAllocationCriteria).HasColumnName("parallel_allocation_criteria");

                entity.Property(e => e.ParallelSchedulingType).HasColumnName("parallel_scheduling_type");

                entity.Property(e => e.ScheduleWorker).HasColumnName("schedule_worker");

                entity.Property(e => e.SetupMandatoryOptimizationCriteria).HasColumnName("setup_mandatory_optimization_criteria");

                entity.Property(e => e.SetupSchedulingType).HasColumnName("setup_scheduling_type");

                entity.Property(e => e.SetupStaticTimeNeedlessCriteria).HasColumnName("setup_static_time_needless_criteria");

                entity.Property(e => e.SetupmatrixDefaultTime).HasColumnName("setupmatrix_default_time");

                entity.Property(e => e.SetupmatrixId).HasColumnName("setupmatrix_id");

                entity.Property(e => e.StablePeriod).HasColumnName("stable_period");
            });

            modelBuilder.Entity<WorkcenterCost>(entity =>
            {
                entity.HasKey(e => new { e.WorkcenterId, e.ValidFrom });

                entity.ToTable("workcenter_cost");

                entity.Property(e => e.WorkcenterId).HasColumnName("workcenter_id");

                entity.Property(e => e.ValidFrom).HasColumnName("valid_from");

                entity.Property(e => e.CostRateIdleTime).HasColumnName("cost_rate_idle_time");

                entity.Property(e => e.CostRateProcessing).HasColumnName("cost_rate_processing");

                entity.Property(e => e.CostRateSetup).HasColumnName("cost_rate_setup");
            });

            modelBuilder.Entity<WorkcenterParallelworkcenter>(entity =>
            {
                entity.HasKey(e => new { e.WorkcenterId, e.ParallelWorkcenterId });

                entity.ToTable("workcenter_parallelworkcenter");

                entity.Property(e => e.WorkcenterId).HasColumnName("workcenter_id");

                entity.Property(e => e.ParallelWorkcenterId).HasColumnName("parallel_workcenter_id");
            });

            modelBuilder.Entity<WorkcenterUdf>(entity =>
            {
                entity.HasKey(e => new { e.WorkcenterId, e.UdfId });

                entity.ToTable("workcenter_udf");

                entity.Property(e => e.WorkcenterId).HasColumnName("workcenter_id");

                entity.Property(e => e.UdfId).HasColumnName("udf_id");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<WorkcenterWorker>(entity =>
            {
                entity.HasKey(e => new { e.WorkcenterId, e.GroupId });

                entity.ToTable("workcenter_worker");

                entity.Property(e => e.WorkcenterId).HasColumnName("workcenter_id");

                entity.Property(e => e.GroupId).HasColumnName("group_id");

                entity.Property(e => e.ActivityType).HasColumnName("activity_type");

                entity.Property(e => e.ActivityqualificationId).HasColumnName("activityqualification_id");

                entity.Property(e => e.ChangeWorkerType).HasColumnName("change_worker_type");

                entity.Property(e => e.WorkerRequirementCount).HasColumnName("worker_requirement_count");

                entity.Property(e => e.WorkerRequirementCountMax).HasColumnName("worker_requirement_count_max");

                entity.Property(e => e.WorkerRequirementUtilization).HasColumnName("worker_requirement_utilization");
            });

            modelBuilder.Entity<Workcentergroup>(entity =>
            {
                entity.ToTable("workcentergroup");

                entity.Property(e => e.WorkcentergroupId)
                    .HasColumnName("workcentergroup_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.AllocationMax).HasColumnName("allocation_max");

                entity.Property(e => e.CapacityType).HasColumnName("capacity_type");

                entity.Property(e => e.IdleTimePeriod).HasColumnName("idle_time_period");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.LineType).HasColumnName("line_type");

                entity.Property(e => e.MandatoryTimeInterval).HasColumnName("mandatory_time_interval");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ParallelAllocationCriteria).HasColumnName("parallel_allocation_criteria");

                entity.Property(e => e.ParallelSchedulingType).HasColumnName("parallel_scheduling_type");
            });

            modelBuilder.Entity<WorkcentergroupCost>(entity =>
            {
                entity.HasKey(e => new { e.ValidFrom, e.WorkcentergroupId });

                entity.ToTable("workcentergroup_cost");

                entity.Property(e => e.ValidFrom).HasColumnName("valid_from");

                entity.Property(e => e.WorkcentergroupId).HasColumnName("workcentergroup_id");

                entity.Property(e => e.CostRateIdleTime).HasColumnName("cost_rate_idle_time");

                entity.Property(e => e.CostRateProcessing).HasColumnName("cost_rate_processing");

                entity.Property(e => e.CostRateSetup).HasColumnName("cost_rate_setup");
            });

            modelBuilder.Entity<WorkcentergroupWorkcenter>(entity =>
            {
                entity.HasKey(e => new { e.WorkcentergroupId, e.WorkcenterId });

                entity.ToTable("workcentergroup_workcenter");

                entity.Property(e => e.WorkcentergroupId).HasColumnName("workcentergroup_id");

                entity.Property(e => e.WorkcenterId).HasColumnName("workcenter_id");

                entity.Property(e => e.GroupType).HasColumnName("group_type");
            });

            modelBuilder.Entity<Worker>(entity =>
            {
                entity.ToTable("worker");

                entity.Property(e => e.WorkerId)
                    .HasColumnName("worker_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.AllocationMax).HasColumnName("allocation_max");

                entity.Property(e => e.CapacityType).HasColumnName("capacity_type");

                entity.Property(e => e.CostRate).HasColumnName("cost_rate");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ProcessingTimePenalty).HasColumnName("processing_time_penalty");

                entity.Property(e => e.SetupTimePenalty).HasColumnName("setup_time_penalty");
            });

            modelBuilder.Entity<WorkerActivityqualification>(entity =>
            {
                entity.HasKey(e => new { e.WorkerId, e.ActivityqualificationId });

                entity.ToTable("worker_activityqualification");

                entity.Property(e => e.WorkerId).HasColumnName("worker_id");

                entity.Property(e => e.ActivityqualificationId).HasColumnName("activityqualification_id");
            });

            modelBuilder.Entity<WorkerUdf>(entity =>
            {
                entity.HasKey(e => new { e.WorkerId, e.UdfId });

                entity.ToTable("worker_udf");

                entity.Property(e => e.WorkerId).HasColumnName("worker_id");

                entity.Property(e => e.UdfId).HasColumnName("udf_id");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<WorkerWorkcenterqualification>(entity =>
            {
                entity.HasKey(e => new { e.WorkerId, e.WorkcenterId, e.ValidFrom, e.WorkcenterqualificationType });

                entity.ToTable("worker_workcenterqualification");

                entity.Property(e => e.WorkerId).HasColumnName("worker_id");

                entity.Property(e => e.WorkcenterId).HasColumnName("workcenter_id");

                entity.Property(e => e.ValidFrom).HasColumnName("valid_from");

                entity.Property(e => e.WorkcenterqualificationType).HasColumnName("workcenterqualification_type");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");

                entity.Property(e => e.ValidUntil).HasColumnName("valid_until");
            });

            modelBuilder.Entity<Workergroup>(entity =>
            {
                entity.ToTable("workergroup");

                entity.Property(e => e.WorkergroupId)
                    .HasColumnName("workergroup_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.AllocationMax).HasColumnName("allocation_max");

                entity.Property(e => e.CapacityType).HasColumnName("capacity_type");

                entity.Property(e => e.CostRate).HasColumnName("cost_rate");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.PoolType).HasColumnName("pool_type");

                entity.Property(e => e.ProcessingTimePenalty).HasColumnName("processing_time_penalty");

                entity.Property(e => e.SetupTimePenalty).HasColumnName("setup_time_penalty");
            });

            modelBuilder.Entity<WorkergroupActivityqualification>(entity =>
            {
                entity.HasKey(e => new { e.WorkergroupId, e.ActivityqualificationId });

                entity.ToTable("workergroup_activityqualification");

                entity.Property(e => e.WorkergroupId).HasColumnName("workergroup_id");

                entity.Property(e => e.ActivityqualificationId).HasColumnName("activityqualification_id");
            });

            modelBuilder.Entity<WorkergroupPoolcapacity>(entity =>
            {
                entity.HasKey(e => new { e.WorkergroupId, e.DateFrom });

                entity.ToTable("workergroup_poolcapacity");

                entity.Property(e => e.WorkergroupId).HasColumnName("workergroup_id");

                entity.Property(e => e.DateFrom).HasColumnName("date_from");

                entity.Property(e => e.Quantity).HasColumnName("quantity");
            });

            modelBuilder.Entity<WorkergroupWorkcenterqualification>(entity =>
            {
                entity.HasKey(e => new { e.WorkergroupId, e.WorkcenterId, e.ValidFrom, e.WorkcenterqualificationType });

                entity.ToTable("workergroup_workcenterqualification");

                entity.Property(e => e.WorkergroupId).HasColumnName("workergroup_id");

                entity.Property(e => e.WorkcenterId).HasColumnName("workcenter_id");

                entity.Property(e => e.ValidFrom).HasColumnName("valid_from");

                entity.Property(e => e.WorkcenterqualificationType).HasColumnName("workcenterqualification_type");

                entity.Property(e => e.PriorityValue).HasColumnName("priority_value");

                entity.Property(e => e.ValidUntil).HasColumnName("valid_until");
            });

            modelBuilder.Entity<WorkergroupWorker>(entity =>
            {
                entity.HasKey(e => new { e.WorkergroupId, e.WorkerId });

                entity.ToTable("workergroup_worker");

                entity.Property(e => e.WorkergroupId).HasColumnName("workergroup_id");

                entity.Property(e => e.WorkerId).HasColumnName("worker_id");

                entity.Property(e => e.GroupType).HasColumnName("group_type");
            });

            modelBuilder.Entity<Workingtimemodel>(entity =>
            {
                entity.ToTable("workingtimemodel");

                entity.Property(e => e.WorkingtimemodelId)
                    .HasColumnName("workingtimemodel_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.GlobalCalendarId).HasColumnName("global_calendar_id");

                entity.Property(e => e.IndividualCalendarId).HasColumnName("individual_calendar_id");

                entity.Property(e => e.Info1).HasColumnName("info1");

                entity.Property(e => e.Info2).HasColumnName("info2");

                entity.Property(e => e.Info3).HasColumnName("info3");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ValidUntil).HasColumnName("valid_until");
            });

            modelBuilder.Entity<WorkingtimemodelShiftmodel>(entity =>
            {
                entity.HasKey(e => new { e.WorkingtimemodelId, e.StartDate });

                entity.ToTable("workingtimemodel_shiftmodel");

                entity.Property(e => e.WorkingtimemodelId).HasColumnName("workingtimemodel_id");

                entity.Property(e => e.StartDate).HasColumnName("start_date");

                entity.Property(e => e.ShiftmodelId).HasColumnName("shiftmodel_id");

                entity.Property(e => e.StartShiftIndex).HasColumnName("start_shift_index");

                entity.Property(e => e.ValidUntil).HasColumnName("valid_until");
            });
        }
    }
}
