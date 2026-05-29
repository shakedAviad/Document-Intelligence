namespace DocumentIntelligence.Core.Documents;

public sealed record SalesRecord(
    string Date,
    string Region,
    string Product,
    int UnitsSold,
    decimal UnitPrice,
    string Currency,
    string SalesRep,
    string Status);
