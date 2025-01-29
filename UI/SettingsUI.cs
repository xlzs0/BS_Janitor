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

using BeatSaberMarkupLanguage.GameplaySetup;
using Zenject;

namespace BS_Janitor.UI
{
    internal class SettingsUI : IInitializable
    {
        private const string _tabName = "Janitor";
        private const string _resourcePath = "BS_Janitor.UI.Views.Settings.bsml";

        public void Initialize()
        {
            AddTab();

            Plugin.OnDisabled -= RemoveTab;
            Plugin.OnDisabled += RemoveTab;
        }

        public void AddTab()
        {
            GameplaySetup.Instance.AddTab(_tabName, _resourcePath, Config.Instance);
        }

        public void RemoveTab()
        {
            GameplaySetup.Instance.RemoveTab(_tabName);
        }
    }
}
