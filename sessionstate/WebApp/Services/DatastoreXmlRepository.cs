
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Google.Cloud.Datastore.V1;
using Google.Cloud.Diagnostics.Common;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebApp
{
    class TracingXmlRepository : IXmlRepository
    {
        private readonly IXmlRepository _inner;
        private readonly IManagedTracer _tracer;
        private readonly ILogger _logger;

        public TracingXmlRepository(
            IXmlRepository inner,
            IManagedTracer tracer,
            ILoggerFactory loggerFactory)
        {
            this._inner = inner;
            this._tracer = tracer;
            _logger = loggerFactory.CreateLogger<TracingXmlRepository>();
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            _logger.LogTrace("GetAllElements()");
            using (_tracer.StartSpan("GetAllElements()"))
            {
                return _inner.GetAllElements();
            }
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            _logger.LogTrace("StoreElement({0})", friendlyName);
            using (_tracer.StartSpan($"StoreElement({friendlyName})"))
            {
                _inner.StoreElement(element, friendlyName);
            }
        }
    }

    class DataStoreXmlRepository : IXmlRepository
    {
        private readonly ILogger<DataStoreXmlRepository> _logger;
        private readonly DatastoreDb _datastore;

        public class Options 
        {
            public string ProjectId { get; set; }
            public string Namespace { get; set; } = "";
        }

        private const string KIND = "XElement", ROOT = "root";
        KeyFactory _keyFactory;

        public DataStoreXmlRepository(IOptions<Options> options,
            ILogger<DataStoreXmlRepository> logger)
        {
            this._logger = logger;
            var opts = options.Value;
            _datastore = DatastoreDb.Create(opts.ProjectId, opts.Namespace);
            _keyFactory = _datastore.CreateKeyFactory(KIND);
        }
        public IReadOnlyCollection<XElement> GetAllElements()
        {
            var query = new Query(KIND) 
            {
                Filter = Filter.HasAncestor(_keyFactory.CreateKey(ROOT))
            };
            var response = _datastore.RunQuery(query);
            var xelements = response.Entities.Select(entity =>
                XElement.Parse((string)entity[KIND]));
            return xelements.ToList();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            Entity entity = new Entity() 
            {
                Key = new KeyFactory(_keyFactory.CreateKey(ROOT), KIND)
                    .CreateKey(friendlyName),
                [KIND] = element.ToString()
            };
            entity[KIND].ExcludeFromIndexes = true;
            _datastore.Upsert(entity);
        }
    }
}