package com.example.mdp;

import android.app.Activity;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.util.Log;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;

public class Configuration extends Activity {
	public final static String storeName = "appData";
	public static final String NULL = "NA";
	private EditText f1Txt;
	private EditText f2Txt;
	private Button save;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.configuration);

		f1Txt = (EditText) findViewById(R.id.f1Txt);
		f2Txt = (EditText) findViewById(R.id.f2Txt);
		save = (Button) findViewById(R.id.saveBtn);
		loadFunctionData();
		save.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View v) {
				saveFunctionData();
				//loadFunctionData();
				finish();
			}
		});
	}

	private void loadFunctionData() {
		SharedPreferences sharedPreferences = getApplicationContext()
				.getSharedPreferences(storeName, MODE_PRIVATE);
		String f1 = sharedPreferences.getString("functionKey1", NULL);
		String f2 = sharedPreferences.getString("functionKey2", NULL);
		Log.d("f1", f1);
		Log.d("f2", f2);
		if (f1 != NULL) {
			f1Txt.setText(f1);
		}
		if (f2 != NULL) {
			f2Txt.setText(f2);
		}
	}

	private void saveFunctionData() {
		SharedPreferences sharedPreferences = getApplicationContext()
				.getSharedPreferences(storeName, MODE_PRIVATE);
		Editor editor = sharedPreferences.edit();
		
		editor.putString("functionKey1", f1Txt.getText().toString());
		editor.putString("functionKey2", f2Txt.getText().toString());
		editor.commit();
		
	}
}
