using System.Diagnostics.Metrics;
using CSVFileUploader.Application.CSV.UploadCsv;

namespace CSVFileUploader.Application.Tests.CSV.UploadCsv
{

    public sealed class UploadObservabilityContractTests
    {
        [Fact]
        public void UploadMetrics_ShouldExposeAllExpectedCounters()
        {
            var expected =
                new[]
                {
                "csv_uploads_started",
                "csv_uploads_completed",
                "csv_uploads_duplicated",
                "csv_uploads_failed",
                "csv_uploads_cancelled",
                "csv_rows_inserted",
                "csv_rows_duplicated",
                "csv_rows_rejected"
                };

            var observed =
                new HashSet<string>(
                    StringComparer.Ordinal);

            using var listener =
                new MeterListener();

            listener.InstrumentPublished =
                (instrument, meterListener) =>
                {
                    if (instrument.Meter.Name ==
                        UploadMetrics.MeterName)
                    {
                        observed.Add(
                            instrument.Name);

                        meterListener.EnableMeasurementEvents(
                            instrument);
                    }
                };

            listener.Start();

            UploadMetrics.UploadsStarted.Add(1);
            UploadMetrics.UploadsCompleted.Add(1);
            UploadMetrics.UploadsDuplicated.Add(1);
            UploadMetrics.UploadsFailed.Add(1);
            UploadMetrics.UploadsCancelled.Add(1);
            UploadMetrics.RowsInserted.Add(1);
            UploadMetrics.RowsDuplicated.Add(1);
            UploadMetrics.RowsRejected.Add(1);

            listener.RecordObservableInstruments();

            foreach (var metric in expected)
            {
                Assert.Contains(
                    metric,
                    observed);
            }

            Assert.Equal(
                expected.Length,
                observed.Count);
        }
    }
}