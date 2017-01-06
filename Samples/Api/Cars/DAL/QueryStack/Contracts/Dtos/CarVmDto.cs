﻿using System.Collections.Generic;

namespace Api.Cars.DAL.QueryStack.Dtos
{
    public class CarVmDto
    {
        public string PublicKey { get; set; }
        public string Color { get; set; }
        public IEnumerable<WheelVmDto> Wheels { get; set; }
    }
}
