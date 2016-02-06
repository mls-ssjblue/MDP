from piConfig import *
from bluetooth import *
import json
import traceback
import threading
import Queue
import errno


class BtThread(threading.Thread):
        mainthread = None
        connected = False
        pi_bt = None
        dataDropped = Queue.Queue()

        def __init__(self, threadID, name):
            threading.Thread.__init__(self)
            self.threadID = threadID
            self.name = name

        def assignMainThread(self, mainThread):
            self.mainthread = mainThread

        def isConnected(self):
            return self.connected

        def send(self, message):
            if self.connected:
                self.piBT.send(message)

        def run(self):
            print "Starting Bluetooth thread"
            while 1:
                self.pi_bt = piBT()
                self.connected = True
                try:
                    while 1:
                        #receivedJSON = self.pi_bt.receive()
                        receive = self.pi_bt.receive()
                        print "Received from Android: " + receive
                        self.mainthread.addToQueue(receive)

                        #data = raw_input("Enter data to send via BT: ")
                        #self.pi_bt.send(data)
                        # code to stop everything
                        ###
                        # if receivedJSON == JSON_STOP:
                        #     self.mainthread.flushCommandQueue()
                        # else:
                        

                except IOError, e:
                    if e.errno == errno.ECONNRESET:
                        print "ERROR: BT disconnected. Try resuming.."
                    else:
                        print "BT Thread Receive Exception: " + e.message
                        print traceback.format_exc()
                    pass
                finally:
                    self.connected = False
                    self.pi_bt.close()


class piBT:
    client_sock = None
    port = None
    
    client_info = None
    buff = None

    def __init__(self):
        # Note: The bluetooth module running on Rpi acts a client and connects to the Bluetooth server running on the Android tablet 

        # self.server_sock = BluetoothSocket(RFCOMM)
        # self.server_sock.bind(("", 1))
        # self.server_sock.listen(10)
        # self.port = self.server_sock.getsockname()[1]
        # advertise_service(self.server_sock, BT_SERVER_NAME,
        #                   service_id=BT_UUID,
        #                   service_classes=[BT_UUID, SERIAL_PORT_CLASS],
        #                   profiles=[SERIAL_PORT_PROFILE],
        #                   )
        # print "Waiting for BT connection on RFCOMM channel " + str(self.port)
        # # need to find out what the stmt below means
        # self.client_sock, self.client_info = self.server_sock.accept()
        # print "Accepted BT connection from ", self.client_info

        # # instantiate file buffer for receival
        # if self.client_sock is not None:
        #     self.buff = self.client_sock.makefile("r")
        uuid = "00001101-0000-1000-8000-00805f9b34fb"
        service_matches =""
        service_matches = find_service( uuid = uuid, address = BT_SERVER)
        if len(service_matches) == 0:
            print "couldn't find the service =("      
        while len(service_matches) ==0 :
            service_matches = find_service( uuid = uuid, address = BT_SERVER)
        if len(service_matches) != 0:
            print " Found the service =(" 
                #sys.exit(0)

        first_match = service_matches[0]
        port = first_match["port"]
        name = first_match["name"]
        host = first_match["host"]

        print "connecting to \"%s\" on %s" % (name, host)

        self.client_sock=BluetoothSocket( RFCOMM )
        self.client_sock.connect((host, port))

        print "connected. type stuff"
        # while True:
        #        # data1 = raw_input()
        #        # if len(data1) == 0: break;
        #        # sock.send("%s\n\r" % data1)
        #     data = sock.recv(1024)
        #     print data

    def receive(self):  #Receive from Android Tablet
        if self.client_sock is None:
            return
        #return receiveData(self.buff, "BT")
        return self.client_sock.recv(1024)
    def send(self, senddata): # Send to Android Tablet
        if self.client_sock is not None:
            try:
                #json_string = json.dumps(senddata)
                self.client_sock.send(senddata)
                print "Send to BT: " + str(senddata)
            except IOError, e:
                print "Bluetooth Sending Exception: " + e.message
                print traceback.format_exc()
                pass

    def close(self):
        self.client_sock.close()
        #self.server_sock.close()
