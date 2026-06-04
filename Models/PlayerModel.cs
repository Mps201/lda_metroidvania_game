using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace lda.Models
{
    public enum AttackDirection { Left, Right, Up, Down }

    public class PlayerModel
    {
        public event Action OnHealthChanged;
        public event Action OnPlayerDeath;
        public event Action OnDamageTaken;

        private Vector2 _position;
        private Vector2 _velocity = Vector2.Zero;

        public Vector2 Position => _position;
        public Vector2 Velocity => _velocity;
        
        public int Health { get; private set; } = 3;
        public bool IsAlive => Health > 0;
        public bool IsGrounded { get; private set; }
        public bool IsAttacking { get; private set; }
        public AttackDirection AttackDir { get; private set; }
        public Rectangle Bounds => new Rectangle((int)Math.Round(_position.X), (int)Math.Round(_position.Y), 32, 32);
        public Rectangle? AttackHitbox { get; private set; }
        public Vector2 RespawnPoint { get; set; } = new Vector2(100, 400);

        private List<Rectangle> _colliders;
        private float _attackTimer, _attackCooldown;
        private bool _isInvincible;
        private float _invincibilityTimer;
        private int _jumpsLeft = 2;
        private float _jumpBufferCounter;

        private const float Gravity = 0.5f, MoveSpeed = 5f, JumpForce = -12.5f, Friction = 0.8f;
        private const float JumpCutoff = 0.55f, AttackDuration = 0.12f, AttackCooldownTime = 0.35f;
        private const float InvincibilityDuration = 2.0f, JumpBufferDuration = 0.15f;

        public bool IsInvincible => _isInvincible;

        public PlayerModel(Vector2 startPos, List<Rectangle> colliders)
        {
            _position = startPos;
            _colliders = colliders;
        }

        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (IsAttacking) { _attackTimer -= delta; if (_attackTimer <= 0) { IsAttacking = false; AttackHitbox = null; } }
            if (_attackCooldown > 0) _attackCooldown -= delta;
            if (_isInvincible) { _invincibilityTimer -= delta; if (_invincibilityTimer <= 0) _isInvincible = false; }
            _jumpBufferCounter -= delta;

            _velocity.Y += Gravity;
            if (_velocity.Y > 15f) _velocity.Y = 15f;

            _position.X += _velocity.X;
            ResolveCollisions(true);
            _position.Y += _velocity.Y;
            ResolveCollisions(false);

            if (IsGrounded)
            {
                _position.Y = (float)Math.Round(_position.Y);
                if (_jumpsLeft < 2) _jumpsLeft = 2;
            }

            if (IsAttacking) UpdateAttackHitbox();
        }

        // 🔹 Методы для изменения Velocity
        public void SetVelocityX(float x) => _velocity.X = x;
        public void SetVelocityY(float y) => _velocity.Y = y;
        public void SetVelocity(Vector2 v) => _velocity = v;
        public void ApplyFriction(float factor) { _velocity.X *= factor; if (Math.Abs(_velocity.X) < 0.1f) _velocity.X = 0; }

        public void SetPositionX(float x) => _position.X = x;
        public void SetPositionY(float y) => _position.Y = y;
        public void SetPosition(Vector2 v) => _position = v;

        public void SetMoveDirection(float dir) => SetVelocityX(dir * MoveSpeed);
        public void Idle() => ApplyFriction(Friction);
        
        public void RequestJump() => _jumpBufferCounter = JumpBufferDuration;
        
        public void ExecuteJump()
        {
            if (_jumpBufferCounter > 0 && _jumpsLeft > 0)
            {
                SetVelocityY(JumpForce);
                _jumpsLeft--;
                _jumpBufferCounter = 0;
            }
        }
        
        public void CutJump() { if (_velocity.Y < 0) SetVelocityY(_velocity.Y * JumpCutoff); }

        public void Attack(AttackDirection dir)
        {
            if (!IsAttacking && _attackCooldown <= 0)
            {
                AttackDir = dir;
                IsAttacking = true;
                _attackTimer = AttackDuration;
                _attackCooldown = AttackCooldownTime;
                UpdateAttackHitbox();
            }
        }

        public void TakeDamage()
        {
            if (!_isInvincible && IsAlive)
            {
                Health--;
                _isInvincible = true;
                _invincibilityTimer = InvincibilityDuration;

                OnDamageTaken?.Invoke();
                OnHealthChanged?.Invoke();
                
                if (Health <= 0)
                {
                    OnPlayerDeath?.Invoke();
                }
            }
        }

        public void Reset()
        {
            SetPosition(RespawnPoint);
            SetVelocity(Vector2.Zero);
            Health = 3;
            _isInvincible = false;
            IsAttacking = false;
            AttackHitbox = null;
            _jumpsLeft = 2;
        }

        private void ResolveCollisions(bool isXAxis)
        {
            if (_colliders == null) return;
            if (!isXAxis) IsGrounded = false;

            foreach (var tile in _colliders)
            {
                if (Bounds.Intersects(tile))
                {
                    if (isXAxis)
                    {
                        if (_velocity.X > 0) SetPositionX(tile.Left - 32);
                        else if (_velocity.X < 0) SetPositionX(tile.Right);
                        SetVelocityX(0);
                    }
                    else
                    {
                        if (_velocity.Y > 0) 
                        { 
                            SetPositionY(_position.Y - (Bounds.Bottom - tile.Top)); 
                            SetVelocityY(0); 
                            IsGrounded = true; 
                        }
                        else if (_velocity.Y < 0) 
                        { 
                            SetPositionY(_position.Y + (tile.Bottom - Bounds.Top)); 
                            SetVelocityY(0); 
                        }
                    }
                }
            }
        }

        private void UpdateAttackHitbox()
        {
            int x = (int)Math.Round(_position.X), y = (int)Math.Round(_position.Y);
            AttackHitbox = AttackDir switch
            {
                AttackDirection.Left => new Rectangle(x - 32, y + 8, 32, 16),
                AttackDirection.Right => new Rectangle(x + 32, y + 8, 32, 16),
                AttackDirection.Up => new Rectangle(x + 4, y - 32, 24, 32),
                AttackDirection.Down => new Rectangle(x + 4, y + 32, 24, 32),
                _ => AttackHitbox
            };
        }

        public void SetWorldColliders(List<Rectangle> colliders)
        {
            _colliders = colliders;
        }
    }
}