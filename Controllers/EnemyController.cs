using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using lda.Models;

namespace lda.Controllers
{
    public class EnemyController
    {
        private readonly EnemyModel _model;
        private readonly PlayerModel _playerModel;
        private readonly List<Rectangle> _colliders;

        private const float DetectionRange = 250f;

        public EnemyController(EnemyModel model, PlayerModel playerModel, List<Rectangle> colliders)
        {
            _model = model;
            _playerModel = playerModel;
            _colliders = colliders;
        }

        public void Update()
        {
            if (!_model.IsAlive) return;

            float distToPlayer = _playerModel.Position.X - _model.Position.X;
            float absDist = Math.Abs(distToPlayer);
            float heightDiff = Math.Abs(_playerModel.Position.Y - _model.Position.Y);

            if (absDist < DetectionRange && heightDiff < 100 && _playerModel.IsAlive)
            {
                if (distToPlayer > 0) _model.Direction = 1;
                else _model.Direction = -1;

                _model.SetVelocityX(_model.Direction * 3.5f);
            }
            else
            {
                _model.SetVelocityX(_model.Direction * 2f);
            }

            if (_model.IsGrounded && !IsGroundAhead())
            {
                _model.Direction *= -1;
            }
        }

        private bool IsGroundAhead()
        {
            if (_colliders == null) return true;

            int checkX = _model.Direction == 1 ? _model.Bounds.Right + 4 : _model.Bounds.Left - 4;
            int checkY = _model.Bounds.Bottom + 4;

            Rectangle probe = new Rectangle(checkX, checkY, 4, 4);

            foreach (var tile in _colliders)
            {
                if (probe.Intersects(tile))
                    return true;
            }

            return false;
        }
    }
}