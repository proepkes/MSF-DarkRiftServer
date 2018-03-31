/// @class dtQueryFilter
///
/// <b>The Default Implementation</b>
/// 
/// At construction: All area costs default to 1.0.  All flags are included
/// and none are excluded.
/// 
/// If a polygon has both an include and an exclude flag, it will be excluded.
/// 
/// The way filtering works, a navigation mesh polygon must have at least one flag 
/// set to ever be considered by a query. So a polygon with no flags will never
/// be considered.
///
/// Setting the include flags to 0 will result in all polygons being excluded.
///
/// <b>Custom Implementations</b>
/// 
/// DT_VIRTUAL_QUERYFILTER must be defined in order to extend this class.
/// 
/// Implement a custom query filter by overriding the virtual passFilter() 
/// and getCost() functions. If this is done, both functions should be as 
/// fast as possible. Use cached local copies of data rather than accessing 
/// your own objects where possible.
/// 
/// Custom implementations do not need to adhere to the flags or cost logic 
/// used by the default implementation.  
/// 
/// In order for A* searches to work properly, the cost should be proportional to
/// the travel distance. Implementing a cost modifier less than 1.0 is likely 
/// to lead to problems during pathfinding.
///
/// @see dtNavMeshQuery
/// 

#if DT_POLYREF64
using dtPolyRef = System.UInt64;
//using dtTileRef = System.UInt64;
#else
using dtPolyRef = System.UInt32;
//using dtTileRef = System.UInt32;
#endif

// Define DT_VIRTUAL_QUERYFILTER if you wish to derive a custom filter from dtQueryFilter.
// On certain platforms indirect or virtual function call is expensive. The default
// setting is to use non-virtual functions, the actual implementations of the functions
// are declared as inline for maximum speed. 

//#define DT_VIRTUAL_QUERYFILTER 1

namespace RecastDetour.Detour
{
    public static partial class Detour{

        public const float H_SCALE = 0.999f; // Search heuristic scale.

        /// Defines polygon filtering and traversal costs for navigation mesh query operations.
        /// @ingroup detour
        public class dtQueryFilter{
            public float[] m_areaCost = new float[DT_MAX_AREAS];		//< Cost per area type. (Used by default implementation.)
            public ushort m_includeFlags;		//< Flags for polygons that can be visited. (Used by default implementation.)
            public ushort m_excludeFlags;		//< Flags for polygons that should not be visted. (Used by default implementation.)

            public dtQueryFilter()
            {
                m_includeFlags=0xffff;
                m_excludeFlags=0;
                for (int i = 0; i < DT_MAX_AREAS; ++i)
                    m_areaCost[i] = 1.0f;
            }

            /// Returns true if the polygon can be visited.  (I.e. Is traversable.)
            ///  @param[in]		ref		The reference id of the polygon test.
            ///  @param[in]		tile	The tile containing the polygon.
            ///  @param[in]		poly  The polygon to test.
#if DT_VIRTUAL_QUERYFILTER
        bool dtQueryFilter::passFilter(const dtPolyRef /*ref*/,
							           const dtMeshTile* /*tile*/,
							           const dtPoly* poly) const
        {
	        return (poly.flags & m_includeFlags) != 0 && (poly.flags & m_excludeFlags) == 0;
        }

        float dtQueryFilter::getCost(const float* pa, const float* pb,
							         const dtPolyRef /*prevRef*/, const dtMeshTile* /*prevTile*/, const dtPoly* /*prevPoly*/,
							         const dtPolyRef /*curRef*/, const dtMeshTile* /*curTile*/, const dtPoly* curPoly,
							         const dtPolyRef /*nextRef*/, const dtMeshTile* /*nextTile*/, const dtPoly* /*nextPoly*/) const
        {
	        return dtVdist(pa, pb) * m_areaCost[curPoly.getArea()];
        }
        #else
            public bool passFilter(dtPolyRef polyRef,
                dtMeshTile tile,
                dtPoly poly)
            {
                return (poly.flags & m_includeFlags) != 0 && (poly.flags & m_excludeFlags) == 0;
            }
#endif

            /// Returns cost to move from the beginning to the end of a line segment
            /// that is fully contained within a polygon.
            ///  @param[in]		pa			The start position on the edge of the previous and current polygon. [(x, y, z)]
            ///  @param[in]		pb			The end position on the edge of the current and next polygon. [(x, y, z)]
            ///  @param[in]		prevRef		The reference id of the previous polygon. [opt]
            ///  @param[in]		prevTile	The tile containing the previous polygon. [opt]
            ///  @param[in]		prevPoly	The previous polygon. [opt]
            ///  @param[in]		curRef		The reference id of the current polygon.
            ///  @param[in]		curTile		The tile containing the current polygon.
            ///  @param[in]		curPoly		The current polygon.
            ///  @param[in]		nextRef		The refernece id of the next polygon. [opt]
            ///  @param[in]		nextTile	The tile containing the next polygon. [opt]
            ///  @param[in]		nextPoly	The next polygon. [opt]
#if DT_VIRTUAL_QUERYFILTER
        float dtQueryFilter::getCost(const float* pa, const float* pb,
							         const dtPolyRef /*prevRef*/, const dtMeshTile* /*prevTile*/, const dtPoly* /*prevPoly*/,
							         const dtPolyRef /*curRef*/, const dtMeshTile* /*curTile*/, const dtPoly* curPoly,
							         const dtPolyRef /*nextRef*/, const dtMeshTile* /*nextTile*/, const dtPoly* /*nextPoly*/) const
        {
	        return dtVdist(pa, pb) * m_areaCost[curPoly.getArea()];
        }
        #else
            public float getCost(float[] pa, float[] pb,
                dtPolyRef prevRef, dtMeshTile prevTile, dtPoly prevPoly,
                dtPolyRef curRef, dtMeshTile curTile, dtPoly curPoly,
                dtPolyRef nextRef, dtMeshTile nextTile, dtPoly nextPoly)
            {
                return dtVdist(pa, pb) * m_areaCost[curPoly.getArea()];
            }
#endif	
	
            /// @name Getters and setters for the default implementation data.
            ///@{

            /// Returns the traversal cost of the area.
            ///  @param[in]		i		The id of the area.
            /// @returns The traversal cost of the area.
            public float getAreaCost(int i) {
                return m_areaCost[i];
            }

            /// Sets the traversal cost of the area.
            ///  @param[in]		i		The id of the area.
            ///  @param[in]		cost	The new cost of traversing the area.
            public void setAreaCost(int i, float cost) {
                m_areaCost[i] = cost;
            }

            /// Returns the include flags for the filter.
            /// Any polygons that include one or more of these flags will be
            /// included in the operation.
            public ushort getIncludeFlags() {
                return m_includeFlags;
            }

            /// Sets the include flags for the filter.
            /// @param[in]		flags	The new flags.
            public void setIncludeFlags(ushort flags) {
                m_includeFlags = flags;
            }

            /// Returns the exclude flags for the filter.
            /// Any polygons that include one ore more of these flags will be
            /// excluded from the operation.
            public ushort getExcludeFlags() {
                return m_excludeFlags;
            }

            /// Sets the exclude flags for the filter.
            /// @param[in]		flags		The new flags.
            public void setExcludeFlags(ushort flags) {
                m_excludeFlags = flags;
            }

            ///@}
        }
    }
}



















