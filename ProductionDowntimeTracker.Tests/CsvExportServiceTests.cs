using System.Text;
using ProductionDowntimeTracker.api.Models;
using ProductionDowntimeTracker.api.Services;
using Xunit;

namespace ProductionDowntimeTracker.Tests
{
    public class CsvExportServiceTests
    {
        [Fact]
        public void GenerateDowntimeCsv_ValidRecords_ReturnsExpectedCsv()
        {
            // Arrange – připravíme službu a dva prostoje.
            var service = new CsvExportService();

            var records = new List<DowntimeRecord>
            {
                // Dokončený prostoj se všemi údaji a českou diakritikou.
                new DowntimeRecord
                {
                    Id = 1,
                    MachineId = 1,
                    Machine = new Machine
                    {
                        Id = 1,
                        Name = "Montážní linka 1"
                    },
                    StartTime = new DateTime(
                        2026, 8, 26, 10, 15, 30,
                        DateTimeKind.Utc),
                    EndTime = new DateTime(
                        2026, 8, 26, 10, 45, 30,
                        DateTimeKind.Utc),
                    CategoryId = 1,
                    Category = new DowntimeCategory
                    {
                        Id = 1,
                        Name = "Porucha senzoru"
                    },
                    Detail = "Poškozený kabel senzoru"
                },

                // Probíhající prostoj nemá konec, kategorii ani detail.
                new DowntimeRecord
                {
                    Id = 2,
                    MachineId = 2,
                    Machine = new Machine
                    {
                        Id = 2,
                        Name = "Vstřikolis 1"
                    },
                    StartTime = new DateTime(
                        2026, 8, 26, 11, 0, 0,
                        DateTimeKind.Utc),
                    EndTime = null,
                    CategoryId = null,
                    Category = null,
                    Detail = null
                }
            };

            string expectedCsv =
                "ID;Stroj;Začátek;Konec;Kategorie;Detail"
                + Environment.NewLine
                + "1;Montážní linka 1;2026-08-26 10:15:30;"
                + "2026-08-26 10:45:30;Porucha senzoru;"
                + "Poškozený kabel senzoru"
                + Environment.NewLine
                + "2;Vstřikolis 1;2026-08-26 11:00:00;;;"
                + Environment.NewLine;

            // Act – vytvoříme CSV a převedeme bajty zpět na text.
            byte[] csvBytes =
                service.GenerateDowntimeCsv(records);

            string actualCsv =
                Encoding.UTF8.GetString(csvBytes);

            // Assert – celý vytvořený obsah musí přesně odpovídat očekávání.
            Assert.Equal(expectedCsv, actualCsv);
        }

        [Fact]
        public void GenerateDowntimeCsv_SpecialCharacters_EscapesValueCorrectly()
        {
            // Arrange
            var service = new CsvExportService();

            var records = new List<DowntimeRecord>
    {
        new DowntimeRecord
        {
            Id = 1,
            MachineId = 1,
            Machine = new Machine
            {
                Id = 1,
                Name = "Robot 1"
            },
            StartTime = new DateTime(
                2026, 8, 26, 12, 0, 0,
                DateTimeKind.Utc),
            EndTime = new DateTime(
                2026, 8, 26, 12, 30, 0,
                DateTimeKind.Utc),
            CategoryId = 1,
            Category = new DowntimeCategory
            {
                Id = 1,
                Name = "Porucha senzoru"
            },

            // Obsahuje uvozovky, středník a nový řádek.
            Detail =
                "Robot \"A\"; porucha"
                + Environment.NewLine
                + "Nutná oprava"
        }
    };

            string expectedCsv =
                "ID;Stroj;Začátek;Konec;Kategorie;Detail"
                + Environment.NewLine
                + "1;Robot 1;2026-08-26 12:00:00;"
                + "2026-08-26 12:30:00;Porucha senzoru;"
                + "\"Robot \"\"A\"\"; porucha"
                + Environment.NewLine
                + "Nutná oprava\""
                + Environment.NewLine;

            // Act
            byte[] csvBytes =
                service.GenerateDowntimeCsv(records);

            string actualCsv =
                Encoding.UTF8.GetString(csvBytes);

            // Assert
            Assert.Equal(expectedCsv, actualCsv);
        }

        [Fact]
        public void GenerateDowntimeCsv_EmptyRecords_ReturnsOnlyHeader()
        {
            // Arrange
            var service = new CsvExportService();
            var records = new List<DowntimeRecord>();

            string expectedCsv =
                "ID;Stroj;Začátek;Konec;Kategorie;Detail"
                + Environment.NewLine;

            // Act
            byte[] csvBytes =
                service.GenerateDowntimeCsv(records);

            string actualCsv =
                Encoding.UTF8.GetString(csvBytes);

            // Assert
            Assert.Equal(expectedCsv, actualCsv);
        }
    }
}