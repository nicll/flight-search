using Confluent.Kafka;
using FlightSearchService.Models.External;
using FlightSearchService.Models.Internal;
using ProtoBuf;
using System.Text.Json;

namespace FlightProcessingService.Workers;

public class OpenskyNetworksWorker : IHostedService, IDisposable
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IProducer<string, byte[]> _producer;
    private CancellationTokenSource? _cts;

    public OpenskyNetworksWorker()
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092"
        };
        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };
        _producer = new ProducerBuilder<string, byte[]>(producerConfig).Build();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_cts is not null and { IsCancellationRequested: false })
            throw new InvalidOperationException("The service has already been started.");

        _cts = new();
        _consumer.Subscribe("import.openskynetworks");
        await Task.Factory.StartNew(Execute, TaskCreationOptions.LongRunning);
    }

    private async Task Execute()
    {
        while (_cts is not null and { IsCancellationRequested: false })
        {
            var consumeResult = _consumer.Consume(_cts.Token);
            var sourceData = JsonSerializer.Deserialize<State>(consumeResult.Message.Value)!;

            var staticData = new AircraftStaticInfo
            {
                CallSign = sourceData.Callsign!,
                LastUpdated = DateTimeOffset.FromUnixTimeSeconds(sourceData.LastContact ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Icao24 = sourceData.Icao24!
            };
            var dynamicData = new AircraftDynamicInfo
            {
                CallSign = sourceData.Callsign!,
                LastUpdated = DateTimeOffset.FromUnixTimeSeconds(sourceData.LastContact ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Latitude = sourceData.Latitude ?? 0,
                Longitude = sourceData.Longitude ?? 0,
                Altitude = sourceData.GeoAltitude,
                Direction = null
            };

            using var ms = new MemoryStream();
            Serializer.Serialize(ms, staticData);
            var staticDataBytes = ms.ToArray();
            ms.Seek(0, SeekOrigin.Begin);
            Serializer.Serialize(ms, dynamicData);
            var dynamicDataBytes = ms.ToArray();

            var staticDataMessage = new Message<string, byte[]>()
            {
                Key = staticData.CallSign,
                Value = staticDataBytes
            };
            var dynamicDataMessage = new Message<string, byte[]>
            {
                Key = dynamicData.CallSign,
                Value = dynamicDataBytes
            };

            await _producer.ProduceAsync("proc.aircraft.static", staticDataMessage, _cts.Token);
            await _producer.ProduceAsync("proc.aircraft.dynamic", dynamicDataMessage, _cts.Token);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel();
        _consumer.Unsubscribe();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _consumer.Dispose();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
