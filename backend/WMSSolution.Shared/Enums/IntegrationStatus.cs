namespace WMSSolution.Shared.Enums;

public enum IntegrationStatus
{
    Ready = 0,
    Processing = 1,
    Done = 2,
    Reject = 3,
    Scheduled = 4
}

public enum HistoryType
{
    Inbound = 1,
    Outbound = 2,
    Swap = 3,
    RequestSwap = 4,
    Others = 5,
}
