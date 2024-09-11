using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace морской_бой
{
    public partial class Form1 : Form
    {
        //переменные для игрока
        sbyte[,] playerMap;
        List<Ship> playerShips;
        byte[] shipCounts;
        byte playerAttacks;

        //переменные для компьютера
        Random rnd = new Random();
        List<Ship> compShips;
        sbyte[,] compMap;
        List<Tuple<byte, byte>> compPossibilities = new List<Tuple<byte, byte>>();
        byte compAttacks;
        bool compHunts;
        Tuple<byte, byte> compAttack;
        byte huntMoves;
        Ship huntedShip;
        List<List<Point>> diagonals;


        public struct Ship
        {
            public byte health;
            public Point head;
            public byte maxLen;
            public ShipPosition orienation;
            public Ship(int hp, Point main, ShipPosition pos)
            {
                health = (byte)hp;
                head = main;
                maxLen = (byte)hp;
                orienation = pos;
            }
        }
       
        public Form1()
        {
            InitializeComponent();
            PrepareField(dgv1);
            playerMap = new sbyte[10, 10];
            playerShips = new List<Ship>();
            //playerAttacks = new byte[10, 10];
            playerAttacks = 0;
            compAttacks = 0;
            for (int i = 0; i < dgv1.RowCount; i++)
            {
                for (int j = 0; j < dgv1.RowCount; j++)
                {
                    playerMap[i, j] = 0;
                    compPossibilities.Add(new Tuple<byte, byte>((byte)i, (byte)j));
                }
            }
            shipCounts = new byte[4] { 4, 3, 2, 1 };
            compHunts = false;
        }

        int currentShipLevel = 1;
        ShipPosition position = ShipPosition.Vertical;
        public enum ShipPosition
        {
            Horizontal, Vertical
        }

        void PrepareField(DataGridView dgv)
        {
      dgv.Rows.Clear();
            dgv.RowTemplate.MinimumHeight = 30;
            dgv.AllowUserToAddRows = false;
            dgv.RowHeadersVisible = false;
            dgv.AutoGenerateColumns = true;
            dgv.RowCount = 10;
            dgv.ColumnCount = 10;
            dgv[0, 0].Selected = false;
        }

        private void dgv1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (currentShipLevel + e.RowIndex > dgv1.RowCount && position == ShipPosition.Vertical) return;
            if (currentShipLevel + e.ColumnIndex > dgv1.ColumnCount && position == ShipPosition.Horizontal) return;
            for (int i = 0; i < currentShipLevel; i++)
            {
                Point currentCell;
                if (position == ShipPosition.Horizontal) currentCell = new Point(e.RowIndex, e.ColumnIndex + i);
                else currentCell = new Point(e.RowIndex + i, e.ColumnIndex);
                if (playerMap[currentCell.X, currentCell.Y] != 0) return;
            }
            for (int i = 0; i < currentShipLevel; i++)
            {
                Point currentCell;
                if (position == ShipPosition.Horizontal) currentCell = new Point(e.RowIndex, e.ColumnIndex + i);
                else currentCell = new Point(e.RowIndex + i, e.ColumnIndex);
                dgv1[currentCell.Y, currentCell.X].Style.BackColor = Color.Gray;
            }

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton obj = sender as RadioButton;
            if (obj.Checked == true) currentShipLevel = int.Parse(obj.Text);
        }
        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton obj = sender as RadioButton;
            int val = int.Parse(obj.Tag.ToString());
            if (obj.Checked == true && val == 1) position = ShipPosition.Vertical;
            else if (val == 0) position = ShipPosition.Horizontal;
        }

        private void dgv1_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (currentShipLevel + e.RowIndex > dgv1.RowCount && position == ShipPosition.Vertical) return;
            if (currentShipLevel + e.ColumnIndex > dgv1.ColumnCount && position == ShipPosition.Horizontal) return;
            for (int i = 0; i < currentShipLevel; i++)
            {
                Point currentCell;
                if (position == ShipPosition.Horizontal) currentCell = new Point(e.RowIndex, e.ColumnIndex + i);
                else currentCell = new Point(e.RowIndex + i, e.ColumnIndex);
                if (playerMap[currentCell.X, currentCell.Y] != 0) return;
            }

            for (int i = 0; i < currentShipLevel; i++)
            {
                Point currentCell;
                if (position == ShipPosition.Horizontal) currentCell = new Point(e.RowIndex, e.ColumnIndex + i);
                else currentCell = new Point(e.RowIndex + i, e.ColumnIndex);
                if (playerMap[currentCell.X, currentCell.Y] != 0) return;
                dgv1[currentCell.Y, currentCell.X].Style.BackColor = Color.White;

            }
        }

        private void dgv1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (shipCounts[currentShipLevel - 1] == 0)
            {
                MessageBox.Show("Корабли такого типа уже закончились");
                return;
            }
      if (playerShips.Count == 9) button1.Enabled = true;
      if (CheckLocationForShip(e.RowIndex, e.ColumnIndex, playerMap) == false)
            {
                MessageBox.Show("Неверный вариант постановки корабля");
                return;
            }
            for (int i = 0; i < currentShipLevel; i++)
            {
                if (position == ShipPosition.Horizontal)
                {
                    dgv1[e.ColumnIndex + i, e.RowIndex].Style.BackColor = Color.Blue;
                    playerMap[e.RowIndex, e.ColumnIndex + i] = (sbyte)(playerShips.Count + 1);
                }
                else
                {

                    dgv1[e.ColumnIndex, e.RowIndex + i].Style.BackColor = Color.Blue;
                    playerMap[e.RowIndex + i, e.ColumnIndex] = (sbyte)(playerShips.Count + 1);
                }
            }
            shipCounts[currentShipLevel - 1] -= 1;
            CheckLabels(shipCounts[currentShipLevel - 1]);
            playerShips.Add(new Ship(currentShipLevel, new Point(e.RowIndex, e.ColumnIndex), position));
        }

        void CheckLabels(int value)
        {
            Label editedLabel;
            if (currentShipLevel == 1) editedLabel = label4;
            else if (currentShipLevel == 2) editedLabel = label5;
            else if (currentShipLevel == 3) editedLabel = label6;
            else editedLabel = label7;
            editedLabel.Text = value.ToString();
            
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            //Thread.Sleep(1000);
            dgv1.Enabled = true;
            dgv1.CellMouseLeave += new DataGridViewCellEventHandler(dgv1_CellMouseLeave);
            dgv1.CellMouseEnter += new DataGridViewCellEventHandler(dgv1_CellMouseEnter);
        }

        private void button1_Click(object sender, EventArgs e)
        {
      dgv1.CellMouseLeave -= new DataGridViewCellEventHandler(dgv1_CellMouseLeave);
      dgv1.CellMouseEnter -= new DataGridViewCellEventHandler(dgv1_CellMouseEnter);
      dgv2.Rows.Clear();
            compMap = new sbyte[10, 10];
            compShips = new List<Ship>();
            PrepareField(dgv2);
            for (int i = 0; i < dgv2.RowCount; i++)
            {
                for (int j = 0; j < dgv2.RowCount; j++)
                {
                    compMap[i, j] = 0;
                }
            }
            for (int n = 0; n < 10; n++)
            {
                byte x = (byte)rnd.Next(0, 10);
                byte y = (byte)rnd.Next(0, 10);
                position = (byte)rnd.Next(2) == 0 ? ShipPosition.Horizontal : ShipPosition.Vertical;
                if (n == 0) currentShipLevel = 4;
                else if (n < 3) currentShipLevel = 3;
                else if (n < 6) currentShipLevel = 2;
                else currentShipLevel = 1;
                if (!CheckLocationForShip(x, y, compMap))
                {
                    n--;
                    continue;
                }
                for (int i = 0; i < currentShipLevel; i++)
                {
                    if (position == ShipPosition.Horizontal)
                    {
                        //dgv2[y + i, x].Style.BackColor = Color.Blue;
                        compMap[x, y + i] = (sbyte)(compShips.Count + 1);
                    }
                    else
                    {
                       // dgv2[y, x + i].Style.BackColor = Color.Blue;
                        compMap[x + i, y] = (sbyte)(compShips.Count + 1);
                    }
                }
                compShips.Add(new Ship(currentShipLevel, new Point(x, y), position));
            }
            List<Point>[] temp = new List<Point>[8];
            diagonals = new List<List<Point>>();
            for (int i = 0; i < 8; i++)
            {
                temp[i] = new List<Point>();
            }
                for (int i = 0; i < 8; i++)
            {
                temp[0].Add(new Point(0 + i, 2 + i));
                temp[1].Add(new Point(2 + i, 0 + i));
                temp[2].Add(new Point(7 - i, 0 + i));
                temp[3].Add(new Point(9 - i, 2 + i));
                if (i < 4)
                {
                    temp[4].Add(new Point(0+i, 6 + i));
                    temp[5].Add(new Point(6 + i, 0 + i));
                    temp[6].Add(new Point(3 - i, 0 + i));
                    temp[7].Add(new Point(9 - i, 6 + i));
                }
                
            }
            for (int i = 0; i < 8; i++)
            {
                diagonals.Add(temp[i]);
            }

        }

        bool CheckLocationForShip(int x, int y, sbyte[,] map)
        {
            if (currentShipLevel + x > dgv1.RowCount && position == ShipPosition.Vertical ||
             currentShipLevel + y > dgv1.ColumnCount && position == ShipPosition.Horizontal) return false;
            int controlSum = 0;
            for (int i = 0; i < currentShipLevel; i++)
            {
                Point currentCell;
                if (position == ShipPosition.Horizontal)
                    currentCell = new Point(x, y + i);
                else currentCell = new Point(x + i, y);
                for (int m = -1; m < 2; m++)
                {
                    for (int n = -1; n < 2; n++)
                    {
                        try
                        {
                            controlSum += map[currentCell.X + m, currentCell.Y + n];
                        }
                        catch
                        {
                            continue;
                        }

                    }
                }
            }
            return controlSum == 0;
        }

        private void dgv2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (compMap[e.RowIndex, e.ColumnIndex] == 0)
            {
                dgv2[e.ColumnIndex, e.RowIndex].Value = '0';
                compMap[e.RowIndex, e.ColumnIndex] = -1;
                listBox1.Items.Add($"Игрок промазал ({e.RowIndex};{e.ColumnIndex})");
                ComputerTurn();
            }
            else if (compMap[e.RowIndex, e.ColumnIndex] > 0)
            {
                listBox1.Items.Add($"Игрок попал ({e.RowIndex};{e.ColumnIndex})");
                dgv2[e.ColumnIndex, e.RowIndex].Style.BackColor = Color.Orange;
                Ship ship = compShips[compMap[e.RowIndex, e.ColumnIndex] - 1];
                ship.health -= 1;
                if (ship.health == 0)
                {
                    KillComputerShip(ship, dgv2);
                    playerAttacks++;
                    if (playerAttacks == 10)
          {
            MessageBox.Show("Победа"); //дописать рестарт игры
            Form1.ActiveForm.Refresh();
          }
           //дописать рестарт игры
                }
                compShips[compMap[e.RowIndex, e.ColumnIndex] - 1] = ship;
                compMap[e.RowIndex, e.ColumnIndex] = -1;
            }
            dgv2[e.ColumnIndex, e.RowIndex].Selected = false;
        }
        void KillComputerShip(Ship ship, DataGridView field)
        {
            position = ship.orienation;
            for (int i = 0; i < ship.maxLen; i++)
            {
                Point currentCell;
                if (position == ShipPosition.Vertical) currentCell = new Point(ship.head.X + i, ship.head.Y);
                else currentCell = new Point(ship.head.X, ship.head.Y + i);
                field[currentCell.Y, currentCell.X].Style.BackColor = Color.Red;
             if (field == dgv1)
              {
          for (int m = -1; m <2; m++)
          {
            for (int n= -1; n < 2; n++)
            {
              try 
              {
                playerMap[currentCell.X + m, currentCell.Y + n] = -1;
                compPossibilities.RemoveAll(point => playerMap[point.Item1, point.Item2] == -1);
              }catch
              {
                continue;
              }
            }
          }
              }
            }
            
        }

        
        void ComputerTurn()
        {
            //определить клетку удара
            //нанести удар
            // провести анализ хода
            if (compHunts) HuntMode();
            else RandomMode();
            
        }

        void RandomMode()
        {  
            Tuple<byte, byte> pos;
            pos = compPossibilities[rnd.Next() % compPossibilities.Count];
            int ind = 0;
            if (compAttacks <= 2)
            {
                while(true)
                {
                    ind = rnd.Next(diagonals.Count);
                    if (diagonals[ind].Count == 0)
                    {
                        diagonals.RemoveAt(ind);                        
                    }
                    else break;
                }
                
                Point p = diagonals[ind][rnd.Next(diagonals[ind].Count)];
                pos = new Tuple<byte, byte>((byte)p.X, (byte)p.Y);
            }
            if (playerMap[pos.Item1, pos.Item2] <= 0)
            {
                listBox1.Items.Add($"Компьютер промазал ({pos.Item1};{pos.Item2})");
                dgv1[pos.Item2, pos.Item1].Value = '0';
                compPossibilities.Remove(pos);
                if (compAttacks <= 2)
                {
                    diagonals[ind].Remove(new Point(pos.Item1, pos.Item2));
                }
                playerMap[pos.Item1, pos.Item2] = -1;
            }
            else if (playerMap[pos.Item1, pos.Item2] > 0)
            {
                listBox1.Items.Add($"Компьютер попал ({pos.Item1};{pos.Item2})");
                dgv1[pos.Item2, pos.Item1].Style.BackColor = Color.Orange;
                Ship ship = playerShips[playerMap[pos.Item1, pos.Item2] - 1];
                ship.health -= 1;
                if (ship.health == 0)
                {
                    KillComputerShip(ship, dgv1);
                    compAttacks++;
          if (compAttacks == 10) 
          {
            MessageBox.Show("Победа компа"); //дописать рестарт игры
            Form1.ActiveForm.Refresh();
          }
          
                }
                else
                {
                    compHunts = true;
                    compAttack = new Tuple<byte, byte>(pos.Item1, pos.Item2);
                    huntMoves = 1;
                    ShipPosition shipPos = rnd.Next(2) == 1 ? ShipPosition.Vertical : ShipPosition.Horizontal;
                    huntedShip = new Ship(1, new Point(pos.Item1, pos.Item2), shipPos);
          playerShips[playerMap[pos.Item1, pos.Item2] - 1] = ship;
          playerMap[pos.Item1, pos.Item2] = -1;
        }
                
                Thread.Sleep(500);
                ComputerTurn();
            }
            
            dgv1[pos.Item2, pos.Item1].Selected = false;
        }
        void HuntMode()
        {
            if (huntMoves == 1)
            {
                change:
                if (huntedShip.orienation == ShipPosition.Vertical)
                {
                    sbyte newX = (sbyte)(rnd.Next(2) == 1 ? compAttack.Item1 + 1 : compAttack.Item1 - 1);                    
                    if (newX == 10) newX -= 2;
                    if (newX == -1) newX += 2;
                    if (playerMap[newX, compAttack.Item2] == -1)
                    {
                        newX = (sbyte)(compAttack.Item1 + compAttack.Item1 - newX);
                        if (newX == 10 || newX == -1 || playerMap[newX, compAttack.Item2 ] == -1)
                           huntedShip.orienation = ShipPosition.Horizontal;
                    } 
                    compAttack = new Tuple<byte, byte>((byte)newX, compAttack.Item2);
                }
                if (huntedShip.orienation == ShipPosition.Horizontal)
                {
                    sbyte newY = (sbyte)(rnd.Next(2) == 1 ? compAttack.Item2 + 1 : compAttack.Item2 - 1);                    
                    if (newY == 10) newY -= 2;
                    if (newY == -1) newY += 2;
                    else if (playerMap[compAttack.Item1, newY] == -1) 
                    {
                        newY = (sbyte)(compAttack.Item2 + compAttack.Item2 - newY);
                        if (newY == 10 || newY == -1 || playerMap[compAttack.Item1, newY] == -1 ) 
                        {
                            huntedShip.orienation = ShipPosition.Vertical;
                            goto change;
                        }
                        
                    }
                    compAttack = new Tuple<byte, byte>((byte)compAttack.Item1, (byte)newY);
                }
            }
            huntMoves++;
            if (playerMap[compAttack.Item1, compAttack.Item2] <= 0)
            {
                listBox1.Items.Add($"Компьютер промазал ({compAttack.Item1};{compAttack.Item2})");
                dgv1[compAttack.Item2, compAttack.Item1].Value = '0';
                compPossibilities.Remove(compAttack);
                playerMap[compAttack.Item1, compAttack.Item2] = -1;
                if (huntMoves == 2)
                {
                    if (huntedShip.orienation == ShipPosition.Vertical)
                    {
                        int newX = huntedShip.head.X + huntedShip.head.X - compAttack.Item1;
                        if (newX == 10)
                        {
                            newX = huntedShip.head.X;
                            huntedShip.orienation = ShipPosition.Horizontal;
                            huntMoves = 1;
                        }
                        if (newX == -1)
                        {
                            newX = huntedShip.head.X;
                            huntedShip.orienation = ShipPosition.Horizontal;
                            huntMoves = 1;
                        }

                        compAttack = new Tuple<byte, byte>((byte)newX, (byte)compAttack.Item2);
                    }
                    if (huntedShip.orienation == ShipPosition.Horizontal)
                    {
                        int newY = huntedShip.head.Y + huntedShip.head.Y - compAttack.Item2;
                        if (newY == 10)
                        {
                            newY = huntedShip.head.Y ;
                            huntedShip.orienation = ShipPosition.Vertical;
                            huntMoves = 1;
                        }
                        if (newY == -1)
                        {
                            newY = huntedShip.head.Y;
                            huntedShip.orienation = ShipPosition.Vertical;
                            huntMoves = 1;
                        }
                        compAttack = new Tuple<byte, byte>((byte)compAttack.Item1, (byte)newY);
                    }
                    
                }
                else if (huntedShip.health <huntMoves)
                {
          if (huntedShip.health == 1)
            {
            if (huntedShip.orienation == ShipPosition.Horizontal)
              huntedShip.orienation = ShipPosition.Vertical;
            else huntedShip.orienation = ShipPosition.Horizontal;

          } 
                    huntMoves = 1;
                    compAttack = new Tuple<byte, byte>((byte)huntedShip.head.X, (byte)huntedShip.head.Y);
                }
            }            
            else if (playerMap[compAttack.Item1, compAttack.Item2] > 0)
            {
                dgv1[compAttack.Item2, compAttack.Item1].Style.BackColor = Color.Orange;
                Ship ship = playerShips[playerMap[compAttack.Item1, compAttack.Item2] - 1];
                ship.health -= 1;
                huntedShip.health += 1;
                listBox1.Items.Add($"Компьютер попал ({compAttack.Item1};{compAttack.Item2})");
                if (ship.health == 0)
                {
                    dgv1[ship.head.Y, ship.head.X].Selected = false;
                    dgv1[huntedShip.head.Y, huntedShip.head.X].Style.BackColor = Color.Red;
          playerShips[playerMap[compAttack.Item1, compAttack.Item2] - 1] = ship;
          playerMap[compAttack.Item1, compAttack.Item2] = -1;
          KillComputerShip(ship, dgv1);
                    compAttacks++;
                    if (compAttacks == 10)
          {
            MessageBox.Show("Победа компа"); 
            Form1.ActiveForm.Refresh();
          }
          compHunts = false;
                     
                  }
                else
                {
                    playerShips[playerMap[compAttack.Item1, compAttack.Item2] - 1] = ship;
                    playerMap[compAttack.Item1, compAttack.Item2] = -1;
                    int[] direction = new int[2] { huntedShip.head.X - compAttack.Item1, huntedShip.head.Y - compAttack.Item2 };
                    if (direction[0] > 1) direction[0] = 1;
                    if (direction[0] < -1) direction[0] = -1;
                    if (direction[1] > 1) direction[1] = 1;
                    if (direction[1] < -1) direction[1] = -1;
                    int newX = compAttack.Item1 - direction[0];
                    int newY = compAttack.Item2 - direction[1];
                    if (newX == 10 || newX == -1)
                    {
                        newX = huntedShip.head.X + huntedShip.head.X - direction[0];
                        compAttack = new Tuple<byte, byte>((byte)newX, (byte)compAttack.Item2);
                    }
                    if (newY == 10 || newY == -1)
                    {
                        newY = huntedShip.head.Y + huntedShip.head.Y - direction[1];
                        compAttack = new Tuple<byte, byte>(compAttack.Item1, (byte)newY);
                    }
                    compAttack = new Tuple<byte, byte>((byte)newX, (byte)newY);
                }
                //Thread.Sleep(500);
                ComputerTurn();
            }
        }

    private void button2_Click(object sender, EventArgs e)
    {
            listBox1.Items.Clear();
      PrepareField(dgv1);
      dgv2.Rows.Clear();
      dgv1.CellMouseLeave += new DataGridViewCellEventHandler(dgv1_CellMouseLeave);
      dgv1.CellMouseEnter += new DataGridViewCellEventHandler(dgv1_CellMouseEnter);
      playerMap = new sbyte[10, 10];
      playerShips = new List<Ship>();
      //playerAttacks = new byte[10, 10];
      playerAttacks = 0;
      compAttacks = 0;
      for (int i = 0; i < dgv1.RowCount; i++)
      {
        for (int j = 0; j < dgv1.RowCount; j++)
        {
          playerMap[i, j] = 0;
          compPossibilities.Add(new Tuple<byte, byte>((byte)i, (byte)j));
        }
      }
      shipCounts = new byte[4] { 4, 3, 2, 1 };
      compHunts = false;
    }
  }
}
