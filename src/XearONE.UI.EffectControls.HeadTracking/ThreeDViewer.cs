using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using XYZ.MeshInOut;
using XYZ.model.geom;

namespace XearONE.UI.EffectControls.HeadTracking;

public class ThreeDViewer : UserControl
{
	public enum Ortho_Mode
	{
		CENTER,
		BLEFT
	}

	private RHBoundingBox boundingBox = new RHBoundingBox();

	private bool monitorLoaded;

	private List<TopoModel> topoModels;

	private Vector3 minPos;

	private Vector3 maxPos;

	private double kScaleFactor = 5.0;

	private int nYaw;

	private int nPitch;

	private int nRoll;

	private static ThreeDViewer instance = new ThreeDViewer();

	private IContainer components;

	private GLControl GL_Monitor;

	public static ThreeDViewer getInstance()
	{
		return instance;
	}

	private ThreeDViewer()
	{
		Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
		Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
		InitializeComponent();
	}

	private void GL_Monitor_Load(object sender, EventArgs e)
	{
		GL_Monitor.AllowDrop = true;
		monitorLoaded = true;
		GL.ClearColor(Color.Black);
		GL.Enable(EnableCap.DepthTest);
	}

	private void UserControlLoad(object sender, EventArgs e)
	{
		ReadSelectedFile();
	}

	private void Configure(GLControl refGLControl, Ortho_Mode ortho)
	{
		GL.ClearColor(Color.Black);
		refGLControl.VSync = false;
		GL.Clear(ClearBufferMask.ColorBufferBit);
		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		GL.ClearColor(Color.FromArgb(10, 16, 28));
		GL.MatrixMode(MatrixMode.Modelview);
		GL.LoadIdentity();
		GL.Viewport(refGLControl.Size);
		if (ortho == Ortho_Mode.CENTER)
		{
			GL.Ortho(-refGLControl.Width / 2, refGLControl.Width / 2, -refGLControl.Height / 2, refGLControl.Height / 2, -20000.0, 20000.0);
		}
		else
		{
			GL.Ortho(0.0, refGLControl.Width, 0.0, refGLControl.Height, -20000.0, 20000.0);
		}
		GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
		GL.ClearDepth(1f);
		GL.Enable(EnableCap.DepthTest);
		GL.DepthFunc(DepthFunction.Lequal);
		GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
		GL.Enable(EnableCap.LineSmooth);
		GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
	}

	private void GL_Monitor_Paint(object sender, PaintEventArgs e)
	{
		if (!monitorLoaded)
		{
			return;
		}
		Configure(GL_Monitor, Ortho_Mode.CENTER);
		float pitchAngle = 0f;
		float rollAngle = 0f;
		float yawAngle = 0f;
		try
		{
			pitchAngle = Convert.ToSingle(nPitch);
			rollAngle = Convert.ToSingle(nRoll);
			yawAngle = Convert.ToSingle(nYaw);
		}
		catch
		{
		}
		double radian = Math.PI / 180.0 * (double)yawAngle;
		GL.Rotate(pitchAngle, Math.Cos(radian), 0.0, 0.0 - Math.Sin(radian));
		GL.Rotate(rollAngle, Math.Sin(radian), 0.0, 0.0 - Math.Cos(radian));
		GL.Rotate(yawAngle, 0f, -1f, 0f);
		GL.Scale(kScaleFactor, kScaleFactor, kScaleFactor);
		GL.Translate(0f - minPos.X, 0f - minPos.Y, 0f - minPos.Z);
		GL.Translate((0f - (maxPos.X - minPos.X)) / 2f, (0f - (maxPos.Y - minPos.Y)) / 2f, (0f - (maxPos.Z - minPos.Z)) / 2f);
		if (topoModels != null)
		{
			foreach (TopoModel topoModel in topoModels)
			{
				topoModel.DrawWithMaterials();
			}
		}
		GL_Monitor.SwapBuffers();
	}

	[DllImport("user32.dll")]
	private static extern uint GetDpiForWindow(IntPtr hwnd);

	public void UpdateScaleFactor(Window parentWindow)
	{
		double scalingFactor = 1.0;
		if (parentWindow != null)
		{
			scalingFactor = (double)GetDpiForWindow(new WindowInteropHelper(parentWindow).Handle) / 96.0;
		}
		double len = boundingBox.Size.Length;
		kScaleFactor = 330.0 * scalingFactor / len;
		minPos = new Vector3(boundingBox.xMinf, boundingBox.yMinf, boundingBox.zMinf);
		maxPos = new Vector3(boundingBox.xMaxf, boundingBox.yMaxf, boundingBox.zMaxf);
		GL_Monitor.Height = Size.Height;
		GL_Monitor.Width = Size.Width;
	}

	private void ReadSelectedFile()
	{
		string modelsDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Models";
		if (Directory.Exists(modelsDir))
		{
			string[] files = Directory.GetFiles(modelsDir, "*.obj");
			if (files.Length != 0)
			{
				loadObjModelFile(files[files.Length - 1], boundingBox);
			}
		}
	}

	private int loadObjModelFile(string objFilename, RHBoundingBox boundingBox)
	{
		MeshIOObj meshIOObj = new MeshIOObj();
		topoModels = new List<TopoModel>();
		return meshIOObj.Load(objFilename, topoModels, boundingBox);
	}

	private int loadStlModel(string stlFilename, RHBoundingBox boundingBox)
	{
		MeshIOStl meshIOStl = new MeshIOStl();
		TopoModel model = new TopoModel();
		int num = meshIOStl.Load(stlFilename, model, boundingBox);
		if (num != 0)
		{
			model = null;
			Configure(GL_Monitor, Ortho_Mode.CENTER);
			GL_Monitor.SwapBuffers();
			return num;
		}
		model.SetColor(Color.Crimson);
		topoModels = new List<TopoModel>();
		topoModels.Add(model);
		return num;
	}

	public void UpdateOrientation(int yaw, int pitch, int roll)
	{
		nYaw = yaw;
		nPitch = pitch;
		nRoll = roll;
		GL_Monitor.Invalidate();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		OpenTK.Graphics.GraphicsMode graphicsMode = new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8);
		this.GL_Monitor = new OpenTK.GLControl(graphicsMode);
		base.SuspendLayout();
		this.GL_Monitor.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
		this.GL_Monitor.Location = new System.Drawing.Point(0, 0);
		this.GL_Monitor.Margin = new System.Windows.Forms.Padding(0);
		this.GL_Monitor.Name = "GL_Monitor";
		this.GL_Monitor.Size = new System.Drawing.Size(330, 250);
		this.GL_Monitor.TabIndex = 15;
		this.GL_Monitor.VSync = false;
		this.GL_Monitor.Load += new System.EventHandler(this.GL_Monitor_Load);
		this.GL_Monitor.Paint += new System.Windows.Forms.PaintEventHandler(this.GL_Monitor_Paint);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
		base.Controls.Add(this.GL_Monitor);
		base.Margin = new System.Windows.Forms.Padding(0);
		base.Name = "ThreeDViewer";
		base.Size = new System.Drawing.Size(330, 250);
		base.Load += new System.EventHandler(this.UserControlLoad);
		base.ResumeLayout(false);
	}
}
