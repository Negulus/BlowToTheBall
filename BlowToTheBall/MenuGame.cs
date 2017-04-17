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
	public class MenuGame
	{
		Game1 game1;

		//Кнопки
		Button Button_Resume;
		Button Button_Next;
		Button Button_MenuMain;
		Button Button_Restart;

		//Текстуры
		Texture2D Texture_Pause;
		Texture2D Texture_Win;
		Texture2D Texture_Lose;
		Texture2D Texture_Star_0;
		Texture2D Texture_Star_1;
		Texture2D Texture_Star_2;
		Texture2D Texture_Star_3;
		List<Texture2D> Texture_Digit;

		//Позиции текстур
		Vector2 Position_BG;
		Vector2 Position_Stars;
		Vector2 Position_Score;
		Vector2 Position_Record;

		//Перемещение таблички
		Matrix Translation;
		Vector3 TranslationVector;
		float Position_Start;
		bool Enable;
		long Timer_Start;
		int Time_Raise;

		//Системные переменные
		TouchLocationState Touch_State;
		Vector2 Touch_Position;
		Vector2 Position_tmp;
		int i;
		int j;
		int tmpi;
		int Score_Count;

		//Таймеры
		Timer Timer_MenuGame;

		//Звуки
		SoundEffect Sound_Win;
		SoundEffect Sound_Lose;

		public MenuGame(Game1 game)
		{
			game1 = game;
	
			//Текстуры фонов
			Texture_Pause = game1.Content.Load<Texture2D>("images/menugame/pause_bg");
			Texture_Win = game1.Content.Load<Texture2D>("images/menugame/win_bg");
			Texture_Lose = game1.Content.Load<Texture2D>("images/menugame/lose_bg");

			//Текстуры кнопок
			Button_Resume.texture = game1.Content.Load<Texture2D>("images/menugame/button_resume");
			Button_Next.texture = game1.Content.Load<Texture2D>("images/menugame/button_next");
			Button_MenuMain.texture = game1.Content.Load<Texture2D>("images/menugame/button_main");
			Button_Restart.texture = game1.Content.Load<Texture2D>("images/menugame/button_restart");

			//Текстуры звёзд и очков
			Texture_Star_0 = game1.Content.Load<Texture2D>("images/menugame/stars_0");
			Texture_Star_1 = game1.Content.Load<Texture2D>("images/menugame/stars_1");
			Texture_Star_2 = game1.Content.Load<Texture2D>("images/menugame/stars_2");
			Texture_Star_3 = game1.Content.Load<Texture2D>("images/menugame/stars_3");
			Texture_Digit = new List<Texture2D>();
			for (i = 0; i < 10; i++)
				Texture_Digit.Add(game1.Content.Load<Texture2D>("images/menugame/digit_" + i.ToString()));

			//Позиции текстур
			Position_BG.X = game1.Center.X - 145;
			Position_BG.Y = 120;
			Position_Stars.X = game1.Center.X - 104;
			Position_Stars.Y = Position_BG.Y + 78;
			Position_Score.X = game1.Center.X - 84;
			Position_Score.Y = Position_BG.Y + 189;
			Position_Record.X = game1.Center.X + 12;
			Position_Record.Y = Position_Score.Y;

			//Перемещение текстур
			TranslationVector = Vector3.Zero;
			Translation = Matrix.CreateTranslation(TranslationVector);
			Time_Raise = 250;

			//Звуки
			Sound_Win = game1.Content.Load<SoundEffect>("audio/win");
			Sound_Lose = game1.Content.Load<SoundEffect>("audio/lose");

			Reset();
		}

		public void Pause()
		{
			Reset();
			//Позиции кнопок
			Button_Resume.position.X = game1.Center.X - Button_Resume.texture.Width / 2;
			Button_Resume.position.Y = Position_BG.Y + 92;
			Button_Restart.position.X = game1.Center.X - Button_Restart.texture.Width / 2;
			Button_Restart.position.Y = Button_Resume.position.Y + 60;
			Button_MenuMain.position.X = game1.Center.X - Button_MenuMain.texture.Width / 2;
			Button_MenuMain.position.Y = Button_Restart.position.Y + 60;

			//Перемещение текстур
			Position_Start = -Position_BG.Y - Texture_Pause.Height;
			TranslationVector.Y = Position_Start;
			Translation = Matrix.CreateTranslation(TranslationVector);
			Time_Raise = 200;
		}

		public void Win(int score_count)
		{
			Reset();

			Score_Count = score_count;

			//Позиции кнопок
			Button_Next.position.X = game1.Center.X - Button_Next.texture.Width / 2;
			Button_Next.position.Y = Position_BG.Y + 248;
			Button_Restart.position.X = game1.Center.X - Button_Restart.texture.Width / 2;
			Button_Restart.position.Y = Button_Next.position.Y + 60;
			Button_MenuMain.position.X = game1.Center.X - Button_MenuMain.texture.Width / 2;
			Button_MenuMain.position.Y = Button_Restart.position.Y + 60;

			//Перемещение текстур
			Position_Start = -Position_BG.Y - Texture_Win.Height;
			TranslationVector.Y = Position_Start;
			Translation = Matrix.CreateTranslation(TranslationVector);
			Time_Raise = 500;

			if (game1.Sound)
				Sound_Win.Play();
		}

		public void Lose()
		{
			Reset();
			//Позиции кнопок
			Button_Restart.position.X = game1.Center.X - Button_Restart.texture.Width / 2;
			Button_Restart.position.Y = Position_BG.Y + 92;
			Button_MenuMain.position.X = game1.Center.X - Button_MenuMain.texture.Width / 2;
			Button_MenuMain.position.Y = Button_Restart.position.Y + 60;

			//Перемещение текстур
			Position_Start = -Position_BG.Y - Texture_Lose.Height;
			TranslationVector.Y = Position_Start;
			Translation = Matrix.CreateTranslation(TranslationVector);
			Time_Raise = 500;

			if (game1.Sound)
				Sound_Lose.Play();
		}

		public void Reset()
		{
			Button_Next.touch = false;
			Button_Restart.touch = false;
			Button_MenuMain.touch = false;
			Button_Resume.touch = false;
			Timer_MenuGame.Action = Actions.Null;
			Timer_Start = -1;
			Enable = false;
		}

		public void Update(GameTime gameTime)
		{
			if (game1.gamePadState.Buttons.Back == ButtonState.Pressed)
			{
				switch (game1.Stat_Curent)
				{
					case GameStat.Pause:
						game1.Stat_Next = GameStat.Resume;
						break;
					case GameStat.Lose:
						game1.Stat_Next = GameStat.MenuMain;
						break;
					case GameStat.Win:
						game1.Stat_Next = GameStat.MenuMain;
						break;
				}
			}

			if (Enable)
			{
				foreach (TouchLocation tl in game1.touchState)
				{
					Touch_State = tl.State;
					Touch_Position = tl.Position;

					if (Touch_State == TouchLocationState.Pressed)
					{
						switch (game1.Stat_Curent)
						{
							case GameStat.Pause:
								if (Button_Restart.touched(Touch_Position, game1))
									Timer_MenuGame.Set(gameTime.TotalGameTime, Actions.Restart, 250);
								else if (Button_MenuMain.touched(Touch_Position, game1))
									Timer_MenuGame.Set(gameTime.TotalGameTime, Actions.MainMenu, 250);
								else if (Button_Resume.touched(Touch_Position, game1))
									Timer_MenuGame.Set(gameTime.TotalGameTime, Actions.Resume, 250);
								break;
							case GameStat.Lose:
								if (Button_Restart.touched(Touch_Position, game1))
									Timer_MenuGame.Set(gameTime.TotalGameTime, Actions.Restart, 250);
								else if (Button_MenuMain.touched(Touch_Position, game1))
									Timer_MenuGame.Set(gameTime.TotalGameTime, Actions.MainMenu, 250);
								break;
							case GameStat.Win:
								if ((game1.Level + 1) < game1.Level_Count)
								{
									if (game1.Level_Data[game1.Level + 1].Enable)
									{
										if (Button_Next.touched(Touch_Position, game1))
											Timer_MenuGame.Set(gameTime.TotalGameTime, Actions.Next, 250);
									}
								}
								if (Button_Restart.touched(Touch_Position, game1))
									Timer_MenuGame.Set(gameTime.TotalGameTime, Actions.Restart, 250);
								else if (Button_MenuMain.touched(Touch_Position, game1))
									Timer_MenuGame.Set(gameTime.TotalGameTime, Actions.MainMenu, 250);
								break;
						}
					}
					break;
				}
			}
			else if (Timer_Start < 0)
			{
				Timer_Start = (long)gameTime.TotalGameTime.TotalMilliseconds;
			}
			else
			{
				TranslationVector.Y = Position_Start - ((float)((long)gameTime.TotalGameTime.TotalMilliseconds - Timer_Start) / Time_Raise) * Position_Start;
				if (TranslationVector.Y >= 0)
				{
					TranslationVector.Y = 0;
					Enable = true;
				}
				Translation = Matrix.CreateTranslation(TranslationVector);
			}

			if (Timer_MenuGame.Get(gameTime.TotalGameTime))
			{
				switch (Timer_MenuGame.Action)
				{
					case Actions.Restart:
						game1.Stat_Next = GameStat.Start;
						break;
					case Actions.MainMenu:
						game1.Stat_Next = GameStat.MenuMain;
						break;
					case Actions.Resume:
						game1.Stat_Next = GameStat.Resume;
						break;
					case Actions.Next:
						game1.Stat_Next = GameStat.Next;
						break;
					case Actions.MenuGameEnable:
						Enable = true;
						break;
				}
				Timer_MenuGame.Action = Actions.Null;
			}
		}

		void DrawScore()
		{
			Position_tmp = Position_Record;
			i = (int)System.Decimal.Divide(Score_Count, 100);
			game1.spriteBatch.Draw(Texture_Digit[i], Position_tmp, Color.White);

			Position_tmp.X += 24;
			j = (int)System.Decimal.Divide((Score_Count - (i * 100)), 10);
			game1.spriteBatch.Draw(Texture_Digit[j], Position_tmp, Color.White);

			Position_tmp.X += 24;
			tmpi = Score_Count - (i * 100) - (j * 10);
			game1.spriteBatch.Draw(Texture_Digit[tmpi], Position_tmp, Color.White);

			Position_tmp = Position_Score;
			i = (int)System.Decimal.Divide(game1.Score, 100);
			game1.spriteBatch.Draw(Texture_Digit[i], Position_tmp, Color.White);

			Position_tmp.X += 24;
			j = (int)System.Decimal.Divide((game1.Score - (i * 100)), 10);
			game1.spriteBatch.Draw(Texture_Digit[j], Position_tmp, Color.White);

			Position_tmp.X += 24;
			tmpi = game1.Score - (i * 100) - (j * 10);
			game1.spriteBatch.Draw(Texture_Digit[tmpi], Position_tmp, Color.White);
		}

		public void Draw()
		{
			game1.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Translation);

			switch (game1.Stat_Curent)
			{
				case GameStat.Pause:
					game1.spriteBatch.Draw(Texture_Pause, Position_BG, Color.White);
					Button_MenuMain.draw(game1.spriteBatch);
					Button_Restart.draw(game1.spriteBatch);
					Button_Resume.draw(game1.spriteBatch);
					break;
				case GameStat.Lose:
					game1.spriteBatch.Draw(Texture_Lose, Position_BG, Color.White);
					Button_MenuMain.draw(game1.spriteBatch);
					Button_Restart.draw(game1.spriteBatch);
					break;
				case GameStat.Win:
					game1.spriteBatch.Draw(Texture_Win, Position_BG, Color.White);
					Button_MenuMain.draw(game1.spriteBatch);
					Button_Restart.draw(game1.spriteBatch);
					if ((game1.Level + 1) < game1.Level_Count)
					{
						if (game1.Level_Data[game1.Level + 1].Enable)
						{
							Button_Next.draw(game1.spriteBatch);
						}
						else
						{
							game1.spriteBatch.Draw(Button_Next.texture, Button_Next.position, new Color(0.5f, 0.5f, 0.5f, 1f));
						}
					}
					else
					{
						game1.spriteBatch.Draw(Button_Next.texture, Button_Next.position, new Color(0.5f, 0.5f, 0.5f, 1f));
					}
					if (game1.Stars == 0)
						game1.spriteBatch.Draw(Texture_Star_0, Position_Stars, Color.White);
					else if (game1.Stars == 1)
						game1.spriteBatch.Draw(Texture_Star_1, Position_Stars, Color.White);
					else if (game1.Stars == 2)
						game1.spriteBatch.Draw(Texture_Star_2, Position_Stars, Color.White);
					else
						game1.spriteBatch.Draw(Texture_Star_3, Position_Stars, Color.White);
					DrawScore();
					break;
			}

			game1.spriteBatch.End();
		}
	}
}