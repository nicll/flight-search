using Confluent.Kafka;
using FlightSearchService.Models.Internal;
using Microsoft.EntityFrameworkCore;
using ProtoBuf;
using System.Collections.Concurrent;

namespace FlightDatabaseImportService.Workers;

public class AircraftStaticDataImportWorker : IHostedService, IDisposable, IDeserializer<AircraftStaticInfo>
{
    private const int DequeueMin = 20, DequeueSleepMs = 2000;
    private readonly IConsumer<string, AircraftStaticInfo> _consumer;
    private readonly FlightDbContext _dbContext;
    private readonly ConcurrentQueue<AircraftStaticInfo> _queue = new();
    private CancellationTokenSource? _cts;

    public AircraftStaticDataImportWorker(FlightDbContext dbContext)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092"
        };
        _consumer = new ConsumerBuilder<string, AircraftStaticInfo>(consumerConfig)
            .SetValueDeserializer(this)
            .Build();
        _dbContext = dbContext;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_cts is not null and { IsCancellationRequested: false })
            throw new InvalidOperationException("The service has already been started.");

        _cts = new();
        _consumer.Subscribe("proc.aircraft.static");
        await Task.Factory.StartNew(Load, TaskCreationOptions.LongRunning);
        await Task.Factory.StartNew(Export, TaskCreationOptions.LongRunning);
    }

    private void Load()
    {
        while (_cts is not null and { IsCancellationRequested: false })
        {
            var consumeResult = _consumer.Consume(_cts.Token);
            var staticData = consumeResult.Message.Value;
            _queue.Enqueue(staticData);
        }
    }

    private async void Export()
    {
        while (_cts is not null and { IsCancellationRequested: false })
        {
            if (_queue.Count < DequeueMin)
            {
                await Task.Delay(DequeueSleepMs, _cts.Token);
                continue;
            }

            var items = new List<AircraftStaticInfo>();
            while (_queue.TryDequeue(out var item))
                items.Add(item);

            try
            {
                var currentDbItemsCallSigns = await _dbContext.AircraftStaticInfos
                    .AsNoTracking()
                    .Select(e => e.CallSign)
                    .ToArrayAsync();
                var alreadyInDbItems = items.Where(i => currentDbItemsCallSigns.Contains(i.CallSign));

                _dbContext.AircraftStaticInfos.UpdateRange(alreadyInDbItems);
                _dbContext.AircraftStaticInfos.AddRange(items.Except(alreadyInDbItems));

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                foreach (var item in items)
                    _queue.Enqueue(item);
            }
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

    public AircraftStaticInfo Deserialize(ReadOnlySpan<byte> data, bool isNull, Confluent.Kafka.SerializationContext context)
    {
        if (isNull)
        {
            return new()
            {
                CallSign = "",
                Icao24 = "",
                LastUpdated = default
            };
        }

        return Serializer.Deserialize<AircraftStaticInfo>(data);
    }
}
