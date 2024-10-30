using System;
using System.IO;
using System.IO.Hashing;
using Microsoft.Extensions.Configuration;

namespace SpsDocumentGenerator.Models.Checksum;

public class Crc32ChecksumCreator : ChecksumCreator
{
    public Crc32ChecksumCreator(IConfiguration config) : base(config)
    {
    }

    protected override string GenerateChecksum(string path)
    {
        var crc32 = new Crc32();

        using (var fs = File.OpenRead(path))
        {
            crc32.Append(fs);
        }

        var checkSum = crc32.GetCurrentHash();
        Array.Reverse(checkSum);
        var hash = BitConverter.ToString(checkSum).Replace("-", "").ToLower();
        return hash;
    }
}