﻿using IntlmillReports.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntlmillReports.Contracts
{
    public interface ILocationRepository
    {
        LocationViewModel GetLocations();
    }
}
