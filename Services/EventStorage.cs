using LiteDB;
using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using trafficStreamer.Common;

namespace trafficStreamer.Services
{
    public class EventStorage
    {
        private readonly Options _options;
        private readonly ILogger<EventStorage> _logger;
        private readonly LiteDatabase _database;
        private readonly LiteCollection<TrafficEvent> _eventsCollection;
        private const string DefaultCollectionName = "trafficEvents";

        public EventStorage(Options options, ILogger<EventStorage> logger)
        {
            _options = options;
            _logger = logger;
            _database = new LiteDatabase(_options.File);
            _eventsCollection = _database.GetCollection<TrafficEvent>(DefaultCollectionName);
            _eventsCollection.EnsureIndex(e => e.Datetime);
        }

        public void FlushData()
        {
            _database.Dispose();
        }

        public void GetEvents( Action<TrafficEvent, TrafficEvent> pushEvent, CancellationToken cancellationToken, string collectionName = DefaultCollectionName)
        {
            var events = _eventsCollection.FindAll().ToArray();
            for (var iEvent = 0 ; iEvent < events.Length; iEvent++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                pushEvent(events[iEvent], events[iEvent + 1]);
            }
        }


        public void GetEvents( Action<TrafficEvent> pushEvent, string collectionName = DefaultCollectionName)
        {
            foreach (var trafficEvent in _eventsCollection.FindAll())
            {
                pushEvent(trafficEvent);
            }
        }


        public void GetEvents( DateTime startTime, DateTime endTime, Action<TrafficEvent> pushEvent)
        {
            foreach (var trafficEvent in _eventsCollection.Find(ev => ev.Datetime > startTime && ev.Datetime < endTime ))
            {
                pushEvent(trafficEvent);
            }
        }

        public void StoreEvent(TrafficEvent trafficEvent)
        {
            if (_database != null)
            {
                _eventsCollection.Insert(trafficEvent);
            }
            else
            {
                _logger.LogError($"database: {_options.File} and collection: {DefaultCollectionName} are not initialized.");
            }
        }
    }
}
