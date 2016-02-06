from piConfig import *
import threading
import traceback
import Queue
from protocol import protocolHandler
from piarduino import ArduinoThread
from piwifi import WifiThread
from pibt  import BtThread


class coreThread(threading.Thread):

    commandQueue = None
    lock = None

    def __init__(self, threadID, name, wifi, bt, arduino):
        threading.Thread.__init__(self)
        self.threadID = threadID
        self.name = name

        # set the command queue
        self.lock = threading.BoundedSemaphore(SEMAPHORE_BUF)
        self.commandQueue = Queue.Queue()

        # assign thread
        self.wifi = wifi
        self.bt = bt
        self.arduino = arduino

        # assign handler for command
        self.protocolHandler = protocolHandler(wifi, bt, arduino)

    def addToQueue(self, data):
        if data is not None:
            print "[Adding to queue: " + str(data) + " ]"
            self.commandQueue.put(data)

    def flushCommandQueue(self): # Stops the robot and clears the command queue
        self.lock = threading.BoundedSemaphore(SEMAPHORE_BUF)
        self.arduino.sendSTOP()
        with self.commandQueue.mutex:
            self.commandQueue.queue.clear()
        print "[ Flushed command queue ]"

    def processCommand(self):   # take commands from the command queue and execute them
        if not self.commandQueue.empty():
            command = self.commandQueue.get()
            self.protocolHandler.decodeCommand(command, self.lock)

    def run(self):
        #while not self.wifi.isConnected() or not self.bt.isConnected():
         #   continue

        print "==========================="
        print " MDP Group 3 Starting Up /"
        print "==========================="

        while 1:
            try:
                if self.wifi.isConnected(): #and self.bt.isConnected():
                    #print "Main thread run"
                    self.processCommand()   # keep processing commands in the command queue continously 
                 # msg = raw_input("get data")
                 # self.arduino_thread.
            except Exception, e:
                print "Unable to decode JSON: " + e.message
                print traceback.format_exc()
                pass

wifi_thread = WifiThread(1, "WIFI")
#arduino_thread = ArduinoThread(2, "ARDUINO")
bt_thread = BtThread(3, "BT")
core = coreThread(0, "CORE", wifi_thread, bt_thread, arduino_thread)

wifi_thread.assignMainThread(core)
arduino_thread.assignMainThread(core)
bt_thread.assignMainThread(core)

wifi_thread.start()

arduino_thread.start()
bt_thread.start()
core.start()

# Order of execution in mainthread.py :
#1. initialise the three threads + main thread
#2. start the three threads + main thread (starting each thread calls the run() function in each respective thread)
#3. The run() function initialises all the three parts and spins off their threads with an infinite loop waiting for connections and data transfer
