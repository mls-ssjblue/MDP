package com.example.mdp;

import android.util.Log;

public class Robot {
	private int row;
	private int col;
	private int orientationRow;
	private int orientationCol;
	
	public int NORTH = 1;
	public int SOUTH = 3;
	public int EAST = 2;
	public int WEST = 4;
	

	
	private int direction;
	

	
	public Robot(int x, int y, int OriX, int OriY){
		this.row = x;
		this.col = y;
		orientationRow = OriX;
		orientationCol = OriY;
		//getDirection();
		Log.i("direction", String.valueOf(Global.direction));
		
	}
	
	public int getDirection(){
		if (row < orientationRow) {
			return SOUTH;
			
		}
		// 
		else if (row > orientationRow) {
			return NORTH;
		}
		//
		else if (col > orientationCol) {
			return WEST;
		}

		else {
			return EAST;
		}

	}
	
	public int getRow() {
		return row;
	}
	public void setRow(int x) {
		this.row = x;
	}
	public int getCol() {
		return col;
	}
	public void setCol(int y) {
		this.col = y;
	}
	public int getOrientationRow() {
		return orientationRow;
	}
	public void setOrientationRow(int orientationX) {
		this.orientationRow = orientationX;
	}
	public int getOrientationCol() {
		return orientationCol;
	}
	public void setOrientationCol(int orientationY) {
		this.orientationCol = orientationY;
	}
	
}
