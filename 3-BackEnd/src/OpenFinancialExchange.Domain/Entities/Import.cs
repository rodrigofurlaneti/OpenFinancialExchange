using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Domain.Entities;

public sealed class Import : AggregateRoot
{
    public string  FileName       { get; private set; } = default!;
    public DateTime ImportedAt    { get; private set; }
    public string  OFXHeader      { get; private set; } = default!;
    public string  OFXData        { get; private set; } = default!;
    public string  OFXVersion     { get; private set; } = default!;
    public string  OFXSecurity    { get; private set; } = default!;
    public string  OFXEncoding    { get; private set; } = default!;
    public string  OFXCharset     { get; private set; } = default!;
    public string  OFXCompression { get; private set; } = default!;
    public string  OFXOldFileUID  { get; private set; } = default!;
    public string  OFXNewFileUID  { get; private set; } = default!;
    public string? Notes          { get; private set; }
    public string? ImportedBy     { get; private set; }

    private Import() { }

    public static Result<Import> Create(
        string fileName,
        string ofxHeader, string ofxData, string ofxVersion, string ofxSecurity,
        string ofxEncoding, string ofxCharset, string ofxCompression,
        string ofxOldFileUID, string ofxNewFileUID,
        string? notes, string? importedBy)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Result.Failure<Import>(new Error("Import.InvalidFileName", "File name is required."));

        var entity = new Import
        {
            FileName       = fileName.Trim(),
            ImportedAt     = DateTime.UtcNow,
            OFXHeader      = ofxHeader.Trim(),
            OFXData        = ofxData.Trim(),
            OFXVersion     = ofxVersion.Trim(),
            OFXSecurity    = ofxSecurity.Trim(),
            OFXEncoding    = ofxEncoding.Trim(),
            OFXCharset     = ofxCharset.Trim(),
            OFXCompression = ofxCompression.Trim(),
            OFXOldFileUID  = ofxOldFileUID.Trim(),
            OFXNewFileUID  = ofxNewFileUID.Trim(),
            Notes          = notes?.Trim(),
            ImportedBy     = importedBy?.Trim(),
            CreatedAt      = DateTime.UtcNow
        };

        entity.RaiseDomainEvent(new Events.ImportCreatedEvent(Guid.NewGuid(), DateTime.UtcNow, entity.FileName));
        return Result.Success(entity);
    }

    public Result UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        SetUpdatedAt(DateTime.UtcNow);
        return Result.Success();
    }
}
