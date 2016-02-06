package com.example.mdp;

import java.util.ArrayList;
import java.util.Timer;
import java.util.TimerTask;

import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.support.v7.app.ActionBar;
import android.support.v7.app.ActionBarActivity;
import android.util.Log;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.CompoundButton;
import android.widget.CompoundButton.OnCheckedChangeListener;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.TextView;
import android.widget.Toast;
import android.widget.ToggleButton;

public class MainActivity extends ActionBarActivity implements
		SensorEventListener {
	private TextView robotStatus;
	private EditText xCoord;
	private EditText yCoord;
	private Button f1Btn;
	private Button f2Btn;
	private Button update;
	private Button confirm;
	private Button cali;
	private ImageButton left, right, up, down;
	private Button exploration, shortest;
	private ToggleButton autoManual;
	private int[][] map = new int[15][20];
	private String[] obs = null;
	private Grid grid;
	private ToggleButton tilt_btn;
	private int checkOri = 0;
	private Timer timer;
	private TimerTask task;

	ActionBar action;
	Robot robot = new Robot(1, 1, 1, 2);
	private static final int NORTH = 0;
	private static final int EAST = 1;
	private static final int SOUTH = 2;
	private static final int WEST = 3;

	// Intent request codes
	private static final int REQUEST_CONNECT_DEVICE = 1;
	private static final int REQUEST_ENABLE_BT = 2;

	// Message types sent from the BluetoothChatService Handler
	public static final int MESSAGE_STATE_CHANGE = 1;
	public static final int MESSAGE_READ = 2;
	public static final int MESSAGE_WRITE = 3;
	public static final int MESSAGE_DEVICE_NAME = 4;
	public static final int MESSAGE_TOAST = 5;

	// Key names received from the BluetoothCommandService Handler
	public static final String DEVICE_NAME = "device_name";
	public static final String TOAST = "toast";
	public static final String GRID = "GRID";

	// Name of the connected device
	private String mConnectedDeviceName = null;
	// Local Bluetooth adapter
	private BluetoothAdapter mBluetoothAdapter = null;
	// Member object for Bluetooth Command Service
	private BluetoothCommandService mCommandService = null;
	private StringBuffer mOutStringBuffer;

	private SharedPreferences sp;
	// public static SharedPreferences shared = getSharedPreferences("btnPref",
	// MODE_PRIVATE);

	private String mapInfo = "";
	private Editor editor;

	// //

	private SensorManager senSensorManager;
	private Sensor senAccelerometer;
	private static final int SHAKE_THRESHOLD = 600;
	private boolean isTilt = false;
	private int lastDirection = 0;

	//original
	// private int[][] lookupX = { { -2, -1, 0, 1, 2 }, { 1, 2, 2, 2, 1 },
	// { 2, 1, 0, -1, -2 }, { -1, -2, -2, -2, -1 } };
	// private int[][] lookupY = { { 1, 2, 2, 2, 1 }, { 2, 1, 0, -1, -2 },
	// { -1, -2, -2, -2, -1 }, { -2, -1, 0, 1, 2 } };
	
	
//	 private int[][] lookupX = { { 1, 2, 2, 2, 1 },  { 2, 1, 0, -1, -2 },
//			 { -1, -2, -2, -2, -1 }, { -2, -1, 0, 1, 2 } };
//	 private int[][] lookupY = {  { 2, 1, 0, -1, -2 }, { -1, -2, -2, -2, -1 },
//			 { -2, -1, 0, 1, 2 },  { 1, 2, 2, 2, 1 } };
	 
	 private int[][] lookupX = {  { -1, -2, -2, -2, -1 },   { -2, -1, 0, 1, 2 },
			 { 1, 2, 2, 2, 1 },  { 2, 1, 0, -1, -2 } };
	 private int[][] lookupY = { { -2, -1, 0, 1, 2 }, { 1, 2, 2, 2, 1 },
			 { 2, 1, 0, -1, -2 }, { -1, -2, -2, -2, -1 } };

	ArrayList<Integer> obsList = new ArrayList<Integer>();

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);

		Log.d("tag", "start");

		// senSensorManager = (SensorManager)
		// getSystemService(Context.SENSOR_SERVICE);
		// senAccelerometer = senSensorManager
		// .getDefaultSensor(Sensor.TYPE_ACCELEROMETER);
		// senSensorManager.registerListener(this, senAccelerometer,
		// SensorManager.SENSOR_DELAY_NORMAL);
		//
		// senSensorManager.unregisterListener(this);

		sp = getApplicationContext().getSharedPreferences("appData",
				MODE_PRIVATE);

		left = (ImageButton) findViewById(R.id.leftButton);
		right = (ImageButton) findViewById(R.id.rightButton);
		up = (ImageButton) findViewById(R.id.upButton);
		down = (ImageButton) findViewById(R.id.downButton);
		cali = (Button) findViewById(R.id.cali);

		grid = (Grid) findViewById(R.id.arena);

		action = getSupportActionBar();
		action.setSubtitle(R.string.title_not_connected);

		f1Btn = (Button) findViewById(R.id.F1);
		f2Btn = (Button) findViewById(R.id.F2);
		robotStatus = (TextView) findViewById(R.id.statusBar);
		confirm = (Button) findViewById(R.id.confirm_coord);

		xCoord = (EditText) findViewById(R.id.x_coordinate);
		yCoord = (EditText) findViewById(R.id.y_coordinate);

		autoManual = (ToggleButton) findViewById(R.id.auto_manual_button);
		update = (Button) findViewById(R.id.update);

		exploration = (Button) findViewById(R.id.exploration_button);
		shortest = (Button) findViewById(R.id.shortest_path_button);
		tilt_btn = (ToggleButton) findViewById(R.id.tilt_btn);

		// Get local Bluetooth adapter
		mBluetoothAdapter = BluetoothAdapter.getDefaultAdapter();

		// If the adapter is null, then Bluetooth is not supported
		if (mBluetoothAdapter == null) {
			Toast.makeText(this, "Bluetooth is not available",
					Toast.LENGTH_SHORT).show();
			finish();
			return;
		}

		f1Btn.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View v) {
				if (mCommandService.getState() == BluetoothCommandService.STATE_CONNECTED) {
					sendMessage(sp
							.getString("functionKey1", Configuration.NULL));
				}
			}
		});

		f2Btn.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View v) {
				if (mCommandService.getState() == BluetoothCommandService.STATE_CONNECTED) {
					sendMessage(sp
							.getString("functionKey2", Configuration.NULL));
				}
			}
		});

		update.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View v) {
				if (mCommandService.getState() == BluetoothCommandService.STATE_CONNECTED) {
					// sendMessage(GRID);

					grid.updateUI();
				}

			}
		});

		cali.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				sendMessage("cali");
			}
		});

		up.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				checkOri = robot.getDirection();
				// down
				if (checkOri == 3) {
					sendMessage("d");
					sendMessage("w");
				} else
					sendMessage("w");

				robotStatus.setText("MOVING UP");

			}
		});

		down.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				checkOri = robot.getDirection();
				// up
				if (checkOri == 1) {
					sendMessage("d");
					sendMessage("s");
				} else
					sendMessage("s");

				robotStatus.setText("MOVING DOWN");
			}
		});

		left.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				checkOri = robot.getDirection();
				// right
				if (checkOri == 2) {
					sendMessage("w");
					sendMessage("a");
				} else
					sendMessage("a");

				robotStatus.setText("MOVING LEFT");
			}
		});

		right.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				checkOri = robot.getDirection();
				// left
				if (checkOri == 4) {
					sendMessage("w");
					sendMessage("d");
				} else
					sendMessage("d");

				robotStatus.setText("MOVING RIGHT");
			}
		});

		autoManual.setOnCheckedChangeListener(new OnCheckedChangeListener() {
			@Override
			public void onCheckedChanged(CompoundButton buttonView,
					boolean isChecked) {
				// Auto
				if (isChecked) {
					update.setEnabled(false);
					task = new TimerTask() {
						public void run() {
							grid.updateMap(obsList, robot);
							grid.updateUI();
						};
					};

					timer = new Timer("myTimer");
					timer.scheduleAtFixedRate(task, 0, 2000);
				}

				// Manual
				else {
					timer.cancel();
					update.setEnabled(true);
					grid.updateMap(obsList, robot);

				}
			}
		});

		confirm.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View v) {
				sendMessage("start " + xCoord.getText() + " "
						+ yCoord.getText());
				robot.setRow(Integer.valueOf(xCoord.getText().toString()));
				robot.setCol(Integer.valueOf(yCoord.getText().toString()));
				// updateRobot(map);
				grid.invalidate();
			}
		});

		tilt_btn.setOnCheckedChangeListener(new OnCheckedChangeListener() {

			@Override
			public void onCheckedChanged(CompoundButton buttonView,
					boolean isChecked) {
				if (isChecked) {
					onResume();
				} else {
					onPause();
				}

			}
		});

		exploration.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				sendMessage("exp");

			}
		});

		shortest.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				sendMessage("sp");

			}
		});

	}

	//
	@Override
	protected void onStart() {
		super.onStart();
		Log.d("dd", "start of activity");
		if (!mBluetoothAdapter.isEnabled()) {
			Intent enableIntent = new Intent(
					BluetoothAdapter.ACTION_REQUEST_ENABLE);
			startActivityForResult(enableIntent, REQUEST_ENABLE_BT);
		}
		// otherwise set up the command service
		else {
			if (mCommandService == null)
				setupCommand();
		}
		// senSensorManager.unregisterListener(this);

	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		MenuInflater inflater = getMenuInflater();
		inflater.inflate(R.menu.menu_main, menu);
		return true;
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
		case R.id.bluetooth:
			// implement the bluetooth on action bar
			// openBluetooth();
			Intent intent = new Intent(this, DeviceListActivity.class);
			startActivityForResult(intent, REQUEST_CONNECT_DEVICE);
			return true;
		case R.id.settings:
			Intent configIntent = new Intent(this, Configuration.class);
			startActivity(configIntent);
			// implement the settings on action bar
			// openSettings();
			return true;
		case R.id.quit:
			quitApp();
			return true;

		default:
			return super.onOptionsItemSelected(item);
		}
	}

	public void onActivityResult(int requestCode, int resultCode, Intent data) {
		switch (requestCode) {
		case REQUEST_CONNECT_DEVICE:
			// When DeviceListActivity returns with a device to connect
			if (resultCode == Activity.RESULT_OK) {
				// Get the device MAC address
				String address = data.getExtras().getString(
						DeviceListActivity.EXTRA_DEVICE_ADDRESS);
				// Get the BLuetoothDevice object
				BluetoothDevice device = mBluetoothAdapter
						.getRemoteDevice(address);
				// Attempt to connect to the device
				Log.d(device.getName(), "about to connect");
				action.setSubtitle("Result OK");
				mCommandService.connect(device);
			}
			break;
		case REQUEST_ENABLE_BT:
			// When the request to enable Bluetooth returns
			if (resultCode == Activity.RESULT_OK) {
				// Bluetooth is now enabled, so set up a chat session
				setupCommand();
			} else {
				// User did not enable Bluetooth or an error occured
				Toast.makeText(this, R.string.bt_not_enabled_leaving,
						Toast.LENGTH_SHORT).show();
				finish();
			}
		}
	}

	private final Handler mHandler = new Handler() {

		@Override
		public void handleMessage(Message msg) {
			switch (msg.what) {
			case MESSAGE_STATE_CHANGE:
				switch (msg.arg1) {
				case BluetoothCommandService.STATE_CONNECTED:
					action.setSubtitle(R.string.title_connected_to);
					action.setSubtitle(mConnectedDeviceName);
					break;
				case BluetoothCommandService.STATE_CONNECTING:
					action.setSubtitle(R.string.title_connecting);
					break;
				case BluetoothCommandService.STATE_LISTEN:
				case BluetoothCommandService.STATE_NONE:
					action.setSubtitle(R.string.title_not_connected);
					break;
				}
				break;
			case MESSAGE_DEVICE_NAME:
				// save the connected device's name
				mConnectedDeviceName = msg.getData().getString(DEVICE_NAME);
				Toast.makeText(getApplicationContext(),
						"Connected to " + mConnectedDeviceName,
						Toast.LENGTH_SHORT).show();
				break;
			case MESSAGE_TOAST:
				Toast.makeText(getApplicationContext(),
						msg.getData().getString(TOAST), Toast.LENGTH_SHORT)
						.show();
				break;
			case MESSAGE_READ:
				byte[] readBuf = (byte[]) msg.obj;
				// construct a string from the valid bytes in the buffer
				mapInfo = new String(readBuf, 0, msg.arg1);

				receiveFromRpi();

				break;
			}
		}
	};

	private void setupCommand() {
		// Initialize the BluetoothChatService to perform bluetooth connections
		Log.d("setup", "setupcommand");
		mCommandService = new BluetoothCommandService(this, mHandler);
		// mCommandService.start();
		mOutStringBuffer = new StringBuffer("");
		Log.d("end of setup", "setupcommand");
	}

	private void sendMessage(String message) {
		if (mCommandService.getState() != BluetoothCommandService.STATE_CONNECTED) {
			Toast.makeText(this, R.string.title_not_connected,
					Toast.LENGTH_SHORT).show();

			return;
		}

		// Check that there's actually something to send
		if (message.length() > 0) {
			// Get the message bytes and tell the BluetoothChatService to write
			byte[] send = message.getBytes();
			mCommandService.write(send);

			// Reset out string buffer to zero and clear the edit text field
			mOutStringBuffer.setLength(0);
			// mOutEditText.setText(mOutStringBuffer);
		}
	}

	private void receiveFromRpi() {
		obsList = new ArrayList<Integer>();
		Log.d("receive", mapInfo);
		int robotX = Integer.valueOf(mapInfo.substring(1, 3));
		int robotY = Integer.valueOf(mapInfo.substring(3, 5));
		int direction = Integer.parseInt(mapInfo.substring(5, 6));
		String posStr = mapInfo.substring(6, 11);

		// add obstacles
		for (int i = 0; i < posStr.length(); i++) {
			int x = Character.getNumericValue(posStr.charAt(i));
			if (x == 1) {
				if (robotX + lookupX[direction][i] > 0  && robotX + lookupX[direction][i] <= 15
						&& robotY + lookupY[direction][i] > 0 && robotY + lookupY[direction][i] <= 20) {
					obsList.add(robotX + lookupX[direction][i]);
					obsList.add(robotY + lookupY[direction][i]);
				}
			}
		}

		getOrientation(robotX, robotY, direction);
		Log.d("Direction", String.valueOf(direction));

		// for map (-1)
		if ((robotX - 1) >= 0 && (robotX - 1 < 15) && (robotY - 1) >= 0
				&& (robotY - 1 < 20) && (robot.getOrientationRow() - 1) >= 0
				&& (robot.getOrientationRow() - 1) < 15
				&& (robot.getOrientationCol() - 1) >= 0
				&& (robot.getOrientationCol() - 1) < 20) {

			robot.setRow(robotX - 1);
			robot.setCol(robotY - 1);
			robot.setOrientationRow(robot.getOrientationRow() - 1);
			robot.setOrientationCol(robot.getOrientationCol() - 1);

			Log.d("Obstacles", obsList.toString());

			grid.updateMap(obsList, robot);
		}
	}

	private void getOrientation(int x, int y, int direction) {

		switch (direction) {

		case NORTH:
			robot.setOrientationRow(x - 1);
			robot.setOrientationCol(y);
			Log.d("North", "N");
			break;

		case EAST:
			robot.setOrientationRow(x);
			robot.setOrientationCol(y + 1);
			Log.d("East", "E");
			break;

		case SOUTH:
			robot.setOrientationRow(x + 1);
			robot.setOrientationCol(y);
			Log.d("South", "S");
			break;

		case WEST:
			robot.setOrientationRow(x);
			robot.setOrientationCol(y - 1);
			Log.d("West", "W");
			break;
		}
		Log.d("head Row", String.valueOf(robot.getOrientationRow()));
		Log.d("head Col", String.valueOf(robot.getOrientationCol()));
	}

	private void quitApp() {
		System.exit(0);
	}

	public void greyOutButton(View view) {
		// Button button = (Button) findViewById(R.id.update);
		// button.setEnabled(false);
	}

	@Override
	public void onSensorChanged(SensorEvent event) {
		Sensor mySensor = event.sensor;
		final double alpha = 5.0;
		double[] gravity = { 0, 0, 0 };

		if (mySensor.getType() == Sensor.TYPE_ACCELEROMETER) {
			double x = event.values[0];
			double y = event.values[1];
			double z = event.values[2];

			if (x > (-2) && x < (2) && y > (-2) && y < (2)) {
				// do nothing
				if (lastDirection != 0) {
					lastDirection = 0;
					Log.i("normal", "Water Level");
				}
			} else {
				if (x < -2) {
					if (lastDirection != 1) {
						lastDirection = 1;
						Log.i("right", "Right");
						sendMessage("d");
					}
				} else if (x > 2) {
					if (lastDirection != 2) {
						lastDirection = 2;
						Log.i("left", "Left");
						sendMessage("a");
					}
				}

				if (y < -2) {
					if (lastDirection != 3) {
						lastDirection = 3;
						Log.i("up", "Up");
						sendMessage("w");
					}
				} else if (y > 2) {
					if (lastDirection != 4) {
						lastDirection = 4;
						Log.i("down", "Down");
						sendMessage("s");
					}
				}
				onResume();
			}
		}

	}

	public void tiltLeft() {

		Toast.makeText(getApplicationContext(), "Tilt Left", Toast.LENGTH_SHORT)
				.show();
		onPause();
	}

	public void tiltRight() {

		Toast.makeText(getApplicationContext(), "Tilt Right",
				Toast.LENGTH_SHORT).show();
		onPause();
	}

	public void moveUp() {

		Toast.makeText(getApplicationContext(), "Move Up", Toast.LENGTH_SHORT)
				.show();
		onPause();
	}

	public void moveDown() {

		Toast.makeText(getApplicationContext(), "Move Down", Toast.LENGTH_SHORT)
				.show();
		onPause();
	}

	@Override
	protected void onPause() {
		super.onPause();
		// senSensorManager.unregisterListener(this);
		Log.d("onPause", "");
	}

	@Override
	protected void onResume() {

		super.onResume();
		// senSensorManager.registerListener(this, senAccelerometer,
		// SensorManager.SENSOR_DELAY_NORMAL);
	}

	@Override
	public void onAccuracyChanged(Sensor sensor, int accuracy) {
		// TODO Auto-generated method stub

	}

}
