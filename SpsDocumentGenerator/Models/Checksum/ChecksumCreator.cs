using System;
using Microsoft.Extensions.Configuration;

namespace SpsDocumentGenerator.Models.Checksum;

public abstract class ChecksumCreator
{
    protected string _checksumPrefix;
    protected readonly bool _isUpperChecksum;

    protected ChecksumCreator(IConfiguration config)
    {
        _checksumPrefix = config["Checksum:Prefix"];
        _isUpperChecksum = Convert.ToBoolean(config["Checksum:IsUpper"]);
    }

    protected abstract string GenerateChecksum(string path);

    public string Create(string path)
    {
        var checksum = GenerateChecksum(path);
        checksum = _isUpperChecksum ? checksum.ToUpper() : checksum.ToLower();
        return $"{_checksumPrefix}{checksum}";
    }
}