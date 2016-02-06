package com.example.mdp;

import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.Set;

import android.app.Activity;
import android.app.ProgressDialog;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Bundle;
import android.util.Log;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;
import android.widget.ToggleButton;

public class DeviceListActivity extends Activity {
	private ToggleButton switchBtn;
	private ListView pairedList;
	private ListView scannedList;
	private TextView bt_status;
	private ProgressDialog mProgressDlg;

	private BluetoothAdapter mBluetoothAdapter;
	private ArrayAdapter<String> pairedListAdapter;
	private ArrayAdapter<String> scannedListAdapter;
	private Set<BluetoothDevice> devicesArray;
	private ArrayList<String> pairedDevices;
	private ArrayList<BluetoothDevice> devices;
	private BroadcastReceiver receiver;
	private IntentFilter filter;
	private boolean isPaired = false;

	private static final int REQUEST_ENABLE_BT = 1;
	private static final int REQUEST_CONNECT_DEVICE = 2;

	public static String EXTRA_DEVICE_ADDRESS = "device_address";

	@Override
	protected void onCreate(Bundle savedInstanceState) {

		super.onCreate(savedInstanceState);
		setContentView(R.layout.device_list);

		// switchBtn = (ToggleButton) findViewById(R.id.switchBtn);
		pairedList = (ListView) findViewById(R.id.paired);
		scannedList = (ListView) findViewById(R.id.others);
		bt_status = (TextView) findViewById(R.id.bt_status);

		//
		// pairedList.setOnItemClickListener(this);
		pairedListAdapter = new ArrayAdapter<String>(this,
				android.R.layout.simple_list_item_1, 0);
		pairedList.setAdapter(pairedListAdapter);
		pairedList.setOnItemClickListener(listViewOnClickListener);

		scannedListAdapter = new ArrayAdapter<String>(this,
				android.R.layout.simple_list_item_1, 0);
		scannedList.setAdapter(scannedListAdapter);
		scannedList.setOnItemClickListener(listViewOnClickListener);

		devices = new ArrayList<BluetoothDevice>();
		pairedDevices = new ArrayList<String>();
		filter = new IntentFilter(BluetoothDevice.ACTION_FOUND);

		//
		mBluetoothAdapter = BluetoothAdapter.getDefaultAdapter();

		if (mBluetoothAdapter == null) {
			Toast.makeText(this, "Bluetooth is not supported",
					Toast.LENGTH_LONG).show();
			finish();
			return;
		}

		mProgressDlg = new ProgressDialog(this);

		mProgressDlg.setMessage("Scanning...");
		mProgressDlg.setCancelable(false);
		mProgressDlg.setButton(DialogInterface.BUTTON_NEGATIVE, "Cancel",
				new DialogInterface.OnClickListener() {
					@Override
					public void onClick(DialogInterface dialog, int which) {
						dialog.dismiss();
						mBluetoothAdapter.cancelDiscovery();
					}
				});

		getPairedDevices();
		mBluetoothAdapter.startDiscovery();

		//
		receiver = new BroadcastReceiver() {
			@Override
			public void onReceive(Context context, Intent intent) {
				// TODO Auto-generated method stub
				String action = intent.getAction();
				devices.clear();
				getPairedDevices();

				if (BluetoothDevice.ACTION_FOUND.equals(action)) {
					isPaired = false;
					BluetoothDevice device = intent
							.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE);

					String s = "";
					devices.add(device);
					for (int a = 0; a < pairedDevices.size(); a++) {
						Log.d("bluetooth devices",
								String.valueOf(device.getName()));

						if (device.getName().equals(pairedDevices.get(a))) {
							// append
							s = "(Paired)";

							pairedListAdapter.add(device.getName() + " " + s
									+ " " + "\n" + device.getAddress());
							pairedDevices.remove(a);
							isPaired = true;
							break;
						} else
							Log.d("device paired but ", "not visible");

					}
					if (!isPaired) {
						scannedListAdapter.add(device.getName() + " " + s + " "
								+ "\n" + device.getAddress());
					}
				}

				else if (BluetoothAdapter.ACTION_DISCOVERY_STARTED
						.equals(action)) {

					mProgressDlg.show();
					// run some code
				} else if (BluetoothAdapter.ACTION_DISCOVERY_FINISHED
						.equals(action)) {

					mProgressDlg.dismiss();
					// run some code

				} else if (BluetoothAdapter.ACTION_STATE_CHANGED.equals(action)) {
					if (mBluetoothAdapter.getState() == mBluetoothAdapter.STATE_OFF) {
						turnOnBT();
					}
				}

			}
		};

		registerReceiver(receiver, filter);
		filter = new IntentFilter(BluetoothAdapter.ACTION_DISCOVERY_STARTED);
		registerReceiver(receiver, filter);
		filter = new IntentFilter(BluetoothAdapter.ACTION_DISCOVERY_FINISHED);
		registerReceiver(receiver, filter);
		filter = new IntentFilter(BluetoothAdapter.ACTION_STATE_CHANGED);
		registerReceiver(receiver, filter);

	}

	private OnItemClickListener listViewOnClickListener = new OnItemClickListener() {
		public void onItemClick(AdapterView<?> av, View v, int position,
				long arg3) {
			// Cancel discovery because it's costly and we're about to connect
			
			mBluetoothAdapter.cancelDiscovery();

			// Get the device MAC address, which is the last 17 chars in the
			// View
			String info = ((TextView) v).getText().toString();
			String address = info.split("\n")[1];
			
			BluetoothDevice device = mBluetoothAdapter.getRemoteDevice(address);
			if (device.getBondState() == BluetoothDevice.BOND_BONDED) {
				
				Intent intent = new Intent();
				intent.putExtra(EXTRA_DEVICE_ADDRESS, address);

				// Set result and finish this Activity
				setResult(Activity.RESULT_OK, intent);
				finish();

			} else {
				showToast("Pairing...");
				pairDevice(device);
				
			}		
		}
		
	};

	public boolean createBond(BluetoothDevice btDevice) throws Exception {
		Class class1 = Class.forName("android.bluetooth.BluetoothDevice");
		Method createBondMethod = class1.getMethod("createBond");
		Boolean returnValue = (Boolean) createBondMethod.invoke(btDevice);
		return returnValue.booleanValue();
	}

	private void pairDevice(BluetoothDevice device) {
		try {
			Method method = device.getClass().getMethod("createBond",
					(Class[]) null);
			method.invoke(device, (Object[]) null);
			devices.add(device);
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	private void unpairDevice(BluetoothDevice device) {
		try {
			Method method = device.getClass().getMethod("removeBond",
					(Class[]) null);
			method.invoke(device, (Object[]) null);

		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	private void turnOnBT() {
		if (!mBluetoothAdapter.isEnabled()) {
			Intent intent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
			startActivityForResult(intent, REQUEST_ENABLE_BT);

		} else {
			bt_status.setText("Bluetooth Enabled");
		}
	}

	private void getPairedDevices() {
		pairedDevices.clear();
		devicesArray = mBluetoothAdapter.getBondedDevices();
		if (devicesArray.size() > 0) {
			for (BluetoothDevice device : devicesArray) {
				// showToast(device.getName());
				pairedDevices.add(device.getName());
			}
		}
	}

	private void getOtherDevices() {
		scannedListAdapter.clear();
		mBluetoothAdapter.startDiscovery();
	}

	private void startDiscovery() {
		// TODO Auto-generated method stub
		mBluetoothAdapter.cancelDiscovery();
		mBluetoothAdapter.startDiscovery();

	}

	private void ensureDiscoverable() {
		if (mBluetoothAdapter.getScanMode() != BluetoothAdapter.SCAN_MODE_CONNECTABLE_DISCOVERABLE) {
			Intent discoverableIntent = new Intent(
					BluetoothAdapter.ACTION_REQUEST_DISCOVERABLE);
			discoverableIntent.putExtra(
					BluetoothAdapter.EXTRA_DISCOVERABLE_DURATION, 200);
			startActivity(discoverableIntent);
		}
	}

	private void showToast(String message) {
		Toast.makeText(getApplicationContext(), message, Toast.LENGTH_SHORT)
				.show();
	}

	@Override
	protected void onStart() {
		super.onStart();
		Log.d("start", "start");

		turnOnBT();
	}

	@Override
	protected void onDestroy() {
		super.onDestroy();

		// Make sure we're not doing discovery anymore
		if (mBluetoothAdapter != null) {
			mBluetoothAdapter.cancelDiscovery();
		}

		// Unregister broadcast listeners
		this.unregisterReceiver(receiver);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		MenuInflater inflater = getMenuInflater();
		inflater.inflate(R.menu.main_actions, menu);

		return super.onCreateOptionsMenu(menu);
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
		// action with ID action_refresh was selected
		case R.id.action_search:
			pairedDevices.clear();
			pairedListAdapter.clear();
			scannedListAdapter.clear();
			mBluetoothAdapter.startDiscovery();
			break;

		case R.id.action_discoverable:
			Toast.makeText(this, "ensure discoverable selected",
					Toast.LENGTH_SHORT).show();
			ensureDiscoverable();
			break;
		default:
			break;

		}
		return true;
	}
}
// //