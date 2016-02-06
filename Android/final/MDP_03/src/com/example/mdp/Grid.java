package com.example.mdp;

import java.util.ArrayList;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Matrix;
import android.graphics.Paint;
import android.graphics.RectF;
import android.util.AttributeSet;
import android.util.Log;
import android.view.SurfaceHolder;
import android.view.SurfaceView;

public class Grid extends SurfaceView implements SurfaceHolder.Callback,
		Runnable {

	private SurfaceHolder holder;
	private Thread mazeThread;

	private final int row = 15, col = 20;
	private int leftCoord, topCoord, rightCoord, botCoord;
	private int gridSize, robotSize;
	private Bitmap robot, bMapScaled, robotPos;
	private boolean running = false;
	private int[][] map = new int[15][20];
	private Robot r = null;

	public Grid(Context context, AttributeSet attrs) {
		super(context, attrs);
		init();
	}

	public Grid(Context context) {
		super(context);
		init();
	}

	public void init() {
		holder = getHolder();
		holder.addCallback(this);
		robot = BitmapFactory
				.decodeResource(getResources(), R.drawable.android);
		Log.d("Grid ", "grid");

		r = new Robot(1, 1, 1, 2);

		for (int i = 0; i < 15; i++) {
			for (int j = 0; j < 20; j++) {
				map[i][j] = 0;
			}
		}
	}

	@Override
	public void surfaceCreated(SurfaceHolder holder) {
		running = true;
	}

	@Override
	public void surfaceChanged(SurfaceHolder holder, int format, int width,
			int height) {
		running = true;
		mazeThread = new Thread(this);
		mazeThread.start();

	}

	@Override
	public void surfaceDestroyed(SurfaceHolder holder) {
		// TODO Auto-generated method stub

	}

	@Override
	protected void onDraw(Canvas canvas) {
		super.onDraw(canvas);
		// drawMap(canvas);

	}

	@Override
	public void run() {
		while (running) {
			Canvas canvas = null;
			try {
				canvas = holder.lockCanvas();
				synchronized (holder) {
					if (canvas != null) {
						drawMap(canvas);
					}
				}
			} finally {
				if (canvas != null) {
					holder.unlockCanvasAndPost(canvas);
				}
			}
		}
	}

	public void updateMap(ArrayList<Integer> ob, Robot r) {
		if (ob != null && !ob.isEmpty() && ob.size() > 1) {
			Log.d("Grid d", String.valueOf(ob.size()));
			for (int i = 0; i < ob.size(); i += 2) {
				Log.d("tag", String.valueOf( (ob.get(i) - 1)));
				map[ob.get(i) - 1][ob.get(i + 1) - 1] = 1;
				Log.d("OB", ob.toString());
				// Log.d("map Update", String.valueOf(map[ob.get(i)][ob.get(i +
				// 1)]));
			}
		}
		this.r = r;
	}

	public void updateUI() {
		running = true;
		run();
	}

	public void drawMap(Canvas canvas) {
		Log.d("Draw Map ", "draw map");
		Log.d("Robot X", String.valueOf(r.getOrientationRow()));

		gridSize = Math.min(canvas.getHeight() / row, canvas.getWidth() / col);
		robotSize = gridSize * 3;
		int xOffset = (canvas.getHeight() - (gridSize * row)) / 2;
		int yOffset = (canvas.getWidth() - (gridSize * col)) / 2;

		// /
		Matrix turnLeft = new Matrix();
		turnLeft.postRotate(-90);
		Matrix turnRight = new Matrix();
		turnRight.postRotate(90);

		bMapScaled = Bitmap.createScaledBitmap(robot, robotSize, robotSize,
				true);
		robotPos = Bitmap.createBitmap(bMapScaled, 0, 0, robotSize, robotSize,
				turnRight, false);

		RectF rect;
		Paint rectColor = new Paint();

		for (int x = 0; x < row; x++) {
			for (int y = 0; y < col; y++) {
				// Log.d("loop X", String.valueOf(x));
				leftCoord = yOffset + gridSize * y;
				topCoord = xOffset + gridSize * x;
				rightCoord = yOffset + gridSize * (y + 1);
				botCoord = xOffset + gridSize * (x + 1);
				rect = new RectF(leftCoord, topCoord, rightCoord, botCoord);

				
				if (x == r.getRow() && y == r.getCol()) {

					rectColor.setColor(Color.BLUE);
					rectColor.setStyle(Paint.Style.FILL);
					canvas.drawRect(rect, rectColor);
					rectColor.setColor(Color.DKGRAY);
					rectColor.setStrokeWidth(2);
					rectColor.setStyle(Paint.Style.STROKE);
					canvas.drawRect(rect, rectColor);
					continue;
				}

				if (x == r.getOrientationRow() && y == r.getOrientationCol()) {
					Log.d("red X", String.valueOf(x));
					rectColor.setColor(Color.RED);
					rectColor.setStyle(Paint.Style.FILL);
					canvas.drawRect(rect, rectColor);
					rectColor.setColor(Color.DKGRAY);
					rectColor.setStrokeWidth(2);
					rectColor.setStyle(Paint.Style.STROKE);
					canvas.drawRect(rect, rectColor);
					continue;
				}

				if (Math.abs(x - r.getRow()) <= 1
						&& Math.abs(y - r.getCol()) <= 1) {
					rectColor.setColor(Color.YELLOW);
					rectColor.setStyle(Paint.Style.FILL);
					canvas.drawRect(rect, rectColor);
					rectColor.setColor(Color.DKGRAY);
					rectColor.setStrokeWidth(2);
					rectColor.setStyle(Paint.Style.STROKE);
					canvas.drawRect(rect, rectColor);
					continue;
				}
				
				if (map[x][y] == 1) {
					rectColor.setColor(Color.BLACK);
					rectColor.setStyle(Paint.Style.FILL);
					canvas.drawRect(rect, rectColor);
					rectColor.setColor(Color.DKGRAY);
					rectColor.setStrokeWidth(2);
					rectColor.setStyle(Paint.Style.STROKE);
					canvas.drawRect(rect, rectColor);
					continue;
				}

				rectColor.setColor(Color.WHITE);
				rectColor.setStyle(Paint.Style.FILL);
				canvas.drawRect(rect, rectColor);
				rectColor.setColor(Color.DKGRAY);
				rectColor.setStrokeWidth(2);
				rectColor.setStyle(Paint.Style.STROKE);
				canvas.drawRect(rect, rectColor);

			}
		}

		// canvas.drawBitmap(robotPos, yOffset + gridSize * xPos, xOffset +
		// gridSize * yPos, null);
		running = false;
	}

}
