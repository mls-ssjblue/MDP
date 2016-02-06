from piConfig import *
import serial
import threading
import json
import traceback
import glob


class ArduinoThread(threading.Thread):
        mainthread = None
        piArduino = None
       

        def __init__(self, threadID, name):
            threading.Thread.__init__(self)
            self.threadID = threadID
            self.name = name

        def assignMainThread(self, mainThread):
            self.mainthread = mainThread

        def send(self, message):
            if self.piArduino is None:
                print "Cannot send data to robot. Robot is gone"
                return
            self.piArduino.send(message)

        def sendStart(self):
            if self.piArduino is None:
                print "Cannot send START to robot. Robot is gone"
                return
            self.piArduino.send(JSON_START)

        def sendStop(self):
            if self.piArduino is None:
                print "Cannot send STOP to robot. Robot is gone"
                return
            self.piArduino.send(JSON_STOP)

        def run(self):
            print "[ Arduino Thread Start ]"
            self.piArduino = piArduino()

            while 1:
                try:
                    #data = raw_input("Enter data1: ")
                    # self.piArduino.ser.write(data)
                    
                    # self.piArduino.ser.write(data)
                    #self.piArduino.send(data)
                    #self.piArduino.receive()
                    receivedMessage = self.piArduino.receive()
                    self.mainthread.addToQueue(receivedMessage)
                except IOError, e:
                    print "Arduino Thread Receive Exception: " + e.message
                    print traceback.format_exc()
                    pass


class piArduino:

    sensorLog = None
    full_list = None
    def __init__(self):
        self.full_list = []
        while 1:
            try:
                arduinoPort = glob.glob("/dev/ttyACM*")[0]
                self.ser = serial.Serial(arduinoPort, 9600)
                print "Arduino Connected"
                break
            except IndexError:
                print "Trying to reconnect Arduino.."
                pass

    def send(self, data):
        #command = json_data["data"]
        try:
            # if command == "E":
            #     self.sensorLog = open("sensor.log", "w")
            #self.ser.write(command)
            self.ser.write(data)
            #self.ser.write(data)

            print "Send to Arduino: " + data

        except AttributeError:
            print "WARNING! Arduino still not connected."
            pass

    def receive(self):
        print "receiving.."
        try:
            #ard_string = self.ser.readline().strip()
            val = self.ser.readline()
            print "From robot: " + val
            return val
            # self.convert2list(val)
            # msg = self.full_list[0]
            # del self.full_list[0]
            # if msg != None:
            #     print "Robot sends:" + msg + 'Z'
            #     return msg + 'Z'
            # try:
            #     #json_data = json.loads(json_string)
            #     # if (json_data["type"] == "reading"):
            #     #     logJSON(json_data, self.sensorLog)
            #     # elif (json_data["data"] == "END_EXP"):
            #     #     self.sensorLog.close()
            #     #print "ROBOT:\n" + json.dumps(json_data, indent=4) + "\n"
            #  ###   # print "ROBOT:\n" + ard_string
            #     # return ard_string
            # except (ValueError, TypeError):
            #     print "From robot: " + ard_string
            #     pass
        except (serial.SerialException, AttributeError):
            print AttributeError
            pass

    def convert2list(self,string):
        string = string.strip()
        string_list = string.split('Z')
        self.full_list.extend(string_list)
        # Return to caller.
       

    