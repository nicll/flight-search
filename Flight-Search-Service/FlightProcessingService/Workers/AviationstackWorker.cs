using System.Text.Json;
using Confluent.Kafka;
using FlightSearchService.Models.External;
using FlightSearchService.Models.Internal;
using ProtoBuf;

namespace FlightProcessingService.Workers;

public class AviationstackWorker : IHostedService, IDisposable
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IProducer<string, byte[]> _producer;
    private CancellationTokenSource? _cts;

    public AviationstackWorker()
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
        _consumer.Subscribe("import.aviationstack");
        await Task.Factory.StartNew(Execute, TaskCreationOptions.LongRunning);
    }

    private async Task Execute()
    {
        while (_cts is not null and { IsCancellationRequested: false })
        {
            var consumeResult = _consumer.Consume(_cts.Token);
            var sourceData = JsonSerializer.Deserialize<Flight>(consumeResult.Message.Value)!;

            var staticData = new AircraftStaticInfo
            {
                CallSign = sourceData.Aircraft.Registration,
                LastUpdated = sourceData.Flight_date,
                Icao24 = sourceData.Aircraft.Icao24
            };
            var dynamicData = new AircraftDynamicInfo
            {
                CallSign = sourceData.Aircraft.Registration,
                LastUpdated = sourceData.Flight_date,
                Latitude = sourceData.Live.Latitude,
                Longitude = sourceData.Live.Longitude,
                Altitude = sourceData.Live.Altitude,
                Direction = sourceData.Live.Direction
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
