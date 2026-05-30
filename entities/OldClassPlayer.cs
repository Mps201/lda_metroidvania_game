using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using lda.Core;

namespace lda.Entities
{
    public enum AttackDirection { Left, Right, Up, Down }

    public class Player : GameObject
    {
        public Vector2 RespawnPoint { get; set; } = new Vector2(100, 400);

        private AttackDirection _attackDir;
        private bool _isAttacking;
        private float _attackTimer;
        private float _attackCooldown;
        private const float AttackDuration = 0.12f;
        private const float AttackCooldownTime = 0.35f;
        private Rectangle _attackHitbox;
        private bool _facingRight = true;

        public int Health { get; private set; } = 3;
        public bool IsAlive => Health > 0;
        private bool _isInvincible;
        private float _invincibilityTimer;
        private const float InvincibilityDuration = 2.0f;

        private const float Gravity = 0.5f;
        private const float MoveSpeed = 5f;
        private const float JumpForce = -12.5f;
        private const float Friction = 0.8f;

        private const float JumpCutoffMultiplier = 0.55f;

        private int _jumpsLeft;
        private const int MaxJumps = 2;

        private const float CameraLookOffset = 140f;

        private bool _isGrounded;
        private List<Rectangle> _worldColliders;
        
        private InputHandler _input;

        public Player(Vector2 position, Texture2D texture, InputHandler input) 
            : base(position, texture)
        {
            _input = input;
            Velocity = Vector2.Zero;
            _jumpsLeft = MaxJumps;
        }

        public void SetWorldColliders(List<Rectangle> colliders) => _worldColliders = colliders;

        public override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_isAttacking)
            {
                _attackTimer -= delta;
                if (_attackTimer <= 0) { _isAttacking = false; _attackHitbox = Rectangle.Empty; }
            }
            if (_attackCooldown > 0) _attackCooldown -= delta;
            if (_isInvincible)
            {
                _invincibilityTimer -= delta;
                if (_invincibilityTimer <= 0) _isInvincible = false;
            }
            
            HandleInput();

            Velocity.Y += Gravity;
            if (Velocity.Y > 15f) Velocity.Y = 15f;
            
            Position.X += Velocity.X;
            CheckCollisions(true);
            
            Position.Y += Velocity.Y;
            CheckCollisions(false);
            
            if (_isGrounded)
            {
                Position.Y = (float)Math.Round(Position.Y);
                if (_jumpsLeft < MaxJumps) _jumpsLeft = MaxJumps;
            }
            
            if (_isAttacking) UpdateAttackHitbox();
        }

        private void HandleInput()
        {
            if (_input.IsKeyDown(Keys.A) || _input.IsKeyDown(Keys.Left))
            { Velocity.X = -MoveSpeed; _facingRight = false; }
            else if (_input.IsKeyDown(Keys.D) || _input.IsKeyDown(Keys.Right))
            { Velocity.X = MoveSpeed; _facingRight = true; }
            else
            {
                Velocity.X *= Friction;
                if (Math.Abs(Velocity.X) < 0.1f) Velocity.X = 0;
            }

            if (_input.IsKeyPressed(Keys.J) && !_isAttacking && _attackCooldown <= 0)
            {
                if (_input.IsKeyDown(Keys.W) || _input.IsKeyDown(Keys.Up)) _attackDir = AttackDirection.Up;
                else if (_input.IsKeyDown(Keys.S) || _input.IsKeyDown(Keys.Down)) _attackDir = AttackDirection.Down;
                else _attackDir = _facingRight ? AttackDirection.Right : AttackDirection.Left;
                StartAttack();
            }

            if (_input.IsKeyPressed(Keys.Space) && _jumpsLeft > 0)
            {
                Velocity.Y = JumpForce;
                _jumpsLeft--;
            }

            if (_input.IsKeyReleased(Keys.Space) && Velocity.Y < 0)
            {
                Velocity.Y *= JumpCutoffMultiplier;
            }
        }

        public Vector2 GetCameraOffset()
        {
            Vector2 offset = Vector2.Zero;
            if (_input.IsKeyDown(Keys.W) || _input.IsKeyDown(Keys.Up))
                offset.Y -= CameraLookOffset;
            else if (_input.IsKeyDown(Keys.S) || _input.IsKeyDown(Keys.Down))
                offset.Y += CameraLookOffset;
            return offset;
        }

        private void StartAttack()
        {
            _isAttacking = true;
            _attackTimer = AttackDuration;
            _attackCooldown = AttackCooldownTime;
            UpdateAttackHitbox();
        }

        private void UpdateAttackHitbox()
        {
            int x = (int)Math.Round(Position.X);
            int y = (int)Math.Round(Position.Y);
            int w = Texture.Width;
            int h = Texture.Height;
            
            _attackHitbox = _attackDir switch
            {
                AttackDirection.Left => new Rectangle(x - 32, y + 8, 32, 16),
                AttackDirection.Right => new Rectangle(x + w, y + 8, 32, 16),
                AttackDirection.Up => new Rectangle(x + 4, y - 32, 24, 32),
                AttackDirection.Down => new Rectangle(x + 4, y + h, 24, 32),
                _ => _attackHitbox
            };
        }

        private void CheckCollisions(bool isXAxis)
        {
            if (_worldColliders == null) return;
            if (!isXAxis) _isGrounded = false;
            
            foreach (var tile in _worldColliders)
            {
                if (Bounds.Intersects(tile))
                {
                    if (isXAxis)
                    {
                        if (Velocity.X > 0) Position.X = tile.Left - Texture.Width;
                        else if (Velocity.X < 0) Position.X = tile.Right;
                        Velocity.X = 0;
                    }
                    else
                    {
                        if (Velocity.Y > 0)
                        {
                            Position.Y -= (Bounds.Bottom - tile.Top);
                            Velocity.Y = 0;
                            _isGrounded = true;
                        }
                        else if (Velocity.Y < 0)
                        {
                            Position.Y += (tile.Bottom - Bounds.Top);
                            Velocity.Y = 0;
                        }
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Texture == null) return;
            if (_isInvincible && (int)(DateTime.Now.Ticks / 1000) % 2 == 0) return;
            
            spriteBatch.Draw(Texture, new Vector2((int)Math.Round(Position.X), (int)Math.Round(Position.Y)), Color.White);
            
            if (_isAttacking)
            {
                Color c = _attackDir switch { AttackDirection.Up => Color.Cyan, AttackDirection.Down => Color.Purple, _ => Color.Yellow };
                spriteBatch.Draw(Texture, _attackHitbox, c * 0.7f);
            }
        }

        public void TakeDamage()
        {
            if (_isInvincible) return;
            Health--;
            _isInvincible = true;
            _invincibilityTimer = InvincibilityDuration;
        }

        public Rectangle? GetAttackHitbox() => _isAttacking ? _attackHitbox : null;
        public bool IsAttacking() => _isAttacking;
        public void ResetHealth()
        {
            Health = 3;
            _isInvincible = false;

            Position = RespawnPoint; 
            Velocity = Vector2.Zero;
        }
    }
}