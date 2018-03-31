using System;

/**
    @typedef dtPolyRef
    @par

    Polygon references are subject to the same invalidate/preserve/restore 
    rules that apply to #dtTileRef's.  If the #dtTileRef for the polygon's
    tile changes, the polygon reference becomes invalid.

    Changing a polygon's flags, area id, etc. does not impact its polygon
    reference.

    @typedef dtTileRef
    @par

    The following changes will invalidate a tile reference:

    - The referenced tile has been removed from the navigation mesh.
    - The navigation mesh has been initialized using a different set
      of #dtNavMeshParams.

    A tile reference is preserved/restored if the tile is added to a navigation 
    mesh initialized with the original #dtNavMeshParams and is added at the
    original reference location. (E.g. The lastRef parameter is used with
    dtNavMesh::addTile.)

    Basically, if the storage structure of a tile changes, its associated
    tile reference changes.
  */

#if DT_POLYREF64
using dtPolyRef = System.UInt64;
using dtTileRef = System.UInt64;
#else
#endif

#if DT_POLYREF64
public static partial class Detour{
    static const uint DT_SALT_BITS = 16;
    static const uint DT_TILE_BITS = 28;
    static const uint DT_POLY_BITS = 20;
}
#endif

namespace RecastDetour.Detour
{
    public static partial class Detour{
        public static bool overlapSlabs(float[] amin, float[] amax,
            float[] bmin,float[] bmax,
            float px, float py)
        {
            // Check for horizontal overlap.
            // The segment is shrunken a little so that slabs which touch
            // at end points are not connected.
            float minx = (float)Math.Max(amin[0]+px,bmin[0]+px);
            float maxx = (float)Math.Min(amax[0]-px,bmax[0]-px);
            if (minx > maxx)
                return false;
	
            // Check vertical overlap.
            float ad = (amax[1]-amin[1]) / (amax[0]-amin[0]);
            float ak = amin[1] - ad*amin[0];
            float bd = (bmax[1]-bmin[1]) / (bmax[0]-bmin[0]);
            float bk = bmin[1] - bd*bmin[0];
            float aminy = ad*minx + ak;
            float amaxy = ad*maxx + ak;
            float bminy = bd*minx + bk;
            float bmaxy = bd*maxx + bk;
            float dmin = bminy - aminy;
            float dmax = bmaxy - amaxy;
		
            // Crossing segments always overlap.
            if (dmin*dmax < 0)
                return true;
		
            // Check for overlap at endpoints.
            float thr = dtSqr(py*2);
            if (dmin*dmin <= thr || dmax*dmax <= thr)
                return true;
		
            return false;
        }

        public static float getSlabCoord(float[] va, int side)
        {
            if (side == 0 || side == 4)
                return va[0];
            else if (side == 2 || side == 6)
                return va[2];
            return 0;
        }
        public static float getSlabCoord(float[] va, int vaStart, int side)
        {
            if (side == 0 || side == 4)
                return va[vaStart+0];
            else if (side == 2 || side == 6)
                return va[vaStart+2];
            return 0;
        }


        public static void calcSlabEndPoints(float[] va, int vaStart, float[] vb, int vbStart, float[] bmin, float[] bmax, int side)
        {
            if (side == 0 || side == 4)
            {
                if (va[vaStart + 2] < vb[vbStart + 2])
                {
                    bmin[0] = va[vaStart + 2];
                    bmin[1] = va[vaStart + 1];
                    bmax[0] = vb[vbStart + 2];
                    bmax[1] = vb[vbStart + 1];
                }
                else
                {
                    bmin[0] = vb[vbStart + 2];
                    bmin[1] = vb[vbStart + 1];
                    bmax[0] = va[vaStart + 2];
                    bmax[1] = va[vaStart + 1];
                }
            }
            else if (side == 2 || side == 6)
            {
                if (va[vaStart + 0] < vb[0])
                {
                    bmin[0] = va[vaStart + 0];
                    bmin[1] = va[vaStart + 1];
                    bmax[0] = vb[vbStart + 0];
                    bmax[1] = vb[vbStart + 1];
                }
                else
                {
                    bmin[0] = vb[vbStart + 0];
                    bmin[1] = vb[vbStart + 1];
                    bmax[0] = va[vaStart + 0];
                    bmax[1] = va[vaStart + 1];
                }
            }
        }

        public static int computeTileHash(int x, int y, int mask)
        {
            const uint h1 = 0x8da6b343; // Large multiplicative constants;
            const uint h2 = 0xd8163841; // here arbitrarily chosen primes
            uint n = (uint)(h1 * x + h2 * y);
            return (int)(n & mask);
        }

        public static uint allocLink(dtMeshTile tile)
        {
            if (tile.linksFreeList == Detour.DT_NULL_LINK)
                return DT_NULL_LINK;
            uint link = tile.linksFreeList;
            tile.linksFreeList = tile.links[link].next;
            return link;
        }

        public static void freeLink(dtMeshTile tile, uint link)
        {
            tile.links[link].next = tile.linksFreeList;
            tile.linksFreeList = link;
        }
    }
}

