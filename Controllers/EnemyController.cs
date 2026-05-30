using Microsoft.Xna.Framework;
using lda.Models;
using System;

namespace lda.Controllers
{
    public class EnemyController
    {
        private readonly EnemyModel _model;
        private readonly PlayerModel _playerModel;

        private const float DetectionRange = 200f; 

        public EnemyController(EnemyModel model, PlayerModel playerModel)
        {
            _model = model;
            _playerModel = playerModel;
        }

        public void Update()
        {
            if (!_model.IsAlive) return;

            float distToPlayer = _playerModel.Position.X - _model.Position.X;
            float absDist = Math.Abs(distToPlayer);

            float heightDiff = Math.Abs(_playerModel.Position.Y - _model.Position.Y);

            if (absDist < DetectionRange && heightDiff < 100 && _playerModel.IsAlive)
            {
                if (distToPlayer > 0) 
                    _model.Direction = 1;
                else 
                    _model.Direction = -1;

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
            int checkX = _model.Direction == 1 ? _model.Bounds.Right + 5 : _model.Bounds.Left - 5;
            Rectangle probe = new Rectangle(checkX, _model.Bounds.Bottom + 2, 10, 10);

            return true;
        }
    }
}