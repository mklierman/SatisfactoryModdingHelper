﻿using SatisfactoryModdingHelper.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryModdingHelper.Contracts.Services
{
    public interface IPluginService
    {
        object SelectedPlugin { get; set; }
        System.Collections.IEnumerable PluginList { get; }

        IEnumerable GetPluginList();
    }
}