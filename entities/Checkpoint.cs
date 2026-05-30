using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using lda.Core;
using lda.Entities;
using lda.Models;

namespace lda.Entities
{
    public class Checkpoint : GameObject
    {
        private bool _isActive;
        private Texture2D _activeTexture;
        private Texture2D _inactiveTexture;

        public Checkpoint(Vector2 position, Texture2D inactiveTex, Texture2D activeTex)
            : base(position, inactiveTex)
        {
            _inactiveTexture = inactiveTex;
            _activeTexture = activeTex;
            _isActive = false;
        }

        public void Update(PlayerModel player)
        {
            if (_isActive) return;
            if (player.Bounds.Intersects(this.Bounds))
            {
                Activate(player);
            }
        }

        private void Activate(PlayerModel player)
        {
            _isActive = true;
            Texture = _activeTexture;
            player.RespawnPoint = this.Position;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 drawPos = new Vector2(
                (int)Math.Round(Position.X), 
                (int)Math.Round(Position.Y));

            spriteBatch.Draw(Texture, drawPos, _isActive ? Color.Orange : Color.Blue);
        }
    }
}