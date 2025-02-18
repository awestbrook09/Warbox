using DotNext;
using StudioCore.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.TableEditor.Tools;

public static class TableGuidTools
{
    /// <summary>
    /// Use this to generate a new GUID (pass in list of Guids to exclude).
    /// </summary>
    public static Guid GenerateGuid(HashSet<Guid> excludedGuids)
    {
        return GenerateUniqueGuidV4(excludedGuids);
    }

    public static Guid GenerateUniqueGuidV4(HashSet<Guid> existingGuids)
    {
        Guid newGuid;

        do
        {
            newGuid = GenerateGuidV4();
        } while (existingGuids.Contains(newGuid));

        return newGuid;
    }

    public static Guid GenerateGuidV4()
    {
        byte[] guidBytes = Guid.NewGuid().ToByteArray();

        guidBytes[7] = (byte)((guidBytes[7] & 0x0F) | 0x40);
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

        return new Guid(guidBytes);
    }

}

