using OpenFinancialExchange.Domain.Primitives;

namespace OpenFinancialExchange.Domain.Entities;

public sealed class OfxImport : AggregateRoot
{
    public string FileName { get; private set; } = null!;
    public string FileHash { get; private set; } = null!;
    public short? OfxHeaderVersion { get; private set; }
    public short? OfxVersion { get; private set; }
    public string? OfxData { get; private set; }
    public string? Encoding { get; private set; }
    public string? Charset { get; private set; }
    public string? Security { get; private set; }
    public string? Compression { get; private set; }
    public string? OldFileUid { get; private set; }
    public string? NewFileUid { get; private set; }
    public DateTime ImportedAt { get; private set; }

    private OfxImport() : base(0) { }  // EF Core

    private OfxImport(string fileName, string fileHash, short? ofxHeaderVersion, short? ofxVersion,
        string? ofxData, string? encoding, string? charset, string? security, string? compression,
        string? oldFileUid, string? newFileUid) : base(0)
    {
        FileName = fileName;
        FileHash = fileHash;
        OfxHeaderVersion = ofxHeaderVersion;
        OfxVersion = ofxVersion;
        OfxData = ofxData;
        Encoding = encoding;
        Charset = charset;
        Security = security;
        Compression = compression;
        OldFileUid = oldFileUid;
        NewFileUid = newFileUid;
        ImportedAt = DateTime.UtcNow;
    }

    public static Result<OfxImport> Create(string fileName, string fileHash, short? ofxHeaderVersion,
        short? ofxVersion, string? ofxData, string? encoding, string? charset, string? security,
        string? compression, string? oldFileUid, string? newFileUid)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Result.Failure<OfxImport>(
                new Error("OfxImport.EmptyFileName", "File name is required."));

        if (string.IsNullOrWhiteSpace(fileHash) || fileHash.Length != 64)
            return Result.Failure<OfxImport>(
                new Error("OfxImport.InvalidFileHash", "File hash must be a 64-character SHA-256 string."));

        return Result.Success(new OfxImport(fileName.Trim(), fileHash, ofxHeaderVersion, ofxVersion,
            ofxData, encoding, charset, security, compression, oldFileUid, newFileUid));
    }
}
