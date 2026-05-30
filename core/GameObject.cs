using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace lda.Core
{
    public abstract class GameObject
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Texture2D Texture;

        public Rectangle Bounds => new Rectangle(
            (int)Math.Round(Position.X), 
            (int)Math.Round(Position.Y), 
            Texture.Width, 
            Texture.Height);

        protected GameObject(Vector2 position, Texture2D texture)
        {
            Position = position;
            Texture = texture;
            Velocity = Vector2.Zero;
        }

        public virtual void Update(GameTime gameTime)
        {
            Position += Velocity;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (Texture != null)
            {
                Vector2 drawPos = new Vector2(
                    (int)Math.Round(Position.X), 
                    (int)Math.Round(Position.Y));
                spriteBatch.Draw(Texture, drawPos, Color.White);
            }
        }
    }
}