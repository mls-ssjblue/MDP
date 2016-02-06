from piConfig import *
import time


class protocolHandler:

    def __init__(self, wifi, bt, arduino):
        self.pc = wifi
        self.android = bt
        self.robot = arduino

    # what to do with the JSON data
    def decodeCommand(self, string, lock):
        options = {"C": self.sendCommand,
                   "R": self.sendReading,
                   "M": self.sendMap,
                   "S": self.sendStatus,
                   "F": self.makeMovement,
                   'F': self.makeMovement,
                   "SP": self.doShortestPath,
                   "exp": self.doExploration
                   }
    

        
        messageType = string[0]
        messageData = string[1:]
        print "in decode command"

        if string == "exp":
            self.doExploration(string,lock)

       
        elif options.get(messageType):
            print "Message Type:" + messageType + "data:" + messageData
            options[messageType](string, lock)
        elif messageType == 'T':
            self.makeMovement(string,lock)
        elif messageType >=  "1" and messageType <= "9":
            self.sendReading(string,lock)
        else:
            print "ERROR: Invalid message format"

        #if messageType == ""

    def sendCommand(self, message_data, lock):
        # if command_data == CMD_START_EXP:
        #     # start robot
        #     lock.acquire()
        #     self.robot.sendSTART()
        #     lock.release()

        #     # inform PC
        #     lock.acquire()
        #     self.pc.send(message_data)
        #     lock.release()

        #     # begin exp
        #     lock.acquire()
        #     self.robot.send(message_data)
        #     lock.release()
        #     print "..starting exploration.."
        # elif command_data == CMD_START_PATH:
        #     # start robot
        #     lock.acquire()
        #     self.robot.sendSTART()
        #     lock.release()

        #     # inform PC
        #     lock.acquire()
        #     self.pc.send(message_data)
        #     lock.release()

        #     # begin shortest path
        #     lock.acquire()
        #     self.robot.send(message_data)
        #     lock.release()
        #     print "..starting shortest path.."
        if command_data == CMD_START_REMOTE:
            # start robot
            lock.acquire()
            self.robot.sendStart()
            lock.release()

            # begin remote control
            lock.acquire()
            self.robot.send(message_data)
            lock.release()
        else:
            print "ERROR: unknown command data - cannot process"

    def sendReading(self, message_data, lock):
        lock.acquire()
        #time.sleep(.1)
        self.pc.send(message_data)
        lock.release()
        print "..sending robot data to PC.."

        lock.acquire()
        self.android.send(message_data)
        lock.release()
        print "..sending robot data to Android.."

    def sendMap(self, message_data, lock):
        lock.acquire()
        self.android.send(message_data)
        lock.release()
        print "..sending map data to Android.."

    def sendStatus(self, message_data, lock):
        if message_data == ST_END_EXP:
            lock.acquire()
            self.pc.send(message_data)
            lock.release()

            lock.acquire()
            self.android.send(message_data)
            lock.release()
            print "..sending end exploration.."
        elif message_data == ST_END_PATH:
            lock.acquire()
            self.pc.send(message_data)
            lock.release()

            lock.acquire()
            self.android.send(message_data)
            lock.release()
            print "..sending end shortest path.."
        elif message_data == ST_END_REMOTE:
            lock.acquire()
            self.android.send(message_data)
            lock.release()
        else:
            print "ERROR: unknown status data - cannot process"

    def makeMovement(self, message_data, lock):
        print "in make movement"
        #send robot command to move ('FS05Z')
        lock.acquire()
        self.robot.send(message_data)
        lock.release()
        print "..sending robot movement.." + message_data

    def doShortestPath(self, message_data, lock):
        #sending message to robot
        lock.acquire()
        self.robot.send(message_data)
        lock.release()

        #send to pc also
        lock.acquire()
        self.pc.send(message_data)
        lock.release()

    def doExploration(self,message_data,lock):
        
        #sending message to robot
        lock.acquire()
        self.robot.send(message_data + 'Z')
        lock.release()
        
        #sending to pc

        lock.acquire()
        self.pc.send(message_data + 'Z')
        lock.release()
