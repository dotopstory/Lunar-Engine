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
using System.Windows.Forms;
using DarkUI.Controls;
using DarkUI.Docking;
using Lunar.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lunar.Editor.Utilities;
using Lunar.Editor.World;
using Color = Microsoft.Xna.Framework.Color;

namespace Lunar.Editor.Controls
{
    public partial class DockTilesetTools : DarkToolWindow
    {
        private TextureLoader _tilesetTextureLoader;
        private List<Texture2D> _tilesets;
        private Camera _camera;
        private Texture2D _pointTexture;
        private Vector2 _selectPosition;
        private Rectangle _selectRectangle;
        private bool _tilesetDragging;
        private Map _currentMap;
        private Project _project;

        public int SelectedTilesetIndex => comboTileset.SelectedIndex;

        public object SelectedTileset => comboTileset.SelectedItem;

        public List<Texture2D> Tilesets => _tilesets;

        public Rectangle SelectRectangle => _selectRectangle;

        public Map Map => _currentMap;

        public DockTilesetTools()
        {
            InitializeComponent();

            _tilesets = new List<Texture2D>();
            _selectPosition = Vector2.Zero;
            _camera = new Camera(new Rectangle(0, 0, this.tilesetView.Width, this.tilesetView.Height));
        }

        public void SetProject(Project project)
        {
            _project = project;
        }

        public void SetMapSubject(Map map)
        {
            _currentMap = map;
            
            // Make sure this control has loaded properly.
            if (_tilesetTextureLoader == null)
                return;

            var lastSelected = comboTileset.SelectedItem;

            _tilesets.Clear();
            comboTileset.Items.Clear();

            foreach (var tileset in map.Tilesets)
            {
                string tilesetPath = _project.ClientRootDirectory + "/" + tileset.Tag.ToString();
                var tilesetTexture = _tilesetTextureLoader.LoadFromFile(tilesetPath);
                tilesetTexture.Tag = tileset.Tag;
                
                _tilesets.Add(tilesetTexture);
                
                comboTileset.Items.Add(Path.GetFileName(tilesetPath));
                comboTileset.SelectedItem = Path.GetFileName(tilesetPath);
            }

            if (lastSelected != null && comboTileset.Items.Contains(lastSelected))
                comboTileset.SelectedItem = lastSelected;
        }

        private void OnTilesetDraw(View view)
        {
            view.SpriteBatch.End();

            view.SpriteBatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.PointClamp, null, null, null, _camera.GetTransformation());

            int tilesetIndex = comboTileset.SelectedIndex;

            if (tilesetIndex < 0 || tilesetIndex >= _tilesets.Count)
                return;

            view.SpriteBatch.Draw(_tilesets[tilesetIndex], Vector2.One, Color.White);

            this.DrawRectangle(view.SpriteBatch, _selectRectangle, Color.DarkRed, 2);

        }

        private void tilesetView_MouseUp(object sender, MouseEventArgs e)
        {
            int mouseX = (int)(e.X + _camera.Position.X);
            int mouseY = (int)(e.Y + _camera.Position.Y);

            int left = (int)_selectPosition.X;
            int top = (int)_selectPosition.Y;


            int width = 0;
            if (mouseX < _selectPosition.X)
            {
                left = mouseX;
                width = (int)Math.Abs((((mouseX / EngineConstants.TILE_WIDTH)) * EngineConstants.TILE_WIDTH) -
                                      _selectPosition.X);
            }
            else
            {
                width = (int)Math.Abs((((mouseX / EngineConstants.TILE_WIDTH) + 1) * EngineConstants.TILE_WIDTH) -
                                      _selectPosition.X);
            }
            width = width < EngineConstants.TILE_WIDTH ? EngineConstants.TILE_WIDTH : width;

            int height = 0;
            if (mouseY < _selectPosition.Y)
            {
                top = mouseY;
                height = (int)Math.Abs((((mouseY / EngineConstants.TILE_HEIGHT)) * EngineConstants.TILE_HEIGHT) - _selectPosition.Y);
            }
            else
            {
                height = (int)Math.Abs((((mouseY / EngineConstants.TILE_HEIGHT) + 1) * EngineConstants.TILE_HEIGHT) - _selectPosition.Y);
            }
            height = height < EngineConstants.TILE_HEIGHT ? EngineConstants.TILE_HEIGHT : height;

            _selectRectangle = new Rectangle(left, top, width, height);
            _tilesetDragging = false;
        }


        private void tilesetView_MouseDown(object sender, MouseEventArgs e)
        {
            _selectPosition = new Vector2((e.X / EngineConstants.TILE_WIDTH) * EngineConstants.TILE_WIDTH, (e.Y / EngineConstants.TILE_HEIGHT) * EngineConstants.TILE_HEIGHT);
            _selectPosition = new Vector2(_selectPosition.X + ((int)_camera.Position.X / EngineConstants.TILE_WIDTH) * EngineConstants.TILE_WIDTH, _selectPosition.Y + ((int)_camera.Position.Y / EngineConstants.TILE_HEIGHT) * EngineConstants.TILE_HEIGHT);
            _selectRectangle = new Rectangle((int)_selectPosition.X, (int)_selectPosition.Y, EngineConstants.TILE_WIDTH, EngineConstants.TILE_HEIGHT);
            _tilesetDragging = true;
        }

        private void tilesetView_MouseMove(object sender, MouseEventArgs e)
        {
            if (_tilesetDragging)
            {
                int left = (int)_selectPosition.X;
                int top = (int)_selectPosition.Y;
                int width = Math.Abs((int)((e.X + _camera.Position.X) - _selectPosition.X));
                int height = Math.Abs((int)((e.Y + _camera.Position.Y) - _selectPosition.Y));

                if ((e.X + _camera.Position.X) < _selectPosition.X)
                {
                    left = (int)(e.X + _camera.Position.X);
                }

                if ((e.Y + _camera.Position.Y) < _selectPosition.Y)
                {
                    top = (int)(e.Y + _camera.Position.Y);
                }

                _selectRectangle = new Rectangle(left, top, width, height);
            }
        }


        private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color, int lineWidth)
        {
            if (_pointTexture == null)
            {
                _pointTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _pointTexture.SetData<Color>(new Color[] { Color.White });
            }

            spriteBatch.Draw(_pointTexture, null, new Rectangle(rectangle.X, rectangle.Y, lineWidth, rectangle.Height + lineWidth), null, null, 0f, null, color, SpriteEffects.None, 1f);
            spriteBatch.Draw(_pointTexture, null, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width + lineWidth, lineWidth), null, null, 0f, null, color, SpriteEffects.None, 1f);
            spriteBatch.Draw(_pointTexture, null, new Rectangle(rectangle.X + rectangle.Width, rectangle.Y, lineWidth, rectangle.Height + lineWidth), null, null, 0f, null, color, SpriteEffects.None, 1f);
            spriteBatch.Draw(_pointTexture, null, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height, rectangle.Width + lineWidth, lineWidth), null, null, 0f, null, color, SpriteEffects.None, 1f);
        }


        private void buttonAddTileset_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.RestoreDirectory = true;
                dialog.InitialDirectory = _project.ClientRootDirectory.FullName;
                dialog.Filter = @"Tileset Files (*.png)|*.png";
                dialog.DefaultExt = ".png";
                dialog.AddExtension = true;
                dialog.Multiselect = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    for (int i = 0; i < dialog.FileNames.Length; i++)
                    {
                        string path = dialog.FileNames[i];

                        var tilesetTexture = _tilesetTextureLoader.LoadFromFile(path);
                        tilesetTexture.Tag = path;

                        _tilesets.Add(tilesetTexture);

                        comboTileset.Items.Add(dialog.SafeFileNames[i]);
                        comboTileset.SelectedItem = dialog.SafeFileNames[i];

                        this.Tileset_Loaded?.Invoke(this, new TilesetLoadedEventArgs(path));
                    }
                }
            }
        }

        private void buttonRemoveTileset_Click(object sender, EventArgs e)
        {
            if (_currentMap.TilesetExists(this.comboTileset.SelectedItem.ToString()))
            {
                string path = _currentMap.GetTileset(this.comboTileset.SelectedItem.ToString()).Tag.ToString();
                this.comboTileset.Items.Remove(this.comboTileset.SelectedItem);
                this.Tileset_Unloaded?.Invoke(this, new TilesetLoadedEventArgs(path));
            }
        }

        private void DockTilesetTools_Load(object sender, EventArgs e)
        {
            _tilesetTextureLoader = new TextureLoader(tilesetView.GraphicsDevice);

            tilesetView.OnDraw = OnTilesetDraw;

            if (_currentMap == null)
                return;

            var lastSelected = comboTileset.SelectedItem;

            _tilesets.Clear();
            comboTileset.Items.Clear();

            foreach (var tileset in _currentMap.Tilesets)
            {
                string tilesetPath = ((SuiteForm)this.ParentForm).Project.ClientRootDirectory + "/" + tileset.Tag.ToString();
                var tilesetTexture = _tilesetTextureLoader.LoadFromFile(tilesetPath);
                tilesetTexture.Tag = tileset.Tag;

                _tilesets.Add(tilesetTexture);

                comboTileset.Items.Add(Path.GetFileName(tilesetPath));
                comboTileset.SelectedItem = Path.GetFileName(tilesetPath);
            }

            if (lastSelected != null && comboTileset.Items.Contains(lastSelected))
                comboTileset.SelectedItem = lastSelected;
        }

        private void scrollX_ValueChanged(object sender, ScrollValueEventArgs e)
        {
            _camera.Position = new Vector2(e.Value, _camera.Position.Y);
        }

        private void scrollY_ValueChanged(object sender, ScrollValueEventArgs e)
        {
            _camera.Position = new Vector2(_camera.Position.X, e.Value);
        }

        private void comboTileset_SelectedValueChanged(object sender, EventArgs e)
        {
            this.scrollX.Maximum = _tilesets[this.SelectedTilesetIndex].Width;
            this.scrollY.Maximum = _tilesets[this.SelectedTilesetIndex].Height;
        }

        private void tilesetView_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control && e.KeyCode == Keys.A)
            {
                if (this.comboTileset.SelectedIndex < _tilesets.Count)
                {
                    _selectPosition = Vector2.Zero;
                    _selectRectangle = new Rectangle(0, 0, _tilesets[this.comboTileset.SelectedIndex].Width, _tilesets[this.comboTileset.SelectedIndex].Height);

                    e.SuppressKeyPress = true;
                }
               
            }
        }

        public event EventHandler<TilesetLoadedEventArgs> Tileset_Loaded;

        public event EventHandler<TilesetLoadedEventArgs> Tileset_Unloaded;

        public class TilesetLoadedEventArgs
        {
            private string _tilesetPath;

            public string TilesetPath => _tilesetPath;

            public TilesetLoadedEventArgs(string tilesetPath)
            {
                _tilesetPath = tilesetPath;
            }
        }

    
    }
}
