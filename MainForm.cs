using System;
using System.Drawing;
using System.Windows.Forms;

namespace SmartSweepers
{
	public partial class MainForm : Form
	{
		Controller _controller;
		CTimer _timer;
		Boolean _done;

		public MainForm()
		{
			InitializeComponent();

			Params rm = new Params();
			Size clientSize = new Size(Params.WindowWidth, Params.WindowHeight);
			this.ClientSize = clientSize;

			//Width = Params.WindowWidth;
			//Height = Params.WindowHeight;

			_done = false;

			_timer = new CTimer(Params.iFramesPerSecond);
			_controller = new Controller(this);

			_timer.Start();
			GoTimer.Enabled = true;
		}

		private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == 'f' || e.KeyChar == 'F')
				_controller.FastRenderToggle();
			else if (e.KeyChar == 'g' || e.KeyChar == 'G')
				_controller.RendererToggle();
			else if (e.KeyChar == 'r' || e.KeyChar == 'R')
				//setup the new controller
				_controller = new Controller(this);
		}

		private void MainForm_Resize(object sender, EventArgs e)
		{
			if (ClientSize.Width == 0 || ClientSize.Height == 0)
				return;
			if (_controller == null)
				return;
			_controller.ControlChangedSize(this);
			//_backBuffer = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			_done = true;
		}

		private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			_timer = null;
			_controller = null;
		}

		private void GoTimer_Tick(object sender, EventArgs e)
		{
			GoTimer.Enabled = false;
			MainLoop();
		}

		void MainLoop()
		{
			while (!_done)
			{
				bool ReadyForNextFrame = _timer.ReadyForNextFrame();
				if (ReadyForNextFrame || _controller.FastRender)
				{
					_controller.Update();

					if (ReadyForNextFrame)
						this.Refresh();
				}

				Application.DoEvents();
			}
		}

		private void paintPanel_Paint(object sender, PaintEventArgs e)
		{
			e.Graphics.Clear(Color.White);
			_controller.Render(e.Graphics);
		}
	}
}
