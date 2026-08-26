using System.Diagnostics.Metrics;
using CSVFileUploader.Application.CSV.UploadCsv;

namespace CSVFileUploader.Application.Tests.CSV.UploadCsv
{

    public sealed class UploadMetricsTests
    {
        [Fact]
        public void UploadMetrics_ShouldExposeExpectedMeterName()
        {
            Assert.Equal(
                "CSVFileUploader.Uploads",
                UploadMetrics.MeterName);
        }

        [Fact]
        public void UploadMetrics_ShouldPublishCounters()
        {
            var listener =
                new MeterListener();

            var measurements =
                new Dictionary<string, long>(
                    StringComparer.Ordinal);

            listener.InstrumentPublished =
                (instrument, meterListener) =>
                {
                    if (instrument.Meter.Name ==
                        UploadMetrics.MeterName)
                    {
                        meterListener.EnableMeasurementEvents(
                            instrument);
                    }
                };

            listener.SetMeasurementEventCallback<long>(
                (instrument, measurement, tags, state) =>
                {
                    if (!measurements.TryGetValue(
                            instrument.Name,
                            out var current))
                    {
                        current = 0;
                    }

                    measurements[instrument.Name] =
                        current + measurement;
                });

            listener.Start();

            UploadMetrics.UploadsStarted.Add(1);
            UploadMetrics.UploadsCompleted.Add(1);

            listener.RecordObservableInstruments();

            try
            {
                Assert.True(
                    measurements.TryGetValue(
                        "csv_uploads_started",
                        out var started));

                Assert.True(
                    measurements.TryGetValue(
                        "csv_uploads_completed",
                        out var completed));

                Assert.True(
                    started >= 1);

                Assert.True(
                    completed >= 1);
            }
            finally
            {
                listener.Dispose();
            }
        }
    }
}