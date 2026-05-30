using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using lda.Core;

namespace lda.Entities
{
    public class Enemy : GameObject
    {
        public bool IsAlive { get; private set; } = true;
        public int Health { get; private set; } = 2;
        
        private const float PatrolSpeed = 2f;
        private const float Gravity = 0.5f;
        
        private int _direction = 1;
        private bool _isGrounded;
        private List<Rectangle> _colliders;
        private bool _isHit;
        private float _hitTimer;

        public Enemy(Vector2 position, Texture2D texture) : base(position, texture) { }

        public void SetWorldColliders(List<Rectangle> colliders)
        {
            _colliders = colliders;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsAlive) return;
            
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_isHit)
            {
                _hitTimer -= delta;
                if (_hitTimer <= 0) _isHit = false;
            }
            
            _isGrounded = false;

            Velocity.Y += Gravity;
            if (Velocity.Y > 15f) Velocity.Y = 15f;

            Velocity.X = PatrolSpeed * _direction;

            Position.X += Velocity.X;
            ResolveCollisions(true);

            Position.Y += Velocity.Y;
            ResolveCollisions(false);

            CheckPlatformEdge();

            if (_isGrounded)
                Position.Y = (float)Math.Round(Position.Y);
        }

        public void TakeDamage(int damage)
        {
            if (!IsAlive || _isHit) return;
            
            Health -= damage;
            _isHit = true;
            _hitTimer = 0.2f;

            Velocity.Y = -3f;
            Velocity.X = -_direction * 2f;
            
            if (Health <= 0)
                Die();
        }

        private void ResolveCollisions(bool isXAxis)
        {
            if (_colliders == null) return;
            
            foreach (var tile in _colliders)
            {
                if (Bounds.Intersects(tile))
                {
                    if (isXAxis)
                    {
                        _direction *= -1;
                        if (_direction == 1)
                            Position.X = tile.Left - Texture.Width;
                        else
                            Position.X = tile.Right;
                        Velocity.X = 0;
                    }
                    else
                    {
                        if (Velocity.Y > 0)
                        {
                            Position.Y = tile.Top - Texture.Height;
                            Velocity.Y = 0;
                            _isGrounded = true;
                        }
                        else if (Velocity.Y < 0)
                        {
                            Position.Y = tile.Bottom;
                            Velocity.Y = 0;
                        }
                    }
                }
            }
        }

        private void CheckPlatformEdge()
        {
            if (!_isGrounded || _colliders == null) return;
            
            int checkX = _direction == 1 ? Bounds.Right + 4 : Bounds.Left - 4;
            int checkY = Bounds.Bottom + 2;
            Rectangle probe = new Rectangle(checkX, checkY, 4, 4);
            
            bool groundAhead = false;
            foreach (var tile in _colliders)
            {
                if (probe.Intersects(tile))
                {
                    groundAhead = true;
                    break;
                }
            }
            
            if (!groundAhead)
                _direction *= -1;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsAlive) return;
            
            Vector2 drawPos = new Vector2(
                (int)Math.Round(Position.X),
                (int)Math.Round(Position.Y));
            
            Color color = _isHit ? Color.Pink : Color.Red;
            spriteBatch.Draw(Texture, drawPos, color);
        }

        public void Die()
        {
            IsAlive = false;
        }
    }
}