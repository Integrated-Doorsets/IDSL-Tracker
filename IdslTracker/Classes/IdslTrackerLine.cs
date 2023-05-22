using System;

namespace IdslTracker
{
    public class IdslTrackerLine
    {
        public string DocNumber { get; internal set; }
        public string ManfSite { get; internal set; }
        public string Contract { get; internal set; }
        public string JobNo { get; internal set; }
        public string BatchRef { get; internal set; }
        public string ProductType { get; internal set; }
        public int? FrameQty { get; internal set; }
        public int? DoorQty { get; internal set; }
        public int? ScreenQty { get; internal set; }
        public int? PanelQty { get; internal set; }
        public int? MiscQty { get; internal set; }
        public int? IronmongeryQty { get; internal set; }
        public decimal Sales { get; internal set; }
        public string Customer { get; internal set; }
        public DateTime? DeliveryDate { get; internal set; }
        public DateTime? FtbDeliveryDate { get; internal set; }
        public string LastStageDoor { get; internal set; }
        public bool FilteredOut { get; internal set; }
        public string ProductionCommentDoor { get; internal set; }
        public int ReportId { get; internal set; }
        public string FilePrintedBy { get; internal set; }
        public DateTime? FilePrintedDate { get; internal set; }
        public int? PegasusDoorQty { get; internal set; }
        public int? PegasusFrameQty { get; internal set; }
        public int? PegasusPanelQty { get; internal set; }
        public int? PegasusScreenQty { get; internal set; }
        public int? PegasusMiscQty { get; internal set; }
        public int? PegasusIronmongeryQty { get; internal set; }
        public int DeliveryDateOverride { get; internal set; }
        public string DeliveryRiskMaterials { get; internal set; }
        public string CustomerStatus { get; internal set; }
        public string MaterialComment { get; internal set; }
        public string SchedulingContact { get; internal set; }
        public string ProcurementContact { get; internal set; }
        public string SalesContact { get; internal set; }
        public bool PjlFileHasBeenPrinted { get; internal set; }
        public bool ManufactureCompleted { get; internal set; }
        public DateTime? ManufactureEndDate { get; internal set; }
        public string ManufactureRep { get; internal set; }
        public string StorageRef { get; internal set; }
        public string ProductionCommentFrame { get; internal set; }
        public string LastStageFrame { get; internal set; }
        public bool HasBeenProcured { get; internal set; }
        public DateTime? ManufactureStartDate { get; internal set; }
        public DateTime? InvoicedDate { get; internal set; }
        public decimal CountWeeksHeld { get; internal set; }
        public DateTime? SopCreatedDate { get; internal set; }
        public string ManualMaterialComment { get; internal set; }
        public int WeekNum { get; internal set; }
        public object ProductionCommentDoorColourHex { get; internal set; }
        public object ProductionCommentFrameColourHex { get; internal set; }
        public string ManfRepAbv { get; internal set; }
        public string ShopfloorComment { get; internal set; }
        public bool IsAccrued { get; internal set; }
        public bool IsHighEndFinish { get; internal set; }
        public string WipCommentary { get; internal set; }
        public string WipFrameCommentary { get; internal set; }
        public string RiskComment { get; internal set; }
        public string RiskRowColorHex { get; internal set; }
        //public int WeekNumTest { get; internal set; }
    }
}