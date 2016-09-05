using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HW_14
{
  public partial class Form1 : Form
  {
    class Planet
    {
      public Rectangle planet;
      public double x;
      public double y;
      public double theta;
      public double r;

      public Planet(Rectangle newp, double newx, double newy, double newt, double newr)
      {
        planet = newp;
        x = newx;
        y = newy;
        theta = newt;
        r = newr;
      }
    }

    public Form1()
    {
      InitializeComponent();
      drawing = pictureBox.CreateGraphics();
    }

    List<Rectangle> CoMs = new List<Rectangle>();
    List<Planet> Planets = new List<Planet>();
    Graphics drawing;

    private void TransformCoordinates(ref int x, ref int y, int edgelength, int mode)
    {
      // center to corner
      if (mode == 0)
      {
        x = x - edgelength / 2;
        y = y - edgelength / 2;
      }
      // corner to center
      else
      {
        x = x + edgelength / 2;
        y = y + edgelength / 2;
      }
    }

    // Mouse down event
    private void pictureBox_MouseDown(object sender, MouseEventArgs e)
    {
      int x = e.X;
      int y = e.Y;

      // Place planet
      if (e.Button == MouseButtons.Left)
      {
        TransformCoordinates(ref x, ref y, 10, 0);
        Rectangle rec = new Rectangle(x, y, 10, 10);
        drawing.FillEllipse(Brushes.Red, rec);

        // Find the sphere of influence and set the starting coordinates
        foreach (var com in CoMs)
        {
          if (rec.IntersectsWith(com))
          {
            x = com.X;
            y = com.Y;
            TransformCoordinates(ref x, ref y, 120, 1);
            x = e.X - x;
            y = e.Y - y;
            break;
          }
        }
        double r = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        double theta = Math.Atan(x / y);
        Planets.Add(new Planet(rec, x, y, theta, r));
      }
      // Place Center of Mass
      else if (e.Button == MouseButtons.Right)
      {
        TransformCoordinates(ref x, ref y, 120, 0);
        Rectangle rec = new Rectangle(x, y, 120, 120);
        drawing.FillEllipse(Brushes.LightGray, rec);
        CoMs.Add(rec);

        x = e.X;
        y = e.Y;
        TransformCoordinates(ref x, ref y, 10, 0);
        Rectangle recS = new Rectangle(x, y, 10, 10);
        drawing.FillEllipse(Brushes.Black, recS);
      }
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      timer.Stop();
      // render all coms again
      foreach(var com in CoMs)
      {
        drawing.FillEllipse(Brushes.LightGray, com);
        int x = com.X, y = com.Y;
        TransformCoordinates(ref x, ref y, 120, 1);
        TransformCoordinates(ref x, ref y, 10, 0);
        Rectangle recS = new Rectangle(x, y, 10, 10);
        drawing.FillEllipse(Brushes.Black, recS);
      }

      // go through each planet and update locations
      for (int i = 0; i < Planets.Count; i++)
      {
        foreach(var com in CoMs)
        {
          // In spere of influence, do orbit
          if (Planets[i].planet.IntersectsWith(com))
          {
            Planet pl = Planets[i];
            UpdatePlanetPosition(com, ref pl);
            Planets[i] = pl;
            drawing.FillEllipse(Brushes.Red, pl.planet);
            break;
          }
        }
      }

      timer.Start();
    }

    private void UpdatePlanetPosition(Rectangle com, ref Planet planet)
    {
      int cx = com.X, cy = com.Y;
      TransformCoordinates(ref cx, ref cy, 120, 1);
      // double x = planet.x - cx, y = planet.y - cy;

      // Realistic orbital velocity! (Falloff by square root of 1/r)
      planet.theta += .4*(Math.Sqrt(1/planet.r));

      planet.x = (planet.r * Math.Sin(planet.theta));
      planet.y = (planet.r * Math.Cos(planet.theta));

      int px = (int)((int)planet.x + cx);
      int py = (int)((int)planet.y + cy);
      TransformCoordinates(ref px, ref py, 10, 0);
      planet.planet.X = px;
      planet.planet.Y = py;
      return;
    }
  }
}
