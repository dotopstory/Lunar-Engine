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
using Lunar.Core.Content.Graphics;
using Lunar.Core.Utilities.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lunar.Core.World.Structure;
using Lunar.Graphics;

namespace Lunar.Editor.World
{
    public class Tile
    {
        private TileDescriptor _descriptor;

        public TileDescriptor Descriptor => _descriptor;

        private long _nextAnimationTime;

        public float ZIndex
        {
            get => this.Sprite.LayerDepth;
            set => this.Sprite.LayerDepth = value;
        }

        public Sprite Sprite { get; set; }

        public Tile(TileDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        public Tile(Texture2D texture, Rectangle sourceRectangle, Vector2 position)
            : this()
        {
            this.Sprite = new Sprite(texture)
            {
                SourceRectangle = sourceRectangle,
                Position = position
            };

            _descriptor.SpriteInfo = new SpriteInfo(texture.Tag.ToString());
            _descriptor.Position = new Vector(position.X, position.Y);
            _descriptor.SpriteInfo.Transform = new Transform()
            {
                Rect = new Rect(this.Sprite.SourceRectangle.Left, this.Sprite.SourceRectangle.Top, this.Sprite.SourceRectangle.Width, this.Sprite.SourceRectangle.Height),
                Color = new Core.Content.Graphics.Color(this.Sprite.Color.R, this.Sprite.Color.G, this.Sprite.Color.B, this.Sprite.Color.A),
                LayerDepth = this.Sprite.LayerDepth,
                Position = this.Sprite.Position
            };
        }

        public Tile()
        {
            _descriptor = new TileDescriptor(null);
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            if (this.Sprite != null)
                spriteBatch.Draw(this.Sprite);
        }

        public void Update(GameTime gameTime)
        {
            if (gameTime.TotalGameTime.TotalMilliseconds >= _nextAnimationTime && this.Descriptor.Animated)
            {

                _nextAnimationTime = (long)gameTime.TotalGameTime.TotalMilliseconds + 300;
            }
        }

       
    }
}