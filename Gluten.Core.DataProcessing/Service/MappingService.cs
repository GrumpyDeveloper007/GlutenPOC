using AutoMapper;
using Gluten.Data.ClientModel;
using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.DataProcessing.Service
{
    /// <summary>
    /// Auto mapper helper service
    /// </summary>
    public class MappingService
    {
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor
        /// </summary>
        public MappingService()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<DetailedTopic, Topic>();
                cfg.CreateMap<DetailedTopic, PinLinkInfo>();
                cfg.CreateMap<Topic, PinLinkInfo>();
                cfg.CreateMap<TopicPin, TopicPinCache>();
                cfg.CreateMap<TopicPinCache, TopicPin>();
            });

            _mapper = config.CreateMapper();
        }

        /// <summary>
        /// Converts one type to another
        /// </summary>
        public outputType Map<outputType, inputType>(inputType inputData)
        {
            return _mapper.Map<outputType>(inputData);
        }
    }
}
