using lda.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace lda.Views.UI
{
    public class HealthBarView
    {
        private readonly PlayerModel _playerModel;
        private readonly Texture2D _heartFull;
        private readonly Texture2D _heartEmpty;
        
        private Vector2 _position;
        private const int HeartSize = 24;
        private const int Spacing = 4;

        public HealthBarView(PlayerModel model, Texture2D heartTex)
        {
            _playerModel = model;
            _heartFull = heartTex;
            _heartEmpty = heartTex;
            
            _position = new Vector2(20, 20);

            _playerModel.OnHealthChanged += OnHealthChanged;
        }

        private void OnHealthChanged()
        {
            Console.WriteLine($"❤️ Health changed: {_playerModel.Health}");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 pos = _position + new Vector2(i * (HeartSize + Spacing), 0);

                Color color = i < _playerModel.Health ? Color.Red : Color.Gray;
                
                spriteBatch.Draw(_heartFull, new Rectangle((int)pos.X, (int)pos.Y, HeartSize, HeartSize), color);
            }
        }
    }
}