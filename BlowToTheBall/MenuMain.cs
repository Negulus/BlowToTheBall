using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Audio;

namespace BlowToTheBall
{
	class MenuMain
	{
		Game1 game1;

		//Кнопки
		Button Button_Play;
		Button Button_Settings;
		Button Button_Exit;
		Button Button_Back;
		Button Button_Sound_On;
		Button Button_Sound_Off;
		Button Button_Reset;
		Button Button_Help;

		//Текстуры
		Texture2D Texture_Text_Main;
		Texture2D Texture_Text_Settings;
		Texture2D Texture_Text_Help;
		Texture2D Texture_Help;
		Texture2D Texture_Help_Trial;

		//Системные переменные
		TouchLocationState Touch_State;
		Vector2 Touch_Position;
		Vector2 Position_Help;
		Vector2 Position_Help_Trial;

		//Таймеры
		Timer Timer_Menu;

		public MenuMain(Game1 game)
		{
			game1 = game;

			//Текстуры кнопок
			Button_Play.texture = game1.Content.Load<Texture2D>("images/buttons/play");
			Button_Settings.texture = game1.Content.Load<Texture2D>("images/buttons/settings");
			Button_Exit.texture = game1.Content.Load<Texture2D>("images/buttons/exit");
			Button_Back.texture = game1.Content.Load<Texture2D>("images/buttons/back");
			Button_Sound_On.texture = game1.Content.Load<Texture2D>("images/buttons/soundon");
			Button_Sound_Off.texture = game1.Content.Load<Texture2D>("images/buttons/soundoff");
			Button_Reset.texture = game1.Content.Load<Texture2D>("images/buttons/reset");
			Button_Help.texture = game1.Content.Load<Texture2D>("images/buttons/help");

			//Позиции кнопок
			Button_Play.position.X = game1.Center.X - Button_Play.texture.Width / 2;
			Button_Play.position.Y = game1.Center.Y - Button_Play.texture.Height - 100;
			Button_Settings.position.X = Button_Play.position.X;
			Button_Settings.position.Y = Button_Play.position.Y + 100;
			Button_Help.position.X = Button_Play.position.X;
			Button_Help.position.Y = Button_Play.position.Y + 200;
			Button_Exit.position.X = Button_Play.position.X;
			Button_Exit.position.Y = Button_Play.position.Y + 300;
			Button_Back.position.X = 0;
			Button_Back.position.Y = game1.graphics.PreferredBackBufferHeight - Button_Back.texture.Height;
			Button_Sound_On.position = Button_Play.position;
			Button_Sound_Off.position = Button_Play.position;
			Button_Reset.position = Button_Settings.position;

			Texture_Text_Main = game1.Content.Load<Texture2D>("images/text/blowtoball");
			Texture_Text_Settings = game1.Content.Load<Texture2D>("images/text/settings");
			Texture_Text_Help = game1.Content.Load<Texture2D>("images/text/help");
			Texture_Help = game1.Content.Load<Texture2D>("images/help/help");
			Texture_Help_Trial = game1.Content.Load<Texture2D>("images/help/help_trial");

			Position_Help.X = 30;
			Position_Help.Y = game1.Pos_Plate.Y + 125;

			Position_Help_Trial.X = Position_Help.X;
			Position_Help_Trial.Y = Position_Help.Y + Texture_Help.Height + 60;

			Reset();
		}

		public void Reset()
		{
			Button_Play.touch = false;
			Button_Settings.touch = false;
			Button_Exit.touch = false;
			Button_Back.touch = false;
			Button_Sound_On.touch = false;
			Button_Sound_Off.touch = false;
			Button_Reset.touch = false;
			Button_Help.touch = false;
			Timer_Menu.Action = Actions.Null;
		}

		public void Update(GameTime gameTime)
		{
			if (game1.gamePadState.Buttons.Back == ButtonState.Pressed)
			{
				switch (game1.Stat_Curent)
				{
					case GameStat.MenuMain:
						game1.Exit();
						break;
					case GameStat.MenuSettings:
						game1.Stat_Next = GameStat.MenuMain;
						break;
					case GameStat.Help:
						game1.Stat_Next = GameStat.MenuMain;
						break;
				}
			}

			foreach (TouchLocation tl in game1.touchState)
			{
				Touch_State = tl.State;
				Touch_Position = tl.Position;

				if (Touch_State == TouchLocationState.Pressed)
				{
					switch (game1.Stat_Curent)
					{
						case GameStat.MenuMain:
							if (Button_Play.touched(Touch_Position, game1))
								Timer_Menu.Set(gameTime.TotalGameTime, Actions.Select, 250);
							else if (Button_Settings.touched(Touch_Position, game1))
								Timer_Menu.Set(gameTime.TotalGameTime, Actions.Settings, 250);
							else if (Button_Help.touched(Touch_Position, game1))
								Timer_Menu.Set(gameTime.TotalGameTime, Actions.Help, 250);
							else if (Button_Exit.touched(Touch_Position, game1))
								Timer_Menu.Set(gameTime.TotalGameTime, Actions.Exit, 250);
							break;
						case GameStat.Help:
							if (Button_Back.touched(Touch_Position, game1))
								game1.Stat_Next = GameStat.MenuMain;
							break;
						case GameStat.MenuSettings:
							if (game1.Sound)
							{
								if (Button_Sound_On.touched(Touch_Position, game1))
									Timer_Menu.Set(gameTime.TotalGameTime, Actions.Sound, 250);
							}
							else
							{
								if (Button_Sound_Off.touched(Touch_Position, game1))
									Timer_Menu.Set(gameTime.TotalGameTime, Actions.Sound, 250);
							}
							if (Button_Reset.touched(Touch_Position, game1))
							{
								game1.ResetData();
								Timer_Menu.Set(gameTime.TotalGameTime, Actions.MainMenu, 500);
							}
							else if (Button_Back.touched(Touch_Position, game1))
							{
								game1.Stat_Next = GameStat.MenuMain;
							}
							break;
					}
				}
				break;
			}


			if (Timer_Menu.Get(gameTime.TotalGameTime))
			{
				switch (Timer_Menu.Action)
				{
					case Actions.Select:
						game1.Stat_Next = GameStat.MenuSelect;
						break;
					case Actions.Settings:
						game1.Stat_Next = GameStat.MenuSettings;
						break;
					case Actions.Exit:
						game1.Exit();
						break;
					case Actions.MainMenu:
						game1.Stat_Next = GameStat.MenuMain;
						break;
					case Actions.Sound:
						if (game1.Sound)
							game1.Sound = false;
						else
							game1.Sound = true;
						Button_Sound_Off.touch = false;
						Button_Sound_On.touch = false;
						break;
					case Actions.Help:
						game1.Stat_Next = GameStat.Help;
						break;
				}
				Timer_Menu.Action = Actions.Null;
			}
		}

		public void Draw()
		{
			game1.GraphicsDevice.Clear(Color.White);

			game1.spriteBatch.Begin();
			game1.spriteBatch.Draw(game1.Texture_BG, Vector2.Zero, Color.White);
			game1.spriteBatch.Draw(game1.Texture_Plate, game1.Pos_Plate, Color.White);

			switch (game1.Stat_Curent)
			{
				case GameStat.MenuMain:
					Draw_Main();
					break;
				case GameStat.MenuSettings:
					Draw_Settings();
					break;
				case GameStat.Help:
					Draw_Help();
					break;
			}

			game1.spriteBatch.End();
		}

		void Draw_Main()
		{
			game1.spriteBatch.Draw(Texture_Text_Main, game1.Pos_Plate, Color.White);
			
			Button_Play.draw(game1.spriteBatch);
			Button_Settings.draw(game1.spriteBatch);
			Button_Help.draw(game1.spriteBatch);
			Button_Exit.draw(game1.spriteBatch);
		}

		void Draw_Help()
		{
			game1.spriteBatch.Draw(Texture_Text_Help, game1.Pos_Plate, Color.White);

			game1.spriteBatch.Draw(Texture_Help, Position_Help, Color.White);

			if (game1.Trial)
				game1.spriteBatch.Draw(Texture_Help_Trial, Position_Help_Trial, Color.White);

			Button_Back.draw(game1.spriteBatch);
		}

		void Draw_Settings()
		{
			game1.spriteBatch.Draw(Texture_Text_Settings, game1.Pos_Plate, Color.White);

			if (game1.Sound)
				Button_Sound_On.draw(game1.spriteBatch);
			else
				Button_Sound_Off.draw(game1.spriteBatch);

			Button_Reset.draw(game1.spriteBatch);

			Button_Back.draw(game1.spriteBatch);
		}
	}
}