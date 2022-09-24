/*! @file       Static/Raster.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-23
**/

using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Ore
{
  [PublicAPI]
  public static class Raster
  {

    public static IEnumerable<Vector2Int> Line(Vector2 start, Vector2 direction, float distance)
    {
      // based on Bresenham implemented by Levi: https://gist.github.com/Pyr3z/46884d67641094d6cf353358566db566
      var a = new Vector2Int((int)start.x, (int)start.y);

      int dx, dy, xinc, yinc, i, error, side;

      xinc = direction.x < 0f ? -1 : +1;
      yinc = direction.y < 0f ? -1 : +1;

      dx = (int)(xinc * direction.x * distance);
      dy = (int)(yinc * direction.y * distance);

      yield return a;

      if (dx == dy) // Handle perfect diagonals
      {
        while (dx --> 0)
        {
          a.x += xinc;
          a.y += yinc;
          yield return a;
        }
        yield break;
      }

      side = -1 * ((dx == 0 ? yinc : xinc) - 1);

      i     = dx + dy;
      error = dx - dy;
      dx *= 2;
      dy *= 2;

      while (i --> 0)
      {
        if (error > 0 || error == side)
        {
          a.x += xinc;
          error -= dy;
        }
        else
        {
          a.y += yinc;
          error += dx;
        }

        yield return a;
      }
    }

  } // end class Raster
}