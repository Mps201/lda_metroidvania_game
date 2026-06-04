using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace lda.Models
{
    public class LevelConfig
    {
        public string Id { get; set; } = "default";
        public int[,] TileData { get; set; }
        public Vector2 PlayerSpawn { get; set; }
        public List<Vector2> EnemySpawns { get; set; } = new();
        public List<ExitZone> Exits { get; set; } = new();
    }

    public class ExitZone
    {
        public Rectangle Bounds { get; set; }
        public string TargetLevelId { get; set; }
        public Vector2 TargetSpawn { get; set; }
    }
}