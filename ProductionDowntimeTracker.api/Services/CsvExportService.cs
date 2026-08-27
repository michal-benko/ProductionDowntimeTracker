using System.Globalization;
using System.Text;
using ProductionDowntimeTracker.api.Models;

namespace ProductionDowntimeTracker.api.Services
{
    public class CsvExportService
    {
        public byte[] GenerateDowntimeCsv(
            IEnumerable<DowntimeRecord> records)
        {
            var csvBuilder = new StringBuilder();

            csvBuilder.AppendLine(
                "ID;Stroj;Začátek;Konec;Kategorie;Detail");

            foreach (DowntimeRecord record in records)
            {
                string startTime = record.StartTime.ToString(
                    "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture);

                string endTime = record.EndTime.HasValue
                    ? record.EndTime.Value.ToString(
                        "yyyy-MM-dd HH:mm:ss",
                        CultureInfo.InvariantCulture)
                    : string.Empty;

                string[] values =
                {
                    record.Id.ToString(CultureInfo.InvariantCulture),
                    record.Machine.Name,
                    startTime,
                    endTime,
                    record.Category?.Name ?? string.Empty,
                    record.Detail ?? string.Empty
                };

                IEnumerable<string> escapedValues =
                    values.Select(EscapeCsvValue);

                csvBuilder.AppendLine(
                    string.Join(";", escapedValues));
            }

            return Encoding.UTF8.GetBytes(csvBuilder.ToString());
        }

        private static string EscapeCsvValue(string value)
        {
            bool requiresQuotes =
                value.Contains(';') ||
                value.Contains('"') ||
                value.Contains('\r') ||
                value.Contains('\n');

            if (!requiresQuotes)
            {
                return value;
            }

            string escapedValue =
                value.Replace("\"", "\"\"");

            return $"\"{escapedValue}\"";
        }
    }
}