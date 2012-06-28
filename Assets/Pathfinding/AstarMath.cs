//Thanks to whoever wrote the Matrix class on the wiki, it has been a great help.

using UnityEngine;
using System.Collections;
using AstarClasses;

namespace AstarMath {

	public class Matrix
	{
		public float[] m;
	
		public Matrix()
		{	 
			LoadIdentity();
		}	 
	
		// Loads this matrix with an identity matrix
	
		public void LoadIdentity()
		{
			m = new float[ 16 ];
	
			for( int x = 0 ; x < 16 ; ++x )
			{
				m[ x ] = 0;
			}	 
	
			m [ 0 ] = 1;
			m [ 5 ] = 1;
			m [ 10 ] = 1;
			m [ 15 ] = 1;
		}
	
		// Returns a translation matrix along the XYZ axes
	
		public static Matrix Translate( float _X, float _Y, float _Z )
		{
			Matrix wk = new Matrix();
	
			wk.m [ 12 ] = _X;
			wk.m [ 13 ] = _Y;
			wk.m [ 14 ] = _Z;
	
			return wk;
		}
		
		public Matrix translate( float _X, float _Y, float _Z )
		{
			m [ 12 ] = _X;
			m [ 13 ] = _Y;
			m [ 14 ] = _Z;
			return this;
		}
	
		// Returns a rotation matrix around the X axis
	
		public static Matrix RotateX( float _Degree )
		{
			Matrix wk = new Matrix();
	
			if( _Degree == 0 )
			{
				return wk;
			}
	
			float C = Mathf.Cos( _Degree * Mathf.Deg2Rad );
			float S = Mathf.Sin( _Degree * Mathf.Deg2Rad );
	
			wk.m [ 5 ] = C;
			wk.m [ 6 ] = S;
			wk.m [ 9 ] = -S;
			wk.m [ 10 ] = C;
	
			return wk;
		}
	
		// Returns a rotation matrix around the Y axis
	
		public static Matrix RotateY( float _Degree )
		{
			Matrix wk = new Matrix();
	
			if( _Degree == 0 )
			{
				return wk;
			}
	
			float C = Mathf.Cos( _Degree * Mathf.Deg2Rad );
			float S = Mathf.Sin( _Degree * Mathf.Deg2Rad );
	
			wk.m [ 0 ] = C;
			wk.m [ 2 ] = -S;
			wk.m [ 8 ] = S;
			wk.m [ 10 ] = C;
	
			return wk;
		}
	
		// Returns a rotation matrix around the Z axis
	
		public static Matrix RotateZ( float _Degree )
		{
			Matrix wk = new Matrix();
	
			if( _Degree == 0 )
			{
				return wk;
			}
	
			float C = Mathf.Cos( _Degree * Mathf.Deg2Rad );
			float S = Mathf.Sin( _Degree * Mathf.Deg2Rad );
	
			wk.m [ 0 ] = C;
			wk.m [ 1 ] = S;
			wk.m [ 4 ] = -S;
			wk.m [ 5 ] = C;
	
			return wk;
		}
	
		// Returns a scale matrix uniformly scaled on the XYZ axes
	
		public static Matrix Scale( float _In )
		{
			return Matrix.Scale( _In, _In, _In );
		}
	
		// Returns a scale matrix scaled on the XYZ axes
	
		public static Matrix Scale( float _X, float _Y, float _Z )
		{
			Matrix wk = new Matrix();
	
			wk.m [ 0 ] = _X;
			wk.m [ 5 ] = _Y;
			wk.m [ 10 ] = _Z;
	
			return wk;
		}
	
		// Transforms a vector with this matrix
	
		public Vector3 TransformVector( Vector3 _V )
		{
			Vector3 vtx = new Vector3(0,0,0);
	
			vtx.x = ( _V.x * m [ 0 ] ) + ( _V.y * m [ 4 ] ) + ( _V.z * m [ 8 ] ) + m [ 12 ];
			vtx.y = ( _V.x * m [ 1 ] ) + ( _V.y * m [ 5 ] ) + ( _V.z * m [ 9 ] ) + m [ 13 ];
			vtx.z = ( _V.x * m [ 2 ] ) + ( _V.y * m [ 6 ] ) + ( _V.z * m [ 10 ] ) + m [ 14 ];
	
			return vtx;
		}
	
		// Overloaded operators
	
		public static Matrix operator *( Matrix _A, Matrix _B )
		{
			Matrix wk = new Matrix();
	
			wk.m [ 0 ] = _A.m [ 0 ] * _B.m [ 0 ] + _A.m [ 4 ] * _B.m [ 1 ] + _A.m [ 8 ] * _B.m [ 2 ] + _A.m [ 12 ] * _B.m [ 3 ];
			wk.m [ 4 ] = _A.m [ 0 ] * _B.m [ 4 ] + _A.m [ 4 ] * _B.m [ 5 ] + _A.m [ 8 ] * _B.m [ 6 ] + _A.m [ 12 ] * _B.m [ 7 ];
			wk.m [ 8 ] = _A.m [ 0 ] * _B.m [ 8 ] + _A.m [ 4 ] * _B.m [ 9 ] + _A.m [ 8 ] * _B.m [ 10 ] + _A.m [ 12 ] * _B.m [ 11 ];
			wk.m [ 12 ] = _A.m [ 0 ] * _B.m [ 12 ] + _A.m [ 4 ] * _B.m [ 13 ] + _A.m [ 8 ] * _B.m [ 14 ] + _A.m [ 12 ] * _B.m [ 15 ];
	
			wk.m [ 1 ] = _A.m [ 1 ] * _B.m [ 0 ] + _A.m [ 5 ] * _B.m [ 1 ] + _A.m [ 9 ] * _B.m [ 2 ] + _A.m [ 13 ] * _B.m [ 3 ];
			wk.m [ 5 ] = _A.m [ 1 ] * _B.m [ 4 ] + _A.m [ 5 ] * _B.m [ 5 ] + _A.m [ 9 ] * _B.m [ 6 ] + _A.m [ 13 ] * _B.m [ 7 ];
			wk.m [ 9 ] = _A.m [ 1 ] * _B.m [ 8 ] + _A.m [ 5 ] * _B.m [ 9 ] + _A.m [ 9 ] * _B.m [ 10 ] + _A.m [ 13 ] * _B.m [ 11 ];
			wk.m [ 13 ] = _A.m [ 1 ] * _B.m [ 12 ] + _A.m [ 5 ] * _B.m [ 13 ] + _A.m [ 9 ] * _B.m [ 14 ] + _A.m [ 13 ] * _B.m [ 15 ];
	
			wk.m [ 2 ] = _A.m [ 2 ] * _B.m [ 0 ] + _A.m [ 6 ] * _B.m [ 1 ] + _A.m [ 10 ] * _B.m [ 2 ] + _A.m [ 14 ] * _B.m [ 3 ];
			wk.m [ 6 ] = _A.m [ 2 ] * _B.m [ 4 ] + _A.m [ 6 ] * _B.m [ 5 ] + _A.m [ 10 ] * _B.m [ 6 ] + _A.m [ 14 ] * _B.m [ 7 ];
			wk.m [ 10 ] = _A.m [ 2 ] * _B.m [ 8 ] + _A.m [ 6 ] * _B.m [ 9 ] + _A.m [ 10 ] * _B.m [ 10 ] + _A.m [ 14 ] * _B.m [ 11 ];
			wk.m [ 14 ] = _A.m [ 2 ] * _B.m [ 12 ] + _A.m [ 6 ] * _B.m [ 13 ] + _A.m [ 10 ] * _B.m [ 14 ] + _A.m [ 14 ] * _B.m [ 15 ];
	
			wk.m [ 3 ] = _A.m [ 3 ] * _B.m [ 0 ] + _A.m [ 7 ] * _B.m [ 1 ] + _A.m [ 11 ] * _B.m [ 2 ] + _A.m [ 15 ] * _B.m [ 3 ];
			wk.m [ 7 ] = _A.m [ 3 ] * _B.m [ 4 ] + _A.m [ 7 ] * _B.m [ 5 ] + _A.m [ 11 ] * _B.m [ 6 ] + _A.m [ 15 ] * _B.m [ 7 ];
			wk.m [ 11 ] = _A.m [ 3 ] * _B.m [ 8 ] + _A.m [ 7 ] * _B.m [ 9 ] + _A.m [ 11 ] * _B.m [ 10 ] + _A.m [ 15 ] * _B.m [ 11 ];
			wk.m [ 15 ] = _A.m [ 3 ] * _B.m [ 12 ] + _A.m [ 7 ] * _B.m [ 13 ] + _A.m [ 11 ] * _B.m [ 14 ] + _A.m [ 15 ] * _B.m [ 15 ];
	
			return wk;
		}
	}
	
	class Arrays {
		public static bool Contains (Node[] arr,Node target) {
			for (int i=0;i<arr.Length;i++) {
				if (arr[i] == target) {
					return true;
				}
			}
			return false;
		}
	}
		
	class AstarSplines {
		public static Vector3 CatmullRom(Vector3 previous,Vector3 start, Vector3 end, Vector3 next, float elapsedTime) {
			// References used:
			// p.266 GemsV1
			//
			// tension is often set to 0.5 but you can use any reasonable value:
			// http://www.cs.cmu.edu/~462/projects/assn2/assn2/catmullRom.pdf
			//
			// bias and tension controls:
			// http://local.wasp.uwa.edu.au/~pbourke/miscellaneous/interpolation/
		
			float percentComplete = elapsedTime;
			float percentCompleteSquared = percentComplete * percentComplete;
			float percentCompleteCubed = percentCompleteSquared * percentComplete;
			
			/*return previous * (-0.5F*percentCompleteCubed +
							 percentCompleteSquared -
							 tension*percentComplete) +
							 
			start * ((2-tension) *percentCompleteCubed +
					 (tension - 3)*percentCompleteSquared + 1.0F) +
					 
			end * ((tension - 2)*percentCompleteCubed +
					 2.0F *percentCompleteSquared +
					 0.5F*percentComplete) +
					 
			next * (0.5F*percentCompleteCubed -
					tension*percentCompleteSquared);*/
					
			return 
			previous * (-0.5F*percentCompleteCubed +
							 percentCompleteSquared -
							 0.5F*percentComplete) +
							 
			start * 
				(1.5F*percentCompleteCubed +
				-2.5F*percentCompleteSquared + 1.0F) +
				
			end * 
				(-1.5F*percentCompleteCubed +
				2.0F*percentCompleteSquared +
				0.5F*percentComplete) +
				
			next * 
				(0.5F*percentCompleteCubed -
				0.5F*percentCompleteSquared);
		}
		
		public static Vector3 CatmullRomOLD (Vector3 previous,Vector3 start, Vector3 end, Vector3 next, float elapsedTime) {
			// References used:
			// p.266 GemsV1
			//
			// tension is often set to 0.5 but you can use any reasonable value:
			// http://www.cs.cmu.edu/~462/projects/assn2/assn2/catmullRom.pdf
			//
			// bias and tension controls:
			// http://local.wasp.uwa.edu.au/~pbourke/miscellaneous/interpolation/
		
			float percentComplete = elapsedTime;
			float percentCompleteSquared = percentComplete * percentComplete;
			float percentCompleteCubed = percentCompleteSquared * percentComplete;
		
			return previous * (-0.5F*percentCompleteCubed +
							 percentCompleteSquared -
							 0.5F*percentComplete) +
			start * (1.5F*percentCompleteCubed +
					 -2.5F*percentCompleteSquared + 1.0F) +
			end * (-1.5F*percentCompleteCubed +
					 2.0F*percentCompleteSquared +
					 0.5F*percentComplete) +
			next * (0.5F*percentCompleteCubed -
					0.5F*percentCompleteSquared);
		}
	}
	
	public class Mathfx {
		public static Vector3 NearestPoint(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
	    {
	        Vector3 lineDirection = Vector3.Normalize(lineEnd-lineStart);
	        float closestPoint = Vector3.Dot((point-lineStart),lineDirection)/Vector3.Dot(lineDirection,lineDirection);
	        return lineStart+(closestPoint*lineDirection);
	    }
	 
	    public static Vector3 NearestPointStrict(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
	    {
	        Vector3 fullDirection = lineEnd-lineStart;
	        Vector3 lineDirection = Vector3.Normalize(fullDirection);
	        float closestPoint = Vector3.Dot((point-lineStart),lineDirection)/Vector3.Dot(lineDirection,lineDirection);
	        return lineStart+(Mathf.Clamp(closestPoint,0.0f,Vector3.Magnitude(fullDirection))*lineDirection);
	    }
	}
	
	public class Polygon {
		public static bool ContainsPoint (Vector2[] polyPoints,Vector2 p) { 
		   int j = polyPoints.Length-1; 
		   bool inside = false; 
		   
		   for (int i = 0; i < polyPoints.Length; j = i++) { 
		      if ( ((polyPoints[i].y <= p.y && p.y < polyPoints[j].y) || (polyPoints[j].y <= p.y && p.y < polyPoints[i].y)) && 
		         (p.x < (polyPoints[j].x - polyPoints[i].x) * (p.y - polyPoints[i].y) / (polyPoints[j].y - polyPoints[i].y) + polyPoints[i].x)) 
		         inside = !inside; 
		   } 
		   return inside; 
		}
	}
}
