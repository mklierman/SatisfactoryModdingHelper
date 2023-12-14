using SatisfactoryModdingHelper.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryModdingHelper.Contracts.Services;

public interface IModService
{
    object SelectedMod { get; set; }
    IEnumerable? ModList { get; }

    IEnumerable? GetModList();

    event EventHandler<object> ModChangedEvent;
}
