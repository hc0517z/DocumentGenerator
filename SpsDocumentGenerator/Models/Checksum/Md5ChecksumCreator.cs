using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace SpsDocumentGenerator.Models.Checksum;

public class Md5ChecksumCreator : ChecksumCreator
{
    public Md5ChecksumCreator(IConfiguration config) : base(config)
    {
        _checksumPrefix = string.Empty;
    }

    protected override string GenerateChecksum(string path)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(path);

        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}