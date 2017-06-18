﻿using System;
using System.Collections.Generic;
using System.Linq;
using Orleans.Providers.Streams.Common;
using Orleans.Runtime;

namespace Orleans.Streams
{
    [Serializable]
    public class RabbitMqBatchContainer : IBatchContainer
    {
        private readonly List<object> _events;
        private readonly Dictionary<string, object> _requestContext;

        public EventSequenceToken EventSequenceToken { set; private get; }
        public StreamSequenceToken SequenceToken => EventSequenceToken;
        public Guid StreamGuid { get; }
        public string StreamNamespace { get; }
        public ulong DeliveryTag { get; set; }
        public bool DeliveryFailure { get; set; }

        public RabbitMqBatchContainer(Guid streamGuid, string streamNamespace, List<object> events, Dictionary<string, object> requestContext)
        {
            StreamGuid = streamGuid;
            StreamNamespace = streamNamespace;
            _events = events;
            _requestContext = requestContext;
        }

        public IEnumerable<Tuple<T, StreamSequenceToken>> GetEvents<T>()
        {
            var res = _events.OfType<T>().Select((e, i) => Tuple.Create<T, StreamSequenceToken>(e, EventSequenceToken.CreateSequenceTokenForEvent(i))).ToList();
            return res;
        }

        public bool ImportRequestContext()
        {
            if (_requestContext == null) return false;
            RequestContext.Import(_requestContext);
            return true;
        }

        public bool ShouldDeliver(IStreamIdentity stream, object filterData, StreamFilterPredicate shouldReceiveFunc)
        {
            var res = _events.Any(item => shouldReceiveFunc(stream, filterData, item));
            return res;
        }
    }
}