using lda.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace lda.Views
{
    public class EnemyView
    {
        private readonly EnemyModel _model;
        private readonly Texture2D _texture;

        public EnemyView(EnemyModel model, Texture2D texture)
        {
            _model = model;
            _texture = texture;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_model.IsAlive) return;

            Vector2 pos = new Vector2(
                (int)_model.Position.X, 
                (int)_model.Position.Y);

            Color color = _model.Health < 2 ? Color.DarkRed : Color.Red;
            
            spriteBatch.Draw(_texture, pos, color);
        }
    }
}