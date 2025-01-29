/*
 *  Copyright (C) 2025 xlzs0
 *
 *  This file is part of BS_Janitor.
 *
 *  BS_Janitor is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published
 *  by the Free Software Foundation, either version 3 of the License,
 *  or (at your option) any later version.
 *
 *  BS_Janitor is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with BS_Janitor.  If not, see <https://www.gnu.org/licenses/>.
 */

using HarmonyLib;
using IPA;
using IPA.Config.Stores;
using IPALogger = IPA.Logging.Logger;
using System.Reflection;
using SiraUtil.Zenject;
using System;
using BS_Janitor.Installers;

namespace BS_Janitor
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Logger { get; private set; }
        internal static Harmony Harmony { get; private set; }
        internal static event Action OnDisabled;

        [Init]
        public Plugin(IPALogger logger, IPA.Config.Config config, Zenjector zenjector)
        {
            Instance = this;
            Logger = logger;
            Harmony = new("xlzs0.BS_Janitor");
            
            Config.Instance = config.Generated<Config>();
            zenjector.Install<MenuInstaller>(Location.Menu);
        }

        [OnEnable]
        public void OnEnable()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [OnDisable]
        public void OnDisable()
        {
            Harmony.UnpatchSelf();
            OnDisabled?.Invoke();
        }
    }
}
