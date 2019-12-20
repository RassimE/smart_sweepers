using System.Diagnostics;

namespace SmartSweepers
{
	internal class CTimer
	{
		private long m_CurrentTime;
		private long m_LastTime;
		private long m_NextTime;
		private long m_FrameTime;
		private long m_PerfCountFreq;

		private double m_TimeElapsed;
		private double m_TimeScale;
		private float m_FPS;

		//---------------------- default constructor ------------------------------
		//
		//-------------------------------------------------------------------------
		public CTimer()
		{
			m_FPS = 0;
			m_TimeElapsed = 0.0;
			m_FrameTime = 0;
			m_LastTime = 0;

			//how many ticks per sec do we get
			m_PerfCountFreq = Stopwatch.Frequency;

			m_TimeScale = 1.0 / m_PerfCountFreq;
		}

		//---------------------- constructor -------------------------------------
		//
		//	use to specify FPS
		//
		//-------------------------------------------------------------------------
		public CTimer(float fps)
		{
			m_FPS = fps;
			m_TimeElapsed = 0.0;
			m_FrameTime = 0;
			m_LastTime = 0;

			//how many ticks per sec do we get
			m_PerfCountFreq = Stopwatch.Frequency;

			m_TimeScale = 1.0 / m_PerfCountFreq;

			//calculate ticks per frame
			m_FrameTime = (long)(m_PerfCountFreq / m_FPS);
		}

		//------------------------Start()-----------------------------------------
		//
		//	call this immediately prior to game loop. Starts the timer (obviously!)
		//
		//--------------------------------------------------------------------------
		//whatdayaknow, this starts the timer
		public void Start()
		{
			//get the time
			m_LastTime = Stopwatch.GetTimestamp();

			//update time to render next frame
			m_NextTime = m_LastTime + m_FrameTime;

			return;
		}

		//-------------------------ReadyForNextFrame()-------------------------------
		//
		//	returns true if it is time to move on to the next frame step. To be used if
		//	FPS is set.
		//
		//----------------------------------------------------------------------------
		//determines if enough time has passed to move onto next frame
		public bool ReadyForNextFrame()
		{
			if (m_FPS == 0)
			{
				System.Windows.Forms.MessageBox.Show("No FPS set in timer", "Doh!");
				return false;
			}

			m_CurrentTime = Stopwatch.GetTimestamp();

			if (m_CurrentTime < m_NextTime)
				return false;

			m_TimeElapsed = (m_CurrentTime - m_LastTime) * m_TimeScale;
			m_LastTime = m_CurrentTime;

			//update time to render next frame
			m_NextTime = m_CurrentTime + m_FrameTime;

			return true;
		}

		//only use this after a call to the above.
		public double GetTimeElapsed() { return m_TimeElapsed; }

		//--------------------------- TimeElapsed --------------------------------
		//
		//	returns time elapsed since last call to this function. Use in main
		//	when calculations are to be based on dt.
		//
		//-------------------------------------------------------------------------
		public double TimeElapsed()
		{
			m_CurrentTime = Stopwatch.GetTimestamp();

			m_TimeElapsed = (m_CurrentTime - m_LastTime) * m_TimeScale;

			m_LastTime = m_CurrentTime;

			return m_TimeElapsed;
		}
	}
}
