using System;

namespace Contracts.Services.Infrastructure.Files.Services.Validation
{
    [Flags]
    public enum Restrictions
    {
        Image = 1,
        Pdf = 2,
        MaxSize50Mb = 4
    }
}