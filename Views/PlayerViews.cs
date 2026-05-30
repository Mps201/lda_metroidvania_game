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
        private bool _isInvincible;

        public PlayerView(PlayerModel model, Texture2D texture)
        {
            _model = model;
            _texture = texture;
            
            _model.OnDamageTaken += () => _isInvincible = true;
            _model.OnHealthChanged += OnHealthChangedHandler;
        }

        private void OnHealthChangedHandler()
        {
            if (_model.Health > 0) 
                _isInvincible = true;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (_texture == null) return;

            Vector2 pos = new Vector2(
                (int)Math.Round(_model.Position.X), 
                (int)Math.Round(_model.Position.Y));

            if (_isInvincible && (int)(gameTime.TotalGameTime.TotalMilliseconds / 100) % 2 != 0)
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

        public void UpdateInvincibility(float delta) { }
    }
}