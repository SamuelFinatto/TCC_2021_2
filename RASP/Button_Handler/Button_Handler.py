import time
from datetime import datetime
import os
import subprocess
import requests
import netifaces as ni
import urllib.parse

def get_ip_address(ifname):
    # s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    # return socket.inet_ntoa(fcntl.ioctl(
    #     s.fileno(),
    #     0x8915,  # SIOCGIFADDR
    #     struct.pack("256s".encode('utf-8'), ifname[:15])
    # )[20:24])
    ip = ni.ifaddresses(ifname)[ni.AF_INET][0]['addr']
    return ip

try:
    os.environ.pop('PYTHONIOENCODING')
except KeyError:
    pass

class Finder:

    def runCommand(self, command):
        print("Running", command)
        cmd = subprocess.Popen(command,cwd="/", stdin=subprocess.PIPE, stdout=subprocess.PIPE,
                                   stderr=subprocess.PIPE, shell=True).communicate()[0]
        print(cmd)
        return cmd


    def __init__(self, *args, **kwargs):
        self.ssid = kwargs['ssid']
        self.password = kwargs['password']
        self.interface_name = kwargs['interface']
        self.main_dict = {}

    def run(self):
        try:
            result = self.connection()
        except Exception as exp:
            print("Couldn't connect to")
            print(exp)
        else:
            if result:
                print("Successfully connected")
                print("Running Command to ESP connect to wifi")
                ip = get_ip_address('eth0')
                print(ip)
                safe_pass = urllib.parse.quote(self.password)
                cmd = 'curl "http://172.16.5.1/setting?pass='+safe_pass+'&ssid='+self.ssid+'&server='+ip+'" -m 4 --request GET --data \'datadatasi*&*H*\''
                try:
                    result = self.runCommand(cmd)
                    print(result)
                except:
                    raise # Not Connected
                else:
                    return True # Connected
            

    def connection(self):
        try:
            self.runCommand('sudo nmcli d wifi rescan')
            time.sleep(2)
            self.runCommand('sudo nmcli d wifi rescan')
            time.sleep(2)
            self.runCommand('sudo nmcli d wifi connect test_tcc password password123')
        except:
            raise # Not Connected
        else:
            return True # Connected

if os.uname()[4][:3] != 'arm':
    # Faking
    import sys
    import fake_rpi
    print("Faking RPi.GPIO")
    sys.modules['RPi'] = fake_rpi.RPi
    sys.modules['RPi.GPIO'] = fake_rpi.RPi.GPIO


import RPi.GPIO as GPIO # Import Raspberry Pi GPIO library
def button_callback(channel):
    print("Button was pushed!")
    print(datetime.now())
    interface_name = "eth0" # i. e wlp2s0  
    password = requests.get('http://127.0.0.1/WiFi/password').content.decode('utf-8')
    ssid = requests.get('http://127.0.0.1/WiFi/ssid').content.decode('utf-8')

    f = Finder(ssid=ssid, password=password, interface=interface_name)
    f.run()

    # networks = list(Cell.all('wlp2s0'))

    # for i in range(0,len(networks)):
    #     print(str(networks[i]) + " is encrypted: "+ str(networks[i].encrypted) + "= " + str(networks[i].encryption_type) + " | address: " +str(networks[i].address))
    # for net in networks:
    #     if net.ssid == "Gothan_GWR-130" :
    #         try:
    #             print("Trying to connect")
    #             scheme = Scheme.for_cell('wlp2s0', net.ssid, net) #"zmjs1648!")
    #             scheme.delete()
    #             scheme.save()
    #             scheme.activate()
    #         except Exception as detail:
    #             print(detail)

    #         break

GPIO.setwarnings(False) # Ignore warning for now
GPIO.setmode(GPIO.BOARD) # Use physical pin numbering
GPIO.setup(10, GPIO.IN, pull_up_down=GPIO.PUD_DOWN) # Set pin 10 to be an input pin and set initial value to be pulled low (off)
GPIO.add_event_detect(10,GPIO.RISING,callback=button_callback) # Setup event on pin 10 rising edge
print("Service is running.")
while True:
    # button_callback(None)
    pass
     # Run until someone presses enter
GPIO.cleanup() # Clean up