using MHAuthorWebsite.Data.Models.Enums;

namespace MHAuthorWebsite.Web.Utils;

public static class AttributeDataTypeMapper
{
    public static readonly Dictionary<AttributeDataType, string> HtmlInputTypes = new()
    {
        { AttributeDataType.Text, "text" },
        { AttributeDataType.Number, "number" },
        { AttributeDataType.Date, "date" },
        { AttributeDataType.Boolean, "checkbox" }
    };
}