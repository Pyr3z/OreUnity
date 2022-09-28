/*! @file       Static/Raster.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-23
**/

using System.Collections;
using System.Collections.Generic;

using JetBrains.Annotations;

using UnityEngine;

using Math = System.Math;


namespace Ore
{

  [PublicAPI]
  public static class Raster
  {

    public static IEnumerable<Vector2Int> Line(int ax, int ay, int bx, int by)
    {
      var rasterizer = new LineDrawer();
      rasterizer.Prepare(ax, ay, bx, by);
      return rasterizer; // the boxing allocation is less than a yield routine.
    }

    public static IEnumerable<Vector2Int> Line(Vector2Int a, Vector2Int b)
    {
      var rasterizer = new LineDrawer();
      rasterizer.Prepare(a, b);
      return rasterizer;
    }

    public static IEnumerable<Vector2Int> Line(Vector2 start, Vector2 direction, float distance)
    {
      var rasterizer = new LineDrawer();
      rasterizer.Prepare(start, direction, distance);
      return rasterizer;
    }


    public static IEnumerable<Vector2Int> Rectangle(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
      throw new System.NotImplementedException();
    }

    public static IEnumerable<Vector2Int> Circle(Vector2 center, float radius)
    {
      // TODO iterate on PyroDK impl.
      throw new System.NotImplementedException();
    }



    /// <summary>
    /// Keep a cached field of this struct to reduce GC allocs and keep nice
    /// algo structure!
    /// </summary>
    /// <remarks>
    /// Based on Bresenham's rasterization, implemented by Levi:
    /// https://gist.github.com/Pyr3z/46884d67641094d6cf353358566db566
    /// </remarks>
    public struct LineDrawer : IEnumerator<Vector2Int>, IEnumerable<Vector2Int>
    {
      [PublicAPI]
      public Vector2Int Current => new Vector2Int(x, y);
      [PublicAPI]
      public Vector2Int Start   => new Vector2Int(sx, sy);

      [PublicAPI]
      public float EuclideanDistance => Vector2Int.Distance(Start, Current);
      [PublicAPI]
      public float ManhattanDistance => D * (xinc * (x - sx) + yinc * (y - sy));
      [PublicAPI]
      public float DiagonalDistance
      {
        get
        {
          float dx = xinc * (x - sx);
          float dy = yinc * (y - sy);

          if (dy < dx)
            return D * (dx - dy) + D2 * dy;
          else
            return D * (dy - dx) + D2 * dx;
        }
      }


      public int x, y;

      private int sx, sy, dx, dy, xinc, yinc, i, error, side, state;
      private int xmin, xmax, ymin, ymax;

      private const int PREPARED  = 0;
      private const int ITERATING = 1;

      private const float D  = 1f;
      private const float D2 = Floats.SQRT2;



      [PublicAPI]
      public static LineDrawer WithBounds(int xmin, int ymin, int xmax, int ymax)
      {
        if (xmax < xmin)
          (xmin,xmax) = (xmax,xmin);
        if (ymax < ymin)
          (ymin,ymax) = (ymax,ymin);

        return new LineDrawer
        {
          xmin = xmin,
          xmax = xmax,
          ymin = ymin,
          ymax = ymax
        };
      }


      [PublicAPI]
      public LineDrawer Prepare(int ax, int ay, int bx, int by)
      {
        x = sx = ax;
        y = sy = ay;

        if (xmin < xmax)
        {
          // invalid if starting out of bounds:
          if (ax < xmin || ax >= xmax || ay < ymin || ay >= ymax)
          {
            x = xmin;
            y = ymin;
            i = 0;
            return this;
          }
        }

        xinc = (bx < ax) ? -1 : 1;
        yinc = (by < ay) ? -1 : 1;
        dx   = xinc * (bx - ax);
        dy   = yinc * (by - ay);

        if (dx == dy)
        {
          i = dx;
        }
        else
        {
          side  = -1 * ((dx == 0 ? yinc : xinc) - 1);
          i     = dx + dy;
          error = dx - dy;
        }

        dx *= 2;
        dy *= 2;

        state = PREPARED;
        return this;
      }

      [PublicAPI]
      public LineDrawer Prepare(Vector2Int a, Vector2Int b)
      {
        return Prepare(a.x, a.y, b.x, b.y);
      }

      [PublicAPI]
      public LineDrawer Prepare(Vector2 start, Vector2 direction, float distance)
      {
        direction = start + direction * distance;
        return Prepare((int)start.x, (int)start.y, (int)direction.x, (int)direction.y);
      }


      public bool MoveNext()
      {
        if (i <= 0)
        {
          return false;
        }

        if (state == PREPARED)
        {
          state = ITERATING;
          return true;
        }

        --i;

        if (dx == dy)
        {
          x += xinc;
          y += yinc;
        }
        else if (error > 0 || error == side)
        {
          x += xinc;
          error -= dy;
        }
        else
        {
          y += yinc;
          error += dx;
        }

        if (xmin == xmax)
          return true;

        if (x < xmin || x >= xmax)
        {
          x -= xinc;
          i = -1;
        }

        if (y < ymin || y >= ymax)
        {
          y -= yinc;
          i = -1;
        }

        return i >= 0;
      }

      public void Reset()
      {
        x     = sx;
        y     = sy;
        i     = (dx + dy) / 2;
        error = (dx - dy) / 2;
        state = PREPARED;
      }


      object IEnumerator.Current => Current;

      // this trick allows this to be used in a foreach loop:
      IEnumerator<Vector2Int> IEnumerable<Vector2Int>.GetEnumerator() => this;
      IEnumerator IEnumerable.GetEnumerator() => this;

      void System.IDisposable.Dispose()
      {
      }
    } // end struct LineDrawer

  } // end class Raster
}