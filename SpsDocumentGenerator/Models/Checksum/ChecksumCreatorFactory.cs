using System;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SpsDocumentGenerator.Models.Checksum;

public class ChecksumCreatorFactory
{
    public static ChecksumCreator Create(HashKind hashKind)
    {
        return hashKind switch
        {
            HashKind.Md5 => Ioc.Default.GetRequiredService<Md5ChecksumCreator>(),
            HashKind.Crc32 => Ioc.Default.GetRequiredService<Crc32ChecksumCreator>(),
            _ => throw new ArgumentOutOfRangeException(nameof(hashKind), hashKind, null)
        };
    }
}