using lda.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace lda.Views
{
    public class PlayerView
    {
        private readonly PlayerModel _model;
        private readonly Texture2D _texture;

        public PlayerView(PlayerModel model, Texture2D texture)
        {
            _model = model;
            _texture = texture;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (_texture == null) return;

            Vector2 pos = new Vector2(
                (int)Math.Round(_model.Position.X), 
                (int)Math.Round(_model.Position.Y));

            if (_model.IsInvincible && (int)(gameTime.TotalGameTime.TotalMilliseconds / 100) % 2 != 0)
                return;

            spriteBatch.Draw(_texture, pos, Color.White);

            if (_model.IsAttacking && _model.AttackHitbox.HasValue)
            {
                Color c = _model.AttackDir switch
                {
                    AttackDirection.Up => Color.Cyan,
                    AttackDirection.Down => Color.Purple,
                    _ => Color.Yellow
                };
                spriteBatch.Draw(_texture, _model.AttackHitbox.Value, c * 0.7f);
            }
        }
    }
}