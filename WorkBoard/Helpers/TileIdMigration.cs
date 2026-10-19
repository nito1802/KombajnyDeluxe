using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace WorkBoard.Helpers
{
    public static class TileIdMigration
    {
        // Match legacy GUIDs across live links and archived versions before deserializing longs.
        public static void Upgrade(JObject state)
        {
            var items = new[] { "LeftButtons", "MiddleButtons", "RightButtons", "LinkButtons", "LinkHistory" }
                .SelectMany(name => (state[name] as JArray)?.OfType<JObject>() ?? Enumerable.Empty<JObject>()).ToList();
            long last = state.Value<long?>("LastCreatedId") ?? 0;
            foreach (var item in items)
                if (long.TryParse((string?)item["LinkId"], NumberStyles.Integer, CultureInfo.InvariantCulture, out long id) && id > last)
                    last = id;
            var migrated = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in items)
            {
                var legacy = (string?)item["LinkId"];
                if (long.TryParse(legacy, NumberStyles.Integer, CultureInfo.InvariantCulture, out long id) && id > 0)
                {
                    item["LinkId"] = id;
                    continue;
                }
                if (string.IsNullOrWhiteSpace(legacy) || !migrated.TryGetValue(legacy, out id))
                {
                    id = checked(++last);
                    if (!string.IsNullOrWhiteSpace(legacy)) migrated[legacy] = id;
                }
                item["LinkId"] = id;
            }
            state["LastCreatedId"] = last;
        }
    }
}
