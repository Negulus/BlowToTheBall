using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.IO;

namespace BlowToTheBall
{
	public class GamePlay
	{
		Game1 game1;

		//Структуры для игры
		struct ObjBall
		{
			public Texture2D Texture;
			public Vector2 Position;
			public Vector2 Speed;
		}
		struct ObjZone
		{
			public Texture2D Texture;
			public Vector2 Position;
		}
		struct HelpPlate
		{
			public Vector2 Position;
			public Texture2D Texture;
			public bool Enable;
			public float Opacity;
			int tmpi;
			int TimeLife;
			int TimeRaise;
			int TimeFade;
			bool Hold;
			public bool Once;
			TimeSpan StartTime;

			public void Start(TimeSpan time, int life, bool hold, bool once)
			{
				if (!Once)
				{
					StartTime = time;
					TimeLife = life;
					TimeRaise = 500;
					TimeFade = 500;
					Hold = hold;
					Opacity = 0f;
					Enable = true;
					Once = true;
				}
			}

			public void Stop(TimeSpan time)
			{
				if (Hold)
				{
					StartTime = time;
					TimeRaise = 0;
					TimeLife = TimeFade;
					Hold = false;
				}
			}

			public void Update(TimeSpan time)
			{
				if (Enable)
				{
					if (!Hold)
					{
						tmpi = (int)time.Subtract(StartTime).TotalMilliseconds;
						if (tmpi > (TimeLife - TimeFade))
						{
							Opacity = (float)(TimeLife - tmpi) / (float)TimeFade;
							if (Opacity < 0.01f)
							{
								Opacity = 0;
								Enable = false;
							}
						}
						else if (tmpi < TimeRaise)
						{
							Opacity = (float)tmpi / (float)TimeRaise;
							if (Opacity > 1)
								Opacity = 1;
						}
						else
						{
							Opacity = 1;
						}
					}
					else
						Opacity = 1;
				}
			}
		}

		//Игровые объекты
		ObjBall Ball;
		ObjZone Zone;
		public GameLevel Game_Level;
		public MenuGame Menu_Game;

		//Игровые текстуры
		Texture2D Texture_Vent;
		Texture2D Texture_Post_G;
		Texture2D Texture_Post_Y;
		Texture2D Texture_Post_R;
		Texture2D Texture_Arrow;
		Texture2D Texture_Arrow_BG;
		Texture2D Texture_Graph;
		Texture2D Texture_Graph_Off;
		Texture2D Texture_Star;

		//Текстуры очков
		Texture2D Texture_Record;
		Texture2D Texture_Score;
		List<Texture2D> Texture_Digit;

		//Текстуры таймера
		Texture2D Texture_Timer;
		List<Texture2D> Texture_Timer_Digit;

		//Точки игры и положения текстур
		Vector2 Position_Record;
		Vector2 Position_Score;
		Vector2 Position_Vent;
		Vector2 Position_Post;
		Vector2 Position_Arrow;
		Vector2 Position_Graph_L;
		Vector2 Position_Graph_R;
		Vector2 Position_Start;
		Vector2 Position_Game;
		Vector2 Position_Timer;
		Vector2 Position_tmp;

		//Экраны графиков и стрелок
		Viewport DefaultViewport;
		Viewport GraphViewportAmplitude;
		Viewport GraphViewportWind;
		Viewport GraphViewportArrow;

		//Таблички помощи
		HelpPlate Help_Plate_1;
		HelpPlate Help_Plate_2;
		HelpPlate Help_Plate_3;

		//Таймер возврата
		Texture2D Texture_Return_1;
		Texture2D Texture_Return_2;
		Texture2D Texture_Return_3;
		Vector2 Position_Return;
		int Return_Timer;
		public int Return_Time;
		public bool Return_En;

		//Таймеры
		Timer Timer_Play;
		Timer Timer_Sound_Fan;

		//Звуки
		SoundEffect Sound_Star;
		SoundEffect Sound_Fan_Raise;
		SoundEffect Sound_Fan;
		public SoundEffectInstance Sound_Fan_Raise_Inst;
		public SoundEffectInstance Sound_Fan_Inst;
		bool Sound_Fan_Run;
		float Sound_Fan_Vol;
		TimeSpan Sound_Fan_Start;

		//Системные переменные
		public TimeSpan GameStartTime;
		public TimeSpan GamePauseTime;
		int Time_Left;
		bool Draw_Win;
		bool Game_Wait;
		bool Game_Begin;
		bool Game_Start;
		bool Game_Run;
		bool Game_End;
		int i;
		int j;
		int tmpi;
		long tmpl;

		public GamePlay(Game1 game)
		{
			game1 = game;

			//Инициализация игровых текстур
			Texture_Vent = game1.Content.Load<Texture2D>("images/game/vent");
			Texture_Post_G = game1.Content.Load<Texture2D>("images/game/post_green");
			Texture_Post_Y = game1.Content.Load<Texture2D>("images/game/post_yellow");
			Texture_Post_R = game1.Content.Load<Texture2D>("images/game/post_red");
			Texture_Arrow = game1.Content.Load<Texture2D>("images/game/arrow");
			Texture_Arrow_BG = game1.Content.Load<Texture2D>("images/game/arrow_bg");
			Texture_Star = game1.Content.Load<Texture2D>("images/game/star");
			Texture_Graph = game1.Content.Load<Texture2D>("images/game/graph");
			Texture_Graph_Off = game1.Content.Load<Texture2D>("images/game/graph_off");

			//Загрузка текстур счётчика очков
			Texture_Record = game1.Content.Load<Texture2D>("images/game/record"); ;
			Texture_Score = game1.Content.Load<Texture2D>("images/game/score"); ;
			Texture_Digit = new List<Texture2D>();
			for (i = 0; i < 10; i++)
				Texture_Digit.Add(game1.Content.Load<Texture2D>("images/game/score_" + i.ToString()));

			//Загрузка текстур таймера
			Texture_Timer = game1.Content.Load<Texture2D>("images/timer/timer_bg"); ;
			Texture_Timer_Digit = new List<Texture2D>();
			for (i = 0; i < 10; i++)
				Texture_Timer_Digit.Add(game1.Content.Load<Texture2D>("images/timer/digit_" + i.ToString()));

			//Инициализация текстуры шара
			Ball.Texture = game1.Content.Load<Texture2D>("images/game/ball");

			//Инициализация текстуры зоны игры
			Zone.Texture = game1.Content.Load<Texture2D>("images/game/zone");

			//Инициалзация Главных точек игры
			Position_Vent.X = game1.Center.X - 199;
			Position_Vent.Y = game1.graphics.PreferredBackBufferHeight - Texture_Vent.Height;
			Position_Post = Position_Vent;
			Position_Arrow.X = game1.Center.X - Texture_Arrow.Width / 2;
			Position_Arrow.Y = 90;
			Position_Start.X = game1.Center.X;
			Position_Start.Y = game1.graphics.PreferredBackBufferHeight - 239;
			Position_Game.X = game1.Center.X;
			Position_Game.Y = 130 + Zone.Texture.Height / 2;
			Position_Graph_L.X = 30;
			Position_Graph_L.Y = game1.graphics.PreferredBackBufferHeight - 260;
			Position_Graph_R.X = game1.graphics.PreferredBackBufferWidth - 30 - Texture_Graph.Width;
			Position_Graph_R.Y = Position_Graph_L.Y;
			Position_Record.X = 5;
			Position_Record.Y = 10;
			Position_Score.X = game1.graphics.PreferredBackBufferWidth - 190;
			Position_Score.Y = 10;
			Position_Timer.X = Position_Graph_L.X + 4;
			Position_Timer.Y = Position_Vent.Y + 62;

			//Инициализация шара
			Ball.Position = Position_Start;
			Ball.Speed = Vector2.Zero;

			//Инициализация зоны игры
			Zone.Position = Position_Game;
			Zone.Position.X -= Zone.Texture.Width / 2;
			Zone.Position.Y -= Zone.Texture.Height / 2;

			//Инициализация дополнительных экранов
			DefaultViewport = game1.GraphicsDevice.Viewport;
			GraphViewportAmplitude = DefaultViewport;
			GraphViewportAmplitude.Width = 100;
			GraphViewportAmplitude.Height = 200;
			GraphViewportAmplitude.X = (int)Position_Graph_R.X + 4;
			GraphViewportAmplitude.Y = (int)Position_Graph_R.Y + 4;
			GraphViewportWind = DefaultViewport;
			GraphViewportWind.Width = 100;
			GraphViewportWind.Height = 200;
			GraphViewportWind.X = (int)Position_Graph_L.X + 4;
			GraphViewportWind.Y = (int)Position_Graph_L.Y + 4;
			GraphViewportArrow = DefaultViewport;

			//Загрузка уровня
			Game_Level = new GameLevel(game1, game1.Level, Position_Game);

			//Загрузка табличек помощи
			if (game1.Level == 0)
			{
				Help_Plate_1.Texture = game1.Content.Load<Texture2D>("images/help/tapscreen");
				Help_Plate_1.Position.X = Position_Game.X + 15;
				Help_Plate_1.Position.Y = Position_Game.Y + 160;

				Help_Plate_2.Texture = game1.Content.Load<Texture2D>("images/help/zone");
				Help_Plate_2.Position.X = Position_Game.X - 99;
				Help_Plate_2.Position.Y = Position_Game.Y - 50;

				Help_Plate_3.Texture = game1.Content.Load<Texture2D>("images/help/timer");
				Help_Plate_3.Position.X = 15;
				Help_Plate_3.Position.Y = Position_Graph_L.Y - 38;
			}

			if (game1.Level == 1)
			{
				Help_Plate_1.Texture = game1.Content.Load<Texture2D>("images/help/stars");
				Help_Plate_1.Position.X = Position_Game.X - 99;
				Help_Plate_1.Position.Y = Position_Game.Y - 50;
			}

			if (game1.Level == 3)
			{
				Help_Plate_1.Texture = game1.Content.Load<Texture2D>("images/help/rotate");
				Help_Plate_1.Position.X = Position_Game.X - 99;
				Help_Plate_1.Position.Y = Position_Game.Y - 50;
			}

			if (game1.Level == 6)
			{
				Help_Plate_1.Texture = game1.Content.Load<Texture2D>("images/help/graph_wind");
				Help_Plate_1.Position.X = Position_Graph_L.X - 16;
				Help_Plate_1.Position.Y = Position_Graph_L.Y + 13 - Help_Plate_1.Texture.Height;

				Help_Plate_2.Texture = game1.Content.Load<Texture2D>("images/help/wind");
				Help_Plate_2.Position.X = game1.Center.X + 25;
				Help_Plate_2.Position.Y = Position_Arrow.Y + Texture_Arrow.Height - 8;
			}

			if (game1.Level == 10)
			{
				Help_Plate_1.Texture = game1.Content.Load<Texture2D>("images/help/graph_fan");
				Help_Plate_1.Position.X = Position_Graph_R.X - 100;
				Help_Plate_1.Position.Y = Position_Graph_R.Y + 13 - Help_Plate_1.Texture.Height;
			}

			//Инициализация игрового меню
			Menu_Game = new MenuGame(game1);

			//Таймер возврата
			Texture_Return_1 = game1.Content.Load<Texture2D>("images/game/return_1");
			Texture_Return_2 = game1.Content.Load<Texture2D>("images/game/return_2");
			Texture_Return_3 = game1.Content.Load<Texture2D>("images/game/return_3");
			Position_Return = Position_Game;
			Position_Return.X -= Texture_Return_1.Width / 2;
			Position_Return.Y -= Texture_Return_1.Height / 2;
			Return_En = false;
			Return_Time = 0;
			Return_Timer = 500;

			//Загрузка звуков
			Sound_Star = game1.Content.Load<SoundEffect>("audio/star");
			Sound_Fan = game1.Content.Load<SoundEffect>("audio/fan");
			Sound_Fan_Inst = Sound_Fan.CreateInstance();
			Sound_Fan_Inst.IsLooped = true;
			Sound_Fan_Inst.Volume = 0.75f;
			Sound_Fan_Raise = game1.Content.Load<SoundEffect>("audio/fan_raise");
			Sound_Fan_Raise_Inst = Sound_Fan_Raise.CreateInstance();
			Sound_Fan_Raise_Inst.IsLooped = false;
			Sound_Fan_Raise_Inst.Volume = 0.75f;
			Sound_Fan_Run = false;
			Sound_Fan_Vol = 0;

			//Инициализация и обнуление других переменных
			Game_Wait = false;
			Game_Begin = false;
			Game_Start = false;
			Game_Run = false;
			Game_End = false;
			Draw_Win = false;
			game1.Score = 0;
			Timer_Play.Action = Actions.Null;
			Timer_Sound_Fan.Action = Actions.Null;
			Time_Left = (int)(Game_Level.Time_Life / 1000);
		}

		//Обновление игры
		public void UpdatePlay(GameTime gameTime)
		{
			//Первая загрузка
			if (!Game_Wait)
			{
				if (game1.Sound)
				{
					Sound_Fan_Inst.Volume = Sound_Fan_Vol;
					Sound_Fan_Inst.Play();
					Sound_Fan_Start = gameTime.TotalGameTime;
				}

				Timer_Play.Set(gameTime.TotalGameTime, Actions.Game_Begin, 500);
				Game_Wait = true;

				if (game1.Level == 0)
				{
					Help_Plate_1.Start(gameTime.TotalGameTime, 1000, true, true);
					Help_Plate_2.Start(gameTime.TotalGameTime, 1000, true, true);
					Help_Plate_3.Start(gameTime.TotalGameTime, 1000, true, true);
				}
				else if (game1.Level == 1)
				{
					Help_Plate_1.Start(gameTime.TotalGameTime, 1000, true, true);
				}
				else if (game1.Level == 3)
				{
					Help_Plate_1.Start(gameTime.TotalGameTime, 1000, true, true);
				}
				else if (game1.Level == 6)
				{
					Help_Plate_1.Start(gameTime.TotalGameTime, 1000, true, true);
					Help_Plate_2.Start(gameTime.TotalGameTime, 1000, true, true);
				}
				else if (game1.Level == 10)
				{
					Help_Plate_1.Start(gameTime.TotalGameTime, 1000, true, true);
				}
			}

			//Начало игры после отсчёта таймера
			if (Game_Begin)
			{
				if (game1.touchState.Any())
				{
					Sound_Fan_Inst.Volume = Sound_Fan_Vol + 0.25f;

					//Влияние амплитуды
					if (Game_Level.Amplitude_en)
						Ball.Speed.Y -= 20 * ((float)gameTime.ElapsedGameTime.Milliseconds / 100) + 4 * Game_Level.Amplitude.CurValue;
					else
						Ball.Speed.Y -= 20 * ((float)gameTime.ElapsedGameTime.Milliseconds / 100);

					Game_Start = true;

					//Вывод табличек помощи
					if (game1.Level == 0)
					{
						Help_Plate_1.Stop(gameTime.TotalGameTime);
						Help_Plate_2.Stop(gameTime.TotalGameTime);
						Help_Plate_3.Stop(gameTime.TotalGameTime);
					}
					else if (game1.Level == 1)
					{
						Help_Plate_1.Stop(gameTime.TotalGameTime);
					}
					else if (game1.Level == 3)
					{
						Help_Plate_1.Stop(gameTime.TotalGameTime);
					}
					else if (game1.Level == 6)
					{
						Help_Plate_1.Stop(gameTime.TotalGameTime);
						Help_Plate_2.Stop(gameTime.TotalGameTime);
					}
					else if (game1.Level == 10)
					{
						Help_Plate_1.Stop(gameTime.TotalGameTime);
					}
				}
				else
				{
					Sound_Fan_Inst.Volume = Sound_Fan_Vol;
				}
			}

			//Начало работы системы
			if (Game_Start && !Game_End)
			{
				//Влияние ветра
				if (Game_Level.Wind_en)
					Ball.Speed.X += ((float)gameTime.ElapsedGameTime.Milliseconds / 100) * Game_Level.Wind.CurValue * 3f;

				//Влияние акселлерометра
				if (Game_Level.Accel_en)
				{
					Ball.Speed.X += ((float)gameTime.ElapsedGameTime.Milliseconds / 100) * 9.8f * game1.accelState.CurrentValue.Acceleration.X;
					Ball.Speed.Y += ((float)gameTime.ElapsedGameTime.Milliseconds / 100) * 9.8f * (1 - Math.Abs(game1.accelState.CurrentValue.Acceleration.X));
				}
				else
				{
					Ball.Speed.Y += ((float)gameTime.ElapsedGameTime.Milliseconds / 100) * 9.8f;
				}

				//Изменение скорости
				Ball.Position.X += Ball.Speed.X * ((float)gameTime.ElapsedGameTime.Milliseconds / 100);
				Ball.Position.Y += Ball.Speed.Y * ((float)gameTime.ElapsedGameTime.Milliseconds / 100);

				//Проигрышь при попадании шарика за пределы экрана
				if (Ball.Position.X < 0 || Ball.Position.Y < 0 || Ball.Position.X > game1.graphics.PreferredBackBufferWidth || Ball.Position.Y > game1.graphics.PreferredBackBufferHeight)
				{
					Game_End = true;
					game1.Stat_Next = GameStat.Lose;
				}

				//Расстояние от центра зоны до шарика
				tmpi = (int)Math.Sqrt(Math.Pow(Ball.Position.X - Position_Game.X, 2) + Math.Pow(Ball.Position.Y - Position_Game.Y, 2));

				//Если игра началась
				if (Game_Run)
				{
					//Проигрышь при выходе шарика за пределы зоны
					if (tmpi > (Zone.Texture.Width / 2))
					{
						Game_End = true;
						game1.Stat_Next = GameStat.Lose;
					}

					//Обновление звёздочек
					tmpl = (long)gameTime.TotalGameTime.Subtract(GameStartTime).TotalMilliseconds;
					for (i = 0; i < Game_Level.Star_Count; i++)
					{
						if (Game_Level.Star[i].Update(tmpl, Ball.Position))
						{
							Game_Level.Star[i].Take = true;
							game1.Score += Game_Level.Star[i].Score;
							if (game1.Sound)
								Sound_Star.Play();
						}
					}

					//Обновление уровня, выигрышь по истечении времени уровня
					if (Game_Level.Update(gameTime))
					{
						Game_End = true;
						Draw_Win = true;
						game1.Stat_Next = GameStat.Win;
					}

					//Обновление таймера обратного отсчёта
					Time_Left = (int)(Game_Level.Time_Life / 1000) - (int)gameTime.TotalGameTime.Subtract(GameStartTime).TotalSeconds;
					if (Time_Left < 0)
						Time_Left = 0;
				}
				else
				{
					//Начать игру если шарик внутри зоны
					if (tmpi < (Zone.Texture.Width / 2 - 10))
					{
						Game_Run = true;
						Game_Level.Start(gameTime);
						GameStartTime = gameTime.TotalGameTime;
					}
				}
			}

			//Обновление графиков
			Game_Level.GraphUpdate();

			if (Timer_Play.Get(gameTime.TotalGameTime))
			{
				switch (Timer_Play.Action)
				{
					case Actions.Game_Begin:
						Game_Begin = true;
						break;
					case Actions.Help_1:
						Help_Plate_1.Start(gameTime.TotalGameTime, 3000, false, true);
						break;
					case Actions.Help_2:
						Help_Plate_2.Start(gameTime.TotalGameTime, 3000, false, true);
						break;
				}
				Timer_Play.Action = Actions.Null;
			}

			//Обновление табличек помощи
			if (game1.Level == 0 || game1.Level == 1 || game1.Level == 3 || game1.Level == 6 || game1.Level == 10)
				Help_Plate_1.Update(gameTime.TotalGameTime);
			if (game1.Level == 0 || game1.Level == 6)
				Help_Plate_2.Update(gameTime.TotalGameTime);
			if (game1.Level == 0)
				Help_Plate_3.Update(gameTime.TotalGameTime);

			//Обновление таймеров звуков
			if (game1.Sound)
			{

				if (!Sound_Fan_Run)
				{
					Sound_Fan_Vol = (float)(gameTime.TotalGameTime.Subtract(Sound_Fan_Start).TotalMilliseconds / 1000) * 0.75f;
					if (Sound_Fan_Vol > 0.75f)
					{
						Sound_Fan_Vol = 0.75f;
						Sound_Fan_Run = true;
					}
					else if (Sound_Fan_Vol < 0)
						Sound_Fan_Vol = 0;
					Sound_Fan_Inst.Volume = Sound_Fan_Vol;
				}

				if (Timer_Sound_Fan.Get(gameTime.TotalGameTime))
				{
					switch (Timer_Sound_Fan.Action)
					{
						case Actions.Sound_Fan_Raise:
							Sound_Fan_Raise_Inst.Volume = 0.1f;
							Sound_Fan_Inst.Play();
							break;
					}
					Timer_Sound_Fan.Action = Actions.Null;
				}
			}
		}

		public void Update(GameTime gameTime)
		{
			if (game1.Stat_Curent == GameStat.Play)
			{
				if (game1.gamePadState.Buttons.Back == ButtonState.Pressed)
					game1.Stat_Next = GameStat.Pause;

				if (Return_En)
				{
					if (Timer_Play.Action == Actions.Null)
						Timer_Play.Set(gameTime.TotalGameTime, Actions.Return, Return_Timer);

					if (Timer_Play.Get(gameTime.TotalGameTime))
					{
						switch (Timer_Play.Action)
						{
							case Actions.Return:
								Return_Time--;
								if (Return_Time <= 0)
								{
									Return_En = false;
									GameStartTime = GameStartTime.Add(gameTime.TotalGameTime.Subtract(GamePauseTime));
									Game_Level.Amplitude.LastTime = Game_Level.Amplitude.LastTime.Add(gameTime.TotalGameTime.Subtract(GamePauseTime));
									Game_Level.Wind.LastTime = Game_Level.Wind.LastTime.Add(gameTime.TotalGameTime.Subtract(GamePauseTime));
									Game_Level.Time_Start = GameStartTime;
									Ball.Speed.X *= 0.2f;
									Ball.Speed.Y *= 0.2f;
								}
								break;
						}
						Timer_Play.Action = Actions.Null;
					}
				}
				else
				{
					UpdatePlay(gameTime);
				}
			}
			else
			{
				Menu_Game.Update(gameTime);
			}
		}

		void DrawRecord()
		{
			game1.spriteBatch.Draw(Texture_Record, Position_Record, Color.White);

			Position_tmp = Position_Record;
			Position_tmp.X += Texture_Record.Width;
			i = (int)System.Decimal.Divide(game1.Level_Data[game1.Level].Score, 100);
			game1.spriteBatch.Draw(Texture_Digit[i], Position_tmp, Color.White);

			Position_tmp.X += 24;
			j = (int)System.Decimal.Divide((game1.Level_Data[game1.Level].Score - (i * 100)), 10);
			game1.spriteBatch.Draw(Texture_Digit[j], Position_tmp, Color.White);

			Position_tmp.X += 24;
			tmpi = game1.Level_Data[game1.Level].Score - (i * 100) - (j * 10);
			game1.spriteBatch.Draw(Texture_Digit[tmpi], Position_tmp, Color.White);
		}
		void DrawScore()
		{
			game1.spriteBatch.Draw(Texture_Score, Position_Score, Color.White);

			Position_tmp = Position_Score;
			Position_tmp.X += Texture_Score.Width;
			i = (int)System.Decimal.Divide(game1.Score, 100);
			game1.spriteBatch.Draw(Texture_Digit[i], Position_tmp, Color.White);

			Position_tmp.X += 24;
			j = (int)System.Decimal.Divide((game1.Score - (i * 100)), 10);
			game1.spriteBatch.Draw(Texture_Digit[j], Position_tmp, Color.White);

			Position_tmp.X += 24;
			tmpi = game1.Score - (i * 100) - (j * 10);
			game1.spriteBatch.Draw(Texture_Digit[tmpi], Position_tmp, Color.White);
		}
		void DrawTimer()
		{
			game1.spriteBatch.Draw(Texture_Timer, Position_Timer, Color.White);

			Position_tmp = Position_Timer;
			Position_tmp.X += 17;
			Position_tmp.Y += 6;
			i = (int)System.Decimal.Divide(Time_Left, 100);
			game1.spriteBatch.Draw(Texture_Timer_Digit[i], Position_tmp, Color.White);

			Position_tmp.X += 26;
			j = (int)System.Decimal.Divide((Time_Left - (i * 100)), 10);
			game1.spriteBatch.Draw(Texture_Timer_Digit[j], Position_tmp, Color.White);

			Position_tmp.X += 26;
			tmpi = Time_Left - (i * 100) - (j * 10);
			game1.spriteBatch.Draw(Texture_Timer_Digit[tmpi], Position_tmp, Color.White);
		}
		public void Draw()
		{
			game1.GraphicsDevice.Clear(Color.SeaShell);

			game1.spriteBatch.Begin();
			game1.spriteBatch.Draw(game1.Texture_BG, Vector2.Zero, Color.White);
			game1.spriteBatch.Draw(Texture_Vent, Position_Vent, Color.White);

			if (Game_Wait && !Game_Begin && !Game_End)
				game1.spriteBatch.Draw(Texture_Post_R, Position_Post, Color.White);
			else if (!Game_Run && !Game_End)
				game1.spriteBatch.Draw(Texture_Post_Y, Position_Post, Color.White);
			else if (!Game_End)
				game1.spriteBatch.Draw(Texture_Post_G, Position_Post, Color.White);
			else if (Draw_Win)
			{
				game1.spriteBatch.Draw(Texture_Post_R, Position_Post, Color.White);
				game1.spriteBatch.Draw(Texture_Post_G, Position_Post, Color.White);
				game1.spriteBatch.Draw(Texture_Post_Y, Position_Post, Color.White);
			}
			else
			{
				game1.spriteBatch.Draw(Texture_Post_R, Position_Post, Color.White);
				game1.spriteBatch.Draw(Texture_Post_Y, Position_Post, Color.White);
			}

			DrawRecord();
			DrawScore();
			DrawTimer();

			game1.spriteBatch.Draw(Zone.Texture, Zone.Position, Color.White);
			for (i = 0; i < Game_Level.Star_Count; i++)
			{
				if (Game_Level.Star[i].Enable_Draw)
				{
					game1.spriteBatch.Draw(Texture_Star, Game_Level.Star[i].position, null, new Color(Game_Level.Star[i].Opacity, Game_Level.Star[i].Opacity, Game_Level.Star[i].Opacity, Game_Level.Star[i].Opacity), 0f, Game_Level.Star[i].origin, Game_Level.Star[i].Scale, SpriteEffects.None, 0f);
				}
			}

			if (Game_Level.Wind_en)
			{
				game1.spriteBatch.Draw(Texture_Graph, Position_Graph_L, Color.White);
				game1.spriteBatch.Draw(Texture_Arrow_BG, Position_Arrow, Color.White);
			}
			else
			{
				game1.spriteBatch.Draw(Texture_Graph_Off, Position_Graph_L, Color.White);
			}

			if (Game_Level.Amplitude_en)
			{
				game1.spriteBatch.Draw(Texture_Graph, Position_Graph_R, Color.White);
			}
			else
			{
				game1.spriteBatch.Draw(Texture_Graph_Off, Position_Graph_R, Color.White);
			}	
			game1.spriteBatch.End();


			//Рисование графика
			game1.basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, 101, 201, 0, 0, 1);
			game1.basicEffect.CurrentTechnique.Passes[0].Apply();

			if (Game_Level.Amplitude_en)
			{
				game1.GraphicsDevice.Viewport = GraphViewportAmplitude;
				if (Game_Level.Amplitude.GraphCountUp > 0)
					game1.graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, Game_Level.Amplitude.GraphUp, 0, Game_Level.Amplitude.GraphCountUp);
				if (Game_Level.Amplitude.GraphCountDown > 0)
					game1.graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, Game_Level.Amplitude.GraphDown, 0, Game_Level.Amplitude.GraphCountDown);
			}

			if (Game_Level.Wind_en)
			{
				game1.GraphicsDevice.Viewport = GraphViewportWind;
				if (Game_Level.Wind.GraphCountUp > 0)
					game1.graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, Game_Level.Wind.GraphUp, 0, Game_Level.Wind.GraphCountUp);
				if (Game_Level.Wind.GraphCountDown > 0)
					game1.graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, Game_Level.Wind.GraphDown, 0, Game_Level.Wind.GraphCountDown);

				//Рисование стрелок
				if (Game_Level.Wind.CurValue > 0)
				{
					GraphViewportArrow.X = (int)Position_Arrow.X + 150;
					GraphViewportArrow.Y = (int)Position_Arrow.Y;
					GraphViewportArrow.Width = (int)(Game_Level.Wind.CurValue * 150f);
					GraphViewportArrow.Height = 50;
					if (GraphViewportArrow.Width > 0)
					{
						game1.GraphicsDevice.Viewport = GraphViewportArrow;
						game1.basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, GraphViewportArrow.Width, GraphViewportArrow.Height, 0, 0, 1);
						game1.basicEffect.CurrentTechnique.Passes[0].Apply();
						game1.spriteBatch.Begin();
						game1.spriteBatch.Draw(Texture_Arrow, new Vector2(-150, 0), Color.White);
						game1.spriteBatch.End();
					}
				}
				else if (Game_Level.Wind.CurValue < 0)
				{
					GraphViewportArrow.X = (int)Position_Arrow.X + 150 - (int)(Game_Level.Wind.CurValue * -150f);
					GraphViewportArrow.Y = (int)Position_Arrow.Y;
					GraphViewportArrow.Width = (int)(Game_Level.Wind.CurValue * -150f);
					GraphViewportArrow.Height = 50;
					if (GraphViewportArrow.Width > 0)
					{
						game1.GraphicsDevice.Viewport = GraphViewportArrow;
						game1.basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, GraphViewportArrow.Width, GraphViewportArrow.Height, 0, 0, 1);
						game1.basicEffect.CurrentTechnique.Passes[0].Apply();
						game1.spriteBatch.Begin();
						game1.spriteBatch.Draw(Texture_Arrow, new Vector2(GraphViewportArrow.Width - 150, 0), Color.White);
						game1.spriteBatch.End();
					}
				}
			}

			//Рисование шарика
			game1.GraphicsDevice.Viewport = DefaultViewport;
			game1.basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, game1.graphics.PreferredBackBufferWidth, game1.graphics.PreferredBackBufferHeight, 0, 0, 1);
			game1.basicEffect.CurrentTechnique.Passes[0].Apply();

			Position_tmp = Ball.Position;
			Position_tmp.X -= Ball.Texture.Width / 2;
			Position_tmp.Y -= Ball.Texture.Height / 2;
			game1.spriteBatch.Begin();

			//Рисование табличек помощи
			if (game1.Level == 0 || game1.Level == 1 || game1.Level == 3 || game1.Level == 6 || game1.Level == 10)
			{
				if (Help_Plate_1.Enable)
					game1.spriteBatch.Draw(Help_Plate_1.Texture, Help_Plate_1.Position, new Color(Help_Plate_1.Opacity, Help_Plate_1.Opacity, Help_Plate_1.Opacity, Help_Plate_1.Opacity));
			}
			if (game1.Level == 0 || game1.Level == 6)
			{
				if (Help_Plate_2.Enable)
					game1.spriteBatch.Draw(Help_Plate_2.Texture, Help_Plate_2.Position, new Color(Help_Plate_2.Opacity, Help_Plate_2.Opacity, Help_Plate_2.Opacity, Help_Plate_2.Opacity));
			}
			if (game1.Level == 0)
			{
				if (Help_Plate_3.Enable)
					game1.spriteBatch.Draw(Help_Plate_3.Texture, Help_Plate_3.Position, new Color(Help_Plate_3.Opacity, Help_Plate_3.Opacity, Help_Plate_3.Opacity, Help_Plate_3.Opacity));
			}

			game1.spriteBatch.Draw(Ball.Texture, Position_tmp, Color.White);

			//Таймер возврата
			if (Return_En)
			{
				if (game1.Stat_Curent == GameStat.Play)
				{
					if (Return_Time == 3)
						game1.spriteBatch.Draw(Texture_Return_3, Position_Return, Color.White);
					else if (Return_Time == 2)
						game1.spriteBatch.Draw(Texture_Return_2, Position_Return, Color.White);
					else if (Return_Time == 1)
						game1.spriteBatch.Draw(Texture_Return_1, Position_Return, Color.White);
				}
			}

			game1.spriteBatch.End();

			if (game1.Stat_Curent == GameStat.Pause || game1.Stat_Curent == GameStat.Win || game1.Stat_Curent == GameStat.Lose)
				Menu_Game.Draw();
		}
	}
}