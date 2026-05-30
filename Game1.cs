using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using System.Linq;
using lda.Core;
using lda.Entities;
using lda.Levels;
using lda.Controllers;
using lda.Views;
using lda.Views.UI;
using lda.Models;

namespace lda;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    private InputHandler _inputHandler;
    private List<GameObject> _gameObjects;

    private PlayerModel _playerModel;
    private PlayerController _playerController;
    private PlayerView _playerView;

    private Camera _camera;
    private TileMap _tileMap;
    private Texture2D _checkpointInactiveTex;
    private Texture2D _checkpointActiveTex;
    private List<Checkpoint> _checkpoints;
    
    private Texture2D _playerTexture;
    private Texture2D _tileTexture;

    private List<EnemyModel> _enemyModels;
    private List<EnemyController> _enemyControllers;
    private List<EnemyView> _enemyViews;

    private HealthBarView _healthBar;
    private Texture2D _heartTexture;

    public Game1()
    {
        System.Console.Error.WriteLine("🔧 [Game1] Constructor called");
        
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 800;
        _graphics.PreferredBackBufferHeight = 600;
        _graphics.IsFullScreen = false;
    }

    protected override void Initialize()
    {
        Console.WriteLine(">>> [Game1] Initialize started...");

        _graphics.ApplyChanges();
        _inputHandler = new InputHandler();
        _gameObjects = new List<GameObject>();

        int[,] levelData = new int[25, 33];

        for (int x = 0; x < 33; x++)
            levelData[18, x] = 1;

        levelData[14, 5] = 1;
        levelData[14, 6] = 1;
        levelData[14, 7] = 1;
        
        levelData[10, 20] = 1;
        levelData[10, 21] = 1;
        levelData[10, 22] = 1;
        levelData[10, 23] = 1;
        
        levelData[16, 25] = 1;
        levelData[16, 26] = 1;
        levelData[16, 27] = 1;
        
        _tileMap = new TileMap(levelData, 32);

        int levelWidth = 33 * 32;
        int levelHeight = 25 * 32;
        _camera = new Camera(levelWidth, levelHeight, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        
        Console.WriteLine(">>> [Game1] Initialize finished.");
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _playerTexture = new Texture2D(GraphicsDevice, 32, 32);
        _playerTexture.SetData(Enumerable.Repeat(Color.White, 32 * 32).ToArray());

        _tileTexture = new Texture2D(GraphicsDevice, 32, 32);
        _tileTexture.SetData(Enumerable.Repeat(Color.Green, 32 * 32).ToArray());

        _checkpointInactiveTex = new Texture2D(GraphicsDevice, 32, 32);
        _checkpointInactiveTex.SetData(Enumerable.Repeat(Color.White, 32 * 32).ToArray());

        _checkpointActiveTex = new Texture2D(GraphicsDevice, 32, 32);
        _checkpointActiveTex.SetData(Enumerable.Repeat(Color.White, 32 * 32).ToArray());

        _playerModel = new PlayerModel(new Vector2(100, 400), _tileMap.SolidTiles);
        _playerController = new PlayerController(_playerModel, _inputHandler);
        _playerView = new PlayerView(_playerModel, _playerTexture);

       _enemyModels = new List<EnemyModel>();
        _enemyControllers = new List<EnemyController>();
        _enemyViews = new List<EnemyView>();

        AddEnemy(new Vector2(400, 500));
        AddEnemy(new Vector2(600, 544));

        _checkpoints = new List<Checkpoint>();
        var cp1 = new Checkpoint(new Vector2(100, 544), _checkpointInactiveTex, _checkpointActiveTex); // На полу (ряд 18 = 576px, чуть выше)
        _checkpoints.Add(cp1);
        
        var cp2 = new Checkpoint(new Vector2(800, 544), _checkpointInactiveTex, _checkpointActiveTex);
        _checkpoints.Add(cp2);

        _heartTexture = new Texture2D(GraphicsDevice, 1, 1);
        _heartTexture.SetData(new Color[] { Color.White });

        _healthBar = new HealthBarView(_playerModel, _heartTexture);
    }

    private void AddEnemy(Vector2 position)
    {
        var model = new EnemyModel(position, _tileMap.SolidTiles);
        var controller = new EnemyController(model, _playerModel);
        var view = new EnemyView(model, _playerTexture);

        _enemyModels.Add(model);
        _enemyControllers.Add(controller);
        _enemyViews.Add(view);
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        _inputHandler.Update();

        _playerController.Update();
        _playerModel.Update(gameTime);

        if (_playerModel.Position.Y > 800 || !_playerModel.IsAlive)
        {
            _playerModel.Reset();
        }

        foreach (var obj in _gameObjects)
            obj.Update(gameTime);

        if (_playerModel.IsAlive)
        {
            _camera.Update(_playerModel.Position, GetCameraOffset());
        }

        for (int i = 0; i < _enemyModels.Count; i++)
        {
            if (!_enemyModels[i].IsAlive) continue;

            _enemyControllers[i].Update();
            _enemyModels[i].Update();

            if (_playerModel.AttackHitbox.HasValue && _playerModel.AttackHitbox.Value.Intersects(_enemyModels[i].Bounds))
            {
                _enemyModels[i].TakeDamage();
            }

            if (_enemyModels[i].Bounds.Intersects(_playerModel.Bounds))
            {
                _playerModel.TakeDamage();
                float dir = _playerModel.Position.X < _enemyModels[i].Position.X ? -1 : 1;
                _playerModel.SetVelocity(new Vector2(dir * 5, -4));
            }
        }

        foreach (var cp in _checkpoints)
        {
            cp.Update(_playerModel);
        }
        
        base.Update(gameTime);
    }

    private Vector2 GetCameraOffset()
    {
        Vector2 offset = Vector2.Zero;
        if (_inputHandler.IsKeyDown(Keys.W) || _inputHandler.IsKeyDown(Keys.Up))
            offset.Y -= 140f;
        else if (_inputHandler.IsKeyDown(Keys.S) || _inputHandler.IsKeyDown(Keys.Down))
            offset.Y += 140f;
        return offset;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        _spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix());
        
        _tileMap.Draw(_spriteBatch, _tileTexture);

        _playerView.Draw(_spriteBatch, gameTime);

        foreach (var view in _enemyViews)
            view.Draw(_spriteBatch);

        foreach (var obj in _gameObjects)
            obj.Draw(_spriteBatch);

        foreach (var cp in _checkpoints)
            cp.Draw(_spriteBatch);
        
        _spriteBatch.End();
        _spriteBatch.Begin();
        _healthBar.Draw(_spriteBatch);

        _spriteBatch.End();
        
        base.Draw(gameTime);
    }
}