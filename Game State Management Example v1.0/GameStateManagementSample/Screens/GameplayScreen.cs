#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;

// Include the necessary SunBurn namespaces.
using SynapseGaming.LightingSystem.Core;
using SynapseGaming.LightingSystem.Editor;
using SynapseGaming.LightingSystem.Effects;
using SynapseGaming.LightingSystem.Lights;
using SynapseGaming.LightingSystem.Rendering;
using SynapseGaming.LightingSystem.Rendering.Forward;
#endregion

namespace GameStateManagementSample
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields
        
        ContentManager content;
        SpriteFont gameFont;

        // Scene constants that control the number of world units visible
        // in the view and the number of units wide the world is.
        const float viewWidth = 2.0f;
        const int worldSize = 10;

        float pauseAlpha;

        InputAction pauseAction;

        // Scene and camera objects.
        BaseRenderableEffect groundMaterial;
        BaseRenderableEffect pipesMaterial;
        BaseRenderableEffect playerMaterial;
        SpriteContainer staticSceneSprites;
        SpriteContainer playerSprites;
        float playerRotation = 0.0f;
        Vector2 playerPosition = new Vector2();
        float playerCurrentAnimationIndex;
        int playerIdleAnimationIndex = 4;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            pauseAction = new InputAction(
                new Buttons[] { Buttons.Start, Buttons.Back },
                new Keys[] { Keys.Escape },
                true);
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
            {
                if (content == null)
                    content = new ContentManager(ScreenManager.Game.Services, "Content");

                gameFont = content.Load<SpriteFont>("gamefont");

                groundMaterial = content.Load<BaseRenderableEffect>("Materials/Forward/tile_floor");
                pipesMaterial = content.Load<BaseRenderableEffect>("Materials/Forward/pipes");
                playerMaterial = content.Load<BaseRenderableEffect>("Materials/Forward/dude_sheet");

                // First create and submit the empty player container.
                playerSprites = ScreenManager.spriteManager.CreateSpriteContainer();
                ScreenManager.sceneInterface.ObjectManager.Submit(playerSprites);

                // Next create the static scenery container.
                staticSceneSprites = ScreenManager.spriteManager.CreateSpriteContainer();

                // Build the static scenery during content load instead of every frame.
                // This accelerates sprite rendering as only dynamic sprites need to be
                // built each frame.
                //
                // Note: this example could just as easily use a single large sprite, but
                // it's illustrating a tile-based background that can contain different
                // materials per-tile.

                int center = worldSize / 2;
                staticSceneSprites.Begin();

                for (int x = 0; x < worldSize; x++)
                {
                    for (int y = 0; y < worldSize; y++)
                    {
                        // Note: the ground layer depth is set to 1, the furthest from the camera (0 is
                        // closest similar to XNA SpriteBatch).
                        staticSceneSprites.Add(groundMaterial, Vector2.One, new Vector2(x - center, y - center), 1.0f);

                        // Note: the pipe layer depth is above the ground and player depth to properly
                        // z-sort AND to add height for shadow casting.
                        staticSceneSprites.Add(pipesMaterial, Vector2.One, new Vector2(x - center, y - center), 0.75f);
                    }
                }

                staticSceneSprites.End();

                // Finally submit the static scenery container.
                ScreenManager.sceneInterface.ObjectManager.Submit(staticSceneSprites);

                // Load the content repository, which stores all assets imported via the editor.
                // This must be loaded before any other assets.
                ScreenManager.contentRepository = content.Load<ContentRepository>("Content");

                // Add objects and lights to the ObjectManager and LightManager. They accept
                // objects and lights in several forms:
                //
                //   -As scenes containing both dynamic (movable) and static objects and lights.
                //
                //   -As SceneObjects and lights, which can be dynamic or static, and
                //    (in the case of objects) are created from XNA Models or custom vertex / index buffers.
                //
                //   -As XNA Models, which can only be static.
                //

                // Load the scene and add it to the managers.
                Scene scene = content.Load<Scene>("Scenes/Scene");

                ScreenManager.sceneInterface.Submit(scene);
                
                // Load the scene environment settings.
                ScreenManager.environment = content.Load<SceneEnvironment>("Environment/Environment");
                
                // TODO: use this.Content to load your game content here

                // A real game would probably have more content than this sample, so
                // it would take longer to load. We simulate that by delaying for a
                // while, giving you a chance to admire the beautiful loading screen.
                Thread.Sleep(1000);

                // once the load has finished, we use ResetElapsedTime to tell the game's
                // timing mechanism that we have just finished a very long frame, and that
                // it should not try to catch up.
                ScreenManager.Game.ResetElapsedTime();
            }
        }

        public override void Deactivate()
        {
            ScreenManager.sceneInterface.Unload();
            ScreenManager.sunBurnCoreSystem.Unload();

            ScreenManager.environment = null;

            base.Deactivate();
        }
        
        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void Unload()
        {
            content.Unload();

            ScreenManager.sceneInterface.Unload();
            ScreenManager.sunBurnCoreSystem.Unload();

            ScreenManager.environment = null;
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                // TODO: this game isn't very fun! You could probably improve
                // it by inserting something more interesting in this space :-)
            }

            // Update all contained managers.
            ScreenManager.sceneInterface.Update(gameTime);
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // Get the time scale since the last update call.
            float timeframe = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float amount = 0.0f;
            Vector2 movedirection = new Vector2();

            if (gamePadState.IsConnected)
            {
                // Get the controller direction.
                movedirection.X = -gamePadState.ThumbSticks.Left.X;
                movedirection.Y = gamePadState.ThumbSticks.Left.Y;

                // Get the controller magnitude.
                amount = movedirection.Length();
            }
            else
            {
                // No gamepad, so grab the keyboard state.
                KeyboardState keyboard = Keyboard.GetState();

                // Get the keyboard direction.
                if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up))
                    movedirection.Y += 1.0f;
                if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
                    movedirection.Y -= 1.0f;
                if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
                    movedirection.X += 1.0f;
                if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
                    movedirection.X -= 1.0f;

                if (movedirection != Vector2.Zero)
                {
                    // Normalize direction to 1.0 magnitude to avoid walking faster at angles.
                    movedirection.Normalize();
                    amount = 1.0f;
                }
            }

            // Increment animation unless idle.
            if (amount == 0.0f)
                playerCurrentAnimationIndex = playerIdleAnimationIndex;
            else
            {
                playerCurrentAnimationIndex += amount * timeframe * 20.0f;
                playerCurrentAnimationIndex = playerCurrentAnimationIndex % 16;

                // Rotate the player towards the controller direction.
                playerRotation = (float)(Math.Atan2(movedirection.Y, movedirection.X) + Math.PI / 2.0);

                // Move player based on the controller direction and time scale.
                playerPosition += movedirection * timeframe;
            }

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            PlayerIndex player;
            if (pauseAction.Evaluate(input, ControllingPlayer, out player) || gamePadDisconnected)
            {
#if WINDOWS_PHONE
                ScreenManager.AddScreen(new PhonePauseScreen(), ControllingPlayer);
#else
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
#endif
            }
            else
            {
                
            }
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Build the dynamic player sprites.
            playerSprites.Begin();

            // Get animation uv offset in the player sprite sheet.
            int animationindex = (int)playerCurrentAnimationIndex;
            float u = (float)(animationindex % 4) * 0.25f;
            float v = (float)(animationindex / 4) * 0.25f;

            // Note: the player layer depth is slightly above the ground depth to properly
            // z-sort AND to add height for shadow casting.
            playerSprites.Add(playerMaterial, Vector2.One * 0.5f, playerPosition, playerRotation, new Vector2(0.25f), new Vector2(u, v), 0.96f);
            playerSprites.End();

            // Render the scene.
            //ScreenManager.sceneState.BeginFrameRendering(view, projection, gameTime, ScreenManager.environment, ScreenManager.frameBuffers, true);
            ScreenManager.sceneState.BeginFrameRendering(playerPosition, viewWidth, ScreenManager.GraphicsDevice.Viewport.AspectRatio, gameTime, ScreenManager.environment, ScreenManager.frameBuffers, true);
            ScreenManager.sceneInterface.BeginFrameRendering(ScreenManager.sceneState);

            // Add custom rendering that should occur before the scene is rendered.

            ScreenManager.sceneInterface.RenderManager.Render();

            // Add custom rendering that should occur after the scene is rendered.

            ScreenManager.sceneInterface.EndFrameRendering();
            ScreenManager.sceneState.EndFrameRendering();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }


        #endregion
    }
}
