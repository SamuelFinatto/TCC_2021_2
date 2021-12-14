# from time import sleep
# from socket import socket, AF_INET, SOCK_DGRAM, SOL_SOCKET, SO_BROADCAST, gethostbyname, gethostname
# PORT = 50000
# MAGIC = "fna349fn" #to make sure we don't confuse or get confused by other programs
# s = socket(AF_INET, SOCK_DGRAM) #create UDP socket
# s.bind(('', 0))
# s.setsockopt(SOL_SOCKET, SO_BROADCAST, 1) #this is a broadcast socket
# my_ip= gethostbyname(gethostname()) #get our IP. Be careful if you have multiple network interfaces or IPs

# while 1:
#     data = MAGIC+my_ip
#     s.sendto(data.encode('UTF-8'), ('<broadcast>', PORT))
#     print ("sent service announcement")
#     sleep(5)
from zeroconf import ServiceBrowser, Zeroconf


class MyListener:

    def remove_service(self, zeroconf, type, name):
        print("Service %s removed" % (name,))

    def add_service(self, zeroconf, type, name):
        info = zeroconf.get_service_info(type, name)
        print("Service %s added, service info: %s" % (name, info))


zeroconf = Zeroconf()
listener = MyListener()
browser = ServiceBrowser(zeroconf, "_http._tcp.local.", listener)
try:
    input("Press enter to exit...\n\n")
finally:
    zeroconf.close()
