﻿/** Copyright 2018 John Lamontagne https://www.rpgorigin.com

	Licensed under the Apache License, Version 2.0 (the "License");
	you may not use this file except in compliance with the License.
	You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

	Unless required by applicable law or agreed to in writing, software
	distributed under the License is distributed on an "AS IS" BASIS,
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	See the License for the specific language governing permissions and
	limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.IO;
using Lunar.Core;
using Lunar.Core.Utilities;
using Lunar.Core.Utilities.Data;
using Lunar.Core.Utilities.Data.FileSystem;
using Lunar.Core.Utilities.Data.Management;
using Lunar.Core.World.Structure;

namespace Lunar.Server.World.Structure
{
    public class MapManager : IService
    {
        private Dictionary<string, Map> _maps;
        private IDataManager<MapDescriptor> _mapDataLoader;

        public MapManager()
        {
            _maps = new Dictionary<string, Map>();

            _mapDataLoader = Server.ServiceLocator.GetService<FSDataFactory>().Create<MapFSDataManager>();

            this.LoadMaps();
        }

        private void LoadMaps()
        {
            Console.WriteLine("Loading Maps...");

            DirectoryInfo directoryInfo = new DirectoryInfo(EngineConstants.FILEPATH_MAPS);
            FileInfo[] files = directoryInfo.GetFiles($"*{EngineConstants.MAP_FILE_EXT}");

            foreach (var file in files)
            {
                Map map = new Map(_mapDataLoader.Load(new MapDataLoaderArguments(Path.GetFileNameWithoutExtension(file.FullName))));

                map.ConstructPathfinder();
                _maps.Add(map.Descriptor.Name, map);
            }

            Console.WriteLine($"Loaded {files.Length} maps.");
        }

        public bool MapExists(string mapName)
        {
            return _maps.ContainsKey(mapName);
        }

        public Map GetMap(string mapName)
        {
            return _maps[mapName];
        }

      

        public void Initalize()
        {
            throw new NotImplementedException();
        }
    }
}
