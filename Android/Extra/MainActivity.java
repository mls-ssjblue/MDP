package u.mdp;

import android.content.Context;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.support.v7.app.ActionBarActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.Toast;
//import android.hardware.SensorEventListener;
//import android.app.Activity;
//import com.example.R;


public class MainActivity extends ActionBarActivity implements SensorEventListener {

    private SensorManager senSensorManager;
    private Sensor senAccelerometer;
    private long lastUpdate =0;
    private float last_x, last_y, last_z;
    private static final int SHAKE_THRESHOLD = 600;
    private boolean isTilt=false;
    private int lastDirection = 0;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        senSensorManager = (SensorManager) getSystemService(Context.SENSOR_SERVICE);
        senAccelerometer = senSensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER);
        senSensorManager.registerListener(this, senAccelerometer, SensorManager.SENSOR_DELAY_NORMAL);
    }

    public void onSensorChanged(SensorEvent event) {
        Sensor mySensor = event.sensor;
        final double alpha = 5.0;
        double [] gravity = {0,0,0};

            if(mySensor.getType() == Sensor.TYPE_ACCELEROMETER) {
                double x = event.values[0];
                double y = event.values[1];
                double z = event.values[2];


 //               Log.i("X=" , Double.toString(x));
 //               Log.i("Y=" , Double.toString(y));
 //               Log.i("Z=" , Double.toString(z));


  /*              gravity[0] = alpha * gravity[0] + (1 - alpha) * event.values[0];
                gravity[1] = alpha * gravity[1] + (1 - alpha) * event.values[1];
                gravity[2] = alpha * gravity[2] + (1 - alpha) * event.values[2];

                x = event.values[0] - gravity[0];
                y = event.values[1] - gravity[1];
                z = event.values[2] - gravity[2];

*/
            //    if (Math.abs(x) > Math.abs(y)) {
            if (x > (-2) && x < (2) && y > (-2) && y < (2)) {
                  // do nothing
                if(lastDirection != 0) {
                    lastDirection = 0;
                    Log.i("normal", "Water Level");
                }
            }
            else {
                if (x < -2) {
                    if(lastDirection != 1) {
                        lastDirection = 1;
                        Log.i("right", "Right");
                        tiltRight();
                    }
                } else if (x > 2) {
                    if(lastDirection != 2) {
                        lastDirection = 2;
                        Log.i("left", "Left");
                        tiltLeft();
                    }
                }

                if (y < -2) {
                    if(lastDirection != 3) {
                        lastDirection = 3;
                        Log.i("up", "Up");
                        moveUp();
                    }
                }
                else if (y > 2) {
                    if(lastDirection != 4) {
                        lastDirection = 4;
                        Log.i("down", "Down");
                        moveDown();
                    }
                }
                onResume();
            }
            //}



            //shake gesture
            /*long curTime = System.currentTimeMillis();

            if((curTime - lastUpdate) > 100) {
                long diffTime = (curTime - lastUpdate);
                lastUpdate = curTime;

                speed = Math.abs(x + y + z - last_x - last_y - last_z)/ diffTime * 10000;
            }

            if (speed > SHAKE_THRESHOLD) {

            }

            last_x = x;
            last_y = y;
            last_z = z; */
        }
    }

    public void onAccuracyChanged(Sensor sensor, int accuracy) {

    }

    protected void onPause() {

        super.onPause();
        senSensorManager.unregisterListener(this);
    }

    protected void onResume() {

        super.onResume();
        senSensorManager.registerListener(this, senAccelerometer, SensorManager.SENSOR_DELAY_NORMAL);
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
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.

        switch(item.getItemId()) {
            case R.id.bluetooth:
                //implement the bluetooth on action bar
                //openBluetooth();
                return true;
            case R.id.settings:
                //implement the settings on action bar
                //openSettings();
                return true;
            case R.id.quit:
                quitApp();
                return true;
            default:
                return super.onOptionsItemSelected(item);
        }

    }

    public void tiltLeft() {

        Toast.makeText(getApplicationContext(),"Tilt Left", Toast.LENGTH_SHORT).show();
        onPause();
    }

    public void tiltRight() {

        Toast.makeText(getApplicationContext(),"Tilt Right", Toast.LENGTH_SHORT).show();
        onPause();
    }

    public void moveUp() {

        Toast.makeText(getApplicationContext(),"Move Up", Toast.LENGTH_SHORT).show();
        onPause();
    }

    public void moveDown() {

        Toast.makeText(getApplicationContext(),"Move Down", Toast.LENGTH_SHORT).show();
        onPause();
    }

    private void quitApp() {
        System.exit(0);
    }

    public void greyOutButton(View view) {
        Button button = (Button)findViewById(R.id.update);
        button.setEnabled(false);
    }

}






