import json

# CONSTANTS #
# commands

CMD_STARTEXPLORE = "E"
CMD_STARTSHORTESTPATH = "P"
CMD_START_REMOTE = "R"

#status

ST_END_EXP = ""

#json
# JSON_START = {"type" :"command","data:":"S"}
# JSON_STOP = {"type":"move","data":"G"}
#network
WIFI_IP = ""
WIFI_PORT = 9063
BT_UUID = ""
BT_SERVER = "08:60:6E:A5:16:34"

#multithreading
SEMAPHORE_BUF = 3

#reusable methodsATATATATATATAT

def receiveJSON(buff, senderName):

    json_recstring = buff.readline()
    if json_string is None:
        return None
    try:
        json_data = json.loads(json_string.strip())
        print "JSON from " + senderName + ": " + str(json_string)
        return json_data
    except ValueError:
        if len(json_string) <= 0:
            raise IOError()
        print "string from " + senderName + ": " + str(json_string)
        pass


def logJSON(json_data,file):
	file.write(json.dumps(json_data,indent = 4) + "\n")

def receiveData(buff, senderName):

    string_rec = buff.readline()
    if string_rec is None:
        return None
    print "string from " + senderName + ":" + string_rec

    return string_rec
 
