using AutoMapper;
using Gluten.Data.Access.DatabaseModel;
using Gluten.Data.ClientModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Data.Access.Service
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
                cfg.CreateMap<PinTopic, PinTopicDb>();
                cfg.CreateMap<PinTopicDb, PinTopic>();
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
