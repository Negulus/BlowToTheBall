using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Devices.Sensors;
using System.IO.IsolatedStorage;

namespace BlowToTheBall
{
	public struct LevelData
	{
		public Vector2 Position;
		public int Score;
		public int Stars;
		public bool Used;
		public bool Enable;

		public bool touched(Vector2 touch_pos, int width, int height)
		{
			if ((touch_pos.X > Position.X) && (touch_pos.X < (Position.X + width)))
			{
				if ((touch_pos.Y > Position.Y) && (touch_pos.Y < (Position.Y + height)))
				{
					return true;
				}
			}
			return false;
		}
	}
	public struct Button
	{
		public Texture2D texture;
		public Vector2 position;
		public bool touch;

		public bool touched(Vector2 touch_pos, Game1 game1)
		{
			if ((touch_pos.X > position.X) && (touch_pos.X < (position.X + texture.Width)))
			{
				if ((touch_pos.Y > position.Y) && (touch_pos.Y < (position.Y + texture.Height)))
				{
					if (game1.Sound)
						game1.Sound_Click.Play();
					touch = true;
					return true;
				}
			}
			touch = false;
			return false;
		}

		public void draw(SpriteBatch spriteBatch)
		{
			if (touch)
				spriteBatch.Draw(texture, position, new Color(1f, 1f, 1f, 0.75f));
			else
				spriteBatch.Draw(texture, position, new Color(1f, 1f, 1f, 1f));
		}
	}
	public enum Actions
	{
		Select,
		Help,
		Settings,
		Exit,
		Null,
		Win,
		Loose,
		Game_Begin,
		Draw_GameMenu,
		Help_1,
		Help_2,
		Help_3,
		MainMenu,
		GameMenu,
		Next,
		Restart,
		Resume,
		Return,
		Sound,
		MenuGameEnable,
		Sound_Fan_Raise
	}
	public struct Timer
	{
		private TimeSpan Time;
		public Actions Action;
		private int Set_Time;

		public void Set(TimeSpan time, Actions action, int set_time)
		{
			Time = time;
			Action = action;
			Set_Time = set_time;
		}

		public bool Get(TimeSpan time)
		{
			if (Action != Actions.Null)
			{
				if (time.Subtract(Time).TotalMilliseconds > Set_Time)
					return true;
			}
			return false;
		}
	}
	public enum GameStat
	{
		MenuMain,
		MenuSelect,
		MenuSettings,
		Start,
		Play,
		Win,
		Lose,
		Pause,
		Next,
		Null,
		Resume,
		Help
	}

	public class Game1 : Microsoft.Xna.Framework.Game
	{
		//Переменные для графики
		public GraphicsDeviceManager graphics;
		public SpriteBatch spriteBatch;
		public BasicEffect basicEffect;
		public Vector2 Center;
		public Vector2 Pos_Plate;

		//Состояния сенсоров
		public GamePadState gamePadState;
		public TouchCollection touchState;
		public Accelerometer accelState;

		//Состояние игры
		public GameStat Stat_Curent;
		public GameStat Stat_Next;
		public int Level_Count;
		public bool Sound;
		public int Score;
		public int Stars;
		public int Level;

		//Данные уровней
		public LevelData[] Level_Data;

		//Общие текстуры
		public Texture2D Texture_BG;
		public Texture2D Texture_Plate;

		//Общие звуки
		public SoundEffect Sound_Click;

		//Разделы игры
		GamePlay Game_Play;
		MenuMain Menu_Main;
		MenuSelect Menu_Select;

		//Выход по 3 нажатиям back
		int Back_Click;
		TimeSpan Last_Back;

		//Системные переменные
		int i;
		byte[] tmpbytes;
		int firm_id;
		bool tmpb;
		IsolatedStorageFile Save_Game;
		IsolatedStorageFileStream File_Stream;
		public bool Trial;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			TargetElapsedTime = TimeSpan.FromTicks(333333);
			InactiveSleepTime = TimeSpan.FromSeconds(1);
		}

		protected override void Initialize()
		{
			firm_id = 99;

			Sound = false;
			Trial = Guide.IsTrialMode;

			Level = 0;
			Level_Count = 20;
			Level_Data = new LevelData[Level_Count];
			LoadData();
			CheckData();

			Back_Click = 0;

			Stat_Curent = GameStat.Null;
			Stat_Next = GameStat.MenuMain;
			base.Initialize();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);

			Texture_BG = Content.Load<Texture2D>("images/background");
			Texture_Plate = Content.Load<Texture2D>("images/plate");

			graphics.PreferredBackBufferWidth = 480;
			graphics.PreferredBackBufferHeight = 800;
			graphics.SupportedOrientations = DisplayOrientation.Portrait;
			graphics.IsFullScreen = true;
			graphics.ApplyChanges();

			basicEffect = new BasicEffect(graphics.GraphicsDevice);
			basicEffect.VertexColorEnabled = true;
			basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height, 0, 0, 1);
			basicEffect.CurrentTechnique.Passes[0].Apply();

			Center.X = graphics.PreferredBackBufferWidth / 2;
			Center.Y = graphics.PreferredBackBufferHeight / 2;

			Pos_Plate.X = Center.X - Texture_Plate.Width / 2;
			Pos_Plate.Y = 20;

			accelState = new Accelerometer();

			//Загрузка общих звуков
			Sound_Click = Content.Load<SoundEffect>("audio/click");
		}

		protected override void UnloadContent()
		{
			if (accelState.State != SensorState.Disabled)
				accelState.Stop();
		}

		protected override void OnExiting(object sender, System.EventArgs args)
		{
			SaveData();
			base.OnExiting(sender, args);
		}

		void CheckData()
		{
			for (i = 0; i < Level_Count; i++)
			{
				tmpb = true;
				if (i == 0)
				{
					Level_Data[i].Enable = true;
					tmpb = false;
				}
				else if (i < 6)
				{
					if (Level_Data[i - 1].Enable && (Level_Data[i - 1].Stars > 0))
					{
						Level_Data[i].Enable = true;
						tmpb = false;
					}
				}
				else if (!Trial)
				{
					if (Level_Data[i - 2].Enable && Level_Data[i - 1].Enable)
					{
						if (Level_Data[i - 2].Stars > 0)
						{
							Level_Data[i].Enable = true;
							tmpb = false;
						}
						else if (Level_Data[i].Stars > 0)
						{
							Level_Data[i].Enable = true;
							tmpb = false;
						}
					}
				}
				if (tmpb)
					Level_Data[i].Enable = false;
			}
		}

		protected void LoadData()
		{
			using (Save_Game = IsolatedStorageFile.GetUserStoreForApplication())
			{
				if (Save_Game.FileExists("BlowToTheBall_Save"))
				{
					using (File_Stream = Save_Game.OpenFile("BlowToTheBall_Save", System.IO.FileMode.Open))
					{
						if (File_Stream != null)
						{
							tmpbytes = new byte[2];

							//Проверка ID сохранения
							if (File_Stream.Read(tmpbytes, 0, 2) > 0)
							{
								if (System.BitConverter.ToInt16(tmpbytes, 0) != firm_id)
									return;
							}

							//Чтение активности звука
							if (File_Stream.Read(tmpbytes, 0, 1) > 0)
								Sound = System.BitConverter.ToBoolean(tmpbytes, 0);
							else
								Sound = false;

							//Чтение резервных переменных
							File_Stream.Read(tmpbytes, 0, 1);
							File_Stream.Read(tmpbytes, 0, 2);
							File_Stream.Read(tmpbytes, 0, 2);
							File_Stream.Read(tmpbytes, 0, 2);
							File_Stream.Read(tmpbytes, 0, 2);
							File_Stream.Read(tmpbytes, 0, 2);
							File_Stream.Read(tmpbytes, 0, 2);
							File_Stream.Read(tmpbytes, 0, 2);
							File_Stream.Read(tmpbytes, 0, 2);
							File_Stream.Read(tmpbytes, 0, 2);

							for (i = 0; i < Level_Count; i++)
							{
								if (File_Stream.Read(tmpbytes, 0, 1) > 0)
									Level_Data[i].Used = System.BitConverter.ToBoolean(tmpbytes, 0);
								else
									Level_Data[i].Used = false;

								if (File_Stream.Read(tmpbytes, 0, 2) > 0)
									Level_Data[i].Score = System.BitConverter.ToInt16(tmpbytes, 0);
								else
									Level_Data[i].Score = 0;

								if (File_Stream.Read(tmpbytes, 0, 2) > 0)
									Level_Data[i].Stars = System.BitConverter.ToInt16(tmpbytes, 0);
								else
									Level_Data[i].Stars = 0;
							}
						}
					}
				}
			}
		}

		public void ResetData()
		{
			for (i = 0; i < Level_Count; i++)
			{
				Level_Data[i].Used = false;
				Level_Data[i].Score = 0;
				Level_Data[i].Stars = 0;
			}
			SaveData();
		}

		public void SaveData()
		{
			Save_Game = IsolatedStorageFile.GetUserStoreForApplication();
			File_Stream = null;
			using (File_Stream = Save_Game.CreateFile("BlowToTheBall_Save"))
			{
				if (File_Stream != null)
				{
					//Запись ID сохранения
					tmpbytes = System.BitConverter.GetBytes(firm_id);
					File_Stream.Write(tmpbytes, 0, 2);

					//Запись активности звука
					tmpbytes = System.BitConverter.GetBytes(Sound);
					File_Stream.Write(tmpbytes, 0, 1);

					//Запись резервных переменных
					tmpbytes = System.BitConverter.GetBytes(Sound);
					File_Stream.Write(tmpbytes, 0, 1);
					tmpbytes = System.BitConverter.GetBytes(0);
					File_Stream.Write(tmpbytes, 0, 2);
					File_Stream.Write(tmpbytes, 0, 2);
					File_Stream.Write(tmpbytes, 0, 2);
					File_Stream.Write(tmpbytes, 0, 2);
					File_Stream.Write(tmpbytes, 0, 2);
					File_Stream.Write(tmpbytes, 0, 2);
					File_Stream.Write(tmpbytes, 0, 2);
					File_Stream.Write(tmpbytes, 0, 2);
					File_Stream.Write(tmpbytes, 0, 2);

					for (i = 0; i < Level_Count; i++)
					{
						tmpbytes = System.BitConverter.GetBytes(Level_Data[i].Used);
						File_Stream.Write(tmpbytes, 0, 1);
						tmpbytes = System.BitConverter.GetBytes(Level_Data[i].Score);
						File_Stream.Write(tmpbytes, 0, 2);
						tmpbytes = System.BitConverter.GetBytes(Level_Data[i].Stars);
						File_Stream.Write(tmpbytes, 0, 2);
					}
				}
			}
		}

		protected override void OnDeactivated(object sender, System.EventArgs args)
		{
			if (Stat_Curent == GameStat.Play)
				Stat_Next = GameStat.Pause;
		}

		protected override void OnActivated(object sender, System.EventArgs args)
		{
			Trial = Guide.IsTrialMode;
		}

		protected override void Update(GameTime gameTime)
		{
			if (gamePadState.Buttons.Back == ButtonState.Pressed)
			{
				if (Back_Click == 0)
					Last_Back = gameTime.TotalGameTime;
				Back_Click++;
				if (gameTime.TotalGameTime.Subtract(Last_Back).TotalMilliseconds > 500)
				{
					Back_Click = 0;
				}
				else if (Back_Click > 2)
				{
					this.Exit();
				}
			}

			ChangeStat(gameTime);

			gamePadState = GamePad.GetState(PlayerIndex.One);
			touchState = TouchPanel.GetState();

			switch (Stat_Curent)
			{
				case GameStat.MenuMain:
				case GameStat.MenuSettings:
				case GameStat.Help:
					Menu_Main.Update(gameTime);
					break;
				case GameStat.MenuSelect:
					Menu_Select.Update(gameTime);
					break;
				case GameStat.Play:
				case GameStat.Win:
				case GameStat.Lose:
				case GameStat.Pause:
					Game_Play.Update(gameTime);
					break;
				default:
					Stat_Next = GameStat.MenuMain;
					break;
			}

			base.Update(gameTime);
		}

		private void Stat_Menu_Main()
		{
			switch (Stat_Curent)
			{
				case GameStat.MenuSelect:
					Menu_Main = new MenuMain(this);
					Menu_Select = null;
					break;
				case GameStat.MenuSettings:
					Menu_Main.Reset();
					break;
				case GameStat.Help:
					Menu_Main.Reset();
					break;
				case GameStat.Win:
					Menu_Main = new MenuMain(this);
					Game_Play = null;
					break;
				case GameStat.Lose:
					Menu_Main = new MenuMain(this);
					Game_Play = null;
					break;
				case GameStat.Pause:
					Menu_Main = new MenuMain(this);
					if (Game_Play.Sound_Fan_Inst.State == SoundState.Playing)
						Game_Play.Sound_Fan_Inst.Stop();
					Game_Play = null;
					break;
				case GameStat.Null:
					Menu_Main = new MenuMain(this);
					break;
			}	
			Stat_Curent = GameStat.MenuMain;
		}

		private void Stat_Menu_Select()
		{
			switch (Stat_Curent)
			{
				case GameStat.MenuMain:
					CheckData();
					Menu_Select = new MenuSelect(this);
					Menu_Main = null;
					break;
			}
			Stat_Curent = GameStat.MenuSelect;
		}

		private void Stat_Menu_Settings()
		{
			switch (Stat_Curent)
			{
				case GameStat.MenuMain:
					Menu_Main.Reset();
					break;
			}
			Stat_Curent = GameStat.MenuSettings;
		}

		private void Stat_Menu_Help()
		{
			switch (Stat_Curent)
			{
				case GameStat.MenuMain:
					Menu_Main.Reset();
					break;
			}
			Stat_Curent = GameStat.Help;
		}

		private void Stat_Game_Start()
		{
			Game_Play = null;
			accelState.Start();
			Game_Play = new GamePlay(this);

			if (Game_Play.Sound_Fan_Inst.State == SoundState.Playing)
				Game_Play.Sound_Fan_Inst.Stop();

			Stat_Curent = GameStat.Play;
		}

		private void Stat_Game_Next()
		{
			Level++;
			Game_Play = null;
			accelState.Start();
			Game_Play = new GamePlay(this);
			Stat_Curent = GameStat.Play;
		}

		private void Stat_Game_Resume(GameTime gameTime)
		{
			accelState.Start();
			Stat_Curent = GameStat.Play;
			Game_Play.Return_En = true;
			Game_Play.Return_Time = 3;
			if (Game_Play.Sound_Fan_Inst.State == SoundState.Paused)
				Game_Play.Sound_Fan_Inst.Resume();
		}

		private void Stat_Game_Pause(GameTime gameTime)
		{
			accelState.Stop();
			Game_Play.Menu_Game.Pause();
			Stat_Curent = GameStat.Pause;
			Game_Play.GamePauseTime = gameTime.TotalGameTime;
			if (Game_Play.Sound_Fan_Inst.State == SoundState.Playing)
				Game_Play.Sound_Fan_Inst.Pause();
		}

		private void Stat_Game_Win()
		{
			accelState.Stop();

			Game_Play.Menu_Game.Win(Game_Play.Game_Level.Score_Count);

			if (Game_Play.Game_Level.Score_Count == 0)
				Stars = 3;
			else
				Stars = (int)(((float)Score / (float)Game_Play.Game_Level.Score_Count) * 3f);

			if (Score >= Level_Data[Level].Score)
			{
				Level_Data[Level].Score = Score;
				Level_Data[Level].Stars = Stars;
			}
			Level_Data[Level].Used = true;
			SaveData();
			CheckData();

			if (Game_Play.Sound_Fan_Inst.State == SoundState.Playing)
				Game_Play.Sound_Fan_Inst.Stop();

			Stat_Curent = GameStat.Win;
		}

		private void Stat_Game_Lose()
		{
			accelState.Stop();
			Game_Play.Menu_Game.Lose();

			if (Game_Play.Sound_Fan_Inst.State == SoundState.Playing)
				Game_Play.Sound_Fan_Inst.Stop();

			Stat_Curent = GameStat.Lose;
		}

		private void ChangeStat(GameTime gameTime)
		{
			if (Stat_Next == GameStat.Null)
				return;

			switch (Stat_Next)
			{
				case GameStat.MenuMain:
					Stat_Menu_Main();
					break;
				case GameStat.MenuSelect:
					Stat_Menu_Select();
					break;
				case GameStat.MenuSettings:
					Stat_Menu_Settings();
					break;
				case GameStat.Help:
					Stat_Menu_Help();
					break;
				case GameStat.Start:
					Stat_Game_Start();
					break;
				case GameStat.Win:
					Stat_Game_Win();
					break;
				case GameStat.Lose:
					Stat_Game_Lose();
					break;
				case GameStat.Pause:
					Stat_Game_Pause(gameTime);
					break;
				case GameStat.Next:
					Stat_Game_Next();
					break;
				case GameStat.Resume:
					Stat_Game_Resume(gameTime);
					break;
			}

			Stat_Next = GameStat.Null;
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			switch (Stat_Curent)
			{
				case GameStat.MenuMain:
				case GameStat.MenuSettings:
				case GameStat.Help:
					Menu_Main.Draw();
					break;
				case GameStat.MenuSelect:
					Menu_Select.Draw();
					break;
				case GameStat.Play:
				case GameStat.Win:
				case GameStat.Lose:
				case GameStat.Pause:
					Game_Play.Draw();
					break;
			}

			base.Draw(gameTime);
		}
	}
}
