using System.Text;
using System.Text.Json;

namespace MHAuthorWebsite.Web.Utils.Helpers;

public static class EditorHelper
{
    public static string ExtractPlainTextFromQuillDelta(string deltaJson)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(deltaJson);
            if (!doc.RootElement.TryGetProperty("ops", out JsonElement ops)) return string.Empty;

            StringBuilder sb = new();

            foreach (JsonElement op in ops.EnumerateArray())
                if (op.TryGetProperty("insert", out JsonElement insert))
                    sb.Append(insert.GetString());

            return sb.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }
}