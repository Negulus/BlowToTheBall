using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace BlowToTheBall
{
	public class GameLevel
	{
		Game1 game1;

		public struct LevelData
		{
			public int[] Time;
			public float[] Value;
			public int Count;
			public int CurTime;
			public float CurValue;
			public TimeSpan LastTime;
			public int Point;
			public VertexPositionColor[] GraphUp;
			public VertexPositionColor[] GraphDown;
			public int GraphCountUp;
			public int GraphCountDown;
			float tmpf;
			bool tmpb;
			int i, j;
			Color GraphColor;

			public void Init()
			{
				CurTime = 0;
				CurValue = 0;
				Point = 0;
				GraphUp = new VertexPositionColor[8];
				GraphDown = new VertexPositionColor[8];
				GraphCountUp = 0;
				GraphCountDown = 0;
				GraphColor = Color.Red;
			}

			public bool Update(bool run, GameTime gameTime)
			{
				if (run)
				{
					if (Point < Count)
					{
						CurTime = (int)gameTime.TotalGameTime.Subtract(LastTime).TotalMilliseconds;
						tmpf = (float)CurTime / (float)Time[Point];

						if (tmpf >= 1)
						{
							LastTime = gameTime.TotalGameTime;
							CurValue = Value[Point];
							Point++;
							CurTime = 0;
							if (Point >= Count)
								return true;
						}
						else
						{
							if (Point == 0)
								CurValue = tmpf * Value[0];
							else
								CurValue = (tmpf * (Value[Point] - Value[Point - 1])) + Value[Point - 1];
						}
					}
					else
					{
						CurTime = (int)gameTime.TotalGameTime.Subtract(LastTime).TotalMilliseconds;
						CurValue = Value[Point - 1];
						return true;
					}
				}
				return false;
			}

			public void GraphUpdate()
			{
				GraphUp[0] = new VertexPositionColor();
				GraphUp[0].Color = GraphColor;
				if (Point == 0)
					GraphUp[0].Position.X = 50;
				else
					GraphUp[0].Position.X = 50 + Value[Point - 1] * 30f;
				GraphUp[0].Position.Y = 150 + CurTime * 0.1f;

				GraphCountUp = 0;
				tmpb = true;
				j = 1;
				for (i = Point; i < Count; i++)
				{
					if (tmpb)
					{
						GraphUp[j] = new VertexPositionColor();
						GraphUp[j].Color = GraphColor;
						GraphUp[j].Position.X = 50 + Value[i] * 30f;
						GraphUp[j].Position.Y = GraphUp[j - 1].Position.Y - (Time[i] * 0.1f);
						if (GraphUp[j].Position.Y < 0)
							tmpb = false;
						j++;
						GraphCountUp++;
					}
					else
						break;
				}
				if (tmpb)
				{
					GraphUp[j] = new VertexPositionColor();
					GraphUp[j].Color = GraphColor;
					GraphUp[j].Position.X = GraphUp[j - 1].Position.X;
					GraphUp[j].Position.Y = 0;
					GraphCountUp++;
				}

				GraphDown[0] = GraphUp[0];
				GraphCountDown = 0;
				tmpb = true;
				j = 1;
				for (i = (Point - 2); i >= -1; i--)
				{
					if (tmpb)
					{
						GraphDown[j] = new VertexPositionColor();
						GraphDown[j].Color = GraphColor;
						if (i < 0)
							GraphDown[j].Position.X = 50;
						else
							GraphDown[j].Position.X = 50 + Value[i] * 30f;
						GraphDown[j].Position.Y = GraphDown[j - 1].Position.Y + (Time[i + 1] * 0.1f);
						if (GraphDown[j].Position.Y > 200)
							tmpb = false;
						j++;
						GraphCountDown++;
					}
					else
						break;
				}
				if (tmpb)
				{
					GraphDown[j] = new VertexPositionColor();
					GraphDown[j].Color = GraphColor;
					GraphDown[j].Position.X = GraphDown[j - 1].Position.X;
					GraphDown[j].Position.Y = 200;
					GraphCountDown++;
				}
			}
		}
		public struct LevelStar
		{
			public Vector2 position;
			public Vector2 origin;
			public long Time_Start;
			public long Time_End;
			public long Time_Take;
			public int Time_Fade;
			public int Score;
			public bool Take;
			public bool Enable;
			public bool Enable_Draw;
			public float Opacity;
			public float Scale;
			int tmpi;

			public bool Update(long time, Vector2 pos)
			{
				if (Enable)
				{
					if (Take)
					{
						if (Time_Take < 0)
							Time_Take = time;

						Opacity = (float)(Time_Take + 200 - time) / 200f;
						if (Opacity < 0)
							Opacity = 0;
						Scale = 1 + (1f - Opacity) * 2f;

						if (time > Time_Take + 200f)
							Enable = false;
					}
					else if (time > Time_Start)
					{
						if (time < Time_End)
						{
							Enable_Draw = true;
							tmpi = (int)Math.Sqrt(Math.Pow(pos.X - position.X, 2) + Math.Pow(pos.Y - position.Y, 2));
							if (tmpi < 40)
								return true;
						}
						else
							Enable_Draw = false;

						if (time > (Time_End - Time_Fade))
						{
							Opacity = (float)(Time_End - time) / (float)Time_Fade;
							if (Opacity < 0.01f)
							{
								Opacity = 0;
							}
						}
					}
					else
						Enable_Draw = false;
				}
				else
					Enable_Draw = false;
				return false;
			}
		}

		//Данные уровня
		public LevelData Amplitude;
		public LevelData Wind;
		public LevelStar[] Star;
		public int Star_Count;
		public int Score_Count;
		public long Time_Life;
		public bool Amplitude_en;
		public bool Wind_en;
		public bool Accel_en;
		public TimeSpan Time_Start;
		Vector2 Position_Game;

		//Системные переменные
		int Num;
		bool Run;
		int i, j;
		bool tmpb;
		Stream Level_File;

		public GameLevel(Game1 game, int num, Vector2 pos)
		{
			game1 = game;

			Position_Game = pos;

			//Инициализация уровня
			Amplitude.Init();
			Wind.Init();
			Star_Count = 0;
			Score_Count = 0;
			Time_Life = 0;
			Amplitude_en = false;
			Wind_en = false;
			Accel_en = false;
			Num = num;
			Run = false;

			//Загрузка уровня из файла
			Load();
		}

		//Загрузка уровня из файла
		public void Load()
		{
			Level_File = TitleContainer.OpenStream("Content/levels/level_" + (Num + 1).ToString() + ".txt");

			List<string> lines = new List<string>();
			using (StreamReader reader = new StreamReader(Level_File))
			{
				string line = reader.ReadLine();
				while (line != null)
				{
					lines.Add(line);
					line = reader.ReadLine();
				}
			}

			tmpb = true;
			i = 0;
			j = 0;
			while (tmpb)
			{
				if (i < lines.Count)
				{
					if (lines[i] == "Amplitude")
						tmpb = false;
					else
						j++;
				}
				else
					tmpb = false;
				i++;
			}

			j = 0;
			tmpb = true;
			while (tmpb)
			{
				if (i < lines.Count)
				{
					if (lines[i] == "Wind")
						tmpb = false;
					else
						j++;
				}
				else
					tmpb = false;
				i++;
			}
			Amplitude.Time = new int[j];
			Amplitude.Value = new float[j];
			Amplitude.Count = j;

			j = 0;
			tmpb = true;
			while (tmpb)
			{
				if (i < lines.Count)
				{
					if (lines[i] == "Stars")
						tmpb = false;
					else
						j++;
				}
				else
					tmpb = false;
				i++;
			}
			Wind.Time = new int[j];
			Wind.Value = new float[j];
			Wind.Count = j;

			j = 0;
			tmpb = true;
			while (tmpb)
			{
				if (i < lines.Count)
				{
					if (lines[i] == "End")
						tmpb = false;
					else
						j++;
				}
				else
					tmpb = false;
				i++;
			}
			Star_Count = j;
			Star = new LevelStar[Star_Count];

			i = 0;
			j = 0;
			tmpb = true;
			while (tmpb)
			{
				if (i < lines.Count)
				{
					if (lines[i] == "Amplitude")
						tmpb = false;
					else
					{
						if (i == 0)
						{
							string[] str = lines[i].Split(' ');
							Time_Life = long.Parse(str[0]);
							if (int.Parse(str[1]) == 1)
								Amplitude_en = true;
							if (int.Parse(str[2]) == 1)
								Wind_en = true;
							if (int.Parse(str[3]) == 1)
								Accel_en = true;
						}
					}
				}
				else
					tmpb = false;
				i++;
				j++;
			}

			j = 0;
			tmpb = true;
			while (tmpb)
			{
				if (i < lines.Count)
				{
					if (lines[i] == "Wind")
						tmpb = false;
					else
					{
						string[] str = lines[i].Split(' ');
						Amplitude.Time[j] = int.Parse(str[0]);
						Amplitude.Value[j] = (float)int.Parse(str[1]) / 100f;
					}
				}
				else
					tmpb = false;
				i++;
				j++;
			}

			j = 0;
			tmpb = true;
			while (tmpb)
			{
				if (i < lines.Count)
				{
					if (lines[i] == "Stars")
						tmpb = false;
					else
					{
						string[] str = lines[i].Split(' ');
						Wind.Time[j] = int.Parse(str[0]);
						Wind.Value[j] = (float)int.Parse(str[1]) / 100f;
					}
				}
				else
					tmpb = false;
				i++;
				j++;
			}

			j = 0;
			tmpb = true;
			while (tmpb)
			{
				if (i < lines.Count)
				{
					if (lines[i] == "End")
						tmpb = false;
					else
					{
						string[] str = lines[i].Split(' ');
						Star[j].Time_Start = long.Parse(str[0]);
						Star[j].Time_End = Star[j].Time_Start + long.Parse(str[1]);
						Star[j].position.X = Position_Game.X + int.Parse(str[2]);
						Star[j].position.Y = Position_Game.Y + int.Parse(str[3]);
						Star[j].Score = int.Parse(str[4]);
						Score_Count += Star[j].Score;
						Star[j].origin.X = 31;
						Star[j].origin.Y = 31;
						Star[j].Take = false;
						Star[j].Enable = true;
						Star[j].Enable_Draw = false;
						Star[j].Time_Take = -1;
						Star[j].Time_Fade = 500;
						Star[j].Opacity = 1f;
						Star[j].Scale = 1f;
					}
				}
				else
					tmpb = false;
				i++;
				j++;
			}
		}

		//Запуск уровня
		public void Start(GameTime gameTime)
		{
			if (Run)
				return;

			Time_Start = gameTime.TotalGameTime;
			Amplitude.LastTime = gameTime.TotalGameTime;
			Wind.LastTime = gameTime.TotalGameTime;
			Run = true;
		}

		//Обновление данных уровня
		public bool Update(GameTime gameTime)
		{
			Wind.Update(Run, gameTime);
			Amplitude.Update(Run, gameTime);

			if (gameTime.TotalGameTime.Subtract(Time_Start).TotalMilliseconds >= Time_Life)
				return true;
			else
				return false;
		}

		//Обновление графиков
		public void GraphUpdate()
		{
			Wind.GraphUpdate();
			Amplitude.GraphUpdate();
		}
	}
}