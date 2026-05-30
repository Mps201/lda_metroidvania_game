using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace lda.Models
{
    public class EnemyModel
    {
        private Vector2 _position;
        private Vector2 _velocity = Vector2.Zero;
        private bool _isGrounded;

        public Vector2 Position => _position;
        public bool IsAlive { get; private set; } = true;
        public int Health { get; private set; } = 2;
        public Rectangle Bounds => new Rectangle((int)_position.X, (int)_position.Y, 32, 32);

        public bool IsGrounded => _isGrounded;

        private const float Gravity = 0.5f;
        private const float PatrolSpeed = 2f;
        private List<Rectangle> _colliders;

        public int Direction { get; set; } = 1;

        public EnemyModel(Vector2 startPos, List<Rectangle> colliders)
        {
            _position = startPos;
            _colliders = colliders;
        }

        public void Update()
        {
            if (!IsAlive) return;

            _velocity.Y += Gravity;
            if (_velocity.Y > 15f) _velocity.Y = 15f;

            _position.X += _velocity.X;
            ResolveCollisions(true);
            _position.Y += _velocity.Y;
            ResolveCollisions(false);

            if (_isGrounded) _position.Y = (float)Math.Round(_position.Y);
        }

        public void TakeDamage()
        {
            Health--;
            if (Health <= 0) IsAlive = false;
        }

        public void StopMovement() => _velocity.X = 0;
        public void SetVelocityX(float x) => _velocity.X = x;

        private void ResolveCollisions(bool isXAxis)
        {
            if (_colliders == null) return;
            if (!isXAxis) _isGrounded = false;

            foreach (var tile in _colliders)
            {
                if (Bounds.Intersects(tile))
                {
                    if (isXAxis)
                    {
                        if (_velocity.X > 0) _position.X = tile.Left - 32;
                        else if (_velocity.X < 0) _position.X = tile.Right;
                        _velocity.X = 0;
                        Direction *= -1; 
                    }
                    else
                    {
                        if (_velocity.Y > 0)
                        {
                            _position.Y -= (Bounds.Bottom - tile.Top);
                            _velocity.Y = 0;
                            _isGrounded = true;
                        }
                        else if (_velocity.Y < 0)
                        {
                            _position.Y += (tile.Bottom - Bounds.Top);
                            _velocity.Y = 0;
                        }
                    }
                }
            }
        }
    }
}