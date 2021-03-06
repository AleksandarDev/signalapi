using System.Collections.Generic;

namespace Signal.Api.Public.Functions.Devices.Dtos
{
    public class DeviceEndpointDto
    {
        public string? Channel { get; set; }

        public IEnumerable<DeviceEndpointContactDto> Contacts { get; set; }

        public class DeviceEndpointContactDto
        {
            public string? Name { get; set; }

            public string? DataType { get; set; }

            public DeviceEndpointContactAccessDto Access { get; set; }

            public double? NoiseReductionDelta { get; set; }
        }
    }
}