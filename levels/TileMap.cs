using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace lda.Levels
{
    public class TileMap
    {
        private int[,] _mapData;
        public int TileSize { get; private set; }

        public List<Rectangle> SolidTiles { get; private set; }

        public TileMap(int[,] mapData, int tileSize)
        {
            _mapData = mapData;
            TileSize = tileSize;
            SolidTiles = new List<Rectangle>();

            GenerateColliders();
        }

        private void GenerateColliders()
        {
            SolidTiles.Clear();
            int rows = _mapData.GetLength(0);
            int cols = _mapData.GetLength(1);

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (_mapData[y, x] == 1)
                    {
                        Rectangle rect = new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize);
                        SolidTiles.Add(rect);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D tileTexture)
        {
            int rows = _mapData.GetLength(0);
            int cols = _mapData.GetLength(1);

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (_mapData[y, x] == 1)
                    {
                        Vector2 pos = new Vector2(x * TileSize, y * TileSize);
                        spriteBatch.Draw(tileTexture, pos, Color.Green);
                    }
                }
            }
        }
    }
}