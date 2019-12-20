using System;
using System.Drawing;

namespace SmartSweepers
{
	internal struct Matrix2D
	{
		private struct S2DMatrix
		{
			public double _11, _12, _13;
			public double _21, _22, _23;
			public double _31, _32, _33;

			public override string ToString()
			{
				string result =
					_11 + "  " + _12 + "  " + _13 + " \n" +
					_21 + "  " + _22 + "  " + _23 + " \n" +
					_31 + "  " + _32 + "  " + _33 + " \n";

				return result;
			}
		}

		S2DMatrix m_Matrix;

		public Matrix2D(int i)
		{
			//initialize the matrix to an identity matrix
			m_Matrix = default(S2DMatrix);
			Identity();
		}

		//create an identity matrix
		public void Identity()
		{
			m_Matrix._11 = 1; m_Matrix._12 = 0; m_Matrix._13 = 0;
			m_Matrix._21 = 0; m_Matrix._22 = 1; m_Matrix._23 = 0;
			m_Matrix._31 = 0; m_Matrix._32 = 0; m_Matrix._33 = 1;
		}

		//create a transformation matrix
		public void Translate(double x, double y)
		{
			S2DMatrix mat;

			mat._11 = 1; mat._12 = 0; mat._13 = 0;

			mat._21 = 0; mat._22 = 1; mat._23 = 0;

			mat._31 = x; mat._32 = y; mat._33 = 1;

			//and multiply
			S2DMatrixMultiply(mat);
		}

		public void Translate(Vector2D vPos)
		{
			S2DMatrix mat;

			mat._11 = 1; mat._12 = 0; mat._13 = 0;

			mat._21 = 0; mat._22 = 1; mat._23 = 0;

			mat._31 = vPos.X; mat._32 = vPos.Y; mat._33 = 1;

			//and multiply
			S2DMatrixMultiply(mat);
		}

		//create a scale matrix
		public void Scale(double xScale, double yScale)
		{
			S2DMatrix mat;

			mat._11 = xScale; mat._12 = 0; mat._13 = 0;

			mat._21 = 0; mat._22 = yScale; mat._23 = 0;

			mat._31 = 0; mat._32 = 0; mat._33 = 1;

			//and multiply
			S2DMatrixMultiply(mat);
		}

		//create a rotation matrix
		public void Rotate(double rotation)
		{
			S2DMatrix mat;

			double Sin = Math.Sin(rotation);
			double Cos = Math.Cos(rotation);

			mat._11 = Cos; mat._12 = Sin; mat._13 = 0;

			mat._21 = -Sin; mat._22 = Cos; mat._23 = 0;

			mat._31 = 0; mat._32 = 0; mat._33 = 1;

			//and multiply
			S2DMatrixMultiply(mat);
		}

		//multiplies m_Matrix with mIn
		//multiply two matrices together
		private void S2DMatrixMultiply(S2DMatrix mIn)
		{
			S2DMatrix mat_temp;

			//first row
			mat_temp._11 = (m_Matrix._11 * mIn._11) + (m_Matrix._12 * mIn._21) + (m_Matrix._13 * mIn._31);
			mat_temp._12 = (m_Matrix._11 * mIn._12) + (m_Matrix._12 * mIn._22) + (m_Matrix._13 * mIn._32);
			mat_temp._13 = (m_Matrix._11 * mIn._13) + (m_Matrix._12 * mIn._23) + (m_Matrix._13 * mIn._33);

			//second
			mat_temp._21 = (m_Matrix._21 * mIn._11) + (m_Matrix._22 * mIn._21) + (m_Matrix._23 * mIn._31);
			mat_temp._22 = (m_Matrix._21 * mIn._12) + (m_Matrix._22 * mIn._22) + (m_Matrix._23 * mIn._32);
			mat_temp._23 = (m_Matrix._21 * mIn._13) + (m_Matrix._22 * mIn._23) + (m_Matrix._23 * mIn._33);

			//third
			mat_temp._31 = (m_Matrix._31 * mIn._11) + (m_Matrix._32 * mIn._21) + (m_Matrix._33 * mIn._31);
			mat_temp._32 = (m_Matrix._31 * mIn._12) + (m_Matrix._32 * mIn._22) + (m_Matrix._33 * mIn._32);
			mat_temp._33 = (m_Matrix._31 * mIn._13) + (m_Matrix._32 * mIn._23) + (m_Matrix._33 * mIn._33);

			m_Matrix = mat_temp;
		}

		//applies a 2D transformation matrix to a array of PointFs
		public void TransformPoints(ref PointF[] vPoint)
		{
			for (int i = 0; i < vPoint.Length; ++i)
			{
				//PointF tmpPt = default(PointF);
				//tmpPt.X = (float)((m_Matrix._11 * vPoint[i].X) + (m_Matrix._21 * vPoint[i].Y) + (m_Matrix._31));
				//tmpPt.Y = (float)((m_Matrix._12 * vPoint[i].X) + (m_Matrix._22 * vPoint[i].Y) + (m_Matrix._32));
				//vPoint[i] = tmpPt;

				float X = (float)((m_Matrix._11 * vPoint[i].X) + (m_Matrix._21 * vPoint[i].Y) + (m_Matrix._31));
				vPoint[i].Y = (float)((m_Matrix._12 * vPoint[i].X) + (m_Matrix._22 * vPoint[i].Y) + (m_Matrix._32));
				vPoint[i].X = X;
			}
		}

	}
}